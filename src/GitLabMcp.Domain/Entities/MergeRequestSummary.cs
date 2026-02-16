namespace GitLabMcp.Domain.Entities;

public sealed record MergeRequestSummary(
    int Iid,
    string Title,
    string State,
    string AuthorName);
