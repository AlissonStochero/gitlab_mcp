using System;

namespace GitLabMcp.Domain.Errors;

public sealed class GitLabApiException : Exception
{
    public GitLabApiException(string message, int? statusCode = null, string? responseBody = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public int? StatusCode { get; }
    public string? ResponseBody { get; }
}
