using System.Threading;

namespace GitLabMcp.Infrastructure.Http;

public sealed class GitLabTokenContext
{
    private static readonly AsyncLocal<string?> CurrentToken = new();

    public string? Token
    {
        get => CurrentToken.Value;
        set => CurrentToken.Value = value;
    }
}
