using System.Diagnostics;
using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Logging;
using Demo.Library.Api.Logging.Options;
using Demo.Library.Api.Logging.Utilities;
using Demo.Library.Api.Persistence.Entities;
using Demo.Library.Api.Persistence;
using Microsoft.Extensions.Options;

namespace Demo.Library.Api.Logging.Middleware;

internal sealed class ResponseActivityLoggingMiddleware(
    LibraryDbContext dbContext,
    IOptions<ActivityLoggingOptions> options,
    ILogger<ResponseActivityLoggingMiddleware> logger) : IMiddleware
{
    private readonly LibraryDbContext dbContext = dbContext;
    private readonly ActivityLoggingOptions options = options.Value;
    private readonly ILogger<ResponseActivityLoggingMiddleware> logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.IsWebActivityRequest())
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        Exception? downstreamException = null;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            downstreamException = ex;
            throw;
        }
        finally
        {
            string? responseBody = null;
            var responseBodyWasTruncated = false;

            try
            {
                if (options.CaptureResponseBody)
                {
                    responseBuffer.Position = 0;
                    using var reader = new StreamReader(responseBuffer, leaveOpen: true);
                    var fullResponseBody = await reader.ReadToEndAsync(context.RequestAborted);
                    (responseBody, responseBodyWasTruncated) = TruncateForStorage(fullResponseBody, options.MaxBodyLength);
                }

                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalBodyStream, context.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Request canceled while copying buffered response content.");
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }

            stopwatch.Stop();

            var activityLog = BuildActivityLog(context, stopwatch.ElapsedMilliseconds, responseBody, responseBodyWasTruncated, downstreamException);
            try
            {
                await dbContext.EndpointActivityLogs.AddAsync(activityLog, context.RequestAborted);
                await dbContext.SaveChangesAsync(context.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Request canceled while saving endpoint activity log.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save endpoint activity log using EF Core.");
            }
        }
    }

    private EndpointActivityLogEntity BuildActivityLog(
        HttpContext context,
        long elapsedMilliseconds,
        string? responseBody,
        bool responseBodyWasTruncated,
        Exception? downstreamException)
    {
        var requestBody = context.Items[HttpRequestLoggingExtensions.ActivityRequestBodyItemKey] as string;
        var requestBodyWasTruncated = context.Items[HttpRequestLoggingExtensions.ActivityRequestBodyWasTruncatedItemKey] as bool? ?? false;

        var userId = context.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)
            && context.Request.Headers.TryGetValue("x-user-id", out var userHeader)
            && !string.IsNullOrWhiteSpace(userHeader.ToString()))
        {
            userId = userHeader.ToString();
        }

        return new EndpointActivityLogEntity
        {
            ActivityType = EndpointActivityLogEntity.WebActivityType,
            CorrelationId = context.GetOrCreateCorrelationId(),
            TimestampUtc = DateTime.UtcNow,
            Method = context.Request.Method,
            Path = context.Request.Path.ToString(),
            QueryString = context.Request.QueryString.Value,
            StatusCode = context.Response.StatusCode,
            DurationMs = elapsedMilliseconds,
            UserId = userId,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            RequestContentType = context.Request.ContentType,
            RequestBody = requestBody,
            RequestBodyWasTruncated = requestBodyWasTruncated,
            ResponseContentType = context.Response.ContentType,
            ResponseBody = responseBody,
            ResponseBodyWasTruncated = responseBodyWasTruncated,
            RequestHeaders = HeaderSanitizer.Sanitize(context.Request.Headers),
            ResponseHeaders = HeaderSanitizer.Sanitize(context.Response.Headers),
            ErrorMessage = downstreamException?.Message
        };
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