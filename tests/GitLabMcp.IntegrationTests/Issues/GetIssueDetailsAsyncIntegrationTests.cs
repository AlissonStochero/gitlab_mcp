using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Errors;
using GitLabMcp.IntegrationTests.TestInfrastructure;
using Xunit;

namespace GitLabMcp.IntegrationTests.Issues;

/// <summary>
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.GetIssueDetailsAsync"/>.
/// </summary>
public sealed class GetIssueDetailsAsyncIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetIssueDetailsAsync_ShouldMapIssue_WhenResponseIsSuccess</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetIssueDetailsAsync_ShouldMapIssue_WhenResponseIsSuccess()
    {
        const string path = "/api/v4/projects/5/issues/77";

        StubJson(HttpMethod.Get, path, (int)HttpStatusCode.OK, ReadFixture("Issues/get-issue-details-success.json"));

        var result = await Sut.GetIssueDetailsAsync(5, 77, CancellationToken.None);

        Assert.Equal(77, result.Iid);
        Assert.Equal("Issue title", result.Title);
        Assert.Equal("opened", result.State);
        Assert.Equal("Eve", result.AuthorName);
        Assert.Equal("Issue description", result.Description);
        Assert.Equal(2, result.Labels.Count);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetIssueDetailsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task GetIssueDetailsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/issues/77";

        StubError(HttpMethod.Get, path, (int)statusCode, ReadFixture(fixture));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Sut.GetIssueDetailsAsync(5, 77, CancellationToken.None));
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetIssueDetailsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task GetIssueDetailsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/issues/77";
        var body = ReadFixture(fixture);

        StubError(HttpMethod.Get, path, (int)statusCode, body);

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetIssueDetailsAsync(5, 77, CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetIssueDetailsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetIssueDetailsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull()
    {
        const string path = "/api/v4/projects/5/issues/77";

        StubJson(HttpMethod.Get, path, (int)HttpStatusCode.OK, ReadFixture("Common/payload-null.json"));

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetIssueDetailsAsync(5, 77, CancellationToken.None));

        Assert.Contains("empty payload", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
