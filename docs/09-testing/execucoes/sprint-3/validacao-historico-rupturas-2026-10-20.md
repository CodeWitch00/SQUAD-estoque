# Histórico de rupturas do lojista

## Entrega

Rota `/Rupturas`, Razor View integrada ao banco e disponível na navegação do LOJISTA. Exibe modelo, marca, cor, numeração, ocorrências e última ruptura em UTC. A consulta agrupa por produto e numeração, ordena por quantidade decrescente e última ocorrência decrescente, com desempate por modelo, marca, cor e numeração. A View percorre a coleção sem reordenar.

A consulta de leitura foi incluída porque a base possuía o ViewModel, mas ainda não possuía endpoint para alimentar a tela. Nenhuma migração ou alteração de saldo foi necessária. O histórico inclui registros de produtos com saldo reposto ou inativos. Não há edição ou exclusão de rupturas.

## Validação de interface — 20/10/2026

Validação em Chromium local, com banco SQLite isolado e dados demonstrativos. Inspeção visual das capturas em desktop 1440×1000 e smartphone emulado 390×844; não foi utilizado aparelho físico.

| Cenário | Resultado |
| --- | --- |
| Histórico preenchido | Modelo, numeração, total e data legíveis; ordem 38 (5), 39 (3), 37 (1) |
| Estado vazio | Mensagem “Nenhuma ruptura registrada” com orientação |
| Smartphone | Itens empilhados com rótulos, largura de conteúdo 390px para viewport 390px |
| Navegação móvel | Menu abre e destaca o destino atual |
| Vendedor | Link ausente e rota redirecionada para acesso negado |
| Histórico versus saldo | Texto explica a diferença e oferece consulta de saldos zerados |
| Reposição | Numerações 38 e 37 seguem no histórico com saldo positivo |
| Console | Nenhum erro JavaScript registrado |

Capturas e dados de verificação em [evidencias/historico-rupturas](evidencias/historico-rupturas).

## Testes

Os testes HTTP cobrem agregação, ordem por quantidade e data, acesso de lojista/vendedor/anônimo, estado vazio, navegação por perfil e preservação de saldo e registros após consulta.

Comandos de validação:

```text
git diff --check
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
node --test tests/frontend/*.test.cjs
```

Build aprovado sem avisos ou erros. Suíte .NET: 240 testes aprovados, zero falhas e zero ignorados. Suíte de frontend: 20 testes aprovados. O resultado do GitHub Actions consta no PR.

## Revisão técnica

Conferidos: autorização no controller, consulta sem rastreamento ou escrita, agrupamento por identidade de produto, ordenação no servidor, codificação de saída do Razor, escopo de navegação, ausência de ações de alteração, estado vazio e comportamento responsivo. A revisão por outro integrante é solicitada no PR; sua aprovação depende do revisor.
