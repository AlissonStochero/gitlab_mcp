using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class AddMergeRequestCommentUseCase
{
    private readonly IGitLabApiClient _client;

    public AddMergeRequestCommentUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        string comment,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        Guard.AgainstNullOrWhiteSpace(comment, nameof(comment));
        return _client.AddMergeRequestCommentAsync(projectId, mergeRequestIid, comment, cancellationToken);
    }
}
