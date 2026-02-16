using System;
using System.Threading.Tasks;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GitLabMcp.Presentation.Http.Middleware;

public sealed class McpAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<McpAuthMiddleware> _logger;

    public McpAuthMiddleware(RequestDelegate next, ILogger<McpAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IMcpRequestAuthenticator authenticator,
        GitLabTokenContext gitLabTokenContext)
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        _logger.LogInformation("MCP request started. Method={Method}, Path={Path}.",
            context.Request.Method,
            context.Request.Path);
        var token = ExtractToken(context);
        if (!authenticator.IsAuthorized(token))
        {
            _logger.LogWarning("Unauthorized MCP request.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        var gitLabToken = ExtractGitLabToken(context);
        if (!string.IsNullOrWhiteSpace(gitLabToken))
        {
            gitLabTokenContext.Token = gitLabToken;
        }

        await _next(context);

        gitLabTokenContext.Token = null;
        _logger.LogInformation("MCP request finished. StatusCode={StatusCode}.",
            context.Response.StatusCode);
    }

    private static string? ExtractToken(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authorization))
        {
            var value = authorization.ToString();
            if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return value.Substring("Bearer ".Length).Trim();
            }
        }

        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return apiKey.ToString().Trim();
        }

        return null;
    }

    private static string? ExtractGitLabToken(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-GitLab-Token", out var gitLabToken))
        {
            return gitLabToken.ToString().Trim();
        }

        if (context.Request.Headers.TryGetValue("X-GitLab-Private-Token", out var gitLabPrivateToken))
        {
            return gitLabPrivateToken.ToString().Trim();
        }

        if (context.Request.Headers.TryGetValue("PRIVATE-TOKEN", out var privateToken))
        {
            return privateToken.ToString().Trim();
        }

        return null;
    }
}
