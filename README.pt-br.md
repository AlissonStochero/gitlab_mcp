# GitLab MCP (.NET)

> [!NOTE]
> **Nota sobre Contribuições**: Agradecemos o interesse em contribuir! Note que as revisões de Pull Requests podem levar algum tempo devido à disponibilidade limitada. Consulte o [Guia de Contribuição](CONTRIBUTING.md) para mais detalhes e sobre quando considerar um fork.


Servidor MCP para GitLab usando transporte HTTP streamable, com ferramentas equivalentes ao script Python do repositório original.

## Recursos
- MCP HTTP streamable em `/api/mcp`
- Ferramentas para projetos, merge requests e issues
- Aliases para compatibilidade (list/get/add legacy)
- Autenticação por `MCP_SERVER_API_KEY` (Bearer ou `X-API-Key`)
- Arquitetura em camadas (Domain/Application/Infrastructure/Presentation)

## Requisitos
- .NET SDK 9.0 (target `net9.0`)
- Token do GitLab (PAT) com acesso aos projetos desejados

## Configuração

### Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `MCP_SERVER_API_KEY` | Chave de API única para autenticar clientes MCP. |
| `MCP_SERVER_API_KEYS` | Lista de chaves permitidas (separadas por `,` ou `;`). Útil para múltiplos clientes. |
| `GITLAB_TOKEN` | Token de Acesso Pessoal (PAT) do GitLab. Usado como **fallback** se o cliente não enviar o header `X-GitLab-Token`. |
| `GITLAB_URL` | URL da instância do GitLab (padrão: `https://gitlab.com`). |

> [!NOTE]
> Se nenhuma chave MCP (`MCP_SERVER_API_KEY` ou `MCP_SERVER_API_KEYS`) for configurada, o servidor aceitará requisições **sem autenticação**. Isso gera um aviso no log e é recomendado apenas para desenvolvimento local.

### Autenticação do GitLab

O servidor prioriza o token enviado pelo cliente MCP nos headers da requisição.
Headers suportados (em ordem de prioridade):
1. `X-GitLab-Token`
2. `X-GitLab-Private-Token`
3. `PRIVATE-TOKEN`

Se nenhum header for encontrado, o servidor tenta usar a variável de ambiente `GITLAB_TOKEN`.

## Rodar localmente

1. Defina as variáveis (exemplo PowerShell):
   ```powershell
   $env:GITLAB_TOKEN="seu_token_gitlab"
   $env:MCP_SERVER_API_KEY="sua_chave_secreta"
   ```

2. Inicie o servidor:
   ```powershell
   dotnet run --project src/GitLabMcp.Presentation.Http --launch-profile http
   ```
   Endpoints:
   - Base: `http://localhost:5282/`
   - MCP: `http://localhost:5282/api/mcp`

## Rodar com Docker

### Desenvolvimento (Hot Reload)
Para desenvolver com hot reload habilitado, use o Docker Compose. Isso montará o diretório atual dentro do container e reiniciará a aplicação a cada mudança de arquivo.

1. Configure o `.env` (opcional) ou exporte as variáveis de ambiente baseadas no `docker-compose.yml`.
2. Execute:
   ```bash
   docker-compose up --build
   ```
   A aplicação estará disponível em `http://localhost:8080/api/mcp`.

### Produção / Deploy
Para construir uma imagem otimizada e pronta para produção:

1. Build da imagem:
   ```bash
   docker build -t gitlab-mcp .
   ```
2. Executar o container:
   ```bash
   docker run -d -p 8080:8080 \
     -e MCP_SERVER_API_KEY="sua_chave_segura" \
     -e GITLAB_URL="https://gitlab.com" \
     --name gitlab-mcp-server \
     gitlab-mcp
   ```
   *Nota: O Token do GitLab deve ser enviado pelo cliente (ex: VS Code, Claude) via header `X-GitLab-Token`.*


## Configurar Clientes MCP

### VS Code (Extensão MCP)
Arquivo `.vscode/mcp.json`:
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

### Codex CLI
Arquivo `~/.codex/config.toml`:
```toml
[mcp_servers.gitlab]
bearer_token_env_var = "MCP_SERVER_API_KEY"
url = "http://localhost:5282/api/mcp"

[mcp_servers.gitlab.env_http_headers]
X-GitLab-Token = "GITLAB_TOKEN"
```

### GitHub Copilot
Configuração do servidor MCP:
```json
"gitlab-mcp": {
    "type": "http",
    "url": "http://localhost:5282/api/mcp",
    "headers": {
        "Authorization": "Bearer ${env:MCP_SERVER_API_KEY}",
        "X-GitLab-Token": "${env:GITLAB_TOKEN}"
    }
}
```

### Google Antigravity
Configuração do servidor MCP:
```json
"gitlab-mcp": {
      "type": "streamable-http",
      "serverUrl": "http://localhost:5282/api/mcp",
      "headers": {
        "Authorization": "Bearer bearer-token-explicito",
        "X-GitLab-Token": "Token-explicito"
      }
    }
```
> **Nota:** É necessário colocar os tokens explicitamente (sem usar variáveis de ambiente como `${env:GITLAB_TOKEN}`), pois o Antigravity possui incompatibilidade com variáveis de ambiente no momento.

## Ferramentas Disponíveis

### Projetos
| Ferramenta | Descrição | Argumentos |
|------------|-----------|------------|
| `get_projects` | Obtém lista de projetos acessíveis. | `search` (opcional): Filtro por nome.<br>`visibility` (padrão: "private"): public, internal, private. |

### Merge Requests
| Ferramenta | Descrição | Argumentos |
|------------|-----------|------------|
| `list_open_merge_requests` | Lista MRs abertos num projeto. | `project_id`: ID do projeto.<br>`state` (padrão: "opened"): opened, merged, closed. |
| `get_merge_request_details` | Detalhes completos de um MR. | `project_id`, `mr_iid` (Internal ID do MR). |
| `get_merge_request_comments` | Lista comentários (notas) de um MR. | `project_id`, `mr_iid`. |
| `add_merge_request_comment` | Adiciona comentário geral ao MR. | `project_id`, `mr_iid`, `comment`. |
| `add_merge_request_diff_comment` | Comenta numa linha específica do diff. | `project_id`, `mr_iid`, `comment`<br>`file_path`: Caminho do arquivo.<br>`line_number`: Linha.<br>`line_type` (padrão: "new"): "new" ou "old". |
| `get_merge_request_diff` | Obtém o diff (mudanças) do MR. | `project_id`, `mr_iid`. |
| `set_merge_request_title` | Altera o título do MR. | `project_id`, `mr_iid`, `title`. |
| `set_merge_request_description` | Altera a descrição do MR. | `project_id`, `mr_iid`, `description`. |
| `approve_merge_request` | Aprova o MR. | `project_id`, `mr_iid`. |

### Issues
| Ferramenta | Descrição | Argumentos |
|------------|-----------|------------|
| `get_issue_details` | Detalhes de uma issue. | `project_id`, `issue_iid`. |

### Aliases (Compatibilidade)
Estes aliases existem para manter compatibilidade com outras versões ou ferramentas existentes:

- `list_merge_requests` → `list_open_merge_requests`
- `get_merge_request` → `get_merge_request_details`
- `get_merge_request_diffs` → `get_merge_request_diff`
- `add_comment` → `add_merge_request_comment`

## Estrutura do Projeto
```
src/
  GitLabMcp.Domain/          # Entidades e Interfaces
  GitLabMcp.Application/     # Casos de Uso (Lógica)
  GitLabMcp.Infrastructure/  # Implementação do Cliente GitLab e Auth
  GitLabMcp.Presentation.Http/ # API Asp.Net Core (Endpoints MCP)
tests/
  GitLabMcp.UnitTests/
  GitLabMcp.IntegrationTests/
```

## Desenvolvimento e Testes

Build:
```bash
dotnet build GitLabMcp.sln
```

Testes:
```bash
dotnet test GitLabMcp.sln
```
