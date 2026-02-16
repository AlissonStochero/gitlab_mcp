using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Application.UseCases.MergeRequests;

public sealed class GetMergeRequestCommentsUseCase
{
    private readonly IGitLabApiClient _client;

    public GetMergeRequestCommentsUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Note>> ExecuteAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(mergeRequestIid, nameof(mergeRequestIid));
        var notes = await _client.GetMergeRequestCommentsAsync(projectId, mergeRequestIid, cancellationToken);
        return notes.Where(note => !note.System).ToList();
    }
}
