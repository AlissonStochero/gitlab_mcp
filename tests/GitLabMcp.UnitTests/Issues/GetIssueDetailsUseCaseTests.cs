using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.Issues;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Entities;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.Issues;

/// <summary>
/// Testes unitários para <see cref="GetIssueDetailsUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada.
/// </summary>
public class GetIssueDetailsUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly GetIssueDetailsUseCase _sut;

    public GetIssueDetailsUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new GetIssueDetailsUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// os parâmetros <paramref name="projectId"/> e <paramref name="issueIid"/>,
    /// e retorna exatamente o objeto devolvido pelo client.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallGetIssueDetailsAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var issueIid = 5;
        var expectedIssue = new IssueDetails(
            issueIid,
            "Title",
            "opened",
            "Author",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "http://url",
            Array.Empty<string>(),
            "Description");

        _client.GetIssueDetailsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expectedIssue);

        // Act
        var result = await _sut.ExecuteAsync(projectId, issueIid);

        // Assert
        await _client.Received(1).GetIssueDetailsAsync(projectId, issueIid, Arg.Any<CancellationToken>());
        Assert.Same(expectedIssue, result);
    }

    /// <summary>
    /// Garante que um <paramref name="projectId"/> inválido (≤ 0) resulta em
    /// <see cref="ValidationException"/> e que nenhuma chamada ao client é realizada.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenProjectIdIsInvalid(int projectId)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(projectId, 5));
        await _client.DidNotReceiveWithAnyArgs().GetIssueDetailsAsync(default, default, default);
    }

    /// <summary>
    /// Garante que um <paramref name="issueIid"/> inválido (≤ 0) resulta em
    /// <see cref="ValidationException"/> e que nenhuma chamada ao client é realizada.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenIssueIidIsInvalid(int issueIid)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExecuteAsync(10, issueIid));
        await _client.DidNotReceiveWithAnyArgs().GetIssueDetailsAsync(default, default, default);
    }
}
