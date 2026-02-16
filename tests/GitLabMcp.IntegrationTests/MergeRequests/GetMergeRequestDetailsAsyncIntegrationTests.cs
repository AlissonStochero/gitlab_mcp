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
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.GetMergeRequestDetailsAsync"/>.
/// </summary>
public sealed class GetMergeRequestDetailsAsyncIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDetailsAsync_ShouldMapDetails_WhenResponseIsSuccess</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetMergeRequestDetailsAsync_ShouldMapDetails_WhenResponseIsSuccess()
    {
        const string path = "/api/v4/projects/5/merge_requests/44";

        StubJson(HttpMethod.Get, path, (int)HttpStatusCode.OK, ReadFixture("MergeRequests/get-merge-request-details-success.json"));

        var result = await Sut.GetMergeRequestDetailsAsync(5, 44, CancellationToken.None);

        Assert.Equal(44, result.Iid);
        Assert.Equal("Add totalizer", result.Title);
        Assert.Equal("opened", result.State);
        Assert.Equal("Carol", result.AuthorName);
        Assert.Equal("feature/totalizer", result.SourceBranch);
        Assert.Equal("main", result.TargetBranch);
        Assert.True(result.HasConflicts);
        Assert.Equal("checking", result.DetailedMergeStatus);
        Assert.Equal(2, result.Labels.Count);
        Assert.Equal(2, result.Reviewers.Count);
        Assert.Single(result.Assignees);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDetailsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task GetMergeRequestDetailsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests/44";

        StubError(HttpMethod.Get, path, (int)statusCode, ReadFixture(fixture));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Sut.GetMergeRequestDetailsAsync(5, 44, CancellationToken.None));
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDetailsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task GetMergeRequestDetailsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests/44";
        var body = ReadFixture(fixture);

        StubError(HttpMethod.Get, path, (int)statusCode, body);

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetMergeRequestDetailsAsync(5, 44, CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetMergeRequestDetailsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetMergeRequestDetailsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull()
    {
        const string path = "/api/v4/projects/5/merge_requests/44";

        StubJson(HttpMethod.Get, path, (int)HttpStatusCode.OK, ReadFixture("Common/payload-null.json"));

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetMergeRequestDetailsAsync(5, 44, CancellationToken.None));

        Assert.Contains("empty payload", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
