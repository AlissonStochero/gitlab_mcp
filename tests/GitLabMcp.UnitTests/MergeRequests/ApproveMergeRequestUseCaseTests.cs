using System.Threading;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Domain.Errors;
using NSubstitute;
using Xunit;

namespace GitLabMcp.UnitTests.MergeRequests;

/// <summary>
/// Testes unitários para <see cref="ApproveMergeRequestUseCase"/>.
/// Valida a delegação correta ao client e as validações de entrada
/// (projectId e mrIid).
/// </summary>
public class ApproveMergeRequestUseCaseTests
{
    private readonly IGitLabApiClient _client;
    private readonly ApproveMergeRequestUseCase _sut;

    public ApproveMergeRequestUseCaseTests()
    {
        _client = Substitute.For<IGitLabApiClient>();
        _sut = new ApproveMergeRequestUseCase(_client);
    }

    /// <summary>
    /// Verifica que o use case delega corretamente ao client passando
    /// projectId e mrIid para aprovar o merge request.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallApproveMergeRequestAsync_WithCorrectParameters()
    {
        // Arrange
        var projectId = 10;
        var mrIid = 5;

        // Act
        await _sut.ExecuteAsync(projectId, mrIid);

        // Assert
        await _client.Received(1).ApproveMergeRequestAsync(projectId, mrIid, Arg.Any<CancellationToken>());
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
        await _client.DidNotReceiveWithAnyArgs().ApproveMergeRequestAsync(default, default, default);
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
        await _client.DidNotReceiveWithAnyArgs().ApproveMergeRequestAsync(default, default, default);
    }
}
