namespace Demo.Library.Api.Logging.Options;

internal sealed class ActivityLoggingOptions
{
    public const string SectionName = "ActivityLogging";

    public bool CaptureRequestBody { get; set; } = true;

    public bool CaptureResponseBody { get; set; } = true;

    public int MaxBodyLength { get; set; } = 16384;
}