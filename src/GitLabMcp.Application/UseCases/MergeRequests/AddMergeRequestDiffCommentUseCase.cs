using System;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Errors;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class AddMergeRequestDiffCommentUseCase
{
    private readonly IGitLabApiClient _client;

    public AddMergeRequestDiffCommentUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        string comment,
        string filePath,
        int lineNumber,
        string? lineType,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        Guard.AgainstNullOrWhiteSpace(comment, nameof(comment));
        Guard.AgainstNullOrWhiteSpace(filePath, nameof(filePath));
        Guard.AgainstNonPositive(lineNumber, nameof(lineNumber));

        var normalizedLineType = string.IsNullOrWhiteSpace(lineType) ? "new" : lineType.Trim().ToLowerInvariant();
        if (!string.Equals(normalizedLineType, "new", StringComparison.Ordinal) &&
            !string.Equals(normalizedLineType, "old", StringComparison.Ordinal))
        {
            throw new ValidationException("lineType must be 'new' or 'old'.");
        }

        return _client.AddMergeRequestDiffCommentAsync(
            projectId,
            mergeRequestIid,
            comment,
            filePath,
            lineNumber,
            normalizedLineType,
            cancellationToken);
    }
}
