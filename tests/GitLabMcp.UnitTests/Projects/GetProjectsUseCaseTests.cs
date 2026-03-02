using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.Projects;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.Projects;

/// <summary>
/// Testes unitários para <see cref="GetProjectsUseCase"/>.
/// Valida a delegação correta ao client e a normalização dos parâmetros
/// opcionais (search e visibility).
/// </summary>
public class GetProjectsUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly GetProjectsUseCase _sut;

    public GetProjectsUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new GetProjectsUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// search e visibility inalterados, e retorna exatamente a lista
    /// devolvida pelo client.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallGetProjectsAsync_WithCorrectParameters()
    {
        // Arrange
        var search = "test-project";
        var visibility = "public";
        var expectedProjects = new List<GitLabProject>();

        _client.GetProjectsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedProjects);

        // Act
        var result = await _sut.ExecuteAsync(search, visibility);

        // Assert
        await _client.Received(1).GetProjectsAsync(search, visibility, Arg.Any<CancellationToken>());
        Assert.Same(expectedProjects, result);
    }

    /// <summary>
    /// Verifica que, quando a visibility é nula, vazia ou composta apenas de espaços,
    /// o use case normaliza para "private" antes de delegar ao client.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldUseDefaultVisibility_WhenVisibilityIsMissing(string? visibility)
    {
        // Arrange
        var search = "test-project";
        var expectedProjects = new List<GitLabProject>();

        _client.GetProjectsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedProjects);

        // Act
        await _sut.ExecuteAsync(search, visibility);

        // Assert
        await _client.Received(1).GetProjectsAsync(search, "private", Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifica que, quando o search é nulo, ele é passado como null ao client
    /// sem normalização, permitindo que o client trate a ausência de filtro.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldPassSearchAsNull_WhenSearchIsMissing()
    {
        // Arrange
        string? search = null;
        var visibility = "internal";
        var expectedProjects = new List<GitLabProject>();

        _client.GetProjectsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedProjects);

        // Act
        await _sut.ExecuteAsync(search, visibility);

        // Assert
        await _client.Received(1).GetProjectsAsync(null, visibility, Arg.Any<CancellationToken>());
    }
}
