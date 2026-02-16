using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class ListOpenMergeRequestsUseCase
{
    private readonly IGitLabApiClient _client;

    public ListOpenMergeRequestsUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task<IReadOnlyList<MergeRequestSummary>> ExecuteAsync(
        int projectId,
        string? state,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        var normalizedState = string.IsNullOrWhiteSpace(state) ? "opened" : state!;
        return _client.ListMergeRequestsAsync(projectId, normalizedState, cancellationToken);
    }
}
