# S2-BE-029 — Desistiu sem persistência

Validação realizada em 01/10/2026 na branch `feat/s2-be-029-emmy`, criada
diretamente de `dev/emmy` (commit `57711d5`, dependência PR #42).
Requisitos: RF-19, US de Desistiu e UC-06.

## Comportamento

`POST /Estoque/RegistrarDesistiu` exige autenticação de VENDEDOR, identidade
válida e antiforgery. Não exige produto ou SKU e não consulta nem grava banco.
Responde HTTP 200 com:

```json
{
  "resultado": "desistiu",
  "atendimentoEncerrado": true,
  "mensagem": "Atendimento encerrado.",
  "novaConsultaUrl": "/Estoque/Consulta"
}
```

A interface envia o POST, bloqueia ações durante a requisição e só conclui
após confirmar o contrato de sucesso. Em falha, permite nova tentativa sem
encerrar o contexto. Em sucesso, mantém a confirmação e retorna à consulta
vazia após 1,2 segundo, como no fluxo anterior. O contexto reside na página;
não há sessão de atendimento ou entidade persistida a remover.
Nenhuma entidade ou migration criada ou alterada.

## Testes automatizados

`DesistiuHttpTests` acrescenta cinco casos HTTP com SQLite relacional isolado:

- Sucesso sem SKU e com SKU enviado, incluindo repetição segura do POST.
- Contrato JSON previsível e nova consulta sem grade, seguida de nova busca.
- Rejeição de anônimo e LOJISTA com token antiforgery válido para a identidade.
- Rejeição de POST sem antiforgery (400) e GET (404 no roteamento MVC atual).
- Em todos os casos, comparação de saldos de todos os SKUs em novo contexto
  e contagens de Movimentacao/Ruptura antes e depois.

Comandos e resultados:

- Baseline: `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`
  — 108 aprovados.
- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj`
  — aprovado, zero avisos e erros.
- Suíte final: `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`
  — 113 aprovados, zero falhas, zero ignorados.
- `git status --short` — arquivos do cartão conferidos individualmente.
- `git diff --check` — apontou espaços e linhas finais em três evidências
  locais preexistentes de venda; preservadas e excluídas do commit.
- Checagem restrita aos arquivos deste cartão — aprovada.

## Validação manual

Aplicação local em `http://localhost:5301`, com banco demonstrativo separado
em diretório temporário ignorado; sem usar Azure ou alterar o banco habitual.

1. Entrar com VENDEDOR demonstrativo.
2. Consultar `Desistiu` e abrir `Tenis Desistiu QA`.
3. Grade inicial: nº 37 saldo 0, nº 38 saldo 1, nº 39 saldo 5.
4. Selecionar nº 39 e clicar em Desistiu.
5. Confirmar bloqueio dos resultados e retorno automático à consulta vazia.
6. Consultar novamente o mesmo produto: grade disponível, seleção anterior limpa
   e saldos 0/1/5 preservados.
7. Conferir diretamente o SQLite isolado: Movimentacao = 0 e Ruptura = 0.

Evidência: [consulta vazia após Desistiu](evidencias/desistiu/nova-consulta.jpg).

## Revisão e integração

Diff revisado localmente: autorização herdada de EstoqueController, antiforgery,
ausência de operações de banco e contrato de conclusão verificados.
O PR usa `dev/emmy` como base para mostrar apenas este cartão, sem repetir #42.
Revisão humana e GitHub Actions devem ser verificadas no PR antes de integrar.
Nenhum merge ou push na main faz parte desta entrega. Trello é atualizado pela
responsável, usando este documento e o PR como evidências.
