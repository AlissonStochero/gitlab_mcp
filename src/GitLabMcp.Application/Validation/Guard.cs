using GitLabMcp.Domain.Errors;

namespace GitLabMcp.Application.Validation;

public static class Guard
{
    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{paramName} is required.");
        }
    }

    public static void AgainstNonPositive(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ValidationException($"{paramName} must be greater than zero.");
        }
    }
}
