# Setup VS Code MCP (GitLab)

## Requisitos
- **.NET SDK 9.0** instalado.
- **GitLab Personal Access Token (PAT)** com escopo `api`.
- Uma chave arbitrária para `MCP_SERVER_API_KEY` (ex: `minha-chave-secreta`).

## 1. Configurar o Servidor

Defina as variáveis no seu ambiente ou no script de inicialização:
```powershell
$env:GITLAB_TOKEN="seu_token_gitlab"
$env:MCP_SERVER_API_KEY="sua_chave_secreta"
```

Inicie o servidor:
```powershell
dotnet run --project src/GitLabMcp.Presentation.Http --launch-profile http
```
O servidor rodará em `http://localhost:5282`.

## 2. Configurar a Extensão MCP no VS Code

Edite o arquivo de configuração da extensão (geralmente em `%APPDATA%/Code/User/globalStorage/mcp-server/mcp.json` ou via comando `MCP: Manage Servers`):

```json
{
  "servers": {
    "gitlab-mcp": {
      "type": "http",
      "url": "http://localhost:5282/api/mcp",
      "headers": {
        "Authorization": "Bearer ${env:MCP_SERVER_API_KEY}",
        "X-GitLab-Token": "${env:GITLAB_TOKEN}"
      }
    }
  }
}
```

> **Nota:** Certifique-se de que as variáveis de ambiente `MCP_SERVER_API_KEY` e `GITLAB_TOKEN` estejam disponíveis para a instância do VS Code (inicie o VS Code pelo terminal onde as vars foram definidas ou defina-as no sistema).
