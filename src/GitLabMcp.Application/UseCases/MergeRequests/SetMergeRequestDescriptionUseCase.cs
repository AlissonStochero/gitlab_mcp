using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class SetMergeRequestDescriptionUseCase
{
    private readonly IGitLabApiClient _client;

    public SetMergeRequestDescriptionUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        string description,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        Guard.AgainstNullOrWhiteSpace(description, nameof(description));
        return _client.SetMergeRequestDescriptionAsync(projectId, mergeRequestIid, description, cancellationToken);
    }
}
