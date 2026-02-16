using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using GitLabMcp.Infrastructure.Configuration;
using GitLabMcp.Infrastructure.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace GitLabMcp.IntegrationTests.TestInfrastructure;

/// <summary>
/// Classe base para testes de integração do <see cref="GitLabApiClient"/> usando WireMock.Net.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private readonly string _fixturesRoot;
    private bool _disposed;
    private string _optionsToken;

    protected IntegrationTestBase(string optionsToken = "options-token")
    {
        _optionsToken = optionsToken;
        Server = WireMockServer.Start(new WireMockServerSettings
        {
            StartAdminInterface = false,
            ReadStaticMappings = false
        });

        _fixturesRoot = Path.Combine(AppContext.BaseDirectory, "Fixtures");
        Logger = Substitute.For<ILogger<GitLabApiClient>>();
        TokenContext = new GitLabTokenContext { Token = null };
        HttpClient = CreateHttpClient();
        Sut = CreateSut();
    }

    /// <summary>
    /// Instância do WireMock utilizada pelos testes.
    /// </summary>
    protected WireMockServer Server { get; }

    /// <summary>
    /// Instância do client sob teste.
    /// </summary>
    protected GitLabApiClient Sut { get; private set; }

    /// <summary>
    /// Contexto do token de autenticação do GitLab.
    /// </summary>
    protected GitLabTokenContext TokenContext { get; }

    /// <summary>
    /// Logger mockado para o client.
    /// </summary>
    protected ILogger<GitLabApiClient> Logger { get; }

    /// <summary>
    /// HttpClient direcionado para o servidor WireMock local.
    /// </summary>
    protected HttpClient HttpClient { get; private set; }

    /// <summary>
    /// Recria o SUT com um novo token padrão de options.
    /// </summary>
    /// <param name="optionsToken">Token padrão das options.</param>
    protected void RecreateSut(string optionsToken)
    {
        _optionsToken = optionsToken;
        HttpClient.Dispose();
        HttpClient = CreateHttpClient();
        Sut = CreateSut();
    }

    /// <summary>
    /// Lê o conteúdo de um fixture JSON relativo à pasta Fixtures.
    /// </summary>
    /// <param name="relativePath">Caminho relativo do fixture.</param>
    /// <returns>Conteúdo do arquivo.</returns>
    protected string ReadFixture(string relativePath)
    {
        var fullPath = Path.Combine(_fixturesRoot, relativePath);
        return File.ReadAllText(fullPath, Encoding.UTF8);
    }

    /// <summary>
    /// Registra um stub JSON no WireMock.
    /// </summary>
    /// <param name="method">Método HTTP.</param>
    /// <param name="path">Caminho absoluto (sem host).</param>
    /// <param name="statusCode">Status de resposta.</param>
    /// <param name="jsonBody">Payload JSON.</param>
    /// <param name="queryParams">Parâmetros de query exatos.</param>
    protected void StubJson(
        HttpMethod method,
        string path,
        int statusCode,
        string jsonBody,
        IReadOnlyDictionary<string, string>? queryParams = null)
    {
        var request = Request.Create().WithPath(path).UsingMethod(method.Method);
        if (queryParams is not null)
        {
            foreach (var (key, value) in queryParams)
            {
                request = request.WithParam(key, value);
            }
        }

        Server
            .Given(request)
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/json")
                .WithBody(jsonBody));
    }

    /// <summary>
    /// Registra um stub de erro no WireMock.
    /// </summary>
    /// <param name="method">Método HTTP.</param>
    /// <param name="path">Caminho absoluto (sem host).</param>
    /// <param name="statusCode">Status de resposta.</param>
    /// <param name="errorBody">Body de erro.</param>
    /// <param name="queryParams">Parâmetros de query exatos.</param>
    protected void StubError(
        HttpMethod method,
        string path,
        int statusCode,
        string errorBody,
        IReadOnlyDictionary<string, string>? queryParams = null)
    {
        StubJson(method, path, statusCode, errorBody, queryParams);
    }

    /// <summary>
    /// Retorna as requisições registradas no WireMock.
    /// </summary>
    /// <returns>Lista de entradas de log.</returns>
    protected IReadOnlyList<ILogEntry> GetLoggedRequests()
    {
        return Server.LogEntries.ToList();
    }

    /// <summary>
    /// Reseta mappings e logs do servidor WireMock.
    /// </summary>
    protected void ResetState()
    {
        Server.Reset();
        TokenContext.Token = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        TokenContext.Token = null;
        HttpClient.Dispose();
        Server.Stop();
        Server.Dispose();
    }

    private HttpClient CreateHttpClient()
    {
        var baseUrl = Server.Url ?? throw new InvalidOperationException("WireMock server URL is not available.");
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        return new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    private GitLabApiClient CreateSut()
    {
        var serverUrl = Server.Url ?? throw new InvalidOperationException("WireMock server URL is not available.");
        var options = Options.Create(new GitLabClientOptions
        {
            BaseUrl = serverUrl,
            Token = _optionsToken
        });

        return new GitLabApiClient(HttpClient, options, TokenContext, Logger);
    }
}
