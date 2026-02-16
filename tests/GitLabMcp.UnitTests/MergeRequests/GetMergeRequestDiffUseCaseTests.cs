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
/// Testes unitários para <see cref="GetMergeRequestDiffUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada
/// (projectId e mrIid).
/// </summary>
public class GetMergeRequestDiffUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly GetMergeRequestDiffUseCase _sut;

    public GetMergeRequestDiffUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new GetMergeRequestDiffUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId e mrIid, e retorna exatamente o objeto
    /// <see cref="MergeRequestDiff"/> devolvido pelo client.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallGetMergeRequestDiffAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var expectedDiff = new MergeRequestDiff(mrIid, new List<DiffChange>());

        _client.GetMergeRequestDiffAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expectedDiff);

        // Act
        var result = await _sut.ExecuteAsync(projectId, mrIid);

        // Assert
        await _client.Received(1).GetMergeRequestDiffAsync(projectId, mrIid, Arg.Any<CancellationToken>());
        Assert.Same(expectedDiff, result);
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(projectId, 5));
        await _client.DidNotReceiveWithAnyArgs().GetMergeRequestDiffAsync(default, default, default);
    }

    /// <summary>
    /// Garante que um mrIid inválido (≤ 0) resulta em
    /// <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenMrIidIsInvalid(int mrIid)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, mrIid));
        await _client.DidNotReceiveWithAnyArgs().GetMergeRequestDiffAsync(default, default, default);
    }
}
