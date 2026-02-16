using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class SetMergeRequestTitleUseCase
{
    private readonly IGitLabApiClient _client;

    public SetMergeRequestTitleUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        string title,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        Guard.AgainstNullOrWhiteSpace(title, nameof(title));
        return _client.SetMergeRequestTitleAsync(projectId, mergeRequestIid, title, cancellationToken);
    }
}
