# Setup Google Antigravity (GitLab MCP)

## Requisitos
- Servidor GitLab MCP rodando (ex: em `http://localhost:5282`).

## Configuração

Adicione o servidor nas configurações do agente no Google Antigravity:

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

> **Aviso:** Nos tokens (`Authorization` e `X-GitLab-Token`), o valor deve ser colocado explicitamente (ex: `"Bearer minha_chave"` e `"glpat-meutoken"`). Não utilize variáveis de ambiente, pois atualmente o Antigravity apresenta incompatibilidade com a resolução das mesmas.
