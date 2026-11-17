# Validação do motivo do ajuste

Data: 26/10/2026. Branch: `dev/emmy`. Requisitos: RF-23, UC-10, RN-02 e RN-04.

O motivo é normalizado no ViewModel antes da validação: espaços nas extremidades são removidos e sequências de espaços, tabulações, quebras de linha e espaços Unicode tornam-se um único espaço. O servidor aplica os atributos de validação do campo, com comprimento de 5 a 500 caracteres após a normalização. O texto normalizado permanece associado à movimentação AJUSTE; usuário autenticado e data UTC continuam obtidos no servidor.

## Verificações automatizadas

- `git diff --check`: aprovado.
- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj`: aprovado, zero avisos e zero erros.
- `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`: 259 aprovados, zero falhas e zero ignorados.
- Cobertura do fluxo: motivo ausente, vazio, somente espaços, menos de 5 caracteres após normalização, limites de 5 e 500, excesso de 501, espaços internos e Unicode, aumento e redução de saldo, identidade e data adulteradas no formulário e saldo sem alteração.
- `AjusteAuthorizationHttpTests` verifica bloqueio de VENDEDOR em GET e POST com token antiforgery válido, além da preservação de saldos e histórico.

Contadores exportados do TRX: [testes-resumo.json](evidencias/ajuste-motivo/testes-resumo.json).

## Validação manual do backend

Aplicação executada em `http://127.0.0.1:5286`, com SQLite isolado em `output/ajuste-motivo/manual.db` e usuários demonstrativos. Requisições HTTP reais com sessão por cookie e token antiforgery; inspeção direta do banco após cada operação. Esta verificação não incluiu inspeção visual do navegador.

| Cenário | Resultado observado |
| --- | --- |
| Vazio, espaços, `abcd`, ` a   b ` e 501 caracteres | Formulário devolvido com erro; saldo 10 e nenhum movimento |
| `  abcde  `, saldo 10 → 15 | Redirecionamento; motivo `abcde`; um AJUSTE |
| 500 caracteres, saldo 15 → 7 | Aceito; segundo AJUSTE |
| Tabulações, espaços internos e quebra de linha, saldo 7 → 12 | Motivo persistido como `Contagem física do estoque`; terceiro AJUSTE |
| Formulário com usuário e data falsos | Movimentos mantiveram o lojista autenticado e a data do servidor |
| Saldo 12 → 12 com motivo válido | Erro de ausência de alteração; saldo 12 e três movimentos preservados |
| VENDEDOR em GET e POST de ajuste | Redirecionamento para acesso negado; saldo e histórico preservados |

Registro das respostas e do estado persistido: [validacao-manual.json](evidencias/ajuste-motivo/validacao-manual.json).

## Evidência para o cartão

Validação de motivo consolidada no servidor: obrigatório, 5 a 500 caracteres normalizados, rejeição de ajuste sem mudança de saldo, rastreabilidade preservada e acesso restrito ao lojista. Build aprovado e suíte com 259 testes aprovados. Validação manual do backend aprovada conforme tabela e registros anexos. A revisão e os resultados do GitHub Actions devem ser conferidos no PR.
