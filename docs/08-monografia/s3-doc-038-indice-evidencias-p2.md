# S3-DOC-038 — Índice de evidências preliminares da P2

> **Status:** preliminar, em elaboração.
> **Não é declaração de entrega final.**
> **Não inclui declaração de conclusão parcial assinada nem via sem assinatura.**

| Campo | Valor |
| --- | --- |
| Cartão | `[S3-DOC-038] Preparar evidências da P2 — aplicação, MVC e persistência` |
| Sprint | Sprint 3 |
| Reunião-alvo | 09/11/2026 |
| Prazo formal | 11/11/2026, 23:59 (America/Sao_Paulo) |
| Área principal | Documentação |
| Branch sugerida | `docs/s3-doc-038-integrante` |
| Baseline de código conferida | `origin/main` em `c6cfdad` (08/11/2026) |
| Árvore de trabalho | `docs/s3-doc-037-integrante` em `9ebe899` (09/11/2026), um commit documental à frente da `origin/main` |
| Data desta organização | 09/11/2026 |
| Versão deste índice | 0.1 — rascunho para revisão por par |
| Responsável | integrante que assumir o cartão |

## 1. Objetivo e limite do artefato

Este índice reúne, em um único ponto, as evidências técnicas preliminares da P2
sobre:

- aplicação web funcional;
- interface gráfica (GUI);
- separação MVC;
- persistência em banco de dados.

Cada evidência é vinculada ao código, à rota executável, à tela, ao teste ou ao
registro que a sustenta. O documento também registra o que ainda não foi
comprovado e as pendências de produção para a Sprint 4.

**Limites explícitos:**

- o índice não substitui a aplicação em execução, o Pull Request, o GitHub
  Actions ou a homologação por outro integrante;
- o índice não declara a P2 como concluída;
- nenhuma declaração de conclusão parcial sem assinatura foi incluída;
- capturas de protótipo HTML não são apresentadas como capturas da aplicação
  real;
- resultados de execução local não são apresentados como homologação no Azure;
- a baseline `c6cfdad` não é a mesma versão atualmente publicada no Azure,
  conforme registrado no runbook e no histórico de validações manuais.

## 2. Como reproduzir a aplicação

Pré-requisitos: Git, SDK .NET 10 e navegador atualizado.

Na raiz do repositório:

```bash
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

A aplicação local usa `http://localhost:5186` por padrão. A conexão SQLite é
`Data Source=Estoque.db`, definida em
[`appsettings.json`](../../src/SquadEstoque.Web/appsettings.json). O schema é
aplicado por `Database.Migrate()` na inicialização.

Execução de referência desta organização, em 09/11/2026, Windows, SDK
`10.0.403`, sobre o código de `origin/main` em `c6cfdad`:

| Verificação | Resultado observado |
| --- | --- |
| Build do projeto de testes (Debug) | Êxito, 0 erros e 0 avisos |
| Suíte .NET (`--no-build`) | 310 aprovados, 0 falhas, 1 ignorado, 311 no total |
| Teste ignorado | `ConsultaP95Tests.RNF02_consulta_p95_must_be_below_500ms`, opt-in e ignorado pela configuração local |
| `git status --short` antes da criação deste arquivo | Nenhuma alteração de código; apenas artefatos documentais |

A contagem incorpora os testes de regressão de segurança do login (S3-QA-029)
e os casos HTTP adicionados com a regressão E2E (S3-QA-030), ambos integrados à
`origin/main` em 08/11/2026. A medição de P95 tem relatório local próprio em
[S3-QA-028](../09-testing/s3-qa-028-p95-consulta.md), mas continua opt-in e não
foi repetida nesta execução.

A fonte oficial de build, testes e cobertura por commit é o
[GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions). O
resultado acima é apenas a referência local desta organização.

### 2.1 Usuários demonstrativos

O seed de usuários só é criado quando `Demo:SeedUsers=true`. As senhas são
armazenadas com BCrypt de custo 12 em
[`Program.cs`](../../src/SquadEstoque.Web/Program.cs). Credenciais
demonstrativas não devem ser usadas no ambiente publicado nem registradas em
capturas.

## 3. Critérios da P2 e estado das evidências

Critérios extraídos de
[`avaliacao-professor.md`](avaliacao-professor.md), seção **P2 — DER**.

| Critério da P2 | Peso | Evidência reunida neste índice | Estado |
| --- | --- | --- | --- |
| Declaração de Conclusão Parcial assinada pela empresa | 1,0 | Nenhuma declaração foi localizada no repositório. O artefato não inclui declaração sem assinatura. | **Pendente** — Sprint 4 |
| Entrega da Aplicação (App Web) completa, funcional, com GUI e conexão com banco | 2,0 | Rotas, controllers, views, build, 310 testes aprovados, fluxos do vendedor e do lojista, migrations e banco SQLite | **Parcialmente comprovado** — falta publicar e homologar a baseline atual |

A entrega da aplicação é sustentada por quatro grupos de evidência:

1. **execução** — build e suíte automatizada;
2. **GUI** — telas Razor, rotas e capturas reais existentes;
3. **MVC** — controllers, models, view models e views separados;
4. **persistência** — `EstoqueContext`, migrations, constraints e testes de
   integração.

## 4. Evidências de aplicação funcional

### 4.1 Fluxos executáveis e rastreabilidade

| Fluxo | Rota principal | Controller e ação | Model / ViewModel | View | Teste automatizado |
| --- | --- | --- | --- | --- | --- |
| Login, logout e acesso negado | `/Account/Login`, `/Account/Logout`, `/Account/AccessDenied` | [`AccountController`](../../src/SquadEstoque.Web/Controllers/AccountController.cs) | `LoginViewModel`, `Usuario` | [`Login.cshtml`](../../src/SquadEstoque.Web/Views/Account/Login.cshtml), [`AccessDenied.cshtml`](../../src/SquadEstoque.Web/Views/Account/AccessDenied.cshtml) | [`AuthenticationAuthorizationTests`](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs), [`BasicRoutesTests`](../../tests/SquadEstoque.Web.Tests/BasicRoutesTests.cs) |
| Catálogo e cadastro de produtos | `/Produtos`, `/Produtos/Create`, `/Produtos/Edit/{id}`, `/Produtos/Details/{id}` | [`ProdutosController`](../../src/SquadEstoque.Web/Controllers/ProdutosController.cs) | `Produto`, `Sku`, `ProdutosIndexViewModel`, `ProdutoCreateViewModel`, `ProdutoEditViewModel` | [`Produtos/`](../../src/SquadEstoque.Web/Views/Produtos/) | [`AdministrativeHttpTests`](../../tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs), [`EstoqueDomainPersistenceTests`](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) |
| Grade de numerações | `/Produtos/AdicionarNumeracoes/{id}`, `/Produtos/Details/{id}` | `ProdutosController.AdicionarNumeracoes`, `Details` | `Sku`, `AdicionarNumeracoesViewModel` | [`AdicionarNumeracoes.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/AdicionarNumeracoes.cshtml), [`Details.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/Details.cshtml) | [`AdministrativeHttpTests`](../../tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs), [`EstoqueDomainPersistenceTests`](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) |
| Entrada, saída e ajuste de estoque | `/Movimentacoes/Entrada`, `/Movimentacoes/EntradaLote`, `/Movimentacoes/Saida`, `/Movimentacoes/Ajuste` | [`MovimentacoesController`](../../src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) | `Movimentacao`, `Sku`, `MovimentacaoCreateViewModel`, `EntradaLoteViewModel`, `AjusteEstoqueViewModel` | [`Movimentacoes/`](../../src/SquadEstoque.Web/Views/Movimentacoes/) | [`AdministrativeHttpTests`](../../tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs), [`AjusteAuthorizationHttpTests`](../../tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs), [`AjusteMotivoHttpTests`](../../tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs), [`UltimaAtualizacaoSaldoHttpTests`](../../tests/SquadEstoque.Web.Tests/UltimaAtualizacaoSaldoHttpTests.cs) |
| Histórico imutável de movimentações | `/Movimentacoes` | `MovimentacoesController.Index` | `Movimentacao`, `MovimentacoesIndexViewModel` | [`Index.cshtml`](../../src/SquadEstoque.Web/Views/Movimentacoes/Index.cshtml) | [`MovimentacaoImutabilidadeHttpTests`](../../tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs), [`MovimentacaoSomenteLeituraHttpTests`](../../tests/SquadEstoque.Web.Tests/MovimentacaoSomenteLeituraHttpTests.cs) |
| Atendimento do vendedor: consulta, seleção, venda, “Não tinha” e “Desistiu” | `/Estoque/Consulta` | [`EstoqueController`](../../src/SquadEstoque.Web/Controllers/EstoqueController.cs) | `Atendimento`, `Sku`, `Movimentacao`, `Ruptura`, `ConsultaEstoqueViewModel` | [`Consulta.cshtml`](../../src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml) | [`ConsultaEstoqueTests`](../../tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs), [`ConsultaOperacionalHttpTests`](../../tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs), [`EstoqueControllerHttpTests`](../../tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs), [`VendaRapidaHttpTests`](../../tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs), [`DesistiuHttpTests`](../../tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs), [`RupturaHttpTests`](../../tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) |
| Atendimentos pendentes e auditáveis | `/Atendimentos` | [`AtendimentosController`](../../src/SquadEstoque.Web/Controllers/AtendimentosController.cs) | `Atendimento`, `AtendimentosIndexViewModel` | [`Atendimentos/Index.cshtml`](../../src/SquadEstoque.Web/Views/Atendimentos/Index.cshtml) | [`AtendimentoAuditavelHttpTests`](../../tests/SquadEstoque.Web.Tests/AtendimentoAuditavelHttpTests.cs) |
| Saldos zerados | `/SaldosZerados` | [`SaldosZeradosController`](../../src/SquadEstoque.Web/Controllers/SaldosZeradosController.cs) | `Produto`, `Sku`, `SaldosZeradosViewModel` | [`SaldosZerados/Index.cshtml`](../../src/SquadEstoque.Web/Views/SaldosZerados/Index.cshtml) | [`SaldosZeradosConsultaTests`](../../tests/SquadEstoque.Web.Tests/SaldosZeradosConsultaTests.cs), [`SaldosZeradosViewModelTests`](../../tests/SquadEstoque.Web.Tests/SaldosZeradosViewModelTests.cs), [`SaldosZeradosAuthorizationHttpTests`](../../tests/SquadEstoque.Web.Tests/SaldosZeradosAuthorizationHttpTests.cs) |
| Histórico de rupturas | `/Rupturas` | [`RupturasController`](../../src/SquadEstoque.Web/Controllers/RupturasController.cs) | `Ruptura`, `Sku`, `Produto`, `RupturasIndexViewModel` | [`Rupturas/Index.cshtml`](../../src/SquadEstoque.Web/Views/Rupturas/Index.cshtml) | [`RupturasControllerTests`](../../tests/SquadEstoque.Web.Tests/RupturasControllerTests.cs), [`RupturasHistoricoHttpTests`](../../tests/SquadEstoque.Web.Tests/RupturasHistoricoHttpTests.cs), [`RupturaPersistenceHttpTests`](../../tests/SquadEstoque.Web.Tests/RupturaPersistenceHttpTests.cs), [`RelatoriosErroHttpTests`](../../tests/SquadEstoque.Web.Tests/RelatoriosErroHttpTests.cs) |
| Entrada fiscal por NF-e | `/EntradasFiscais`, `/EntradasFiscais/Importar`, `/EntradasFiscais/Conferir/{id}` | [`EntradasFiscaisController`](../../src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs) | `EntradaFiscal`, `ItemEntradaFiscal`, `VinculoProdutoFornecedor`, `EntradaFiscalViewModels` | [`EntradasFiscais/`](../../src/SquadEstoque.Web/Views/EntradasFiscais/) | [`EntradaFiscalHttpTests`](../../tests/SquadEstoque.Web.Tests/EntradaFiscalHttpTests.cs) |
| Saúde da aplicação | `/health` | `Program.cs` (`MapHealthChecks`) | — | — | Smoke de publicação descrito no [runbook do Azure](../07-operacional/azure.md) |

### 4.2 Regras de negócio comprovadas por teste

| Regra | Evidência automatizada |
| --- | --- |
| SKU é único por produto e numeração | [`EstoqueDomainPersistenceTests`](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs); constraint em [`EstoqueContext`](../../src/SquadEstoque.Web/Data/EstoqueContext.cs) |
| Saldo nunca fica negativo | constraint `chk_sku_saldo_nao_negativo`; testes `Sku_with_negative_balance_cannot_be_persisted` e cenários de venda sem saldo |
| Venda reduz exatamente um par e registra uma saída | [`VendaRapidaHttpTests`](../../tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs), [`VendaTransacionalTests`](../../tests/SquadEstoque.Web.Tests/VendaTransacionalTests.cs), [`EstoqueControllerHttpTests`](../../tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs) |
| Venda concorrente não permite saldo negativo | [`VendaTransacionalTests`](../../tests/SquadEstoque.Web.Tests/VendaTransacionalTests.cs) e [`RollbackVendaTests`](../../tests/SquadEstoque.Web.Tests/RollbackVendaTests.cs) |
| “Não tinha” cria ruptura sem alterar saldo nem movimentação | [`RupturaPersistenceHttpTests`](../../tests/SquadEstoque.Web.Tests/RupturaPersistenceHttpTests.cs), [`RupturaHttpTests`](../../tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) |
| “Desistiu” encerra o atendimento sem movimentação ou ruptura | [`DesistiuHttpTests`](../../tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) |
| Movimentações são imutáveis | [`MovimentacaoImutabilidadeHttpTests`](../../tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs), [`MovimentacaoSomenteLeituraHttpTests`](../../tests/SquadEstoque.Web.Tests/MovimentacaoSomenteLeituraHttpTests.cs) |
| Ajuste exige motivo e preserva histórico | [`AjusteMotivoHttpTests`](../../tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs), [`AjusteAuthorizationHttpTests`](../../tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs) |
| Saltos de schema são aplicados por migration | [`EnumDomainMigrationTests`](../../tests/SquadEstoque.Web.Tests/EnumDomainMigrationTests.cs), [`SkuUltimaAtualizacaoMigrationTests`](../../tests/SquadEstoque.Web.Tests/SkuUltimaAtualizacaoMigrationTests.cs), [`ConsultaIndexesMigrationTests`](../../tests/SquadEstoque.Web.Tests/ConsultaIndexesMigrationTests.cs) |

### 4.3 Regressão funcional e de segurança já executada

| Evidência | Escopo | Resultado registrado | Limite |
| --- | --- | --- | --- |
| [S3-QA-030 — regressão E2E](../09-testing/execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Login e logout, cadastro e grade, venda, ruptura, desistência, ajuste e relatórios do lojista | 7 casos aprovados após o reteste do E2E-02 em 07/11/2026; estado final do banco exportado em [`evidencias/s3-qa-030/`](../09-testing/evidencias/s3-qa-030/) | Correção integrada em `c6cfdad`; aceite formal não declarado; execução local sem capturas e sem revisão independente |
| [S3-QA-029 — segurança do login](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) | Senha inválida, POST de login sem antiforgery e logout sem token | 3 testes automatizados incorporados à suíte (`Existing_user_with_invalid_password_does_not_authenticate`, `Login_rejects_post_without_antiforgery_token`, `Logout_rejects_post_without_antiforgery_token_and_keeps_session`) | Cobertura automatizada; não substitui homologação no ambiente publicado |
| [S3-QA-028 — P95 da consulta](../09-testing/s3-qa-028-p95-consulta.md) | 15 P95 de servidor em 5 cenários e 3 rodadas | 15/15 abaixo de 500 ms na execução local de 29/10/2026, commit `91b9e8c` | Teste opt-in, anterior à baseline `c6cfdad`; não é medição do ambiente publicado |

## 5. Evidências de GUI

### 5.1 Telas implementadas

| Tela | Rota | View | Perfil |
| --- | --- | --- | --- |
| Login | `/Account/Login` | [`Login.cshtml`](../../src/SquadEstoque.Web/Views/Account/Login.cshtml) | Público |
| Acesso negado | `/Account/AccessDenied` | [`AccessDenied.cshtml`](../../src/SquadEstoque.Web/Views/Account/AccessDenied.cshtml) | Autenticado sem permissão |
| Consulta e atendimento | `/Estoque/Consulta` | [`Consulta.cshtml`](../../src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml) | `VENDEDOR` |
| Produtos | `/Produtos` | [`Produtos/Index.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/Index.cshtml) | `LOJISTA` |
| Novo produto | `/Produtos/Create` | [`Produtos/Create.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/Create.cshtml) | `LOJISTA` |
| Detalhe e grade | `/Produtos/Details/{id}` | [`Produtos/Details.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/Details.cshtml) | `LOJISTA` |
| Editar produto | `/Produtos/Edit/{id}` | [`Produtos/Edit.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/Edit.cshtml) | `LOJISTA` |
| Adicionar numerações | `/Produtos/AdicionarNumeracoes/{id}` | [`Produtos/AdicionarNumeracoes.cshtml`](../../src/SquadEstoque.Web/Views/Produtos/AdicionarNumeracoes.cshtml) | `LOJISTA` |
| Entrada de estoque | `/Movimentacoes/Entrada` | [`Movimentacoes/Entrada.cshtml`](../../src/SquadEstoque.Web/Views/Movimentacoes/Entrada.cshtml) | `LOJISTA` |
| Entrada em lote | `/Movimentacoes/EntradaLote` | [`Movimentacoes/EntradaLote.cshtml`](../../src/SquadEstoque.Web/Views/Movimentacoes/EntradaLote.cshtml) | `LOJISTA` |
| Saída de estoque | `/Movimentacoes/Saida` | [`Movimentacoes/Saida.cshtml`](../../src/SquadEstoque.Web/Views/Movimentacoes/Saida.cshtml) | `LOJISTA` e `VENDEDOR` na implementação atual |
| Ajuste manual | `/Movimentacoes/Ajuste` | [`Movimentacoes/Ajuste.cshtml`](../../src/SquadEstoque.Web/Views/Movimentacoes/Ajuste.cshtml) | `LOJISTA` |
| Histórico de movimentações | `/Movimentacoes` | [`Movimentacoes/Index.cshtml`](../../src/SquadEstoque.Web/Views/Movimentacoes/Index.cshtml) | `LOJISTA` |
| Saldos zerados | `/SaldosZerados` | [`SaldosZerados/Index.cshtml`](../../src/SquadEstoque.Web/Views/SaldosZerados/Index.cshtml) | `LOJISTA` |
| Histórico de rupturas | `/Rupturas` | [`Rupturas/Index.cshtml`](../../src/SquadEstoque.Web/Views/Rupturas/Index.cshtml) | `LOJISTA` |
| Atendimentos | `/Atendimentos` | [`Atendimentos/Index.cshtml`](../../src/SquadEstoque.Web/Views/Atendimentos/Index.cshtml) | `LOJISTA` |
| Entradas fiscais | `/EntradasFiscais` | [`EntradasFiscais/Index.cshtml`](../../src/SquadEstoque.Web/Views/EntradasFiscais/Index.cshtml) | `LOJISTA` |
| Importar NF-e | `/EntradasFiscais/Importar` | [`EntradasFiscais/Importar.cshtml`](../../src/SquadEstoque.Web/Views/EntradasFiscais/Importar.cshtml) | `LOJISTA` |
| Conferir NF-e | `/EntradasFiscais/Conferir/{id}` | [`EntradasFiscais/Conferir.cshtml`](../../src/SquadEstoque.Web/Views/EntradasFiscais/Conferir.cshtml) | `LOJISTA` |

A identidade visual e a responsividade de base estão no
[`_Layout.cshtml`](../../src/SquadEstoque.Web/Views/Shared/_Layout.cshtml), com
`viewport`, logo da SQUAD, menu por perfil e classes responsivas. O layout do
vendedor e o do lojista são distintos.

### 5.2 Capturas reais já versionadas

| Fluxo | Captura | Data e ambiente | Limite |
| --- | --- | --- | --- |
| Login desktop | [login-desktop.png](../09-testing/evidencias/sprint-2/login-responsivo/login-desktop.png) | 16 e 18/09/2026, aplicação local | Baseline anterior; recapturar na versão da P2 |
| Login mobile 390×844 | [login-mobile-390x844.png](../09-testing/evidencias/sprint-2/login-responsivo/login-mobile-390x844.png) | 16 e 18/09/2026, aplicação local | Viewport emulado |
| Consulta e seleção de SKU | [01-mobile-selecao.png](../09-testing/evidencias/sprint-2/nao-tinha/01-mobile-selecao.png) | 28/09/2026, aplicação local | Viewport emulado |
| “Não tinha” registrado | [02-mobile-registrado.png](../09-testing/evidencias/sprint-2/nao-tinha/02-mobile-registrado.png) | 28/09/2026, aplicação local | Viewport emulado |
| Saldo zero na grade | [03-mobile-saldo-zero.png](../09-testing/evidencias/sprint-2/nao-tinha/03-mobile-saldo-zero.png) | 28/09/2026, aplicação local | Viewport emulado |
| Grade com saldo disponível | [desktop-saldo-disponivel.png](../09-testing/evidencias/sprint-2/s2-fe-016/desktop-saldo-disponivel.png) | Sprint 2, aplicação local | Baseline anterior |
| Grade com último par | [mobile-ultimo-par.png](../09-testing/evidencias/sprint-2/s2-fe-016/mobile-ultimo-par.png) | Sprint 2, aplicação local | Viewport emulado |
| Venda sem saldo rejeitada | [venda-nao-registrada.jpeg](../09-testing/evidencias/sprint-2/venda-sem-saldo/venda-nao-registrada.jpeg) | Sprint 2, aplicação local | Registro pontual |
| Saldos zerados em 320, 390, 768 e 1440 px | [SaldosZerados-320.png](../05-ux/evidencias/s3-fe-031/SaldosZerados-320.png), [390](../05-ux/evidencias/s3-fe-031/SaldosZerados-390.png), [768](../05-ux/evidencias/s3-fe-031/SaldosZerados-768.png), [1440](../05-ux/evidencias/s3-fe-031/SaldosZerados-1440.png) | 01/11/2026, Playwright headless e SQLite descartável | Sem homologação por outro integrante |
| Rupturas em 320, 390, 768 e 1440 px | [Rupturas-320.png](../05-ux/evidencias/s3-fe-031/Rupturas-320.png), [390](../05-ux/evidencias/s3-fe-031/Rupturas-390.png), [768](../05-ux/evidencias/s3-fe-031/Rupturas-768.png), [1440](../05-ux/evidencias/s3-fe-031/Rupturas-1440.png) | 01/11/2026, Playwright headless e SQLite descartável | Sem homologação por outro integrante |
| Estados vazio e erro | [Saldos vazios](../05-ux/evidencias/s3-fe-031/SaldosZerados-vazio.png), [rupturas vazias](../05-ux/evidencias/s3-fe-031/Rupturas-vazio.png), [falha em saldos](../05-ux/evidencias/s3-fe-031/SaldosZerados-erro.png), [falha em rupturas](../05-ux/evidencias/s3-fe-031/Rupturas-erro.png) | 01/11/2026, aplicação local | Inspeção visual automatizada |

O relatório [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md) registra a
validação em quatro larguras e o resultado em
[`resultados.json`](../05-ux/evidencias/s3-fe-031/resultados.json). O
[registro consolidado de validações manuais](../09-testing/homologacoes/registro-validacoes-manuais.md)
confirma responsividade do vendedor em 09/10/2026 e histórico de rupturas em
20/10/2026.

### 5.3 Protótipos

Os arquivos de [`docs/05-ux/prototipos/`](../05-ux/prototipos/) e as capturas
[`vendedor-s2-atendimento-mobile.png`](../05-ux/evidencias/vendedor-s2-atendimento-mobile.png)
e
[`vendedor-s2-desistiu-sem-sku-mobile.png`](../05-ux/evidencias/vendedor-s2-desistiu-sem-sku-mobile.png)
documentam a **referência de interação**. Eles não são evidência de execução da
aplicação Razor e não devem ser usados para substituir as capturas reais.

## 6. Evidências de MVC

### 6.1 Separação das camadas

| Camada | Local | Conteúdo atual |
| --- | --- | --- |
| Controllers | [`Controllers/`](../../src/SquadEstoque.Web/Controllers/) | 9 controllers: Account, Atendimentos, EntradasFiscais, Estoque, Home, Movimentacoes, Produtos, Rupturas e SaldosZerados |
| Models de domínio | [`Models/Entities/`](../../src/SquadEstoque.Web/Models/Entities/) | 9 entidades persistidas em 7 arquivos: Usuario, Produto, Sku, Movimentacao, Ruptura, Atendimento, EntradaFiscal, ItemEntradaFiscal e VinculoProdutoFornecedor |
| ViewModels | [`Models/ViewModels/`](../../src/SquadEstoque.Web/Models/ViewModels/) | 15 arquivos de modelos de tela, entrada e erro |
| Views | [`Views/`](../../src/SquadEstoque.Web/Views/) | 25 arquivos Razor, incluindo layout, validações e telas por controller |
| Persistência | [`Data/EstoqueContext.cs`](../../src/SquadEstoque.Web/Data/EstoqueContext.cs) | `DbContext` com `DbSet`, índices, constraints e relacionamentos |
| Composição e pipeline | [`Program.cs`](../../src/SquadEstoque.Web/Program.cs) | DI do `EstoqueContext`, autenticação por cookie, autorização, health check e `MapControllerRoute` |

O projeto adota MVC tradicional do ASP.NET Core. Os controllers acessam o
`EstoqueContext` diretamente; não há Repository Pattern, Unit of Work, camada de
serviços artificial ou DTO sem necessidade, conforme a
[decisão de arquitetura](../04-arquitetura/arquitetura.md).

### 6.2 Evidência de que as regras não estão nas Views

As Views Razor exibem dados, estados e mensagens. Consultas, gravações,
transações, validações de saldo e autorização ficam em controllers, models e no
`EstoqueContext`. Exemplos:

- [`EstoqueController`](../../src/SquadEstoque.Web/Controllers/EstoqueController.cs)
  executa a seleção de SKU, a venda, a ruptura e a desistência;
- [`MovimentacoesController`](../../src/SquadEstoque.Web/Controllers/MovimentacoesController.cs)
  valida quantidade, saldo e motivo antes de persistir;
- [`EstoqueContext`](../../src/SquadEstoque.Web/Data/EstoqueContext.cs)
  declara unicidade, `CHECK` de saldo, `CHECK` de quantidade, enums e
  `DeleteBehavior.Restrict`;
- as Views de
  [`Rupturas`](../../src/SquadEstoque.Web/Views/Rupturas/Index.cshtml) e
  [`SaldosZerados`](../../src/SquadEstoque.Web/Views/SaldosZerados/Index.cshtml)
  apenas renderizam os ViewModels já preparados.

### 6.3 Cobertura automatizada por camada

| Camada | Arquivos de teste |
| --- | --- |
| Rotas, autenticação e autorização | [`BasicRoutesTests`](../../tests/SquadEstoque.Web.Tests/BasicRoutesTests.cs), [`AuthenticationAuthorizationTests`](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs), [`AjusteAuthorizationHttpTests`](../../tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs), [`SaldosZeradosAuthorizationHttpTests`](../../tests/SquadEstoque.Web.Tests/SaldosZeradosAuthorizationHttpTests.cs) |
| Controllers e views HTTP | [`AdministrativeHttpTests`](../../tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs), [`EstoqueControllerHttpTests`](../../tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs), [`ConsultaOperacionalHttpTests`](../../tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs), [`RupturasControllerTests`](../../tests/SquadEstoque.Web.Tests/RupturasControllerTests.cs) |
| Persistência e domínio | [`EstoqueDomainPersistenceTests`](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs), [`VendaTransacionalTests`](../../tests/SquadEstoque.Web.Tests/VendaTransacionalTests.cs), [`RollbackVendaTests`](../../tests/SquadEstoque.Web.Tests/RollbackVendaTests.cs) |
| Migrations e schema | [`EnumDomainMigrationTests`](../../tests/SquadEstoque.Web.Tests/EnumDomainMigrationTests.cs), [`SkuUltimaAtualizacaoMigrationTests`](../../tests/SquadEstoque.Web.Tests/SkuUltimaAtualizacaoMigrationTests.cs), [`ConsultaIndexesMigrationTests`](../../tests/SquadEstoque.Web.Tests/ConsultaIndexesMigrationTests.cs) |
| Frontend isolado | [`tests/frontend/`](../../tests/frontend/) — 4 arquivos de teste com Node.js e Playwright |

## 7. Evidências de persistência

### 7.1 Banco e schema

| Evidência | Local |
| --- | --- |
| Contexto EF Core | [`Data/EstoqueContext.cs`](../../src/SquadEstoque.Web/Data/EstoqueContext.cs) |
| Entidades persistidas | [`Models/Entities/`](../../src/SquadEstoque.Web/Models/Entities/) |
| Migrations | [`Migrations/Estoque/`](../../src/SquadEstoque.Web/Migrations/Estoque/) |
| Connection string local | [`appsettings.json`](../../src/SquadEstoque.Web/appsettings.json): `Data Source=Estoque.db` |
| Banco publicado | `/home/data/Estoque.db`, conforme [runbook do Azure](../07-operacional/azure.md) |
| Migração automática na inicialização | `estoqueContext.Database.Migrate()` em [`Program.cs`](../../src/SquadEstoque.Web/Program.cs) |

Migrations versionadas:

1. `20260817120442_InitialSquadSchema`
2. `20260901000759_AddEnumDomainConstraints`
3. `20261009181737_AddAtendimentoAuditavel`
4. `20261009200729_AddEntradaFiscalEImagemProduto`
5. `20261026220326_AddSkuUltimaAtualizacaoSaldo`
6. `20261029210004_AddConsultaIndexes`

### 7.2 Integridade declarada no `EstoqueContext`

| Garantia | Implementação |
| --- | --- |
| SKU único por produto e numeração | índice único `(ProdutoId, Numeracao)` |
| Saldo não negativo | `CHECK (SaldoAtual >= 0)` |
| Quantidade de movimentação positiva | `CHECK (Quantidade > 0)` |
| Domínio de perfil e tipo | `CHECK` para `Usuario.Perfil` e `Movimentacao.Tipo` |
| Consistência de atendimento | `CHECK` de status, resultado, saldo e conclusão |
| Preservação do histórico | `DeleteBehavior.Restrict` nas relações principais |
| Desempenho de consulta | índices de produto, saldo zero, ruptura e atendimento |
| Unicidade de vínculos | `ChaveAcesso`, `AtendimentoId` e vínculo fornecedor |

### 7.3 Comprovação de persistência

| Evidência | Resultado | Limite |
| --- | --- | --- |
| Testes de integração com SQLite em memória | Constraints, rollback, concorrência e persistência cobertos por [`EstoqueDomainPersistenceTests`](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs), [`VendaTransacionalTests`](../../tests/SquadEstoque.Web.Tests/VendaTransacionalTests.cs) e [`RollbackVendaTests`](../../tests/SquadEstoque.Web.Tests/RollbackVendaTests.cs) | Banco isolado de teste |
| Testes de migration | [`EnumDomainMigrationTests`](../../tests/SquadEstoque.Web.Tests/EnumDomainMigrationTests.cs), [`SkuUltimaAtualizacaoMigrationTests`](../../tests/SquadEstoque.Web.Tests/SkuUltimaAtualizacaoMigrationTests.cs) e [`ConsultaIndexesMigrationTests`](../../tests/SquadEstoque.Web.Tests/ConsultaIndexesMigrationTests.cs) | Validam o schema, não o banco publicado |
| Persistência no Azure após reinício | Produto fictício permaneceu após reinício em 24/09/2026, conforme [validação de persistência](../07-operacional/validacao-persistencia-azure.md) | Versão anterior à baseline `c6cfdad`; repetir após a próxima publicação |
| Restauração de backup | Ainda não testada | Pendência de produção |

**Conclusão de persistência:** há implementação funcional e cobertura
automatizada. A persistência no ambiente publicado foi comprovada em
24/09/2026, mas **não há evidência de que a baseline `c6cfdad` esteja publicada**
nem de que a persistência tenha sido repetida após uma nova publicação.

## 8. Evidências de segurança e controle de acesso

Embora o critério principal da P2 seja a entrega da aplicação, a segurança
sustenta a demonstração funcional.

| Controle | Evidência |
| --- | --- |
| Login por cookie | [`Program.cs`](../../src/SquadEstoque.Web/Program.cs) e [`AccountController`](../../src/SquadEstoque.Web/Controllers/AccountController.cs) |
| Hash BCrypt com custo 12 | `BCrypt.Net.BCrypt.HashPassword(..., 12)` e `Verify`; teste `Seeded_users_use_bcrypt_with_work_factor_12` em [`AuthenticationAuthorizationTests`](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) |
| Autorização por perfil | `[Authorize(Roles = "LOJISTA")]` em Produtos, Entradas Fiscais, Atendimentos, Rupturas e Saldos Zerados; `[Authorize(Roles = "VENDEDOR")]` em Estoque; `[Authorize(Roles = "LOJISTA")]` nas ações administrativas de Movimentações (Index, Entrada, EntradaLote e Ajuste) |
| Bloqueio de escrita do vendedor no ajuste | [`AjusteAuthorizationHttpTests`](../../tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs), com GET e POST negados e saldos preservados |
| Antiforgery | `[ValidateAntiForgeryToken]` em todas as ações `[HttpPost]`; os formulários Razor geram o `__RequestVerificationToken` automaticamente |
| Logout | `AccountController.Logout` e teste `Logout_clears_authentication_session` |

**Ponto de atenção:** a rota `/Movimentacoes/Saida` aceita `LOJISTA` e
`VENDEDOR` na implementação atual, enquanto o inventário de telas descreve a
saída administrativa como fluxo do `LOJISTA`. A divergência deve ser decidida e
registrada na Sprint 4; ela não é apresentada aqui como falha da P2, mas precisa
estar coerente com a documentação de UX antes da demonstração.

## 9. Mapeamento para os critérios de produção do professor

Os critérios abaixo são os da seção **Avaliação — Produção** de
[`avaliacao-professor.md`](avaliacao-professor.md). Eles não substituem os dois
critérios da P2; são evidências de apoio que o professor também avalia no
conjunto do projeto.

| Critério de produção | Peso | Evidência deste índice | Estado |
| --- | --- | --- | --- |
| Qualidade da escrita e ortografia | 1,0 | Este documento e os documentos referenciados | Pendente de revisão por outro integrante |
| Controle de versão (Git/GitHub) | 1,5 | `origin/main` em `c6cfdad`; branch e PR serão criados após autorização | Pendente de execução |
| Gestão do Trello/Kanban e Sprints | 1,0 | Cartão S3-DOC-038 e checklist deste artefato | Pendente de movimentação e anexos no Trello |
| Presença do grupo em reuniões/aulas | 1,5 | Não comprovável por documento | Fora do escopo deste artefato |
| Arquitetura MVC e persistência | 1,5 | 9 controllers, 9 entidades, 15 view models, 25 views, `EstoqueContext`, 6 migrations e 310 testes aprovados na suíte local | **Comprovado em código e testes** |
| Segurança e controle de acesso | 2,0 | Cookie, BCrypt custo 12, autorização por perfil e testes de acesso negado | **Comprovado em código e testes**; falta smoke manual na versão publicada |
| Interface, responsividade e identidade visual | 1,5 | Layout com logo e viewport; capturas reais em 320, 390, 768 e 1440 px; registro de responsividade | **Parcial**; faltam capturas de todas as telas na baseline da P2 |

## 10. O que ainda não está comprovado

1. A versão `c6cfdad` não foi publicada no Azure nesta organização. O runbook
   registra `ead5edb` (21/09/2026) como último deploy manual e
   `AZURE_CD_ENABLED` como pendente.
2. A persistência no Azure foi validada em 24/09/2026, antes da baseline atual.
3. A restauração do backup SQLite nunca foi executada.
4. Não há declaração de conclusão parcial assinada pela empresa no repositório.
5. Não há captura atualizada de todas as telas da P2 em desktop e mobile.
6. A homologação da venda rápida mobile no ambiente publicado está bloqueada
   porque o Azure exibia versão antiga da grade, conforme o
   [registro consolidado](../09-testing/homologacoes/registro-validacoes-manuais.md).
7. Não há revisão deste índice por outro integrante.
8. O teste de P95 é opt-in e fica ignorado na suíte padrão. Há um resultado
   local aprovado em 29/10/2026 (S3-QA-028), mas ele não é apresentado como
   aprovação de desempenho da versão publicada.
9. O documento
   [`inventario-telas-e-mapa-navegacao.md`](../05-ux/inventario-telas-e-mapa-navegacao.md)
   e o
   [`plano-de-testes.md`](../09-testing/plano-de-testes.md) contêm afirmações de
   baselines anteriores que precisam ser reconciliadas com o código atual antes
   da P2.
10. A movimentação do cartão no Trello e o anexo de evidências não foram
    executados por este artefato.

## 11. Pendências para a Sprint 4

| ID | Pendência | Critério | Evidência de conclusão | Prioridade |
| --- | --- | --- | --- | --- |
| S4-P2-01 | Obter a Declaração de Conclusão Parcial preenchida e assinada pela empresa | P2 — Declaração | PDF ou imagem assinada, com data e identificação da empresa | P0 |
| S4-P2-02 | Publicar a baseline aprovada da P2 (`c6cfdad` ou sucessora) no Azure e registrar o commit publicado | P2 — Aplicação | Deploy concluído, `/health` = `Healthy` e commit registrado no runbook | P0 |
| S4-P2-03 | Repetir a persistência após a nova publicação | P2 — Aplicação / banco | Registro fictício criado, reinício do App Service e consulta confirmada | P0 |
| S4-P2-04 | Testar a restauração de um backup SQLite | P2 — Persistência | Backup restaurado em cópia descartável com `PRAGMA integrity_check` = `ok` | P1 |
| S4-P2-05 | Capturar todas as telas do MVP na versão publicada, em 390 px e 1440 px | P2 — GUI | Conjunto de PNGs com data, commit e rota; sem dados pessoais | P1 |
| S4-P2-06 | Homologar os fluxos principais por outro integrante no ambiente publicado | P2 — Aplicação | Registro em `registro-validacoes-manuais.md` com executor, data, commit e resultado | P1 |
| S4-P2-07 | Revisar o seed demonstrativo e as credenciais antes da demonstração (cartão `S4-BE-003`) | P2 — Segurança | Seed desativado ou restrito, credenciais rotacionadas e registro no runbook | P1 |
| S4-P2-08 | Decidir e alinhar a autorização de `/Movimentacoes/Saida` para `VENDEDOR` | Produção — Segurança | Decisão registrada e teste de autorização atualizado | P2 |
| S4-P2-09 | Reconciliar inventário de telas e plano de testes com o código atual | P2 — Documentação | Documentos atualizados e links conferidos | P2 |
| S4-P2-10 | Executar a suíte completa em Release e conferir o P95 no ambiente adequado | P2 — Aplicação | Resultado do GitHub Actions e evidência de P95 com volume e ambiente descritos | P2 |
| S4-P2-11 | Anexar commit, PR, capturas e resultado da CI ao cartão S3-DOC-038 | Gestão / P2 | Cartão com links e evidência visual | P1 |
| S4-P2-12 | Solicitar revisão de outro integrante e mover o cartão para Testes e depois para Feito | Gestão | Revisão aprovada, PR mergeado e cartão atualizado | P1 |
| S4-P2-13 | Registrar o aceite formal do S3-QA-030 após revisão independente da regressão E2E | P2 — Aplicação | Relatório revisado, correção `DEF-S3QA030-01` confirmada e aceite registrado | P1 |
| S4-P2-14 | Reconciliar a contagem de testes e as evidências S3-QA-029/030 no plano de testes e na monografia | P2 — Documentação | Documentos citam a mesma baseline e a mesma contagem da suíte | P2 |

## 12. Checklist de revisão deste índice

- [ ] O leitor consegue executar a aplicação pelos comandos da seção 2.
- [ ] Cada fluxo aponta para controller, model/view model, view e teste.
- [ ] Cada critério da P2 tem estado explícito: comprovado, parcial ou pendente.
- [ ] Nenhuma captura de protótipo foi apresentada como aplicação real.
- [ ] Nenhuma declaração sem assinatura foi incluída.
- [ ] Nenhum resultado de teste local foi apresentado como homologação no Azure.
- [ ] As pendências da Sprint 4 têm critério de conclusão verificável.
- [ ] Links relativos foram conferidos no GitHub.
- [ ] `git diff --check` não aponta espaços em branco problemáticos.
- [ ] O cartão foi atualizado com branch, commit ou PR e evidência visual.

## 13. Referências

- [Critérios de avaliação do professor](avaliacao-professor.md)
- [Documento de arquitetura](../04-arquitetura/arquitetura.md)
- [Inventário de telas e mapa de navegação](../05-ux/inventario-telas-e-mapa-navegacao.md)
- [Plano de testes](../09-testing/plano-de-testes.md)
- [Registro consolidado de validações manuais](../09-testing/homologacoes/registro-validacoes-manuais.md)
- [Validação de persistência no Azure](../07-operacional/validacao-persistencia-azure.md)
- [Runbook do Azure](../07-operacional/azure.md)
- [S3-FE-031 — visões do lojista](../05-ux/evidencias/s3-fe-031/README.md)
- [S3-QA-025 — última atualização do saldo](../09-testing/s3-qa-025-ultima-atualizacao.md)
- [S3-QA-028 — P95 da consulta](../09-testing/s3-qa-028-p95-consulta.md)
- [S3-QA-030 — regressão E2E](../09-testing/execucoes/sprint-3/s3-qa-030-regressao-e2e.md)
- [S3-DB-026 — índices de consulta](../09-testing/s3-db-026-indices-consulta.md)
- [Auditoria MVC de 04/11/2026](../04-arquitetura/auditoria-mvc-2026-11-04.md)
- [README do projeto](../../README.md)
- [Guia de contribuição](../../CONTRIBUTING.md)
- [GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions)

## 14. Histórico de revisões deste índice

| Versão | Data | Alteração | Situação |
| --- | --- | --- | --- |
| 0.1 | 09/11/2026 | Organização inicial das evidências de aplicação, GUI, MVC, persistência e segurança; pendências da Sprint 4 | Rascunho para revisão por par; não commitado nem anexado ao Trello |
