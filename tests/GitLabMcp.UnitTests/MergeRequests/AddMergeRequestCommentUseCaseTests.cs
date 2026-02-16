using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.MergeRequests;

/// <summary>
/// Testes unitários para <see cref="AddMergeRequestCommentUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada
/// (projectId, mrIid e comment).
/// </summary>
public class AddMergeRequestCommentUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly AddMergeRequestCommentUseCase _sut;

    public AddMergeRequestCommentUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new AddMergeRequestCommentUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId, mrIid e o texto do comentário sem alterações.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallAddMergeRequestCommentAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var comment = "LGTM!";

        // Act
        await _sut.ExecuteAsync(projectId, mrIid, comment);

        // Assert
        await _client.Received(1).AddMergeRequestCommentAsync(projectId, mrIid, comment, Arg.Any<CancellationToken>());
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(projectId, 5, "comment"));
        await _client.DidNotReceiveWithAnyArgs().AddMergeRequestCommentAsync(default, default, default!, default);
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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, mrIid, "comment"));
        await _client.DidNotReceiveWithAnyArgs().AddMergeRequestCommentAsync(default, default, default!, default);
    }

    /// <summary>
    /// Garante que um comentário nulo, vazio ou composto apenas de espaços
    /// resulta em <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenCommentIsInvalid(string? comment)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, 5, comment!));
        await _client.DidNotReceiveWithAnyArgs().AddMergeRequestCommentAsync(default, default, default!, default);
    }
}
