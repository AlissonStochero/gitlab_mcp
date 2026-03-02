using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Errors;
using GitLabMcp.IntegrationTests.TestInfrastructure;
using Xunit;

namespace GitLabMcp.IntegrationTests.MergeRequests;

/// <summary>
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.GetMergeRequestDiffAsync"/>.
/// </summary>
public sealed class GetMergeRequestDiffAsyncIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDiffAsync_ShouldMapChanges_WhenResponseIsSuccess</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetMergeRequestDiffAsync_ShouldMapChanges_WhenResponseIsSuccess()
    {
        const string path = "/api/v4/projects/5/merge_requests/44/changes";

        StubJson(HttpMethod.Get, path, (int)HttpStatusCode.OK, ReadFixture("MergeRequests/get-merge-request-diff-success.json"));

        var result = await Sut.GetMergeRequestDiffAsync(5, 44, CancellationToken.None);

        Assert.Equal(44, result.Iid);
        Assert.Equal(2, result.Changes.Count);
        Assert.Equal("src/new.cs", result.Changes[0].NewPath);
        Assert.Equal("src/old.cs", result.Changes[0].OldPath);
        Assert.Contains("+new", result.Changes[0].Diff);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDiffAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task GetMergeRequestDiffAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests/44/changes";

        StubError(HttpMethod.Get, path, (int)statusCode, ReadFixture(fixture));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Sut.GetMergeRequestDiffAsync(5, 44, CancellationToken.None));
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDiffAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task GetMergeRequestDiffAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests/44/changes";
        var body = ReadFixture(fixture);

        StubError(HttpMethod.Get, path, (int)statusCode, body);

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetMergeRequestDiffAsync(5, 44, CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDiffAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetMergeRequestDiffAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull()
    {
        const string path = "/api/v4/projects/5/merge_requests/44/changes";

        StubJson(HttpMethod.Get, path, (int)HttpStatusCode.OK, ReadFixture("Common/payload-null.json"));

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetMergeRequestDiffAsync(5, 44, CancellationToken.None));

        Assert.Contains("empty payload", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
