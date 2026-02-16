using System.Collections.Generic;

namespace GitLabMcp.Domain.Entities;

public sealed record MergeRequestDiff(
    int Iid,
    IReadOnlyList<DiffChange> Changes);
