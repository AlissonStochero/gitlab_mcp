using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Domain.Errors;
using GitLabMcp.IntegrationTests.TestInfrastructure;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace GitLabMcp.IntegrationTests.Projects;

/// <summary>
/// Testes de integração para <see cref="GitLabMcp.Infrastructure.Http.GitLabApiClient.GetProjectsAsync"/>.
/// </summary>
public sealed class GetProjectsAsyncIntegrationTests : IntegrationTestBase
{
    private const string Path = "/api/v4/projects";

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldMapProjects_WhenResponseIsSuccess</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldMapProjects_WhenResponseIsSuccess()
    {
        StubJson(
            HttpMethod.Get,
            Path,
            (int)HttpStatusCode.OK,
            ReadFixture("Projects/get-projects-success.json"),
            new System.Collections.Generic.Dictionary<string, string>
            {
                ["visibility"] = "private",
                ["membership"] = "true",
                ["per_page"] = "100",
                ["search"] = "finance"
            });

        var result = await Sut.GetProjectsAsync("finance", "private", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(101, result[0].Id);
        Assert.Equal("Project Alpha", result[0].Name);
        Assert.Equal("group/project-alpha", result[0].PathWithNamespace);
        Assert.Equal("private", result[0].Visibility);

        var request = GetLoggedRequests().Single().RequestMessage;
        Assert.Equal("/api/v4/projects", request.Path);
        Assert.Contains("search=finance", request.RawQuery);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldNotSendSearchParam_WhenSearchIsNull</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldNotSendSearchParam_WhenSearchIsNull()
    {
        StubJson(
            HttpMethod.Get,
            Path,
            (int)HttpStatusCode.OK,
            ReadFixture("Projects/get-projects-success.json"),
            new System.Collections.Generic.Dictionary<string, string>
            {
                ["visibility"] = "private",
                ["membership"] = "true",
                ["per_page"] = "100"
            });

        await Sut.GetProjectsAsync(null, "private", CancellationToken.None);

        var request = GetLoggedRequests().Single().RequestMessage;
        Assert.DoesNotContain("search=", request.RawQuery, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Common/error-401.json")]
    [InlineData(HttpStatusCode.Forbidden, "Common/error-403.json")]
    public async Task GetProjectsAsync_ShouldThrowUnauthorizedException_WhenStatusIsUnauthorized(
        HttpStatusCode statusCode,
        string fixture)
    {
        StubError(
            HttpMethod.Get,
            Path,
            (int)statusCode,
            ReadFixture(fixture),
            new System.Collections.Generic.Dictionary<string, string>
            {
                ["visibility"] = "private",
                ["membership"] = "true",
                ["per_page"] = "100"
            });

        var action = async () => await Sut.GetProjectsAsync(null, "private", CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(action);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Common/error-404.json")]
    [InlineData(HttpStatusCode.InternalServerError, "Common/error-500.json")]
    public async Task GetProjectsAsync_ShouldThrowGitLabApiException_WhenStatusIsFailure(
        HttpStatusCode statusCode,
        string fixture)
    {
        var body = ReadFixture(fixture);
        StubError(
            HttpMethod.Get,
            Path,
            (int)statusCode,
            body,
            new System.Collections.Generic.Dictionary<string, string>
            {
                ["visibility"] = "private",
                ["membership"] = "true",
                ["per_page"] = "100"
            });

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetProjectsAsync(null, "private", CancellationToken.None));

        Assert.Equal((int)statusCode, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldThrowGitLabApiException_WhenPayloadIsNull()
    {
        StubJson(
            HttpMethod.Get,
            Path,
            (int)HttpStatusCode.OK,
            ReadFixture("Common/payload-null.json"),
            new System.Collections.Generic.Dictionary<string, string>
            {
                ["visibility"] = "private",
                ["membership"] = "true",
                ["per_page"] = "100"
            });

        var ex = await Assert.ThrowsAsync<GitLabApiException>(() => Sut.GetProjectsAsync(null, "private", CancellationToken.None));

        Assert.Contains("empty payload", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldUseContextToken_WhenContextTokenIsPresent</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldUseContextToken_WhenContextTokenIsPresent()
    {
        TokenContext.Token = "context-token";

        Server
            .Given(Request.Create()
                .WithPath(Path)
                .UsingGet()
                .WithParam("visibility", "private")
                .WithParam("membership", "true")
                .WithParam("per_page", "100")
                .WithHeader(headers =>
                    headers.TryGetValue("PRIVATE-TOKEN", out var values) &&
                    values.Contains("context-token", StringComparer.Ordinal)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ReadFixture("Projects/get-projects-success.json")));

        var result = await Sut.GetProjectsAsync(null, "private", CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldUseOptionsToken_WhenContextTokenIsMissing</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldUseOptionsToken_WhenContextTokenIsMissing()
    {
        RecreateSut("options-token");

        Server
            .Given(Request.Create()
                .WithPath(Path)
                .UsingGet()
                .WithParam("visibility", "private")
                .WithParam("membership", "true")
                .WithParam("per_page", "100")
                .WithHeader(headers =>
                    headers.TryGetValue("PRIVATE-TOKEN", out var values) &&
                    values.Contains("options-token", StringComparer.Ordinal)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ReadFixture("Projects/get-projects-success.json")));

        var result = await Sut.GetProjectsAsync(null, "private", CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldPrioritizeContextToken_WhenBothTokensArePresent</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldPrioritizeContextToken_WhenBothTokensArePresent()
    {
        RecreateSut("options-token");
        TokenContext.Token = "context-token";

        Server
            .Given(Request.Create()
                .WithPath(Path)
                .UsingGet()
                .WithParam("visibility", "private")
                .WithParam("membership", "true")
                .WithParam("per_page", "100")
                .WithHeader(headers =>
                    headers.TryGetValue("PRIVATE-TOKEN", out var values) &&
                    values.Contains("context-token", StringComparer.Ordinal) &&
                    !values.Contains("options-token", StringComparer.Ordinal)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ReadFixture("Projects/get-projects-success.json")));

        var result = await Sut.GetProjectsAsync(null, "private", CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Descreve o cenário validado pelo teste de integração <c>GetProjectsAsync_ShouldNotSendTokenHeader_WhenNoTokenIsConfigured</c>.
    /// O teste executa o fluxo HTTP real contra WireMock.Net e verifica o comportamento esperado do client.
    /// Inclui validação de contrato da requisição, tratamento de resposta e assertivas de resultado/exceção.
    /// </summary>
    [Fact]
    public async Task GetProjectsAsync_ShouldNotSendTokenHeader_WhenNoTokenIsConfigured()
    {
        RecreateSut(string.Empty);

        Server
            .Given(Request.Create()
                .WithPath(Path)
                .UsingGet()
                .WithParam("visibility", "private")
                .WithParam("membership", "true")
                .WithParam("per_page", "100")
                .WithHeader(headers => headers.ContainsKey("PRIVATE-TOKEN")))
            .RespondWith(Response.Create().WithStatusCode(500).WithBody("{\"message\":\"token should not be present\"}"));

        Server
            .Given(Request.Create()
                .WithPath(Path)
                .UsingGet()
                .WithParam("visibility", "private")
                .WithParam("membership", "true")
                .WithParam("per_page", "100")
                .WithHeader(headers => !headers.ContainsKey("PRIVATE-TOKEN")))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ReadFixture("Projects/get-projects-success.json")));

        var result = await Sut.GetProjectsAsync(null, "private", CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
