using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class GetMergeRequestDetailsUseCase
{
    private readonly IGitLabApiClient _client;

    public GetMergeRequestDetailsUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task<MergeRequestDetails> ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        return _client.GetMergeRequestDetailsAsync(projectId, mergeRequestIid, cancellationToken);
    }
}
