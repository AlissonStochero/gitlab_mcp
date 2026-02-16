namespace GitLabMcp.Domain.Entities;

public sealed record GitLabProject(
    int Id,
    string Name,
    string PathWithNamespace,
    string Visibility);
