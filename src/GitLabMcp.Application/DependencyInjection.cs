using GitLabMcp.Application.UseCases.Issues;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Application.UseCases.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace GitLabMcp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<GetProjectsUseCase>();
        services.AddTransient<ListOpenMergeRequestsUseCase>();
        services.AddTransient<GetMergeRequestDetailsUseCase>();
        services.AddTransient<GetMergeRequestCommentsUseCase>();
        services.AddTransient<AddMergeRequestCommentUseCase>();
        services.AddTransient<AddMergeRequestDiffCommentUseCase>();
        services.AddTransient<GetMergeRequestDiffUseCase>();
        services.AddTransient<GetIssueDetailsUseCase>();
        services.AddTransient<SetMergeRequestTitleUseCase>();
        services.AddTransient<SetMergeRequestDescriptionUseCase>();
        services.AddTransient<ApproveMergeRequestUseCase>();
        return services;
    }
}
