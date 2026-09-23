# Validação da navegação por perfil

**Data:** 03/09/2026  
**Branch:** `dev/rayana`  
**Rastreabilidade:** RF-02, RNF-06 e UC-01

## Escopo validado

- Login do perfil `VENDEDOR` com entrada direta em `/Estoque/Consulta`.
- Login do perfil `LOJISTA` com entrada em `/Produtos`.
- Navegação do vendedor restrita à consulta operacional.
- Navegação do lojista restrita aos comandos administrativos disponíveis.
- Logout acessível para os dois perfis.
- Marca do cabeçalho direcionada à entrada do perfil, sem depender da Home provisória.
- Legado Movie preservado.

## Resultado

| Cenário | Resultado esperado | Situação |
|---|---|---|
| Login de vendedor | Redirecionar para `/Estoque/Consulta` | Aprovado |
| Login de lojista | Redirecionar para `/Produtos` | Aprovado |
| Menu do vendedor | Exibir Consulta e Sair; ocultar Produtos e Movimentações | Aprovado |
| Menu do lojista | Exibir Produtos, Movimentações e Sair; ocultar Consulta | Aprovado |
| Marca do sistema | Retornar à entrada correspondente ao perfil | Aprovado |
| Logout | Encerrar a sessão e retornar a `/Account/Login` | Aprovado |

## Evidência automatizada

Os cenários são cobertos por `AuthenticationAuthorizationTests`, incluindo os destinos
pós-login, o conteúdo da navegação por perfil e a invalidação da sessão após logout.

O legado `Movie` não foi alterado neste cartão.
