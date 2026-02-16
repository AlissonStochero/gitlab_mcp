using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.MergeRequests;

/// <summary>
/// Testes unitários para <see cref="SetMergeRequestTitleUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada
/// (projectId, mrIid e title).
/// </summary>
public class SetMergeRequestTitleUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly SetMergeRequestTitleUseCase _sut;

    public SetMergeRequestTitleUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new SetMergeRequestTitleUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId, mrIid e o novo título sem alterações.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallSetMergeRequestTitleAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var title = "New Title";

        // Act
        await _sut.ExecuteAsync(projectId, mrIid, title);

        // Assert
        await _client.Received(1).SetMergeRequestTitleAsync(projectId, mrIid, title, Arg.Any<CancellationToken>());
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(projectId, 5, "title"));
        await _client.DidNotReceiveWithAnyArgs().SetMergeRequestTitleAsync(default, default, default!, default);
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, mrIid, "title"));
        await _client.DidNotReceiveWithAnyArgs().SetMergeRequestTitleAsync(default, default, default!, default);
    }

    /// <summary>
    /// Garante que um título nulo, vazio ou composto apenas de espaços
    /// resulta em <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenTitleIsInvalid(string? title)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, 5, title!));
        await _client.DidNotReceiveWithAnyArgs().SetMergeRequestTitleAsync(default, default, default!, default);
    }
}
