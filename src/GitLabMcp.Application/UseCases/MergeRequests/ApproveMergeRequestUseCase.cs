using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class ApproveMergeRequestUseCase
{
    private readonly IGitLabApiClient _client;

    public ApproveMergeRequestUseCase(IGitLabApiClient client)
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
        return _client.ApproveMergeRequestAsync(projectId, mergeRequestIid, cancellationToken);
    }
}
