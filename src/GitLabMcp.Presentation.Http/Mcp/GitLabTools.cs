using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitLabMcp.Application.UseCases.Issues;
using GitLabMcp.Application.UseCases.MergeRequests;
using GitLabMcp.Application.UseCases.Projects;
using GitLabMcp.Domain.Entities;
using GitLabMcp.Domain.Errors;
using ModelContextProtocol.Server;

namespace GitLabMcp.Presentation.Http.Mcp;

[McpServerToolType]
public sealed class GitLabTools
{
    private readonly GetProjectsUseCase _getProjects;
    private readonly ListOpenMergeRequestsUseCase _listOpenMergeRequests;
    private readonly GetMergeRequestDetailsUseCase _getMergeRequestDetails;
    private readonly GetMergeRequestCommentsUseCase _getMergeRequestComments;
    private readonly AddMergeRequestCommentUseCase _addMergeRequestComment;
    private readonly AddMergeRequestDiffCommentUseCase _addMergeRequestDiffComment;
    private readonly GetMergeRequestDiffUseCase _getMergeRequestDiff;
    private readonly GetIssueDetailsUseCase _getIssueDetails;
    private readonly SetMergeRequestTitleUseCase _setMergeRequestTitle;
    private readonly SetMergeRequestDescriptionUseCase _setMergeRequestDescription;
    private readonly ApproveMergeRequestUseCase _approveMergeRequest;
    private readonly UnapproveMergeRequestUseCase _unapproveMergeRequest;

    public GitLabTools(
        GetProjectsUseCase getProjects,
        ListOpenMergeRequestsUseCase listOpenMergeRequests,
        GetMergeRequestDetailsUseCase getMergeRequestDetails,
        GetMergeRequestCommentsUseCase getMergeRequestComments,
        AddMergeRequestCommentUseCase addMergeRequestComment,
        AddMergeRequestDiffCommentUseCase addMergeRequestDiffComment,
        GetMergeRequestDiffUseCase getMergeRequestDiff,
        GetIssueDetailsUseCase getIssueDetails,
        SetMergeRequestTitleUseCase setMergeRequestTitle,
        SetMergeRequestDescriptionUseCase setMergeRequestDescription,
        ApproveMergeRequestUseCase approveMergeRequest,
        UnapproveMergeRequestUseCase unapproveMergeRequest)
    {
        _getProjects = getProjects;
        _listOpenMergeRequests = listOpenMergeRequests;
        _getMergeRequestDetails = getMergeRequestDetails;
        _getMergeRequestComments = getMergeRequestComments;
        _addMergeRequestComment = addMergeRequestComment;
        _addMergeRequestDiffComment = addMergeRequestDiffComment;
        _getMergeRequestDiff = getMergeRequestDiff;
        _getIssueDetails = getIssueDetails;
        _setMergeRequestTitle = setMergeRequestTitle;
        _setMergeRequestDescription = setMergeRequestDescription;
        _approveMergeRequest = approveMergeRequest;
        _unapproveMergeRequest = unapproveMergeRequest;
    }

    [McpServerTool, Description("Obtem uma lista de projetos do GitLab acessiveis com o token configurado.")]
    public async Task<string> get_projects(
        [Description("Filtro por nome (opcional).")] string? search = null,
        [Description("Visibilidade (public, internal, private).")] string? visibility = "private")
    {
        var projects = await _getProjects.ExecuteAsync(search, visibility);
        if (projects.Count == 0)
        {
            return "Encontrados 0 projetos.";
        }

        var lines = projects
            .Select(project => $"ID: {project.Id} - {project.Name} ({project.PathWithNamespace}) - {project.Visibility}");
        return $"Encontrados {projects.Count} projetos:\n\n{string.Join("\n", lines)}";
    }

    [McpServerTool, Description("Lista todas as merge requests abertas no projeto especificado.")]
    public async Task<string> list_open_merge_requests(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("Estado (opened, merged, closed).")] string? state = "opened")
    {
        var mergeRequests = await _listOpenMergeRequests.ExecuteAsync(project_id, state);
        if (mergeRequests.Count == 0)
        {
            return $"Encontrados 0 merge requests no projeto {project_id}.";
        }

        var lines = mergeRequests
            .Select(mr => $"#{mr.Iid} - {mr.Title} ({mr.State}) - {mr.AuthorName}");
        return $"Encontrados {mergeRequests.Count} merge requests no projeto {project_id}:\n\n{string.Join("\n", lines)}";
    }

    [McpServerTool, Description("Obtem informacoes detalhadas sobre uma merge request especifica.")]
    public async Task<string> get_merge_request_details(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
    {
        var mr = await _getMergeRequestDetails.ExecuteAsync(project_id, mr_iid);
        var description = string.IsNullOrWhiteSpace(mr.Description) ? "Sem descricao" : mr.Description;
        var labels = mr.Labels.Count > 0 ? string.Join(", ", mr.Labels) : "Sem labels";
        var reviewers = mr.Reviewers.Count > 0 ? string.Join(", ", mr.Reviewers) : "Nenhum";
        var assignees = mr.Assignees.Count > 0 ? string.Join(", ", mr.Assignees) : "Nenhum";
        var draftIndicator = mr.Draft ? " [DRAFT]" : "";
        var conflictIndicator = mr.HasConflicts ? " ⚠️ TEM CONFLITOS" : "";
        var mergedAt = mr.MergedAt.HasValue ? $"\nMerged em: {mr.MergedAt.Value:O}" : "";

        return $"Merge Request #{mr.Iid}: {mr.Title}{draftIndicator}\n\n" +
               $"Estado: {mr.State}{conflictIndicator}\n" +
               $"Merge Status: {mr.DetailedMergeStatus}\n" +
               $"Branch: {mr.SourceBranch} -> {mr.TargetBranch}\n" +
               $"Autor: {mr.AuthorName}\n" +
               $"Assignees: {assignees}\n" +
               $"Reviewers: {reviewers}\n" +
               $"Labels: {labels}\n" +
               $"Criado em: {mr.CreatedAt:O}\n" +
               $"Atualizado em: {mr.UpdatedAt:O}\n" +
               $"URL: {mr.WebUrl}{mergedAt}\n\n" +
               $"Descricao:\n{description}";
    }

    [McpServerTool, Description("Obtem comentarios (notas) de uma merge request.")]
    public async Task<string> get_merge_request_comments(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
    {
        var notes = await _getMergeRequestComments.ExecuteAsync(project_id, mr_iid);
        if (notes.Count == 0)
        {
            return $"Comentarios do Merge Request #{mr_iid}:\n\nNenhum comentario encontrado";
        }

        var formatted = notes
            .Select(note => FormatNote(note));
        return $"Comentarios do Merge Request #{mr_iid}:\n\n{string.Join("\n", formatted)}";
    }

    [McpServerTool, Description("Adiciona um comentario geral a uma merge request.")]
    public async Task<string> add_merge_request_comment(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid,
        [Description("Texto do comentario.")] string comment)
    {
        await _addMergeRequestComment.ExecuteAsync(project_id, mr_iid, comment);
        return $"Comentario adicionado com sucesso ao MR #{mr_iid}.";
    }

    [McpServerTool, Description("Adiciona um comentario em uma linha especifica de um arquivo em um MR.")]
    public async Task<string> add_merge_request_diff_comment(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid,
        [Description("Texto do comentario.")] string comment,
        [Description("Caminho do arquivo.")] string file_path,
        [Description("Numero da linha.")] int line_number,
        [Description("Tipo de linha (new ou old).")] string? line_type = "new")
    {
        await _addMergeRequestDiffComment.ExecuteAsync(project_id, mr_iid, comment, file_path, line_number, line_type);
        return $"Comentario adicionado na linha {line_number} de {file_path} no MR #{mr_iid}.";
    }

    [McpServerTool, Description("Obtem o diff de uma merge request.")]
    public async Task<string> get_merge_request_diff(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
    {
        var diff = await _getMergeRequestDiff.ExecuteAsync(project_id, mr_iid);
        var builder = new StringBuilder();
        builder.AppendLine($"Merge Request #{mr_iid} - Mudancas:");
        builder.AppendLine();

        foreach (var change in diff.Changes)
        {
            builder.AppendLine($"Arquivo: {change.NewPath}");
            if (change.NewFile)
            {
                builder.AppendLine("(Arquivo novo)");
            }
            else if (change.DeletedFile)
            {
                builder.AppendLine("(Arquivo removido)");
            }
            else if (change.RenamedFile)
            {
                builder.AppendLine($"(Renomeado de: {change.OldPath})");
            }

            builder.AppendLine("```diff");
            builder.AppendLine(change.Diff);
            builder.AppendLine("```");
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    [McpServerTool, Description("Obtem informacoes detalhadas sobre uma issue especifica.")]
    public async Task<string> get_issue_details(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da issue.")] int issue_iid)
    {
        var issue = await _getIssueDetails.ExecuteAsync(project_id, issue_iid);
        var description = string.IsNullOrWhiteSpace(issue.Description) ? "Sem descricao" : issue.Description;
        var labels = issue.Labels.Count > 0 ? string.Join(", ", issue.Labels) : "Sem labels";

        return $"Issue #{issue.Iid}: {issue.Title}\n\n" +
               $"Estado: {issue.State}\n" +
               $"Autor: {issue.AuthorName}\n" +
               $"Criado em: {issue.CreatedAt:O}\n" +
               $"Atualizado em: {issue.UpdatedAt:O}\n" +
               $"URL: {issue.WebUrl}\n" +
               $"Labels: {labels}\n\n" +
               $"Descricao:\n{description}";
    }

    [McpServerTool, Description("Define o titulo de uma merge request.")]
    public async Task<string> set_merge_request_title(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid,
        [Description("Novo titulo.")] string title)
    {
        await _setMergeRequestTitle.ExecuteAsync(project_id, mr_iid, title);
        return $"Titulo do MR #{mr_iid} atualizado para: {title}.";
    }

    [McpServerTool, Description("Define a descricao de uma merge request.")]
    public async Task<string> set_merge_request_description(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid,
        [Description("Nova descricao.")] string description)
    {
        await _setMergeRequestDescription.ExecuteAsync(project_id, mr_iid, description);
        return $"Descricao do MR #{mr_iid} atualizada com sucesso.";
    }

    [McpServerTool, Description("Aprova um merge request.")]
    public async Task<string> approve_merge_request(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
    {
        await _approveMergeRequest.ExecuteAsync(project_id, mr_iid);
        return $"Merge Request #{mr_iid} aprovado com sucesso.";
    }

    [McpServerTool, Description("Revoga a aprovacao de uma merge request. O usuario autenticado deve ter aprovado o MR previamente.")]
    public async Task<string> unapprove_merge_request(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
    {
        try
        {
            await _unapproveMergeRequest.ExecuteAsync(project_id, mr_iid);
            return $"Aprovacao do Merge Request #{mr_iid} revogada com sucesso.";
        }
        catch (GitLabApiException ex) when (ex.StatusCode == 404)
        {
            return $"Nao foi possivel revogar a aprovacao do MR #{mr_iid}. " +
                   "Verifique se o usuario autenticado ja aprovou este merge request, " +
                   "se o project_id e mr_iid estao corretos, e se o projeto tem aprovacoes habilitadas.";
        }
    }

    [McpServerTool, Description("Alias de list_open_merge_requests.")]
    public Task<string> list_merge_requests(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("Estado (opened, merged, closed).")] string? state = "opened")
        => list_open_merge_requests(project_id, state);

    [McpServerTool, Description("Alias de get_merge_request_details.")]
    public Task<string> get_merge_request(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
        => get_merge_request_details(project_id, mr_iid);

    [McpServerTool, Description("Alias de get_merge_request_diff.")]
    public Task<string> get_merge_request_diffs(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid)
        => get_merge_request_diff(project_id, mr_iid);

    [McpServerTool, Description("Alias de add_merge_request_comment.")]
    public Task<string> add_comment(
        [Description("ID do projeto GitLab.")] int project_id,
        [Description("IID da merge request.")] int mr_iid,
        [Description("Texto do comentario.")] string comment)
        => add_merge_request_comment(project_id, mr_iid, comment);

    private static string FormatNote(Note note)
        => $"**{note.AuthorName}** em {note.CreatedAt:O}:\n{note.Body}\n";
}
