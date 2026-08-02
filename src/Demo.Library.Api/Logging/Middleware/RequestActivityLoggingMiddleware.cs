using System.Text;
using Demo.Library.Api.Logging;
using Demo.Library.Api.Logging.Options;
using Microsoft.Extensions.Options;

namespace Demo.Library.Api.Logging.Middleware;

internal sealed class RequestActivityLoggingMiddleware(
    IOptions<ActivityLoggingOptions> options) : IMiddleware
{
    private readonly ActivityLoggingOptions options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.IsWebActivityRequest())
        {
            await next(context);
            return;
        }

        context.GetOrCreateCorrelationId();

        if (options.CaptureRequestBody && CanHaveBody(context.Request))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var fullBody = await reader.ReadToEndAsync(context.RequestAborted);
            context.Request.Body.Position = 0;

            var (bodyToStore, wasTruncated) = TruncateForStorage(fullBody, options.MaxBodyLength);
            context.Items[HttpRequestLoggingExtensions.ActivityRequestBodyItemKey] = bodyToStore;
            context.Items[HttpRequestLoggingExtensions.ActivityRequestBodyWasTruncatedItemKey] = wasTruncated;
        }

        await next(context);
    }

    private static bool CanHaveBody(HttpRequest request)
    {
        if (!request.Body.CanRead)
        {
            return false;
        }

        return HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method)
            || HttpMethods.IsDelete(request.Method);
    }

    private static (string Body, bool WasTruncated) TruncateForStorage(string? body, int maxLength)
    {
        if (string.IsNullOrEmpty(body))
        {
            return (string.Empty, false);
        }

        if (maxLength <= 0 || body.Length <= maxLength)
        {
            return (body, false);
        }

        return (body[..maxLength], true);
    }
}