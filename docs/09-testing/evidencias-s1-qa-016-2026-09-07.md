# S1-QA-016 - Conferência das evidências de autorização

**Data da conferência:** 07/09/2026

**Escopo desta contribuição:** documentar a verificação de testes existentes e as fontes consultadas. Não foram implementados novos testes nem alteradas permissões.

## Resultado técnico

Os três cenários do cartão já estão cobertos em [ConsultaEstoqueTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs), na versão principal consultada, commit `bcee8217055d7a13f7da2e0862165490ffbb7361`.

| Perfil | Teste existente | Comportamento verificado |
| --- | --- | --- |
| Anônimo | `Anonymous_user_is_redirected_to_login` | HTTP 302 para `/Account/Login?ReturnUrl=%2FEstoque%2FConsulta`. |
| VENDEDOR | `Vendedor_sees_accessible_initial_state` | HTTP 200 e conteúdo da consulta. |
| LOJISTA | `Lojista_is_redirected_to_access_denied` | HTTP 302 para `/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta`. |

O [EstoqueController](../../src/SquadEstoque.Web/Controllers/EstoqueController.cs) utiliza `[Authorize(Roles = "VENDEDOR")]`. O comportamento acima corresponde à regra implementada para `GET /Estoque/Consulta`.

## Evidência de execução existente

[GitHub Actions - execução 33818328193](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/33818328193), disparada por push do commit `bcee8217055d7a13f7da2e0862165490ffbb7361` e concluída com sucesso em 03/09/2026:

- build da aplicação aprovado;
- build dos testes aprovado;
- 60 testes aprovados, 0 falhas, 0 ignorados;
- duração dos testes registrada: 15 segundos;
- ambiente: Ubuntu, .NET 10, configuração Release.

Esses resultados foram consultados no pipeline remoto. Não são uma nova execução realizada nesta contribuição e não substituem o resultado do CI deste PR documental.

## Histórico e autoria

O [relatório da Sprint 1](RELATORIO-sprint1.md) já registra S1-QA-016 como concluído, com Rayana como responsável e referência ao commit `61648a9`. O histórico desse commit contém os testes da consulta. Esta contribuição apenas reúne a conferência e seus links; não reivindica a autoria da implementação original nem redefine o estado do cartão.

A [validação da consulta de 31/08/2026](validacao-consulta-vendedor-2026-08-31.md) registra uma verificação anterior de S1-FE-018 a partir do HTML servido e das regras CSS, além de 29 testes aprovados naquela versão. O próprio documento informa que não foi possível gerar capturas pelo navegador headless e recomenda conferência visual final. Esse registro anterior não constitui validação manual realizada nesta contribuição.

## Pendências e limites da conferência

- O cartão exige PR separado e revisado, salvo pareamento registrado com S1-BE-014. A consulta de PRs associados ao commit `61648a9` não retornou resultados; a listagem consultada não identificou PR específico de S1-QA-016. Isso não comprova ausência de pareamento, pois o registro do Trello não foi consultado.
- A liderança deve conciliar essa condição com a conclusão já registrada no relatório da Sprint 1. Um PR documental não comprova retroativamente revisão ou pareamento da implementação original.
- Build e testes locais não foram executados nesta conferência: o ambiente informou ausência do SDK .NET. A alteração é exclusivamente Markdown, validada por revisão de conteúdo, referências e diff.
- Não foi realizada nova validação manual da aplicação.
- Nenhum cartão do Trello foi aberto, alterado ou movido durante esta atividade.

## Como revisar este registro

1. Conferir os três métodos indicados e o atributo de autorização no Controller.
2. Abrir a execução de CI indicada e confirmar o commit, os builds e o total de testes.
3. Comparar o histórico com o relatório da Sprint 1 e verificar separadamente a evidência de revisão ou pareamento exigida pelo cartão.
4. Avaliar este PR como documentação das evidências existentes, mantendo a autoria e as pendências registradas.

### Referências fixadas na versão conferida

- [Testes de consulta no commit verificado](https://github.com/CodeWitch00/SQUAD-estoque/blob/bcee8217055d7a13f7da2e0862165490ffbb7361/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs).
- [Regra do Controller no commit verificado](https://github.com/CodeWitch00/SQUAD-estoque/blob/bcee8217055d7a13f7da2e0862165490ffbb7361/src/SquadEstoque.Web/Controllers/EstoqueController.cs).
