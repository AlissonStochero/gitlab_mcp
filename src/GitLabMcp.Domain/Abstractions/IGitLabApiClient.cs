using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Domain.Abstractions;

public interface IGitLabApiClient
{
    Task<IReadOnlyList<GitLabProject>> GetProjectsAsync(string? search, string visibility, CancellationToken cancellationToken);
    Task<IReadOnlyList<MergeRequestSummary>> ListMergeRequestsAsync(int projectId, string state, CancellationToken cancellationToken);
    Task<MergeRequestDetails> GetMergeRequestDetailsAsync(int projectId, int mergeRequestIid, CancellationToken cancellationToken);
    Task<IReadOnlyList<Note>> GetMergeRequestCommentsAsync(int projectId, int mergeRequestIid, CancellationToken cancellationToken);
    Task AddMergeRequestCommentAsync(int projectId, int mergeRequestIid, string comment, CancellationToken cancellationToken);
    Task AddMergeRequestDiffCommentAsync(int projectId, int mergeRequestIid, string comment, string filePath, int lineNumber, string lineType, CancellationToken cancellationToken);
    Task<MergeRequestDiff> GetMergeRequestDiffAsync(int projectId, int mergeRequestIid, CancellationToken cancellationToken);
    Task<IssueDetails> GetIssueDetailsAsync(int projectId, int issueIid, CancellationToken cancellationToken);
    Task SetMergeRequestTitleAsync(int projectId, int mergeRequestIid, string title, CancellationToken cancellationToken);
    Task SetMergeRequestDescriptionAsync(int projectId, int mergeRequestIid, string description, CancellationToken cancellationToken);
    Task ApproveMergeRequestAsync(int projectId, int mergeRequestIid, CancellationToken cancellationToken);
}
