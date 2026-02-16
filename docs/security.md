# Segurança

## Autenticação MCP (Cliente <-> Servidor)

O servidor usa uma chave de API para validar que apenas clientes autorizados podem conectar.

- **`MCP_SERVER_API_KEY`**: Define uma única chave.
- **`MCP_SERVER_API_KEYS`**: Define múltiplas chaves (separadas por `,` ou `;`).

O cliente deve enviar esta chave no header `Authorization: Bearer <CHAVE>` ou `X-API-Key: <CHAVE>`.

> **Aviso:** Se nenhuma destas variáveis for definida, o servidor desativa a autenticação e aceita qualquer conexão. Isso deve ser usado **apenas em desenvolvimento**.

## Autenticação GitLab (Servidor <-> GitLab)

O servidor precisa de um Token de Acesso Pessoal (PAT) para chamar a API do GitLab.

### Prioridade de Token
1. **Header do Cliente:** O servidor verifica se o cliente MCP enviou o token nos headers `X-GitLab-Token`, `X-GitLab-Private-Token` ou `PRIVATE-TOKEN`. Isso permite que cada usuário use seu próprio token.
2. **Variável de Ambiente:** Se o cliente não enviar o token, o servidor usa o valor de `GITLAB_TOKEN` definido no ambiente onde o servidor está rodando.

### Boas Práticas
- Use HTTPS em produção para proteger os tokens em trânsito.
- Não comite tokens em repositórios.
- Restrinja o escopo da `MCP_SERVER_API_KEY` se possível.
