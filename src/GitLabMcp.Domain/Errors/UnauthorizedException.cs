using System;

namespace GitLabMcp.Domain.Errors;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
