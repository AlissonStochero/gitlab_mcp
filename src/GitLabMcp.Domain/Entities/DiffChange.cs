namespace GitLabMcp.Domain.Entities;

public sealed record DiffChange(
    string NewPath,
    string OldPath,
    string Diff,
    bool NewFile,
    bool DeletedFile,
    bool RenamedFile);
