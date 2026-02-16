using GitLabMcp.Application;
using GitLabMcp.Infrastructure;
using GitLabMcp.Presentation.Http.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Mcp", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Mcp");

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/mcp"),
    mcpApp => { mcpApp.UseMiddleware<McpAuthMiddleware>(); });

app.MapMcp("/api/mcp");
app.MapGet("/", () => "GitLab MCP server is running.");

if (app.Environment.IsDevelopment())
{
    var dataSources = app.Services.GetRequiredService<IEnumerable<Microsoft.AspNetCore.Routing.EndpointDataSource>>();
    foreach (var dataSource in dataSources)
    {
        foreach (var endpoint in dataSource.Endpoints.OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>())
        {
            var methods = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods;
            app.Logger.LogInformation("Endpoint mapped: {Pattern} ({Methods})",
                endpoint.RoutePattern.RawText,
                methods == null ? "any" : string.Join(", ", methods));
        }
    }
}

app.Run();
