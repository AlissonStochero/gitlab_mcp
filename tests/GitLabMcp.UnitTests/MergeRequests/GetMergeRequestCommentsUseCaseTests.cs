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
/// Testes unitários para <see cref="GetMergeRequestCommentsUseCase"/>.
/// Valida a filtragem de notas do sistema, edge cases de lista vazia
/// e as validações de entrada (projectId e mrIid).
/// </summary>
public class GetMergeRequestCommentsUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly GetMergeRequestCommentsUseCase _sut;

    public GetMergeRequestCommentsUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new GetMergeRequestCommentsUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case filtra notas do sistema (System = true)
    /// e retorna apenas notas de usuários reais, validando tanto a
    /// quantidade quanto o conteúdo dos registros retornados.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnOnlyNonSystemNotes()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var notes = new List<Note>
        {
            new Note("User 1", "User comment", DateTimeOffset.UtcNow, false),
            new Note("System", "System message", DateTimeOffset.UtcNow, true),
            new Note("User 2", "Another user comment", DateTimeOffset.UtcNow, false)
        };

        _client.GetMergeRequestCommentsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(notes);

        // Act
        var result = await _sut.ExecuteAsync(projectId, mrIid);

        // Assert
        await _client.Received(1).GetMergeRequestCommentsAsync(projectId, mrIid, Arg.Any<CancellationToken>());
        Assert.Equal(2, result.Count);
        Assert.All(result, note => Assert.False(note.System));
        Assert.Contains(result, n => n.AuthorName == "User 1");
        Assert.Contains(result, n => n.AuthorName == "User 2");
    }

    /// <summary>
    /// Verifica que, quando todas as notas retornadas pelo client são notas
    /// do sistema, o resultado é uma lista vazia.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenAllNotesAreSystem()
    {
        // Arrange
        var notes = new List<Note>
        {
            new Note("System", "System message 1", DateTimeOffset.UtcNow, true),
            new Note("System", "System message 2", DateTimeOffset.UtcNow, true)
        };

        _client.GetMergeRequestCommentsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(notes);

        // Act
        var result = await _sut.ExecuteAsync(10, 5);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifica que, quando o client retorna uma lista vazia de notas,
    /// o resultado também é uma lista vazia (sem exceção).
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenClientReturnsEmptyList()
    {
        // Arrange
        _client.GetMergeRequestCommentsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Note>());

        // Act
        var result = await _sut.ExecuteAsync(10, 5);

        // Assert
        Assert.Empty(result);
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
        await _client.DidNotReceiveWithAnyArgs().GetMergeRequestCommentsAsync(default, default, default);
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
        await _client.DidNotReceiveWithAnyArgs().GetMergeRequestCommentsAsync(default, default, default);
    }
}
