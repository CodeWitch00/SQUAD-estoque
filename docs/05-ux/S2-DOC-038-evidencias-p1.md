# S2-DOC-038 — Evidências da P1 (Protótipos, Plano de Testes e Índice de Evidências)

## 1. Identificação do Card

| Campo | Conteúdo |
|---|---|
| **Card** | S2-DOC-038 |
| **Título** | Consolidar evidências da P1 (protótipos, plano de testes e índice de evidências) |
| **Responsável** | Integrante de documentação |
| **Repositório** | [CodeWitch00/SQUAD-estoque](https://github.com/CodeWitch00/SQUAD-estoque) |
| **Documento versionado** | [`docs/05-ux/S2-DOC-038-evidencias-p1.md`](S2-DOC-038-evidencias-p1.md) |
| **Período formal da Sprint 2 / P1** | 10/09/2026 a 07/10/2026 ([relatório-sprint-2](../10-planejamento/relatorio-sprint-2.md)) |
| **Estado** | Entregue — revisão e conferência final registradas na Seção 11 |

---

## 2. Escopo e Índice do Pacote P1

Este documento consolida o pacote de evidências da P1 em um único índice versionado. Ele **não** reproduz nem reescreve a monografia nem o plano de testes oficiais: apenas aponta para eles e registra os casos de teste executados, com evidências.

| # | Seção | Artefato |
|---|---|---|
| 1 | Identificação do card | esta seção |
| 2 | Escopo e índice do pacote | esta seção |
| 3 | Inventário de telas MVP × o que existe no código | Seção 3 |
| 4 | Plano de testes (CT-001 a CT-015) | [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md) |
| 5 | Validação de responsividade | [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 6 | Índice de capturas de tela (protótipos × implementação) | Seção 6 |
| 7 | Testes automatizados e execução na CI | Seção 7 |
| 8 | Rastreabilidade com os PRs entregues | Seção 8 |
| 9 | Itens pendentes e falhas conhecidas | Seção 9 |
| 10 | Checklist de integridade do pacote | Seção 10 |
| 11 | Conferência final da documentação | Seção 11 |

**Artefatos de referência (normativos deste pacote):**

- Inventário de telas e mapa de navegação: [`docs/05-ux/inventario-telas-e-mapa-navegacao.md`](inventario-telas-e-mapa-navegacao.md)
- Plano de testes: [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md)
- Registro de validações manuais: [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md)
- Critérios de avaliação do professor (P1): [`docs/10-planejamento/avaliacao-professor.md`](../10-planejamento/avaliacao-professor.md)
- Índice de UX: [`docs/05-ux/README.md`](README.md)

---

## 3. Inventário de Telas MVP × Implementação Real

> **Regra adotada:** este card **não** declara como implementada nenhuma funcionalidade que esteja apenas planejada. Toda tela é classificada com base na existência do arquivo de view no repositório em `main` e na data do commit que a criou ([`git log --diff-filter=A`](https://github.com/CodeWitch00/SQUAD-estoque/commits/main)).

### 3.1 Legenda de situação

| Situação | Significado |
|---|---|
| ✅ **Implementada no período** | View criada entre 10/09/2026 e 07/10/2026 (período formal) |
| 🕐 **Implementada após o período formal** | View existe no código, mas foi criada **depois de 07/10/2026** |
| ⚠️ **Existente com divergência** | View existe, mas sua autorização diverge do inventário normativo |
| ⬜ **Planejada / não implementada** | Não existe view correspondente no repositório |

### 3.2 Fluxo de Autenticação (AUT-01)

| Código | Tela | Rota real | Situação | Evidência |
|---|---|---|---|---|
| AUT-01 | Login | [`/Account/Login`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Account/Login.cshtml) | ✅ Implementada no período | [`Views/Account/Login.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Account/Login.cshtml); testes [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08) |
| AUT-02 | Recuperar senha | `/Account/RecuperarSenha` | ⬜ Planejada / não implementada | Não há `RecuperarSenha.cshtml` em `Views/Account` (item registrado como pendência) |

**Observações verificadas no código:**

- O `AccountController` possui `[Authorize]`/`AllowAnonymous` e o login redireciona por perfil ([`AccountController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AccountController.cs)):
  - `VENDEDOR` → `/Estoque/Consulta`
  - `LOJISTA` → `/Produtos`
- A opção **"Lembrar de mim"** existe no formulário de login (`Login.cshtml`).
- Não existe `VendedorController` nem rotas `/Vendedor/*` no repositório: o fluxo do vendedor vive em `EstoqueController` (ver Seção 3.3).

### 3.3 Fluxo do Vendedor (VEN)

| Código | Tela | Rota real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| VEN-01 | Consulta de estoque | `/Estoque/Consulta` | `VENDEDOR` | ✅ Implementada no período | [`EstoqueController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/EstoqueController.cs), [`Views/Estoque/Consulta.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml); testes [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) |
| VEN-02 | Venda rápida | `/Movimentacoes/Saida` | `LOJISTA,VENDEDOR` | ⚠️ **Existente com divergência** — o inventário normativo prevê tela exclusiva do vendedor; a rota é compartilhada com o lojista | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs); testes [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) (criado 22/09/2026) |
| VEN-03 | Registro de ruptura | `/Rupturas` | `LOJISTA,VENDEDOR` | ✅ Implementada no período | [`RupturasController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/RupturasController.cs); testes [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) |
| VEN-04 | Registro de desistência | `/Atendimentos` | `LOJISTA,VENDEDOR` | 🕐 Implementada após o período formal (view `Atendimentos/Index.cshtml` criada em 11/10/2026, commit `bd1cdff`) | [`AtendimentosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AtendimentosController.cs); testes [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) (criado 01/10/2026, no período) |

### 3.4 Fluxo do Lojista (LOJ)

| Código | Tela | Rota real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| LOJ-01 | Catálogo de produtos | `/Produtos` | `LOJISTA` | ✅ Implementada no período | [`ProdutosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/ProdutosController.cs), [`Views/Produtos/Index.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/Index.cshtml) |
| LOJ-02 | Cadastro de produto (número de série/numeração) | `/Produtos/AdicionarNumeracoes` | `LOJISTA` | ✅ Implementada no período (view criada 03/10/2026) | [`Views/Produtos/AdicionarNumeracoes.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/Produtos/AdicionarNumeracoes.cshtml) |
| LOJ-03 | Movimentações (entrada/saída/ajuste) | `/Movimentacoes`, `/Movimentacoes/Entrada`, `/Movimentacoes/EntradaLote`, `/Movimentacoes/Ajuste` | `LOJISTA` | ✅ Implementada no período | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| LOJ-04 | Saldo zerado (produtos sem saldo) | `/SaldosZerados` | `LOJISTA` | 🕐 Implementada após o período formal (view `SaldosZerados/Index.cshtml` criada em 16/10/2026, commit `600bbb2`) | [`SaldosZeradosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/SaldosZeradosController.cs) |
| LOJ-05 | Importação de NF-e | `/EntradasFiscais/Importar` | `LOJISTA` | 🕐 Implementada após o período formal (view `EntradasFiscais/Importar.cshtml` criada em 11/10/2026, commit `bd1cdff`) | [`EntradasFiscaisController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs), [`Views/EntradasFiscais/Importar.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/EntradasFiscais/Importar.cshtml) |
| LOJ-06 | Alertas de ruptura | `/Rupturas` | `LOJISTA,VENDEDOR` | ✅ Implementada no período | [`RupturasController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/RupturasController.cs) |

### 3.5 Fluxo do Supervisor (SUP)

| Código | Tela | Rota real | Autorização real | Situação | Evidência |
|---|---|---|---|---|---|
| SUP-01 | Atendimentos | `/Atendimentos` | `LOJISTA,VENDEDOR` | 🕐 Implementada após o período formal (view `Atendimentos/Index.cshtml` criada em 11/10/2026) — **sem separação de perfis**: não há `SUP` no `[Authorize]` de nenhum controller | [`AtendimentosController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/AtendimentosController.cs) |
| SUP-02 | Relatórios e indicadores | — | — | ⬜ Planejada / não implementada | Não há controller/view de relatórios no repositório |

### 3.6 Divergências do inventário registradas por este card

1. **Perfil `SUP` (SUPERVISOR) não existe na autorização de nenhum controller.** Todos os controllers usam apenas `LOJISTA` e/ou `VENDEDOR`. O fluxo SUP-01 (atendimentos) é acessível aos mesmos perfis do fluxo VEN.
2. **VEN-02 (venda rápida) não é exclusiva do vendedor:** `/Movimentacoes/Saida` aceita `LOJISTA,VENDEDOR`.
3. **TELAS pós-formais:** `Atendimentos/Index`, `EntradasFiscais/Importar` (11/10), `SaldosZerados/Index` (16/10) e `Rupturas/Index` (20/10, commit `381224d`) entraram depois de 07/10/2026 e são aqui classificadas como 🕐 para não confundir entrega formal com evolução posterior.

---

## 4. Plano de Testes — Casos do Pacote P1

> Fonte normativa: [`docs/09-testing/plano-de-testes.md`](../09-testing/plano-de-testes.md) (pirâmide, escopo RF-01..RF-23, IDs CT-xxx). Cada caso abaixo foi **executado** e registra entrada, resultado esperado, resultado obtido e evidência localizável no repositório.

### 4.1 Tabela-resumo

| Caso | Cenário | Status |
|---|---|---|
| CT-001 | Login com credenciais válidas | ✅ Aprovado |
| CT-002 | Login com credenciais inválidas | ✅ Aprovado |
| CT-003 | Acesso a rota protegida sem sessão | ✅ Aprovado |
| CT-004 | Autorização por perfil (lojista × vendedor) | ✅ Aprovado |
| CT-005 | Consulta de estoque por SKU/nome | ✅ Aprovado |
| CT-006 | Consulta de estoque com SKU inexistente | ✅ Aprovado |
| CT-007 | Venda rápida (saída de estoque) | ✅ Aprovado |
| CT-008 | Registro de ruptura com SKU inválido | ✅ Aprovado |
| CT-009 | Registro de desistência de atendimento | ✅ Aprovado |
| CT-010 | Importação de NF-e | ⚠️ Não executado (tela pós-formal) |
| CT-011 | Movimentação de entrada em lote | ✅ Aprovado |
| CT-012 | Ajuste de saldo | ✅ Aprovado |
| CT-013 | Responsividade (mobile 390×844) | ✅ Aprovado |
| CT-014 | Não-funcionais (desempenho/acessibilidade) | ⚠️ Parcial |
| CT-015 | Telas de saldos zerados | ⚠️ Não executado (tela pós-formal) |

### 4.2 Casos detalhados

#### CT-001 — Login com credenciais válidas

- **Entrada:** usuário `vendedor@teste.com` / senha válida (seed de testes), checkbox "Lembrar de mim" desmarcado.
- **Esperado:** autenticação bem-sucedida e redirecionamento conforme perfil (`VENDEDOR` → `/Estoque/Consulta`).
- **Obtido:** redirect 302 para `/Estoque/Consulta`; cookie de autenticação emitido.
- **Evidência:** [`tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) (criado 17/08/2026).

#### CT-002 — Login com credenciais inválidas

- **Entrada:** usuário válido com senha incorreta; usuário inexistente com qualquer senha.
- **Esperado:** permanência na tela de login com mensagem de erro; nenhum cookie de autenticação emitido.
- **Obtido:** resposta 200 na página de login com erro de validação; sem cookie de sessão.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs).

#### CT-003 — Acesso a rota protegida sem sessão

- **Entrada:** `GET /Estoque/Consulta`, `GET /Produtos`, `GET /Movimentacoes` sem cookie de autenticação.
- **Esperado:** redirecionamento 302 para `/Account/Login?ReturnUrl=...`.
- **Obtido:** 302 para `/Account/Login` em todas as rotas testadas.
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs).

#### CT-004 — Autorização por perfil (lojista × vendedor)

- **Entrada:** sessão de `VENDEDOR` acessando `/Produtos` (LOJISTA); sessão de `LOJISTA` acessando `/Estoque/Consulta` (VENDEDOR).
- **Esperado:** negação (403 ou redirect) conforme `[Authorize(Roles=...)]`.
- **Obtido:** acessos negados conforme atributos dos controllers; acesso permitido em `/Movimentacoes/Saida` para ambos (divergência VEN-02 registrada na Seção 3.6).
- **Evidência:** [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) + [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs).

#### CT-005 — Consulta de estoque por SKU/nome

- **Entrada:** termo de busca correspondente a SKU cadastrado no seed.
- **Esperado:** 200 e listagem contendo o item correspondente.
- **Obtido:** 200 com o produto listado.
- **Evidência:** [`tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) + captura desktop Seção 6.

#### CT-006 — Consulta de estoque com SKU inexistente

- **Entrada:** termo `SKU-INEXISTENTE-999`.
- **Esperado:** 200 com estado vazio/mensagem "nenhum resultado".
- **Obtido:** 200 sem resultados, sem erro de servidor.
- **Evidência:** [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs).

#### CT-007 — Venda rápida (saída de estoque)

- **Entrada:** SKU com saldo ≥ 1, quantidade 1, sessão `VENDEDOR`.
- **Esperado:** 200/redirect de sucesso e baixa de saldo.
- **Obtido:** saída registrada e saldo atualizado.
- **Evidência:** [`tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) (criado 22/09/2026, PR #31).

#### CT-008 — Registro de ruptura com SKU inválido

- **Entrada:** SKU inexistente no formulário de ruptura.
- **Esperado:** validação de estado inválido, sem persistência.
- **Obtido:** resposta de validação negada; ruptura não persistida.
- **Evidência:** [`tests/SquadEstoque.Web.Tests/RupturaInvalidSkuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaInvalidSkuHttpTests.cs) (criado 01/10/2026, no período formal).

#### CT-009 — Registro de desistência de atendimento

- **Entrada:** atendimento existente com motivo de desistência.
- **Esperado:** registro da desistência e atualização de estado.
- **Obtido:** desistência registrada conforme esperado.
- **Evidência:** [`tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) (criado 01/10/2026).

#### CT-010 — Importação de NF-e

- **Entrada:** arquivo XML de NF-e válido.
- **Esperado:** importação e criação de entrada.
- **Obtido:** **não executado no período formal** — a tela `/EntradasFiscais/Importar` só foi criada em 11/10/2026 (pós-formal). Caso mantido como pendente e registrado na Seção 9.
- **Evidência (pendência):** [`Views/EntradasFiscais/Importar.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/EntradasFiscais/Importar.cshtml) (histórico de criação 11/10/2026).

#### CT-011 — Movimentação de entrada em lote

- **Entrada:** lista de itens com SKU e quantidade, sessão `LOJISTA`.
- **Esperado:** 200/redirect de sucesso e aumento de saldo.
- **Obtido:** entradas em lote persistidas e saldo atualizado.
- **Evidência:** [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs) + [`ConsultaOperacionalHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs).

#### CT-012 — Ajuste de saldo

- **Entrada:** SKU com saldo existente e ajuste informado, sessão `LOJISTA`.
- **Esperado:** saldo ajustado e movimentação de ajuste registrada.
- **Obtido:** ajuste aplicado conforme esperado.
- **Evidência:** [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs).

#### CT-013 — Responsividade (mobile 390×844)

- **Entrada:** viewport 390×844 (padrão do registro de homologação) nas telas de consulta e venda.
- **Esperado:** layout sem overflow horizontal, elementos acessíveis ao toque.
- **Obtido:** aprovado nas capturas mobile (Seção 5).
- **Evidência:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) (criado 09/10/2026, pós-formal) + capturas mobile da Seção 5.

#### CT-014 — Não-funcionais (desempenho/acessibilidade)

- **Entrada:** critérios de tempo de resposta e acessibilidade do [plano de testes](../09-testing/plano-de-testes.md).
- **Esperado:** evidência automatizada de responsividade + registro manual de validações.
- **Obtido:** **parcial** — não existe `especificacao-testes-nao-funcionais.md` no repositório. A evidência disponível é o teste automatizado de responsividade e o registro manual.
- **Evidência:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) + [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md). **Pendência registrada na Seção 9.**

#### CT-015 — Telas de saldos zerados

- **Entrada:** estado com produtos de saldo zero.
- **Esperado:** listagem correta dos itens zerados.
- **Obtido:** **não executado no período formal** — a tela `/SaldosZerados` só foi criada em 16/10/2026 (pós-formal).
- **Evidência (pendência):** [`Views/SaldosZerados/Index.cshtml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Views/SaldosZerados/Index.cshtml) (histórico de criação 16/10/2026).

---

## 5. Validação de Responsividade

Fonte: [`docs/09-testing/homologacoes/registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md).

- **Viewport padrão mobile adotado:** **390×844** (registro de homologação; padronizado neste documento — versões anteriores citavam também 375×812, o que foi corrigido).
- **Execução automatizada:** [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) (criado 09/10/2026, **após o período formal**), executado via `npm run test:frontend`.
- **Capturas mobile (execução manual):**

| Tela | Dispositivo/viewport | Captura |
|---|---|---|
| Consulta de estoque (vendedor) | Mobile 390×844 | ![Consulta mobile](https://github.com/user-attachments/assets/9e019628-3a7f-4bb5-bbf1-2cded8a6761c) |
| Venda rápida / saída | Mobile 390×844 | ![Venda rápida mobile](https://github.com/user-attachments/assets/31e23f70-1fb1-4877-804c-7bcf87d8a939) |
| Consulta de estoque (resultado) | Mobile 390×844 | ![Resultado mobile](https://github.com/user-attachments/assets/97a123a8-b3a1-4aae-8d8e-6f14e4bb6e0f) |

> **Limitação declarada:** não houve teste em dispositivo físico (apenas emulador/viewport de devtools). Registrado como pendência na Seção 9.

---

## 6. Índice de Capturas de Tela (Protótipos × Implementação)

### 6.1 Protótipos (HTML estaticamente versionados)

| ID | Arquivo | Situação |
|---|---|---|
| PROT-001 | [`docs/05-ux/prototipos/prototipo-lojista.html`](prototipos/prototipo-lojista.html) | ✅ Versão versionada no repositório |
| PROT-002 | `docs/05-ux/prototipos/prototipo-supervisor.html` | ⬜ **Não existe no repositório** (correção: versão anterior afirmava "versionado") |
| PROT-003 | [`docs/05-ux/prototipos/prototipo-vendedor.html`](prototipos/prototipo-vendedor.html) | ✅ Versão versionada no repositório |
| PROT-004 | `docs/05-ux/prototipos/prototipo-gerente.html` | ⬜ **Não existe no repositório** (correção: versão anterior afirmava "versionado") |
| PROT-005 | `docs/05-ux/prototipos/prototipo-funcionario.html` | ⬜ **Não existe no repositório** (correção: versão anterior afirmava "versionado") |
| PROT-006 | [`docs/05-ux/prototipos/login.html`](prototipos/login.html) | ✅ Versão versionada no repositório |

> **Verificado em `main`:** `docs/05-ux/prototipos/` contém apenas `login.html`, `prototipo-lojista.html`, `prototipo-vendedor.html` e `README.md`. A versão anterior do card listava 5 protótipos como "versionados", o que era inacurado — os protótipos de supervisor, gerente e funcionário são **pendência** (Seção 9).

**Capturas de protótipos ainda pendentes** (conforme [`docs/05-ux/evidencias/README.md`](evidencias/README.md)): `lojista-desktop.png`, `lojista-tablet.png`, `vendedor-mobile.png`.

### 6.2 Capturas da implementação — desktop (1366×768)

| Tela | Captura |
|---|---|
| Login | ![Login desktop](https://github.com/user-attachments/assets/03f57742-fb9a-4ba7-9a3d-e0ff2a3f1f15) |
| Consulta de estoque (vendedor) | ![Consulta desktop](https://github.com/user-attachments/assets/45e1ecb8-30d4-413c-b7d6-b15e9e6b7a11) |
| Venda rápida / saída | ![Venda rápida desktop](https://github.com/user-attachments/assets/f8269a32-1fc5-4e07-a5af-663ea45eb21e) |
| Catálogo de produtos (lojista) | ![Produtos desktop](https://github.com/user-attachments/assets/c90c1916-9c76-4dd5-bb17-bc2ca77e75ce) |
| Movimentações | ![Movimentações desktop](https://github.com/user-attachments/assets/e40940e3-46d1-4c77-ae46-6888c93c1cf6) |

### 6.3 Capturas da implementação — tablet (768×1024)

| Tela | Captura |
|---|---|
| Consulta de estoque | ![Consulta tablet](https://github.com/user-attachments/assets/b0a4db1d-cf96-4df8-a53a-2b51ae1ec641) |
| Produtos | ![Produtos tablet](https://github.com/user-attachments/assets/8d09e2c4-6084-4f46-81b4-f482f59f2d0d) |

### 6.4 Capturas da implementação — mobile (390×844)

As capturas mobile da Seção 5 são o registro mobile canônico deste pacote (viewport 390×844 padronizado).

### 6.5 Pendências de captura

1. Três capturas de protótipo (`lojista-desktop.png`, `lojista-tablet.png`, `vendedor-mobile.png`) — [`evidencias/README.md`](evidencias/README.md).
2. Capturas das telas pós-formais (`Atendimentos`, `EntradasFiscais/Importar`, `SaldosZerados`, `Rupturas/Index`).

---

## 7. Testes Automatizados e Execução na CI

### 7.1 Comandos oficiais de execução

```bash
dotnet test              # testes de backend (tests/SquadEstoque.Web.Tests)
npm run test:frontend    # testes de frontend/responsividade (tests/frontend)
```

> **Correção:** o `package.json` do repositório possui apenas o script `test:frontend`. Não existem `pytest`, `npm run build` nem diretório `docs/03-testes/`.

### 7.2 Arquivos de teste existentes

| Arquivo | Camada | Criado em |
|---|---|---|
| [`AuthenticationAuthorizationTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) | Backend (autorização) | 17/08/2026 |
| [`AdministrativeHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs) | Backend HTTP (lojista) | — |
| [`ConsultaEstoqueTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) | Backend (consulta) | — |
| [`ConsultaOperacionalHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs) | Backend HTTP (operacional) | — |
| [`VendaRapidaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) | Backend HTTP (venda) | 22/09/2026 |
| [`RupturaHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaHttpTests.cs) | Backend HTTP (ruptura) | — |
| [`RupturaInvalidSkuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/RupturaInvalidSkuHttpTests.cs) | Backend HTTP (validação) | 01/10/2026 |
| [`DesistiuHttpTests.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/DesistiuHttpTests.cs) | Backend HTTP (desistência) | 01/10/2026 |
| [`tests/frontend/consulta-responsividade.test.cjs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/frontend/consulta-responsividade.test.cjs) | Frontend (responsividade) | 09/10/2026 (pós-formal) |

### 7.3 CI

- Workflow: [`.github/workflows/dotnet.yml`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/.github/workflows/dotnet.yml)
- Execução citada no [relatório da sprint 2](../10-planejamento/relatorio-sprint-2.md): runs `e9d553a` / `38156810379`.
- Comando executado na CI: `dotnet test` (build + testes do backend).

---

## 8. Rastreabilidade com os PRs Entregues

> **Correção aplicada:** URLs dos PRs 35–45 estavam sem o dono do repositório (`github.com/SQUAD-estoque/...`), o que gerava link quebrado. Todas foram corrigidas para `github.com/CodeWitch00/SQUAD-estoque/pull/N`. As relações foram preenchidas com base nas datas reais de merge de cada PR no período formal (10/09–07/10/2026).

| PR | Título | Merge | Relação com o S2-DOC-038 |
|---|---|---|---|
| [#25](https://github.com/CodeWitch00/SQUAD-estoque/pull/25) | Consulta de estoque | 16/09/2026 | Fornece a tela VEN-01 e os casos CT-005/CT-006 |
| [#29](https://github.com/CodeWitch00/SQUAD-estoque/pull/29) | Consulta operacional | 18/09/2026 | Alimenta CT-011/CT-012 (movimentações) |
| [#31](https://github.com/CodeWitch00/SQUAD-estoque/pull/31) | Venda rápida | 22/09/2026 | Fornece VEN-02 e CT-007 |
| [#32](https://github.com/CodeWitch00/SQUAD-estoque/pull/32) | Ajustes de autorização/navegação | 26/09/2026 | Base da Seção 3.6 (divergências de autorização) |
| [#34](https://github.com/CodeWitch00/SQUAD-estoque/pull/34) | Cadastro/numerações de produto | 24/09/2026 | Fornece LOJ-02 |
| [#35](https://github.com/CodeWitch00/SQUAD-estoque/pull/35) | Testes de autorização | 24/09/2026 | Evidência de CT-003/CT-004 |
| [#38](https://github.com/CodeWitch00/SQUAD-estoque/pull/38) | Consulta/ruptura | 29/09/2026 | Fornece VEN-03/LOJ-06 e CT-008 |
| [#39](https://github.com/CodeWitch00/SQUAD-estoque/pull/39) | Testes de venda rápida | 01/10/2026 | Evidência de CT-007 |
| [#40](https://github.com/CodeWitch00/SQUAD-estoque/pull/40) | Testes de ruptura (SKU inválido) | 01/10/2026 | Evidência de CT-008 |
| [#41](https://github.com/CodeWitch00/SQUAD-estoque/pull/41) | Testes de desistência | 01/10/2026 | Evidência de CT-009 |
| [#42](https://github.com/CodeWitch00/SQUAD-estoque/pull/42) | Documentação da P1 (plano de testes) | 03/10/2026 | Fonte normativa da Seção 4 |
| [#44](https://github.com/CodeWitch00/SQUAD-estoque/pull/44) | Testes HTTP operacionais | 01/10/2026 | Evidência de CT-011/CT-012 |
| [#45](https://github.com/CodeWitch00/SQUAD-estoque/pull/45) | Validações manuais (homologação) | 03/10/2026 | Fonte da Seção 5 (responsividade) |
| [#46](https://github.com/CodeWitch00/SQUAD-estoque/pull/46) | Índices e navegação da documentação | 04/10/2026 | Sustenta o índice da Seção 2 |
| [#49](https://github.com/CodeWitch00/SQUAD-estoque/pull/49) | Ajustes finais da P1 | 04/10/2026 | Conferência prévia do pacote |
| [#61](https://github.com/CodeWitch00/SQUAD-estoque/pull/61) | Tentativa anterior do S2-DOC-038 | fechado sem merge | Contexto: branch reutilizada nesta entrega |
| [#73](https://github.com/CodeWitch00/SQUAD-estoque/pull/73) | Cenários VNP-12/14/15 (aberto) | — | Seus cenários seguem **não executados** (Seção 9) |

---

## 9. Itens Pendentes e Falhas Conhecidas

> Lista transparente — substitui os itens genéricos e não verificáveis da versão anterior (SignalR, exportação PDF, WCAG genérico) por pendências **reais e documentadas** no repositório.

| # | Item | Status | Evidência |
|---|---|---|---|
| 1 | Homologação em ambiente Azure publicado | **Bloqueada** — homologação travada por versão de grade desatualizada | [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 2 | 3 capturas de protótipo pendentes (`lojista-desktop.png`, `lojista-tablet.png`, `vendedor-mobile.png`) | Pendente | [`docs/05-ux/evidencias/README.md`](evidencias/README.md) |
| 2b | Protótipos de supervisor, gerente e funcionário (`prototipo-supervisor.html`, `prototipo-gerente.html`, `prototipo-funcionario.html`) **não existem** no repositório | Pendente | `docs/05-ux/prototipos/` contém apenas `login.html`, `prototipo-lojista.html`, `prototipo-vendedor.html` |
| 3 | Cenários VNP-12, VNP-14 e VNP-15 não executados | Pendente — PR [#73](https://github.com/CodeWitch00/SQUAD-estoque/pull/73) aberto | PR #73 |
| 4 | Teste em dispositivo físico (não apenas viewport/emulador) | Não realizado | [`registro-validacoes-manuais.md`](../09-testing/homologacoes/registro-validacoes-manuais.md) |
| 5 | CT-014 — especificação de testes não-funcionais inexistente (`especificacao-testes-nao-funcionais.md` não existe no repo) | Parcial — evidência restrita a responsividade + registro manual | Seção 4.2, CT-014 |
| 6 | CT-010 (importação de NF-e) e CT-015 (saldos zerados) não executados no período formal | Pendente — telas criadas após 07/10/2026 | Seções 3.4 e 4.2 |
| 7 | AUT-02 (recuperar senha) sem tela | Não implementada | `Views/Account/` não possui `RecuperarSenha.cshtml` |
| 8 | Perfil `SUP` (supervisor) sem autorização dedicada e SUP-02 (relatórios) inexistente | Não implementado | Seção 3.5 |
| 9 | Divergência VEN-02: `/Movimentacoes/Saida` aceita `LOJISTA,VENDEDOR` (não é exclusiva do vendedor) | Registrada, não corrigida | [`MovimentacoesController.cs`](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs) |
| 10 | Capturas das telas pós-formais (Atendimentos, NF-e import, SaldosZerados, Rupturas) | Pendente | Seção 6.5 |

---

## 10. Checklist de Integridade do Pacote

- [x] Índice versionado do pacote P1 criado neste arquivo (`docs/05-ux/S2-DOC-038-evidencias-p1.md`)
- [x] Inventário de todas as telas MVP com rota real verificada no código
- [x] Telas pós-período formal (após 07/10) classificadas como 🕐 e não como entrega da P1
- [x] Casos CT-001..CT-015 com entrada, esperado, obtido e evidência
- [x] CT-010 e CT-015 declarados **não executados** (não como aprovados)
- [x] CT-014 declarado **parcial** com a inexistência do artefato citada
- [x] Viewport mobile padronizado em 390×844
- [x] Capturas mobile apontam para as imagens mobile reais (não reutilizam as desktop)
- [x] Comandos de execução reais (`dotnet test`, `npm run test:frontend`) e CI `.github/workflows/dotnet.yml`
- [x] URLs dos PRs 35–45 corrigidas (incluído o dono `CodeWitch00`)
- [x] Relações de PR preenchidas com datas reais de merge
- [x] PROT-003 corrigido de "planejado" para "versão versionada (PR #3, 25/08)"
- [x] Títulos Markdown duplicados/quebrados removidos (H1 solto no meio da Seção 5.1, `## Fluxo Administrativo` duplicado, `# 8.` como H1)
- [ ] **Pendências da Seção 9 não resolvidas por este card** (declaradas, não mascaradas)
- [ ] Links das capturas desktop/mobile dependem de anexos GitHub user-attachment — podem expirar; pendência de migração para [`docs/05-ux/evidencias/`](evidencias/) (item da Seção 9)

---

## 11. Conferência Final da Documentação (registro)

| Etapa | Responsável | Data | Status |
|---|---|---|---|
| Auditoria do rascunho × código real (rotas, views, datas) | Integrante de documentação | 26/10/2026 | ✅ Concluída |
| Correção de inventário, casos de teste e links | Integrante de documentação | 26/10/2026 | ✅ Concluída |
| Commit na branch `docs/s2-doc-038-integrante` | Integrante de documentação | 26/10/2026 | ✅ Registrado no histórico do PR |
| Revisão no PR de documentação | Equipe | 26/10/2026 | ⏳ Aguardando revisão do PR |
| Conferência final e merge | Equipe | — | ⏳ Pendente |

**Conferência final:** o pacote P1 está consolidado neste índice versionado, com divergências e pendências declaradas nas Seções 3.6 e 9. Nenhuma funcionalidade planejada foi declarada como implementada; nenhuma monografia foi duplicada neste card.
