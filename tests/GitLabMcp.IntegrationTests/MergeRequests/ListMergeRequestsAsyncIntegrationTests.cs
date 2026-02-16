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
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.ListMergeRequestsAsync"/>.
/// </summary>
public sealed class ListMergeRequestsAsyncIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>ListMergeRequestsAsync_ShouldMapMergeRequests_WhenResponseIsSuccess</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task ListMergeRequestsAsync_ShouldMapMergeRequests_WhenResponseIsSuccess()
    {
        const int projectId = 5;
        const string path = "/api/v4/projects/5/merge_requests";

        StubJson(
            HttpMethod.Get,
            path,
            (int)HttpStatusCode.OK,
            ReadFixture("MergeRequests/list-merge-requests-success.json"),
            new System.Collections.Generic.Dictionary<string, string> { ["state"] = "opened" });

        var result = await Sut.ListMergeRequestsAsync(projectId, "opened", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(11, result[0].Iid);
        Assert.Equal("Implement endpoint", result[0].Title);
        Assert.Equal("opened", result[0].State);
        Assert.Equal("Alice", result[0].AuthorName);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>ListMergeRequestsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task ListMergeRequestsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests";

        StubError(
            HttpMethod.Get,
            path,
            (int)statusCode,
            ReadFixture(fixture),
            new System.Collections.Generic.Dictionary<string, string> { ["state"] = "opened" });

        await Assert.ThrowsAsync<UnauthorizedException>(() => Sut.ListMergeRequestsAsync(5, "opened", CancellationToken.None));
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>ListMergeRequestsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task ListMergeRequestsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests";
        var body = ReadFixture(fixture);

        StubError(
            HttpMethod.Get,
            path,
            (int)statusCode,
            body,
            new System.Collections.Generic.Dictionary<string, string> { ["state"] = "opened" });

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.ListMergeRequestsAsync(5, "opened", CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>ListMergeRequestsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task ListMergeRequestsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull()
    {
        const string path = "/api/v4/projects/5/merge_requests";

        StubJson(
            HttpMethod.Get,
            path,
            (int)HttpStatusCode.OK,
            ReadFixture("Common/payload-null.json"),
            new System.Collections.Generic.Dictionary<string, string> { ["state"] = "opened" });

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.ListMergeRequestsAsync(5, "opened", CancellationToken.None));

        Assert.Contains("empty payload", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
