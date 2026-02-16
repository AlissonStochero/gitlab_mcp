using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.MergeRequests;

/// <summary>
/// Testes unitários para <see cref="ListOpenMergeRequestsUseCase"/>.
/// Valida a delegação correta ao client, a normalização do state
/// para "opened" quando ausente, e as validações de entrada (projectId).
/// </summary>
public class ListOpenMergeRequestsUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly ListOpenMergeRequestsUseCase _sut;

    public ListOpenMergeRequestsUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new ListOpenMergeRequestsUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId e state inalterados, e retorna exatamente a lista
    /// devolvida pelo client.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallListMergeRequestsAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var state = "closed";
        var expectedMrs = new List<MergeRequestSummary>();

        _client.ListMergeRequestsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedMrs);

        // Act
        var result = await _sut.ExecuteAsync(projectId, state);

        // Assert
        await _client.Received(1).ListMergeRequestsAsync(projectId, state, Arg.Any<CancellationToken>());
        Assert.Same(expectedMrs, result);
    }

    /// <summary>
    /// Verifica que, quando o state é nulo, vazio ou composto apenas de espaços,
    /// o use case normaliza para "opened" antes de delegar ao client.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldUseDefaultState_WhenStateIsMissing(string? state)
    {
        // Arrange
        var projectId = 10;
        var expectedMrs = new List<MergeRequestSummary>();

        _client.ListMergeRequestsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedMrs);

        // Act
        await _sut.ExecuteAsync(projectId, state);

        // Assert
        await _client.Received(1).ListMergeRequestsAsync(projectId, "opened", Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Garante que um projectId inválido (≤ 0) resulta em
    /// <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenProjectIdIsInvalid(int projectId)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(projectId, "opened"));
        await _client.DidNotReceiveWithAnyArgs().ListMergeRequestsAsync(default, default!, default);
    }
}
