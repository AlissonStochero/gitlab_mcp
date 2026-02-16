using System;
using System.Threading;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitLabMcp.Infrastructure.Security;

public sealed class McpRequestAuthenticator : IMcpRequestAuthenticator
{
    private readonly IReadOnlyCollection<string> _apiKeys;
    private readonly ILogger<McpRequestAuthenticator> _logger;
    private int _warned;

    public McpRequestAuthenticator(IOptions<McpAuthOptions> options, ILogger<McpRequestAuthenticator> logger)
    {
        _apiKeys = options.Value.GetApiKeys();
        _logger = logger;
    }

    public bool IsAuthorized(string? token)
    {
        if (_apiKeys.Count == 0)
        {
            if (Interlocked.Exchange(ref _warned, 1) == 0)
            {
                _logger.LogWarning("MCP_SERVER_API_KEY is not configured. Requests will be accepted without auth.");
            }
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return _apiKeys.Contains(token.Trim());
    }
}
