using System;
using System.Collections.Generic;

namespace GitLabMcp.Domain.Entities;

public sealed record IssueDetails(
    int Iid,
    string Title,
    string State,
    string AuthorName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string WebUrl,
    IReadOnlyList<string> Labels,
    string Description);
