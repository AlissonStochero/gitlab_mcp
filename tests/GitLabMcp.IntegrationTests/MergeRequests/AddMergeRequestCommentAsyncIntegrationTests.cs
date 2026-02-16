using System;
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
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.AddMergeRequestCommentAsync"/>.
/// </summary>
public sealed class AddMergeRequestCommentAsyncIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestCommentAsync_ShouldPostCommentBody_WhenResponseIsSuccess</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task AddMergeRequestCommentAsync_ShouldPostCommentBody_WhenResponseIsSuccess()
    {
        const string path = "/api/v4/projects/5/merge_requests/44/notes";

        StubJson(HttpMethod.Post, path, (int)HttpStatusCode.OK, "{}", null);

        await Sut.AddMergeRequestCommentAsync(5, 44, "review comment", CancellationToken.None);

        var request = GetLoggedRequests().Single().RequestMessage;
        Assert.False(string.IsNullOrWhiteSpace(request.Body));
        using var json = JsonDocument.Parse(request.Body!);
        Assert.Equal("review comment", json.RootElement.GetProperty("body").GetString());
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestCommentAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task AddMergeRequestCommentAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests/44/notes";

        StubError(HttpMethod.Post, path, (int)statusCode, ReadFixture(fixture));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Sut.AddMergeRequestCommentAsync(5, 44, "review comment", CancellationToken.None));
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>AddMergeRequestCommentAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task AddMergeRequestCommentAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        const string path = "/api/v4/projects/5/merge_requests/44/notes";
        var body = ReadFixture(fixture);

        StubError(HttpMethod.Post, path, (int)statusCode, body);

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.AddMergeRequestCommentAsync(5, 44, "review comment", CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }
}
