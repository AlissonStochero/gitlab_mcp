using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;
using GitLabMcp.Domain.Errors;
using GitLabMcp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitLabMcp.Infrastructure.Http;

public sealed class GitLabApiClient : IGitLabApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitLabApiClient> _logger;
    private readonly GitLabTokenContext _tokenContext;
    private readonly string _defaultToken;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _missingTokenLogged;

    public GitLabApiClient(
        HttpClient httpClient,
        IOptions<GitLabClientOptions> options,
        GitLabTokenContext tokenContext,
        ILogger<GitLabApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _tokenContext = tokenContext;
        _defaultToken = options.Value.Token ?? string.Empty;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyList<GitLabProject>> GetProjectsAsync(
        string? search,
        string visibility,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects?visibility={Uri.EscapeDataString(visibility)}&membership=true&per_page=100";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var projects = await ReadJsonAsync<List<ProjectDto>>(response, cancellationToken);
        return projects
            .Select(project => new GitLabProject(
                project.Id,
                project.Name ?? string.Empty,
                project.PathWithNamespace ?? string.Empty,
                project.Visibility ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<MergeRequestSummary>> ListMergeRequestsAsync(
        int projectId,
        string state,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests?state={Uri.EscapeDataString(state)}";
        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var mergeRequests = await ReadJsonAsync<List<MergeRequestDto>>(response, cancellationToken);
        return mergeRequests
            .Select(mr => new MergeRequestSummary(
                mr.Iid,
                mr.Title ?? string.Empty,
                mr.State ?? string.Empty,
                mr.Author?.Name ?? string.Empty))
            .ToList();
    }

    public async Task<MergeRequestDetails> GetMergeRequestDetailsAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}";
        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var mr = await ReadJsonAsync<MergeRequestDto>(response, cancellationToken);
        var reviewers = (mr.Reviewers ?? Array.Empty<ReviewerDto>())
            .Select(r => r.Name ?? string.Empty)
            .Where(n => n.Length > 0)
            .ToList();
        var assignees = (mr.Assignees ?? Array.Empty<AssigneeDto>())
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.Length > 0)
            .ToList();
        return new MergeRequestDetails(
            mr.Iid,
            mr.Title ?? string.Empty,
            mr.State ?? string.Empty,
            mr.Author?.Name ?? string.Empty,
            mr.CreatedAt,
            mr.UpdatedAt,
            mr.MergedAt,
            mr.WebUrl ?? string.Empty,
            mr.Description ?? string.Empty,
            mr.SourceBranch ?? string.Empty,
            mr.TargetBranch ?? string.Empty,
            mr.Draft,
            mr.HasConflicts,
            mr.DetailedMergeStatus ?? string.Empty,
            mr.Labels ?? Array.Empty<string>(),
            reviewers,
            assignees);
    }

    public async Task<IReadOnlyList<Note>> GetMergeRequestCommentsAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/notes";
        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var notes = await ReadJsonAsync<List<NoteDto>>(response, cancellationToken);
        return notes
            .Select(note => new Note(
                note.Author?.Name ?? string.Empty,
                note.Body ?? string.Empty,
                note.CreatedAt,
                note.System))
            .ToList();
    }

    public async Task AddMergeRequestCommentAsync(
        int projectId,
        int mergeRequestIid,
        string comment,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/notes";
        var response = await SendJsonAsync(HttpMethod.Post, url, new { body = comment }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task AddMergeRequestDiffCommentAsync(
        int projectId,
        int mergeRequestIid,
        string comment,
        string filePath,
        int lineNumber,
        string lineType,
        CancellationToken cancellationToken)
    {
        var diffRefs = await GetMergeRequestDiffRefsAsync(projectId, mergeRequestIid, cancellationToken);
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/discussions";
        var position = new
        {
            base_sha = diffRefs.BaseSha,
            start_sha = diffRefs.StartSha,
            head_sha = diffRefs.HeadSha,
            position_type = "text",
            new_path = filePath,
            old_path = filePath,
            new_line = lineType == "new" ? lineNumber : (int?)null,
            old_line = lineType == "old" ? lineNumber : (int?)null
        };
        var payload = new { body = comment, position };
        var response = await SendJsonAsync(HttpMethod.Post, url, payload, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<MergeRequestDiff> GetMergeRequestDiffAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/changes";
        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var changes = await ReadJsonAsync<MergeRequestChangesDto>(response, cancellationToken);
        var mappedChanges = (changes.Changes ?? new List<MergeRequestChangeDto>())
            .Select(change => new DiffChange(
                change.NewPath ?? string.Empty,
                change.OldPath ?? string.Empty,
                change.Diff ?? string.Empty,
                change.NewFile,
                change.DeletedFile,
                change.RenamedFile))
            .ToList();
        return new MergeRequestDiff(mergeRequestIid, mappedChanges);
    }

    public async Task<IssueDetails> GetIssueDetailsAsync(
        int projectId,
        int issueIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/issues/{issueIid}";
        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var issue = await ReadJsonAsync<IssueDto>(response, cancellationToken);
        return new IssueDetails(
            issue.Iid,
            issue.Title ?? string.Empty,
            issue.State ?? string.Empty,
            issue.Author?.Name ?? string.Empty,
            issue.CreatedAt,
            issue.UpdatedAt,
            issue.WebUrl ?? string.Empty,
            issue.Labels ?? Array.Empty<string>(),
            issue.Description ?? string.Empty);
    }

    public async Task SetMergeRequestTitleAsync(
        int projectId,
        int mergeRequestIid,
        string title,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}";
        var response = await SendJsonAsync(HttpMethod.Put, url, new { title }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetMergeRequestDescriptionAsync(
        int projectId,
        int mergeRequestIid,
        string description,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}";
        var response = await SendJsonAsync(HttpMethod.Put, url, new { description }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task ApproveMergeRequestAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/approve";
        var response = await SendAsync(HttpMethod.Post, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UnapproveMergeRequestAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/unapprove";
        var response = await SendAsync(HttpMethod.Post, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<MergeRequestDiffRefsDto> GetMergeRequestDiffRefsAsync(
        int projectId,
        int mergeRequestIid,
        CancellationToken cancellationToken)
    {
        var url = $"api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}";
        var response = await SendAsync(HttpMethod.Get, url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var mr = await ReadJsonAsync<MergeRequestWithDiffRefsDto>(response, cancellationToken);
        if (mr.DiffRefs == null ||
            string.IsNullOrWhiteSpace(mr.DiffRefs.BaseSha) ||
            string.IsNullOrWhiteSpace(mr.DiffRefs.StartSha) ||
            string.IsNullOrWhiteSpace(mr.DiffRefs.HeadSha))
        {
            throw new GitLabApiException("GitLab API did not provide diff refs for this merge request.");
        }

        return mr.DiffRefs;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, url);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendJsonAsync(
        HttpMethod method,
        string url,
        object payload,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, url, payload);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, object? payload = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        var token = ResolveToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", token);
        }

        return request;
    }

    private string? ResolveToken()
    {
        if (!string.IsNullOrWhiteSpace(_tokenContext.Token))
        {
            return _tokenContext.Token;
        }

        if (!string.IsNullOrWhiteSpace(_defaultToken))
        {
            return _defaultToken;
        }

        if (!_missingTokenLogged)
        {
            _missingTokenLogged = true;
            _logger.LogWarning("No GitLab token was provided. Configure GITLAB_TOKEN or send X-GitLab-Token/PRIVATE-TOKEN in the MCP request.");
        }

        return null;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase ?? "GitLab API request failed." : body;
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedException("GitLab API request unauthorized.");
        }

        throw new GitLabApiException(message, (int)response.StatusCode, body);
    }

    private async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
        if (payload is null)
        {
            throw new GitLabApiException("GitLab API returned an empty payload.");
        }

        return payload;
    }

    private sealed record ProjectDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("path_with_namespace")]
        public string? PathWithNamespace { get; init; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; init; }
    }

    private sealed record AuthorDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed record MergeRequestDto
    {
        [JsonPropertyName("iid")]
        public int Iid { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("state")]
        public string? State { get; init; }

        [JsonPropertyName("author")]
        public AuthorDto? Author { get; init; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        [JsonPropertyName("merged_at")]
        public DateTimeOffset? MergedAt { get; init; }

        [JsonPropertyName("web_url")]
        public string? WebUrl { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("source_branch")]
        public string? SourceBranch { get; init; }

        [JsonPropertyName("target_branch")]
        public string? TargetBranch { get; init; }

        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        [JsonPropertyName("has_conflicts")]
        public bool HasConflicts { get; init; }

        [JsonPropertyName("detailed_merge_status")]
        public string? DetailedMergeStatus { get; init; }

        [JsonPropertyName("labels")]
        public string[]? Labels { get; init; }

        [JsonPropertyName("reviewers")]
        public ReviewerDto[]? Reviewers { get; init; }

        [JsonPropertyName("assignees")]
        public AssigneeDto[]? Assignees { get; init; }
    }

    private sealed record ReviewerDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed record AssigneeDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed record NoteDto
    {
        [JsonPropertyName("author")]
        public AuthorDto? Author { get; init; }

        [JsonPropertyName("body")]
        public string? Body { get; init; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        [JsonPropertyName("system")]
        public bool System { get; init; }
    }

    private sealed record MergeRequestChangesDto
    {
        [JsonPropertyName("changes")]
        public List<MergeRequestChangeDto>? Changes { get; init; }
    }

    private sealed record MergeRequestChangeDto
    {
        [JsonPropertyName("old_path")]
        public string? OldPath { get; init; }

        [JsonPropertyName("new_path")]
        public string? NewPath { get; init; }

        [JsonPropertyName("diff")]
        public string? Diff { get; init; }

        [JsonPropertyName("new_file")]
        public bool NewFile { get; init; }

        [JsonPropertyName("deleted_file")]
        public bool DeletedFile { get; init; }

        [JsonPropertyName("renamed_file")]
        public bool RenamedFile { get; init; }
    }

    private sealed record MergeRequestWithDiffRefsDto
    {
        [JsonPropertyName("diff_refs")]
        public MergeRequestDiffRefsDto? DiffRefs { get; init; }
    }

    private sealed record MergeRequestDiffRefsDto
    {
        [JsonPropertyName("base_sha")]
        public string? BaseSha { get; init; }

        [JsonPropertyName("start_sha")]
        public string? StartSha { get; init; }

        [JsonPropertyName("head_sha")]
        public string? HeadSha { get; init; }
    }

    private sealed record IssueDto
    {
        [JsonPropertyName("iid")]
        public int Iid { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("state")]
        public string? State { get; init; }

        [JsonPropertyName("author")]
        public AuthorDto? Author { get; init; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        [JsonPropertyName("web_url")]
        public string? WebUrl { get; init; }

        [JsonPropertyName("labels")]
        public string[]? Labels { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }
    }
}
