using System;
using System.Net.Http.Headers;
using GitLabMcp.Domain.Abstractions;
using GitLabMcp.Infrastructure.Configuration;
using GitLabMcp.Infrastructure.Http;
using GitLabMcp.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GitLabMcp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GitLabClientOptions>(options =>
        {
            options.BaseUrl = configuration["GITLAB_URL"] ?? "https://gitlab.com";
            options.Token = configuration["GITLAB_TOKEN"] ?? string.Empty;
        });

        services.Configure<McpAuthOptions>(options =>
        {
            options.ApiKey = configuration["MCP_SERVER_API_KEY"];
            options.ApiKeys = configuration["MCP_SERVER_API_KEYS"];
        });

        services.AddSingleton<IMcpRequestAuthenticator, McpRequestAuthenticator>();
        services.AddScoped<GitLabTokenContext>();

        services.AddHttpClient<IGitLabApiClient, GitLabApiClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GitLabClientOptions>>().Value;
                var baseUrl = options.BaseUrl;
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    baseUrl = "https://gitlab.com";
                }

                if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
                {
                    baseUrl += "/";
                }

                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("gitlab-mcp");
            });

        return services;
    }
}
