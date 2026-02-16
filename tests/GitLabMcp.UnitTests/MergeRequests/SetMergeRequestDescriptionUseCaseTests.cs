using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.MergeRequests;

/// <summary>
/// Testes unitários para <see cref="SetMergeRequestDescriptionUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada
/// (projectId, mrIid e description).
/// </summary>
public class SetMergeRequestDescriptionUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly SetMergeRequestDescriptionUseCase _sut;

    public SetMergeRequestDescriptionUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new SetMergeRequestDescriptionUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId, mrIid e a nova descrição sem alterações.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallSetMergeRequestDescriptionAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var description = "New Description";

        // Act
        await _sut.ExecuteAsync(projectId, mrIid, description);

        // Assert
        await _client.Received(1).SetMergeRequestDescriptionAsync(projectId, mrIid, description, Arg.Any<CancellationToken>());
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(projectId, 5, "description"));
        await _client.DidNotReceiveWithAnyArgs().SetMergeRequestDescriptionAsync(default, default, default!, default);
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, mrIid, "description"));
        await _client.DidNotReceiveWithAnyArgs().SetMergeRequestDescriptionAsync(default, default, default!, default);
    }

    /// <summary>
    /// Garante que uma descrição nula, vazia ou composta apenas de espaços
    /// resulta em <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenDescriptionIsInvalid(string? description)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, 5, description!));
        await _client.DidNotReceiveWithAnyArgs().SetMergeRequestDescriptionAsync(default, default, default!, default);
    }
}
