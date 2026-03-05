# GitLab MCP (.NET)

[![Build Status](https://github.com/AlissonStochero/gitlab_mcp/actions/workflows/ci.yml/badge.svg)](https://github.com/AlissonStochero/gitlab_mcp/actions/workflows/ci.yml)
[![Code Coverage](https://codecov.io/gh/AlissonStochero/gitlab_mcp/graph/badge.svg?token=I9CBRT9V9I)](https://codecov.io/gh/AlissonStochero/gitlab_mcp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> [!WARNING]
> **Temporary Security Note**: Before making this repository public, please remove the `?token=YOUR_TOKEN` parameter from the Code Coverage badge URL above to prevent unauthorized access. Once public, Codecov will generate the badge without needing a token.

> [!NOTE]
> **Portuguese Version**: This project has a [Portuguese README (Leia-me em Português)](README.pt-br.md).

> [!NOTE]
> **Contribution Note**: We appreciate your interest in contributing! Please note that Pull Request reviews may take some time due to limited availability. Check the [Contributing Guide](CONTRIBUTING.md) for more details and when to consider a fork.

MCP (Model Context Protocol) server for GitLab using streamable HTTP transport.

## Features
- Streamable HTTP MCP at `/api/mcp`
- Tools for projects, merge requests, and issues
- Compatibility aliases (legacy list/get/add)
- Authentication via `MCP_SERVER_API_KEY` (Bearer or `X-API-Key`)
- Layered Architecture (Domain/Application/Infrastructure/Presentation)

## Requirements
- .NET SDK 9.0 (target `net9.0`)
- GitLab Personal Access Token (PAT) with access to the target projects

## Configuration

### Environment Variables

| Variable | Description |
|----------|-------------|
| `MCP_SERVER_API_KEY` | Unique API key to authenticate MCP clients. |
| `MCP_SERVER_API_KEYS` | List of allowed keys (separated by `,` or `;`). Useful for multiple clients. |
| `GITLAB_TOKEN` | GitLab Personal Access Token (PAT). Used as a **fallback** if the client does not send the `X-GitLab-Token` header. |
| `GITLAB_URL` | GitLab instance URL (default: `https://gitlab.com`). |

> [!NOTE]
> If no MCP key (`MCP_SERVER_API_KEY` or `MCP_SERVER_API_KEYS`) is configured, the server will accept requests **without authentication**. This generates a warning in the logs and is recommended only for local development.

### GitLab Authentication

The server prioritizes the token sent by the MCP client in the request headers.
Supported headers (in order of priority):
1. `X-GitLab-Token`
2. `X-GitLab-Private-Token`
3. `PRIVATE-TOKEN`

If no header is found, the server attempts to use the `GITLAB_TOKEN` environment variable.

## Running Locally

1. Set the variables (PowerShell example):
   ```powershell
   $env:GITLAB_TOKEN="your_gitlab_token"
   $env:MCP_SERVER_API_KEY="your_secret_key"
   ```

2. Start the server:
   ```powershell
   dotnet run --project src/GitLabMcp.Presentation.Http --launch-profile http
   ```
   Endpoints:
   - Base: `http://localhost:5282/`
   - MCP: `http://localhost:5282/api/mcp`

## Running with Docker

### Development (Hot Reload)
To develop with hot reload enabled, use Docker Compose. This will mount the current directory inside the container and restart the application upon any file change.

1. Configure the `.env` file (optional) or export the environment variables based on `docker-compose.yml`.
2. Run:
   ```bash
   docker-compose up --build
   ```
   The application will be available at `http://localhost:8080/api/mcp`.

### Production / Deployment
To build an optimized, production-ready image:

1. Build the image:
   ```bash
   docker build -t gitlab-mcp .
   ```
2. Run the container:
   ```bash
   docker run -d -p 8080:8080 \
     -e MCP_SERVER_API_KEY="your_secure_key" \
     -e GITLAB_URL="https://gitlab.com" \
     --name gitlab-mcp-server \
     gitlab-mcp
   ```
   *Note: The GitLab Token must be sent by the client (e.g., VS Code, Claude) via the `X-GitLab-Token` header.*


## Configuring MCP Clients

### VS Code (MCP Extension)
File `.vscode/mcp.json`:
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
File `~/.codex/config.toml`:
```toml
[mcp_servers.gitlab]
bearer_token_env_var = "MCP_SERVER_API_KEY"
url = "http://localhost:5282/api/mcp"

[mcp_servers.gitlab.env_http_headers]
X-GitLab-Token = "GITLAB_TOKEN"
```

### GitHub Copilot
MCP server configuration:
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
MCP server configuration:
```json
"gitlab-mcp": {
      "type": "streamable-http",
      "serverUrl": "http://localhost:5282/api/mcp",
      "headers": {
        "Authorization": "Bearer explicit-bearer-token",
        "X-GitLab-Token": "explicit-gitlab-token"
      }
    }
```
> **Note:** It is necessary to place the tokens explicitly (without using environment variables like `${env:GITLAB_TOKEN}`) because Antigravity currently has incompatibility with environment variables.

## Available Tools

### Projects
| Tool | Description | Arguments |
|------|-------------|-----------|
| `get_projects` | Gets a list of accessible projects. | `search` (optional): Filter by name.<br>`visibility` (default: "private"): public, internal, private. |

### Merge Requests
| Tool | Description | Arguments |
|------|-------------|-----------|
| `list_open_merge_requests` | Lists open MRs in a project. | `project_id`: Project ID.<br>`state` (default: "opened"): opened, merged, closed. |
| `get_merge_request_details` | Full details of an MR. | `project_id`, `mr_iid` (MR Internal ID). |
| `get_merge_request_comments` | Lists comments (notes) from an MR. | `project_id`, `mr_iid`. |
| `add_merge_request_comment` | Adds a general comment to the MR. | `project_id`, `mr_iid`, `comment`. |
| `add_merge_request_diff_comment` | Comments on a specific diff line. | `project_id`, `mr_iid`, `comment`<br>`file_path`: File path.<br>`line_number`: Line number.<br>`line_type` (default: "new"): "new" or "old". |
| `get_merge_request_diff` | Gets the diff (changes) of the MR. | `project_id`, `mr_iid`. |
| `set_merge_request_title` | Changes the MR title. | `project_id`, `mr_iid`, `title`. |
| `set_merge_request_description` | Changes the MR description. | `project_id`, `mr_iid`, `description`. |
| `approve_merge_request` | Approves the MR. | `project_id`, `mr_iid`. |

### Issues
| Tool | Description | Arguments |
|------|-------------|-----------|
| `get_issue_details` | Details of a single issue. | `project_id`, `issue_iid`. |

### Aliases (Compatibility)
These aliases exist to maintain compatibility with other versions or existing tools:

- `list_merge_requests` → `list_open_merge_requests`
- `get_merge_request` → `get_merge_request_details`
- `get_merge_request_diffs` → `get_merge_request_diff`
- `add_comment` → `add_merge_request_comment`

## Project Structure
```
src/
  GitLabMcp.Domain/          # Entities and Interfaces
  GitLabMcp.Application/     # Use Cases (Logic)
  GitLabMcp.Infrastructure/  # GitLab Client Implementation and Auth
  GitLabMcp.Presentation.Http/ # Asp.Net Core API (MCP Endpoints)
tests/
  GitLabMcp.UnitTests/
  GitLabMcp.IntegrationTests/
```

## Development and Testing

Build:
```bash
dotnet build GitLabMcp.sln
```

Tests:
```bash
dotnet test GitLabMcp.sln
```
