using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.MergeRequests;

/// <summary>
/// Testes unitários para <see cref="AddMergeRequestDiffCommentUseCase"/>.
/// Valida normalização de lineType, delegação ao client e as validações de entrada
/// (projectId, mrIid, comment, filePath, lineNumber e lineType).
/// </summary>
public class AddMergeRequestDiffCommentUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly AddMergeRequestDiffCommentUseCase _sut;

    public AddMergeRequestDiffCommentUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new AddMergeRequestDiffCommentUseCase(_client);
    }

    /// <summary>
    /// Verifica que o lineType é normalizado antes de ser enviado ao client:
    /// valores válidos são convertidos para lowercase, e valores nulos/whitespace
    /// assumem o padrão "new".
    /// </summary>
    [Theory]
    [InlineData("new", "new")]
    [InlineData("OLD", "old")]
    [InlineData(null, "new")]
    [InlineData("   ", "new")]
    public async Task ExecuteAsync_ShouldCallAddMergeRequestDiffCommentAsync_WithNormalizedLineType(string? inputLineType, string expectedLineType)
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var comment = "Diff comment";
        var filePath = "src/file.cs";
        var lineNumber = 15;

        // Act
        await _sut.ExecuteAsync(projectId, mrIid, comment, filePath, lineNumber, inputLineType);

        // Assert
        await _client.Received(1).AddMergeRequestDiffCommentAsync(
            projectId, mrIid, comment, filePath, lineNumber, expectedLineType, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Garante que um lineType inválido (diferente de "new" ou "old") resulta em
    /// <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData("invalid")]
    [InlineData("newer")]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenLineTypeIsInvalid(string lineType)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ExecuteAsync(10, 5, "comment", "file.cs", 15, lineType));
        await _client.DidNotReceiveWithAnyArgs()
            .AddMergeRequestDiffCommentAsync(default, default, default!, default!, default, default!, default);
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
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ExecuteAsync(projectId, 5, "comment", "file.cs", 15, "new"));
        await _client.DidNotReceiveWithAnyArgs()
            .AddMergeRequestDiffCommentAsync(default, default, default!, default!, default, default!, default);
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
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ExecuteAsync(10, mrIid, "comment", "file.cs", 15, "new"));
        await _client.DidNotReceiveWithAnyArgs()
            .AddMergeRequestDiffCommentAsync(default, default, default!, default!, default, default!, default);
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
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ExecuteAsync(10, 5, comment!, "file.cs", 15, "new"));
        await _client.DidNotReceiveWithAnyArgs()
            .AddMergeRequestDiffCommentAsync(default, default, default!, default!, default, default!, default);
    }

    /// <summary>
    /// Garante que um filePath nulo, vazio ou composto apenas de espaços
    /// resulta em <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenFilePathIsInvalid(string? filePath)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ExecuteAsync(10, 5, "comment", filePath!, 15, "new"));
        await _client.DidNotReceiveWithAnyArgs()
            .AddMergeRequestDiffCommentAsync(default, default, default!, default!, default, default!, default);
    }

    /// <summary>
    /// Garante que um lineNumber inválido (≤ 0) resulta em
    /// <see cref="ValidationException"/> e que o client não é chamado.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenLineNumberIsInvalid(int lineNumber)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ExecuteAsync(10, 5, "comment", "file.cs", lineNumber, "new"));
        await _client.DidNotReceiveWithAnyArgs()
            .AddMergeRequestDiffCommentAsync(default, default, default!, default!, default, default!, default);
    }
}
