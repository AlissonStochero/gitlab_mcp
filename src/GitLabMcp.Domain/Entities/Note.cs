using System;

namespace GitLabMcp.Domain.Entities;

public sealed record Note(
    string AuthorName,
    string Body,
    DateTimeOffset CreatedAt,
    bool System);
