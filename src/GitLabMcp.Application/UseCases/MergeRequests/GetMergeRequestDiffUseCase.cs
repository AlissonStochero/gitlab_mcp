using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class GetMergeRequestDiffUseCase
{
    private readonly IGitLabApiClient _client;

    public GetMergeRequestDiffUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task<MergeRequestDiff> ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        return _client.GetMergeRequestDiffAsync(projectId, mergeRequestIid, cancellationToken);
    }
}
