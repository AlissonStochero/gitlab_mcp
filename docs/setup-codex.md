# Setup Codex CLI (GitLab MCP)

## Requisitos
- Servidor GitLab MCP rodando (ex: em `http://localhost:5282`).
- Codex CLI instalada.

## Configuração

Edite o arquivo `~/.codex/config.toml`:

```toml
[mcp_servers.gitlab]
bearer_token_env_var = "MCP_SERVER_API_KEY"
url = "http://localhost:5282/api/mcp"

[mcp_servers.gitlab.env_http_headers]
X-GitLab-Token = "GITLAB_TOKEN"
```

## Variáveis de Ambiente

Certifique-se de exportar as variáveis antes de rodar o `codex`:

```bash
export MCP_SERVER_API_KEY="sua_chave_secreta"
export GITLAB_TOKEN="seu_token_gitlab"
```

Ou no Windows (PowerShell):

```powershell
$env:MCP_SERVER_API_KEY="sua_chave_secreta"
$env:GITLAB_TOKEN="seu_token_gitlab"
```
