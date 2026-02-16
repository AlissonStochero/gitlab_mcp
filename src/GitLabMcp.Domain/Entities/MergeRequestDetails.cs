namespace GitLabMcp.Domain.Entities;

public sealed record MergeRequestDetails(
    int Iid,
    string Title,
    string State,
    string AuthorName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? MergedAt,
    string WebUrl,
    string Description,
    string SourceBranch,
    string TargetBranch,
    bool Draft,
    bool HasConflicts,
    string DetailedMergeStatus,
    IReadOnlyList<string> Labels,
    IReadOnlyList<string> Reviewers,
    IReadOnlyList<string> Assignees);
