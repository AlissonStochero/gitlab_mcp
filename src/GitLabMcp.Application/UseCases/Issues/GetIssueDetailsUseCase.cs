using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.Validation;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;

namespace GitLabMcp.Application.UseCases.Issues;

public sealed class GetIssueDetailsUseCase
{
    private readonly IGitLabApiClient _client;

    public GetIssueDetailsUseCase(IGitLabApiClient client)
    {
        _client = client;
    }

    public Task<IssueDetails> ExecuteAsync(
        int projectId,
        int issueIid,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositive(projectId, nameof(projectId));
        Guard.AgainstNonPositive(issueIid, nameof(issueIid));
        return _client.GetIssueDetailsAsync(projectId, issueIid, cancellationToken);
    }
}
