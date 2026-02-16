using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Errors;
using GitLabMcp.IntegrationTests.TestInfrastructure;
using Xunit;

namespace GitLabMcp.IntegrationTests.MergeRequests;

/// <summary>
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.AddMergeRequestDiffCommentAsync"/>.
/// </summary>
public sealed class AddMergeRequestDiffCommentAsyncIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestDiffCommentAsync_ShouldSendNewLinePosition_WhenLineTypeIsNew</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task AddMergeRequestDiffCommentAsync_ShouldSendNewLinePosition_WhenLineTypeIsNew()
    {
        const string mrPath = "/api/v4/projects/5/merge_requests/44";
        const string discussionsPath = "/api/v4/projects/5/merge_requests/44/discussions";

        StubJson(HttpMethod.Get, mrPath, (int)HttpStatusCode.OK, ReadFixture("MergeRequests/get-merge-request-with-diff-refs-success.json"));
        StubJson(HttpMethod.Post, discussionsPath, (int)HttpStatusCode.OK, "{}");

        await Sut.AddMergeRequestDiffCommentAsync(5, 44, "Inline review", "src/new.cs", 30, "new", CancellationToken.None);

        var entries = GetLoggedRequests();
        Assert.Equal(2, entries.Count);

        Assert.False(string.IsNullOrWhiteSpace(entries[1].RequestMessage.Body));
        using var json = JsonDocument.Parse(entries[1].RequestMessage.Body!);
        var root = json.RootElement;
        var position = root.GetProperty("position");

        Assert.Equal("Inline review", root.GetProperty("body").GetString());
        Assert.Equal("base123", position.GetProperty("base_sha").GetString());
        Assert.Equal("start123", position.GetProperty("start_sha").GetString());
        Assert.Equal("head123", position.GetProperty("head_sha").GetString());
        Assert.Equal("text", position.GetProperty("position_type").GetString());
        Assert.Equal("src/new.cs", position.GetProperty("new_path").GetString());
        Assert.Equal("src/new.cs", position.GetProperty("old_path").GetString());
        Assert.Equal(30, position.GetProperty("new_line").GetInt32());
        Assert.Equal(JsonValueKind.Null, position.GetProperty("old_line").ValueKind);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestDiffCommentAsync_ShouldSendOldLinePosition_WhenLineTypeIsOld</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task AddMergeRequestDiffCommentAsync_ShouldSendOldLinePosition_WhenLineTypeIsOld()
    {
        const string mrPath = "/api/v4/projects/5/merge_requests/44";
        const string discussionsPath = "/api/v4/projects/5/merge_requests/44/discussions";

        StubJson(HttpMethod.Get, mrPath, (int)HttpStatusCode.OK, ReadFixture("MergeRequests/get-merge-request-with-diff-refs-success.json"));
        StubJson(HttpMethod.Post, discussionsPath, (int)HttpStatusCode.OK, "{}");

        await Sut.AddMergeRequestDiffCommentAsync(5, 44, "Inline review", "src/new.cs", 12, "old", CancellationToken.None);

        var entries = GetLoggedRequests();
        Assert.Equal(2, entries.Count);

        Assert.False(string.IsNullOrWhiteSpace(entries[1].RequestMessage.Body));
        using var json = JsonDocument.Parse(entries[1].RequestMessage.Body!);
        var position = json.RootElement.GetProperty("position");

        Assert.Equal(12, position.GetProperty("old_line").GetInt32());
        Assert.Equal(JsonValueKind.Null, position.GetProperty("new_line").ValueKind);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestDiffCommentAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task AddMergeRequestDiffCommentAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string mrPath = "/api/v4/projects/5/merge_requests/44";

        StubError(HttpMethod.Get, mrPath, (int)statusCode, ReadFixture(fixture));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Sut.AddMergeRequestDiffCommentAsync(5, 44, "Inline review", "src/new.cs", 30, "new", CancellationToken.None));
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestDiffCommentAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task AddMergeRequestDiffCommentAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string mrPath = "/api/v4/projects/5/merge_requests/44";
        var body = ReadFixture(fixture);

        StubError(HttpMethod.Get, mrPath, (int)statusCode, body);

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() =>
            Sut.AddMergeRequestDiffCommentAsync(5, 44, "Inline review", "src/new.cs", 30, "new", CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestDiffCommentAsync_ShouldThrowGitLabApiException_WhenDiffRefsAreMissing</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task AddMergeRequestDiffCommentAsync_ShouldThrowGitLabApiException_WhenDiffRefsAreMissing()
    {
        const string mrPath = "/api/v4/projects/5/merge_requests/44";

        StubJson(HttpMethod.Get, mrPath, (int)HttpStatusCode.OK, ReadFixture("MergeRequests/get-merge-request-without-diff-refs.json"));

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() =>
            Sut.AddMergeRequestDiffCommentAsync(5, 44, "Inline review", "src/new.cs", 30, "new", CancellationToken.None));

        Assert.Contains("did not provide diff refs", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(GetLoggedRequests());
        Assert.Equal(mrPath, GetLoggedRequests().Single().RequestMessage.Path);
    }
}
