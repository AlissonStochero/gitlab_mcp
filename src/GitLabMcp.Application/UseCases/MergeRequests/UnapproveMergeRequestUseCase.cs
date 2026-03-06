using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class UnapproveMergeRequestUseCase
{
    private readonly IGitLabApiClient _client;

    public UnapproveMergeRequestUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        return _client.UnapproveMergeRequestAsync(projectId, mergeRequestIid, cancellationToken);
    }
}
