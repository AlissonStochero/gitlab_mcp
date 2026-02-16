using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Application.UseCases.Projects;

public sealed class GetProjectsUseCase
{
    private readonly IGitLabApiClient _client;

    public GetProjectsUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task<IReadOnlyList<GitLabProject>> ExecuteAsync(
        string? search,
        string? visibility,
        CancellationToken cancellationToken = default)
    {
        var normalizedVisibility = string.IsNullOrWhiteSpace(visibility) ? "private" : visibility!;
        return _client.GetProjectsAsync(search, normalizedVisibility, cancellationToken);
    }
}
