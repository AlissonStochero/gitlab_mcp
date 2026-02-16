namespace GitLabMcp.Infrastructure.Configuration;

public sealed class GitLabClientOptions
{
    public string BaseUrl { get; set; } = "https://gitlab.com";
    public string Token { get; set; } = string.Empty;
}
