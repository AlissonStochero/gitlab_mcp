namespace GitLabMcp.Domain.Abstractions;

public interface IMcpRequestAuthenticator
{
    bool IsAuthorized(string? token);
}
