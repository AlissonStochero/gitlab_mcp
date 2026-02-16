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
/// Testes unitários para <see cref="GetMergeRequestDetailsUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada
/// (projectId e mrIid).
/// </summary>
public class GetMergeRequestDetailsUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly GetMergeRequestDetailsUseCase _sut;

    public GetMergeRequestDetailsUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new GetMergeRequestDetailsUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId e mrIid, e retorna exatamente o objeto
    /// <see cref="MergeRequestDetails"/> devolvido pelo client.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallGetMergeRequestDetailsAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;
        var expectedMr = new MergeRequestDetails(
            mrIid, 
            "Title", 
            "opened", 
            "Author", 
            DateTimeOffset.UtcNow, 
            DateTimeOffset.UtcNow, 
            null, 
            "p://url", 
            "Description", 
            "source", 
            "target", 
            false, 
            false, 
            "can_be_merged", 
            Array.Empty<string>(), 
            Array.Empty<string>(), 
            Array.Empty<string>());

        _client.GetMergeRequestDetailsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expectedMr);

        // Act
        var result = await _sut.ExecuteAsync(projectId, mrIid);

        // Assert
        await _client.Received(1).GetMergeRequestDetailsAsync(projectId, mrIid, Arg.Any<CancellationToken>());
        Assert.Same(expectedMr, result);
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
        await _client.DidNotReceiveWithAnyArgs().GetMergeRequestDetailsAsync(default, default, default);
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
        await _client.DidNotReceiveWithAnyArgs().GetMergeRequestDetailsAsync(default, default, default);
    }
}
