using System;

namespace GitLabMcp.Domain.Errors;

public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
