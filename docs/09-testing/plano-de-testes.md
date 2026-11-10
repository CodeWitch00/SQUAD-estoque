# Plano de testes SQUAD Estoque

## 1. Identificação

| Campo | Valor |
|---|---|
| Projeto | SQUAD Estoque |
| Versão do documento | 2.1 |
| Status | Estratégia vigente; resultados da Sprint 3 registrados por caso; homologação publicada pendente |
| Última atualização | 09/11/2026 |
| Atualização desta versão | S3-DOC-036 — resultados completos da Sprint 3 |
| Tipo | Aplicação web ASP.NET Core MVC |
| Estratégia | Pirâmide de testes |
| Perfis | `LOJISTA` e `VENDEDOR` |
| Responsáveis | Equipe de desenvolvimento e qualidade |
| Fontes | SRS, casos de uso, domínio, arquitetura, UX, código implementado e execuções da Sprint 3 |
| Revisão do plano | A cada alteração de requisito, regra de negócio ou fluxo crítico |

Este é o plano geral de qualidade do projeto e reúne também os casos de autenticação, sessão e perfis.

## 2. Objetivo

Definir a estratégia oficial para planejar e executar os testes do SQUAD Estoque, garantindo rastreabilidade entre requisitos, cenários, entradas e resultados esperados sem duplicar no repositório os resultados já preservados pela CI e pelo histórico Git.

O documento atende ao critério da P1 de plano de testes estruturado, com casos, entradas, saídas esperadas e cenários, e acrescenta controles de execução e revisão para evitar resultados presumidos.

O plano procura responder:

- o que será testado;
- em qual nível da pirâmide cada comportamento deve ser validado;
- quais entradas e resultados são esperados;
- quais ferramentas e ambientes serão usados;
- o que já possui automação e o que ainda é lacuna;
- quais critérios permitem liberar uma versão.

## 3. Referências

- [Especificação de requisitos](../02-requisitos/srs.md)
- [Casos de uso](../02-requisitos/casos-de-uso.md)
- [Regras do domínio](../01-negocio/dominio.md)
- [Arquitetura](../04-arquitetura/arquitetura.md)
- [Inventário de telas e navegação](../05-ux/inventario-telas-e-mapa-navegacao.md)
- [Preparação e fluxo de desenvolvimento](../../CONTRIBUTING.md)
- [Registro consolidado de validações manuais](homologacoes/registro-validacoes-manuais.md)
- [Execuções da Sprint 3](execucoes/sprint-3/)
- [P95 da consulta — S3-QA-028](s3-qa-028-p95-consulta.md)
- [Última atualização do saldo — S3-QA-025](s3-qa-025-ultima-atualizacao.md)
- [Índices de consulta — S3-DB-026](s3-db-026-indices-consulta.md)
- [Evidências das visões do lojista — S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md)
- [Regressão E2E — S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)
- [Relatório da Sprint 1](../10-planejamento/relatorio-sprint-1.md)
- [Relatório da Sprint 2](../10-planejamento/relatorio-sprint-2.md)
- [GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions)
- [Testes automatizados](../../tests/SquadEstoque.Web.Tests/)

## 4. Escopo

### 4.1 Incluído

- RF-01 a RF-23;
- RNF-01 a RNF-07;
- RN-01 a RN-07;
- UC-01 a UC-12 e UC-S1 a UC-S5;
- autenticação, sessão e autorização;
- cadastro de Produto e grade de SKUs;
- entrada, saída e ajuste de estoque;
- consulta de estoque pelo vendedor;
- venda, ruptura e desistência;
- visões de saldo zerado e histórico de rupturas;
- persistência, concorrência, segurança, desempenho e usabilidade crítica;
- regressão das rotas e comportamentos já implementados.

### 4.2 Fora do escopo

Conforme o SRS, não serão testados como funcionalidades do SQUAD:

- emissão de nota fiscal;
- funções de PDV, financeiro ou ERP;
- integração automática com sistemas externos;
- operação multi-loja;

## 5. Estratégia em pirâmide

```text
                         Poucos — 5% a 10%
                 Testes ponta a ponta (E2E)
              Fluxos críticos em navegador real
            --------------------------------------
                     Moderados — 25% a 35%
                    Testes de integração
          MVC + autenticação + EF Core + SQLite
        --------------------------------------------
                         Muitos — 60% a 70%
                        Testes unitários
          Regras, validações, cálculos e estados
```

As faixas são metas de distribuição dos cenários automatizados, não cotas rígidas. Um comportamento deve ser testado no nível mais baixo capaz de fornecer confiança suficiente. Não se repete a mesma combinação de entradas em todas as camadas sem uma justificativa de risco.

### 5.1 Base — testes unitários

Devem ser numerosos, rápidos, determinísticos e independentes de navegador, rede e banco real. São apropriados para:

- validações de Models e ViewModels;
- classificação visual do saldo;
- cálculos de entrada, saída e ajuste;
- rejeição de quantidade e motivo inválidos;
- regras de decisão extraídas para funções testáveis;
- formatação e filtros isoláveis.

Meta operacional: cada teste deve executar em milissegundos e não compartilhar estado com outro teste.

### 5.2 Meio — testes de integração

Validam a colaboração entre componentes reais do projeto:

- Controllers MVC e Razor Views;
- filtros de autenticação e autorização;
- cookies e antiforgery;
- EF Core, constraints e SQLite em memória;
- transações e persistência de entidades relacionadas;
- códigos HTTP, redirecionamentos e efeitos no banco.

Usar `WebApplicationFactory` quando o comportamento passar pela aplicação HTTP. Usar `EstoqueContext` com SQLite em memória para constraints e persistência. O provedor `InMemory` do EF Core não deve substituir SQLite em testes de integridade relacional.

### 5.3 Topo — testes ponta a ponta

Devem ser poucos e cobrir apenas jornadas de alto valor em navegador real:

- vendedor consulta e registra uma venda;
- vendedor registra uma ruptura;
- lojista cadastra produto e grade e registra entrada;
- lojista ajusta saldo com motivo;
- controle de acesso após login e logout.

E2E não deve ser usado para enumerar todas as validações de campo. Essas combinações pertencem às camadas unitária e de integração.

### 5.4 Testes transversais

Desempenho, disponibilidade, compatibilidade, segurança e usabilidade complementam a pirâmide. Eles possuem ferramentas e cadências próprias e não devem ser apresentados como testes unitários.

### 5.5 Separação por forma de execução

Nível de teste e forma de execução são classificações diferentes. Um teste de integração, por exemplo, pode ser automatizado. O catálogo e o relatório devem informar as duas classificações sem misturá-las.

| Grupo oficial | Identificação no caso | Escopo | Registro esperado |
|---|---|---|---|
| Testes automatizados | Forma `Automatizado`; níveis `Unitário`, `Integração` ou `E2E` | Código executável localmente e na CI, incluindo UT, CT e IT já implementados | Nome do teste, comando/job, commit e artefato do runner |
| Testes de integração | Nível `Integração`; forma `Automatizado` ou `Manual assistido` | MVC, HTTP, autenticação, persistência, banco e colaboração entre componentes | Resposta, efeitos persistidos e ausência de alteração parcial em falhas |
| Validações manuais | Forma `Manual`; níveis `E2E`, `Aceite` ou `Não funcional` | Usabilidade, inspeção visual, responsividade, compatibilidade e smoke de publicação | Executor, ambiente, passos, observado e evidência anexada |

Os casos unitários estão no catálogo UT; os casos de integração, nos catálogos CT, IT e ADM; as jornadas E2E, na seção 11; e as validações manuais e não funcionais, na seção 12 e no [registro consolidado](homologacoes/registro-validacoes-manuais.md). A existência de automação não dispensa uma validação manual quando o critério depender de percepção visual, dispositivo real ou ambiente publicado.

## 6. Ferramentas

| Finalidade | Ferramenta adotada ou indicada | Situação |
|---|---|---|
| Framework de automação .NET | xUnit | Adotado |
| Execução e descoberta | Microsoft.NET.Test.Sdk e xunit.runner.visualstudio | Adotado |
| Aplicação HTTP em teste | `Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory` | Adotado |
| Persistência isolada | EF Core + SQLite em memória | Adotado |
| Hash de senha | BCrypt.Net | Adotado |
| Visualização no VS Code | C# Dev Kit / Test Explorer | Indicado |
| E2E em navegador | Playwright para .NET | Parcial: validação visual automatizada em S3-FE-031; jornadas S3-QA-030 executadas manualmente assistidas |
| Carga e P95 | k6 ou NBomber; linha de base local em TestServer com xUnit | Parcial: RNF-02 medido localmente em S3-QA-028; carga de homologação pendente |
| Inspeção manual HTTP | DevTools do navegador | Adotado para validação complementar |
| Integração contínua | GitHub Actions | Adotado |

Não adicionar uma ferramenta nova quando a infraestrutura atual já consegue provar o comportamento com clareza.

## 7. Ambientes e dados

### 7.1 Ambientes

| Ambiente | Uso | Banco | Observações |
|---|---|---|---|
| Teste automatizado local/CI | Unitários e integração | SQLite em memória | Isolado e recriado por execução |
| Desenvolvimento local | Exploração e homologação inicial | SQLite local | Seed de desenvolvimento permitido |
| Homologação | E2E, compatibilidade, desempenho e aceite | Base exclusiva | Nunca usar dados reais de clientes |
| Produção | Monitoramento e smoke test não destrutivo | Base de produção | Não executar casos que alterem estoque sem autorização operacional |

### 7.2 Massa mínima

| ID | Dado | Valores |
|---|---|---|
| USR-LOJ-01 | Lojista ativo | `lojista@squad.com`, perfil `LOJISTA` |
| USR-VEN-01 | Vendedor ativo | `vendedor@squad.com`, perfil `VENDEDOR` |
| USR-INV-01 | Usuário inexistente | `usuario.invalido@example.test` |
| PRD-01 | Produto com grade | Tênis Teste, numerações 37, 38, 39 e 40 |
| SKU-00 | Indisponível | saldo 0 |
| SKU-01 | Último par | saldo 1 |
| SKU-02 | Disponível | saldo 2 ou maior |
| SKU-RUP | SKU com rupturas | pelo menos três ocorrências em datas controladas |

Senhas de teste só podem existir no seed de desenvolvimento/teste. Logs e evidências não devem exibir senha, hash ou cookie completo.

## 8. Modelo oficial dos casos de teste

Todo caso novo ou detalhado deve usar os campos abaixo. Os catálogos resumidos podem apresentar apenas as colunas necessárias para planejamento, desde que apontem para um caso detalhado ou preservem ID, requisito, cenário, entrada, resultado esperado e situação de cobertura.

| Campo obrigatório | Como preencher |
|---|---|
| Identificador | Código único e estável, como `UT-01`, `IT-18` ou `NFT-MOB-03` |
| Requisito | RF, RNF, RN, caso de uso e/ou risco rastreado |
| Cenário | Título objetivo, indicando comportamento positivo, negativo ou transversal |
| Forma de execução | `Automatizado`, `Manual` ou `Manual assistido` |
| Nível | `Unitário`, `Integração`, `E2E`, `Aceite` ou `Não funcional` |
| Pré-condição | Estado, perfil, massa, ambiente e dependências necessários |
| Passos | Sequência numerada e reproduzível de ações |
| Entrada | Valores, arquivos, requisições e variações usadas |
| Saída esperada | Resposta visível, código, estado persistido e efeitos que não devem ocorrer |
| Resultado obtido | Comportamento realmente observado; deixar `Não executado` durante o planejamento |
| Status de execução | Um dos estados definidos na seção 8.3 |
| Evidência | Link para job, log, relatório ou captura; usar `Não gerada` antes da execução |

### 8.1 Template reutilizável

```markdown
### <ID> — <cenário>

- **Requisito:** <RF/RNF/RN/UC/risco>
- **Forma de execução:** <Automatizado | Manual | Manual assistido>
- **Nível:** <Unitário | Integração | E2E | Aceite | Não funcional>
- **Pré-condição:** <estado inicial, perfil, massa e ambiente>
- **Passos:**
  1. <ação reproduzível>
  2. <ação reproduzível>
- **Entrada:** <dados e variações>
- **Saída esperada:** <resposta e efeitos esperados>
- **Resultado obtido:** Não executado
- **Status de execução:** Não executado
- **Evidência:** Não gerada
```

Planejar ou implementar um teste não significa executá-lo. Ao criar o caso, `Resultado obtido`, `Status de execução` e `Evidência` devem permanecer, respectivamente, `Não executado`, `Não executado` e `Não gerada`. Esses campos só podem ser alterados com base em uma execução real identificada por data, commit, ambiente e executor.

### 8.2 Padrão para automação

Usar Arrange, Act, Assert:

1. **Arrange:** preparar somente os dados exigidos pelo cenário;
2. **Act:** executar uma ação pública do sistema;
3. **Assert:** confirmar resposta e efeitos persistidos ou sua ausência.

Convenções:

- nomear pelo comportamento: `Saida_with_insufficient_balance_is_rejected`;
- manter um motivo principal de falha por teste;
- usar cliente novo por teste HTTP e preservar cookies apenas dentro do mesmo cenário;
- desabilitar redirecionamento automático quando o destino fizer parte do aceite;
- enviar token antiforgery em operações `POST`;
- controlar data/hora em vez de depender do relógio real;
- evitar `Thread.Sleep`, rede externa e ordem de execução;
- verificar estado antes e depois em operações de estoque;
- em falhas, confirmar também que nenhum registro parcial foi salvo;
- todo bug reproduzível corrigido deve receber teste de regressão no nível adequado.

### 8.3 Estados de especificação, cobertura e execução

O plano mantém três informações independentes para evitar a declaração indevida de execução:

| Dimensão | Valores permitidos | Significado |
|---|---|---|
| Situação de especificação | `Planejado`, `Parcial`, `Dependente`, `Especificado` | Maturidade documental do caso |
| Situação de automação | `Não automatizado`, `Parcial`, `Existente` | Existência e abrangência do código de teste |
| Status de execução | `Não executado`, `Em execução`, `Aprovado`, `Reprovado`, `Bloqueado`, `Não aplicável` | Resultado de uma execução concreta |

`Existente` nos catálogos significa que há código automatizado na baseline; não significa que ele foi executado no commit atual. Somente `Aprovado` ou `Reprovado` representa resultado, e esses estados exigem evidência da execução real. `Bloqueado` exige causa e condição de retomada; `Não aplicável` exige justificativa.

### 8.4 Cobertura obrigatória de cenários

| Categoria | Casos iniciais de referência |
|---|---|
| Positivos | UT-02, IT-01, IT-07, IT-10, E2E-01 a E2E-07 |
| Negativos e limites | UT-01, UT-03 a UT-05, CT-AUT-04 a CT-AUT-06, IT-02, IT-08, IT-11, IT-12 e IT-17 |
| Permissão e segurança | CT-AUT-09 a CT-AUT-15, IT-03, NFT-04 e NFT-05 |
| Responsividade e compatibilidade | NFT-01, NFT-07 e NFT-MOB-01 a NFT-MOB-04 |
| Desempenho | NFT-02 e NFT-PERF-01 a NFT-PERF-03 |
| Concorrência | IT-18 e ADM-SAI-04 |

Cada requisito deve possuir os cenários aplicáveis ao seu risco. Quando uma categoria não se aplicar, a decisão deve ser registrada na matriz ou no caso, sem criar teste artificial apenas para preencher a classificação.

## 9. Catálogo de testes da base da pirâmide

Legenda:

- **Existente:** há teste automatizado na baseline, sem afirmar execução no commit atual;
- **Planejado:** pode ser criado sobre funcionalidade existente e ainda está `Não executado`;
- **Dependente:** aguarda a implementação do requisito correspondente e ainda está `Não executado`.

| ID | Requisitos | Unidade/cenário | Entradas | Resultado esperado | Situação |
|---|---|---|---|---|---|
| UT-01 | RF-05, UC-07 | Campos obrigatórios do Produto | nome, marca, categoria e cor vazios | Modelo inválido, com erro para cada campo obrigatório | **Existente:** `Produto_requires_identification_fields` |
| UT-02 | RF-05 | Produto válido | quatro campos preenchidos com valores válidos | Modelo válido | Planejado |
| UT-03 | RF-06, RF-08, RN-01 | Normalização/validação de numeração | numeração vazia, válida e repetida | Vazia é rejeitada; válida é aceita; repetição é encaminhada à proteção de unicidade | Planejado |
| UT-04 | RF-09 | Quantidade de entrada | 0, negativa e positiva | Somente quantidade positiva é aceita | Planejado |
| UT-05 | RF-11, RN-02 | Cálculo de saída | saldo 5; saídas 2, 5 e 6 | Resultados 3, 0 e rejeição, respectivamente | Planejado |
| UT-06 | RF-15, UC-03 | Estado visual do SKU | saldos 0, 1 e 2 | `Indisponível`, `Último par` e `Disponível` | Dependente do módulo de consulta |
| UT-07 | RF-13, UC-02 | Tamanho mínimo da busca | textos com 0, 1 e 2 caracteres | Busca só é habilitada com pelo menos 2 caracteres | Dependente do módulo de consulta |
| UT-08 | RF-16 | Opções do resultado | estado após consulta válida | Apenas `Vendeu`, `Não tinha` e `Desistiu` são oferecidos | Dependente do módulo do vendedor |
| UT-09 | RF-17, RN-02 | Venda de um par | saldos 2, 1 e 0 | Novos saldos 1, 0 e rejeição | Dependente do módulo do vendedor |
| UT-10 | RF-19, RF-20 | Desistência | atendimento iniciado e ação `Desistiu` | Nenhuma mudança; nova consulta continua permitida | Dependente do módulo do vendedor |
| UT-11 | RF-23, UC-10 | Validação do motivo de ajuste | motivo vazio, espaços e texto válido | Vazios são rejeitados; texto válido é aceito | **Parcial:** rejeição de motivo ausente, vazio, curto e com espaços comprovada por HTTP em S3-QA-020; não há teste unitário dedicado do ViewModel |
| UT-12 | RF-23, RN-02 | Cálculo do ajuste | saldo atual 2; novo saldo 6, 0 e valor negativo | Saldos finais 6 e 0; quantidades absolutas 4 e 2 no movimento AJUSTE, respectivamente; novo saldo negativo é rejeitado | **Parcial:** diferenças 5 e 3 comprovadas por HTTP em S3-QA-020; rejeição de saldo negativo não foi executada |
| UT-13 | RF-21, UC-11 | Filtro de saldo zerado | SKUs com saldos 0, 1 e 3 | Somente saldo 0 compõe o resultado | **Existente e aprovado:** `SaldosZeradosViewModelTests` e `SaldosZeradosConsultaTests`; execução de 01/11/2026 em S3-FE-031 |
| UT-14 | RF-22, UC-12 | Agrupamento de rupturas | ocorrências repetidas por produto/SKU | Frequência correta por modelo e numeração | **Parcial:** agrupamento e contagem comprovados em `RupturasControllerTests` e `RupturasHistoricoHttpTests`; o ViewModel de S3-BE-013 não possui teste unitário próprio |
| UT-15 | RF-04, RNF-04 | Política de hash | hash bcrypt de custo 12 e hash de custo inferior | Custo 12 é aceito; custo inferior é reprovado | Planejado |

## 10. Catálogo de testes do meio da pirâmide

### 10.1 Autenticação e rotas

| ID | Requisitos | Cenário e entradas/ações | Resultado esperado | Situação |
|---|---|---|---|---|
| CT-AUT-01 | RF-01, UC-01 | Anônimo envia `GET /Account/Login` | `200 OK`; formulário com e-mail, senha, `POST` e antiforgery | Existente |
| CT-AUT-02 | RF-01, RF-02 | Login `lojista@squad.com` / `123`; abrir `/Produtos` | Cookie autenticado como `LOJISTA`; Produtos responde `200` | Existente |
| CT-AUT-03 | RF-01, RF-02 | Login `vendedor@squad.com` / `123`; abrir `/Movimentacoes/Saida` | Cookie autenticado como `VENDEDOR`; Saída responde `200` | Existente |
| CT-AUT-04 | RF-01, UC-01 | E-mail inexistente e senha incorreta | Login recusado; mensagem genérica; rota protegida continua pedindo login | Parcial: falta afirmar mensagem e limpeza da senha |
| CT-AUT-05 | RF-01, UC-01 | E-mail cadastrado e senha incorreta | Mesmo erro genérico, sem revelar se o e-mail existe; sem autenticação | Planejado |
| CT-AUT-06 | RF-01, UC-01 | E-mail e senha vazios | Erros de obrigatoriedade; nenhum cookie autenticado | Planejado |
| CT-AUT-07 | RF-01 | Autenticado envia `GET /Account/Login` | Redirecionamento para a página inicial | Existente |
| CT-AUT-08 | RF-01 | Lojista veio de `/Produtos` e autentica com `ReturnUrl` local | Redirecionamento para `/Produtos`; URL externa nunca é aceita | Planejado |
| CT-AUT-09 | RF-02 | Anônimo abre `/Produtos` | `302` para Login com `ReturnUrl`; nenhum conteúdo protegido | Existente |
| CT-AUT-10 | RF-02 | Anônimo abre `/Movimentacoes` | `302` para Login com `ReturnUrl` | Existente |
| CT-AUT-11 | RF-02 | Lojista abre Produtos, Movimentações e Ajuste | Todas respondem `200` | Parcial: falta Ajuste |
| CT-AUT-12 | RF-02 | Vendedor abre `/Movimentacoes/Saida` | `200 OK` | Existente |
| CT-AUT-13 | RF-02 | Vendedor abre `/Produtos` diretamente | `302` para Acesso Negado; nenhum dado retornado | Existente |
| CT-AUT-14 | RF-02, RN-04 | Vendedor envia `GET /Movimentacoes/Ajuste` | Acesso negado; formulário não retornado | Planejado prioritário |
| CT-AUT-15 | RF-02, RN-04 | Vendedor envia `POST /Movimentacoes/Ajuste` válido | Acesso negado; saldo e histórico inalterados | Planejado prioritário |
| CT-AUT-16 | RF-03 | Vendedor faz requisições sucessivas no mesmo cliente | Sessão e perfil permanecem ativos, sem novo login | Planejado |
| CT-AUT-17 | RF-03 | Lojista navega por três rotas no mesmo cliente | Sessão permanece autenticada e autorizada | Planejado |
| CT-AUT-18 | RF-03 | Lojista faz logout e tenta reabrir Produtos | Logout redireciona ao Login; Produtos volta a exigir autenticação | Existente |
| CT-AUT-19 | RF-03 | Avançar relógio controlado além da validade do cookie | Cookie expirado é recusado; rota redireciona ao Login | Planejado |
| CT-AUT-20 | RF-04, RNF-04 | Ler `SenhaHash` persistido | Valor não é a senha em claro e possui formato bcrypt | Planejado prioritário |
| CT-AUT-21 | RF-04, RNF-04 | Verificar algoritmo e custo do hash | Senha correta valida; custo é maior ou igual a 12 | Planejado prioritário |
| CT-AUT-22 | RF-04 | Verificar senha incorreta contra o hash | Validação falsa; hash não é alterado | Planejado |

O destino pós-login ainda possui uma divergência: o UC-01 exige uma tela inicial por perfil, enquanto a implementação e a baseline atuais redirecionam ambos os perfis para `/`. A rota esperada deve ser atualizada quando Produto/UX definir os destinos finais.

### 10.2 Cadastro, grade e persistência

| ID | Requisitos | Cenário | Entradas/ações | Resultado esperado | Situação |
|---|---|---|---|---|---|
| IT-01 | RF-05, UC-07 | Lojista cadastra produto válido | `POST /Produtos/Create` com nome, marca, categoria e cor | Produto persistido; redirecionamento de sucesso; dados aparecem na listagem | Planejado |
| IT-02 | RF-05 | Cadastro inválido | Campo obrigatório ausente e token válido | Tela retorna com erros; nenhum Produto é salvo | Planejado |
| IT-03 | RF-02 | Vendedor tenta cadastrar produto | Login `VENDEDOR`; `GET` e `POST /Produtos/Create` | Acesso negado; nenhum Produto é salvo | Parcial: o controller inteiro já é protegido, mas falta caso específico de escrita |
| IT-04 | RF-06, RF-07, UC-08, UC-S1 | Criação da grade | Produto válido; numerações 37, 38 e 39 | Três SKUs com IDs únicos e vínculo ao Produto são persistidos | **Parcial:** `Lojista_creates_complete_product_and_grade_via_http` comprova a grade persistida e carregada pelo Produto; falta afirmar explicitamente os IDs únicos e cada `ProdutoId` |
| IT-05 | RF-08, RN-01 | SKU duplicado | Mesmo Produto e numeração 38 duas vezes | Persistência rejeitada pela constraint; nenhum duplicado permanece | **Existente:** `Sku_with_same_product_and_numeracao_cannot_be_persisted_twice` |
| IT-06 | RF-07, RN-01 | Mesma numeração em produtos diferentes | Dois Produtos; numeração 38 em ambos | Ambos os SKUs são aceitos e possuem IDs distintos | Planejado |

### 10.3 Estoque e movimentações

| ID | Requisitos | Cenário | Entradas/ações | Resultado esperado | Situação |
|---|---|---|---|---|---|
| IT-07 | RF-09, RF-10, UC-09, UC-S5 | Entrada válida | saldo 3; entrada 4; lojista autenticado | Saldo 7; movimentação `ENTRADA` com quantidade, usuário e data/hora | **Parcial:** `Entrada_registers_movement_and_increases_balance` não afirma todos os campos |
| IT-08 | RF-09 | Entrada inválida | quantidade 0 ou negativa | Erro de validação; saldo e histórico inalterados | Planejado |
| IT-09 | RF-10, UC-S5 | Auditoria de movimentação | entrada, saída e ajuste válidos | Cada registro contém tipo, quantidade, usuário e data/hora | Parcial nos testes existentes |
| IT-10 | RF-11, RN-02, UC-S4 | Saída suficiente | saldo 5; saída 2 | Saldo 3; uma movimentação `SAIDA` | **Existente:** `Saida_registers_movement_and_reduces_balance` |
| IT-11 | RF-11, RN-02, UC-S4 | Saída insuficiente | saldo 1; saída 2 | Erro claro; saldo 1; nenhuma movimentação | **Existente:** `Saida_with_insufficient_balance_is_rejected_by_controller` |
| IT-12 | RN-02 | Constraint de saldo | tentar persistir SKU com saldo -1 | Banco rejeita a gravação | **Existente:** `Sku_with_negative_balance_cannot_be_persisted` |
| IT-13 | RF-12 | Última atualização | entrada, venda, ajuste, ruptura, desistência e rejeições | Data/hora do SKU muda somente após operação confirmada; operações sem efeito preservam o valor | **Existente e aprovado:** [UltimaAtualizacaoSaldoHttpTests](../../tests/SquadEstoque.Web.Tests/UltimaAtualizacaoSaldoHttpTests.cs), 16 cenários em 29/10/2026; [evidência](s3-qa-025-ultima-atualizacao.md) |
| IT-14 | RN-03 | Imutabilidade | GET/POST de edição e exclusão por LOJISTA, VENDEDOR e anônimo; PUT/PATCH/DELETE nas rotas de alteração por LOJISTA | Ações ausentes no MVC; HTTP 404 ou 405; campos preservados após cada tentativa; histórico disponível ao lojista | **Existente e aprovado:** [MovimentacaoImutabilidadeHttpTests](../../tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs) e [MovimentacaoSomenteLeituraHttpTests](../../tests/SquadEstoque.Web.Tests/MovimentacaoSomenteLeituraHttpTests.cs); smoke HTTP de 14/10/2026 no [registro consolidado](homologacoes/registro-validacoes-manuais.md) |
| IT-15 | RF-23, RN-04, UC-10 | Ajuste válido | saldo 2; novo saldo 6; motivo “Contagem física”; `LOJISTA` | Saldo 6; movimentação `AJUSTE`, diferença 4, motivo e responsável | **Existente e aprovado:** `Ajuste_registers_one_traceable_movement_and_updates_balance`, `AjusteMotivoHttpTests` e `AjusteAuthorizationHttpTests`; 8 cenários em 22/10/2026 ([S3-QA-020](execucoes/sprint-3/s3-qa-020-motivo-ajuste.md)) e 7 em 12/10/2026 ([S3-QA-021](execucoes/sprint-3/s3-qa-021-autorizacao-ajuste.md)) |
| IT-16 | RF-23, UC-10 | Ajuste sem motivo | saldo 2; novo saldo 6; motivo vazio | Rejeição; saldo 2; nenhuma movimentação | **Existente e aprovado:** `Ajuste_without_reason_is_rejected_by_controller`; motivo ausente, vazio e só espaços cobertos por `AjusteMotivoHttpTests` em 22/10/2026 |
| IT-17 | RF-23, RN-02 | Ajuste para saldo negativo | novo saldo apurado menor que 0 | Rejeição; nenhuma alteração ou movimentação | Planejado — não executado; a rejeição de saldo negativo não foi comprovada por teste |
| IT-18 | RN-07, UC-S2 | Venda concorrente do último par | saldo 1; dois vendedores confirmam simultaneamente | Uma operação vence; outra falha; saldo final 0; exatamente uma saída | **Existente e aprovado em linha de base local:** `Concurrent_saidas_sell_available_stock_only_once` com 30 repetições em 11/10/2026 ([S3-QA-003](execucoes/sprint-3/s3-qa-003-venda-simultanea.md)), `VendaTransacionalTests` e S3-BE-001; não representa dois navegadores nem carga |

### 10.4 Consulta e atendimento do vendedor

Os casos detalhados, incluindo pré-condições, passos e resultados esperados, estão nas tabelas desta seção. A implementação automatizada correspondente permanece em [`tests/`](../../tests/).

| ID | Requisitos | Cenário | Entradas/ações | Resultado esperado | Situação |
|---|---|---|---|---|---|
| IT-19 | RF-13, UC-02 | Busca parcial | termo existente com pelo menos 2 caracteres | Produtos correspondentes retornados | **Existente e aprovado em E2E local:** buscas `Tênis` e `tenis` em 02/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| IT-20 | RF-13, UC-02 | Busca sem resultado | termo inexistente | Mensagem “Produto não encontrado”; lista vazia | **Existente e aprovado em E2E local:** termo `zzz` em 02/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| IT-21 | RF-14, UC-03 | Grade completa | selecionar PRD-01 | Todas as numerações e respectivos saldos são exibidos | **Existente e aprovado em E2E local:** grade 37–40 no reteste de 07/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| IT-22 | RF-15, UC-03 | Estados da grade | SKUs com saldo 0, 1 e 2 | Cada SKU recebe estado visual correto | **Existente e aprovado em E2E local:** `Indisponível`, `Último par` e `Disponível` observados em 02/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| IT-23 | RF-16 | Ações do atendimento | consulta válida | As três opções de resultado são exibidas | **Existente e aprovado em E2E local:** `Vendeu`, `Não tinha` e `Desistiu` exercitados em 02 e 03/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| IT-24 | RF-10, RF-17, UC-04, UC-S2, UC-S5 | Resultado `Vendeu` | SKU com saldo 5, 3, 2 ou 1; vendedor autenticado | Saldo reduz exatamente 1; uma saída vinculada ao SKU e ao vendedor; confirmação e grade atualizada | **Existente e aprovado:** `Vendedor_sells_exactly_one_pair_without_changing_any_other_sku` e `Vendedor_can_post_venda_for_sku_and_records_authenticated_user`; duas vendas 3→2→1 no E2E local de 02/11/2026 |
| IT-25 | RF-17, RN-02, UC-S4 | Venda sem saldo | SKU saldo 0 | Rejeição; saldo 0; nenhuma movimentação ou ruptura | **Existente e aprovado:** `Vendedor_cannot_sell_zero_stock_and_creates_no_movement`, `Vender_rejects_zero_balance_without_creating_movement_or_rupture` e `Venda_rapida_rejects_zero_balance_without_movement`; venda com saldo 0 rejeitada no E2E local |
| IT-26 | RF-18, RN-05, RN-06, UC-05, UC-S3 | Resultado `Não tinha` | SKU válido; vendedor autenticado | Uma Ruptura com SKU, vendedor e data; saldo inalterado e nenhuma movimentação | **Existente e aprovado:** `Registrar_nao_tinha_creates_rupture_without_changing_any_stock_or_movement`; 3 rupturas no SKU 40 sem movimentação no E2E local de 02/11/2026 |
| IT-27 | RN-06 | Ruptura sem SKU | `sku_id` ausente ou inválido | Rejeição; nenhuma Ruptura | Planejado/dependente do endpoint — não executado |
| IT-28 | RF-19, UC-06 | Resultado `Desistiu` | atendimento iniciado | Nenhuma movimentação ou ruptura; retorno à busca | **Existente e aprovado em E2E local:** atendimento concluído como `DESISTIU`, saldo preservado e retorno à busca em 03/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| IT-29 | RF-20 | Nova consulta sem registrar resultado | abandonar resultado e iniciar nova busca | Nova busca permitida sem efeito no estoque | **Existente e aprovado em E2E local:** nova busca `Tênis` abriu a grade sem bloqueio em 03/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |

### 10.5 Visões do lojista

| ID | Requisitos | Cenário | Entradas/ações | Resultado esperado | Situação |
|---|---|---|---|---|---|
| IT-30 | RF-21, UC-11 | Saldos zerados | Produtos com SKUs 0, 1 e 2 | Somente saldo 0 é listado, agrupado por modelo | **Existente e aprovado:** `SaldosZeradosViewModelTests`, `SaldosZeradosConsultaTests` e `SaldosZeradosAuthorizationHttpTests`; capturas em 320/390/768/1440 px em [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md) |
| IT-31 | RF-21, RF-02 | Vendedor tenta abrir saldos zerados | Login `VENDEDOR`; URL direta | Acesso negado, sem dados de estoque | **Existente e aprovado:** `SaldosZeradosAuthorizationHttpTests`; VENDEDOR e anônimo negados em 01/11/2026 |
| IT-32 | RF-22, UC-12 | Histórico de rupturas | Rupturas repetidas para diferentes SKUs | Modelo, numeração e frequência corretos | **Existente e aprovado:** `RupturasControllerTests` e `RupturasHistoricoHttpTests`; capturas em 320/390/768/1440 px em [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md) |
| IT-33 | RF-22, RF-02 | Vendedor tenta abrir histórico | Login `VENDEDOR`; URL direta | Acesso negado | **Existente e aprovado:** `RupturasHistoricoHttpTests` restringe ao LOJISTA; VENDEDOR e anônimo negados em 01/11/2026 |

## 11. Catálogo de testes do topo da pirâmide

Executar em homologação com Playwright ou navegador real quando as jornadas estiverem estáveis. Na Sprint 3, as jornadas foram executadas localmente de forma manual assistida; a validação em ambiente publicado permanece pendente.

| ID | Jornada | Entradas principais | Resultado esperado | Requisitos | Situação |
|---|---|---|---|---|---|
| E2E-01 | Login e logout de lojista | credenciais válidas; acessar Produtos; sair | Home do perfil; área autorizada; após sair, rota protegida pede login | RF-01 a RF-03, UC-01 | **Aprovado** em 02/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| E2E-02 | Cadastro até entrada inicial | lojista cria PRD-01, grade 37–40 e entrada | Produto e SKUs visíveis; saldos e movimentações corretos | RF-05 a RF-10, UC-07 a UC-09 | **Aprovado no reteste** de 07/11/2026; a execução original foi reprovada por `DEF-S3QA030-01` e a correção foi integrada ao `main` pelo PR #95 em 08/11/2026 (`c6cfdad`) |
| E2E-03 | Consulta e venda | vendedor busca PRD-01, escolhe SKU-02 e marca `Vendeu` | Grade aparece; saldo reduz em 1; confirmação visível | RF-13 a RF-17, UC-02 a UC-04 | **Aprovado em ambiente local** em 02/11/2026; ressalva cosmética `DEF-S3QA030-02`; ambiente publicado segue bloqueado por `DEF-VEN-MOB-01` |
| E2E-04 | Consulta e ruptura | vendedor busca, escolhe SKU e marca `Não tinha` | Ruptura registrada; saldo não muda | RF-18, RN-05, RN-06, UC-05 | **Aprovado** em 02/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| E2E-05 | Desistência e continuidade | vendedor consulta, desiste e inicia nova consulta | Nenhum efeito no estoque; uso continua sem bloqueio | RF-19, RF-20, UC-06 | **Aprovado** em 03/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |
| E2E-06 | Ajuste administrativo | lojista ajusta SKU com motivo | Novo saldo e histórico rastreável; vendedor não acessa a função | RF-23, RN-04, UC-10 | **Aprovado** em 02/11/2026; vendedor recebeu `AccessDenied` |
| E2E-07 | Decisão de reposição | lojista abre saldos zerados e rupturas | Agrupamentos e frequências correspondem à massa preparada | RF-21, RF-22, UC-11, UC-12 | **Aprovado** em 02/11/2026 ([S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md)) |

A execução de 02, 03 e 07/11/2026 foi manual assistida, local, com Kestrel e SQLite descartável, sem capturas de tela. Os 7 casos foram aprovados após o reteste do E2E-02 e o PR #95 foi integrado ao `main` em 08/11/2026 (`c6cfdad`), mas o aceite formal continua pendente de revisão por outro integrante e de validação em ambiente publicado. Os defeitos `DEF-S3QA030-02` (“1 pares”) e `DEF-S3QA030-03` (mensagem em inglês) permanecem abertos.

## 12. Requisitos não funcionais

| ID | Requisito | Método e entradas | Resultado esperado | Ferramenta/cadência |
|---|---|---|---|---|
| NFT-01 | RNF-01 | Abrir jornadas E2E em Chrome e Safari mobile, sem instalação | Sistema funciona pelo navegador | Playwright + validação manual por release |
| NFT-02 | RNF-02 | Linha de base local em TestServer com 1 usuário; carga de 20 usuários e medição mobile pendentes | P95 de servidor < 500 ms e resultado visível em até 3 s com perfil de rede definido | **Parcial:** 15/15 P95 < 500 ms em 29/10/2026 ([S3-QA-028](s3-qa-028-p95-consulta.md)); capacidade e ambiente publicado não comprovados; k6/NBomber antes de release |
| NFT-03 | RNF-03 | Health check durante janela de 12 horas por dia, 7 dias | Disponibilidade atende à meta operacional que ainda deve ser quantificada no SRS | Monitoramento contínuo; requisito precisa de meta percentual |
| NFT-04 | RNF-04 | Inspecionar hashes persistidos | bcrypt válido, custo >= 12, nenhuma senha em claro | xUnit a cada CI |
| NFT-05 | RNF-05 | Requisitar HTTP em homologação/produção e inspecionar headers | Redirecionamento para HTTPS; tráfego final somente HTTPS; HSTS quando aplicável | Teste de integração/configuração e smoke por release |
| NFT-06 | RNF-06 | Usuário vendedor sem treinamento realiza primeira consulta | No máximo 2 toques a partir do login, sem ajuda | Teste de usabilidade com pelo menos 5 participantes representativos |
| NFT-07 | RNF-07 | Viewports 360×800, 390×844 e 430×932 | Sem rolagem horizontal, sobreposição ou ação inacessível | Playwright visual + manual por release |

Para NFT-02, documentar volume de dados, número de usuários virtuais, duração, máquina e perfil de rede. Um tempo medido sem essas condições não é evidência reproduzível. A medição de S3-QA-028 é opt-in, fica ignorada na suíte padrão, usou 1 usuário e TestServer no mesmo processo; não comprova capacidade, ambiente Azure nem o critério de 3 s no navegador.

## 13. Matriz de rastreabilidade resumida

| Requisito | Casos principais |
|---|---|
| RF-01 a RF-04 | CT-AUT-01 a CT-AUT-22, UT-15, E2E-01, NFT-04 |
| RF-05 | UT-01, UT-02, IT-01 a IT-03, E2E-02 |
| RF-06 a RF-08 | UT-03, IT-04 a IT-06, E2E-02 |
| RF-09 a RF-12 | UT-04, UT-05, IT-07 a IT-14, E2E-02 |
| RF-13 a RF-15 | UT-06, UT-07, IT-19 a IT-22, E2E-03, NFT-02 |
| RF-16 a RF-20 | UT-08 a UT-10, IT-23 a IT-29, E2E-03 a E2E-05 |
| RF-21 e RF-22 | UT-13, UT-14, IT-30 a IT-33, E2E-07 |
| RF-23 | UT-11, UT-12, IT-15 a IT-17, E2E-06 |
| RNF-01 a RNF-07 | NFT-01 a NFT-07 |
| RN-01 | UT-03, IT-04 a IT-06 |
| RN-02 | UT-05, UT-09, UT-12, IT-11, IT-12, IT-17, IT-18, IT-25 |
| RN-03 | IT-09, IT-14 |
| RN-04 | CT-AUT-14, CT-AUT-15, IT-03, IT-15, E2E-06 |
| RN-05 e RN-06 | IT-26, IT-27, E2E-04 |
| RN-07 | IT-18 |

### 13.1 Rastreabilidade dos requisitos atualizados na Sprint 3

| Requisito | Casos principais | Resultado registrado | Evidência | Limites |
|---|---|---|---|---|
| RN-03 | IT-14 | **Aprovado:** edição e exclusão ausentes no MVC; 12 tentativas por GET/POST recusadas com 404/405 e campos preservados | [MovimentacaoImutabilidadeHttpTests](../../tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs), [MovimentacaoSomenteLeituraHttpTests](../../tests/SquadEstoque.Web.Tests/MovimentacaoSomenteLeituraHttpTests.cs) e [registro consolidado](homologacoes/registro-validacoes-manuais.md) | Smoke HTTP local de 14/10/2026; sem Azure ou teste visual |
| RN-07 | IT-18, UT-05, IT-25 | **Aprovado em linha de base local:** uma única venda vence a disputa; saldo final zero e exatamente uma saída em 30 repetições | [S3-QA-003](execucoes/sprint-3/s3-qa-003-venda-simultanea.md), [S3-BE-001](execucoes/sprint-3/s3-be-001-decremento-atomico.md), [S3-BE-002](execucoes/sprint-3/s3-be-002-venda-transacional.md) e [saída da concorrência](execucoes/sprint-3/s3-qa-003-resultados.txt) | Teste de persistência, não dois navegadores nem carga; revisão independente no PR |
| RF-12 | IT-13 | **Aprovado:** 16 cenários HTTP comprovam alteração da data somente após entrada, venda ou ajuste confirmados | [S3-QA-025](s3-qa-025-ultima-atualizacao.md), `UltimaAtualizacaoSaldoHttpTests` e [PR #83](https://github.com/CodeWitch00/SQUAD-estoque/pull/83) | Sem homologação manual em navegador; a tela pode formatar somente até minutos |
| RF-21 | UT-13, IT-30, IT-31, E2E-07 | **Aprovado:** somente SKUs ativos com saldo zero são agrupados; consulta, autorização e telas responsivas validadas | [S3-BE-007](execucoes/sprint-3/s3-be-007-viewmodel-saldos-zerados.md), [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md), [PR #90](https://github.com/CodeWitch00/SQUAD-estoque/pull/90) e [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Aparelho físico, leitor de tela e Safari não validados |
| RF-22 | UT-14, IT-32, IT-33, E2E-07 | **Aprovado:** agrupamento, contagem e última ocorrência corretos; histórico restrito ao LOJISTA e telas responsivas validadas | [S3-BE-013](execucoes/sprint-3/s3-be-013-viewmodel-rupturas.md), [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md), [PR #90](https://github.com/CodeWitch00/SQUAD-estoque/pull/90) e [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Volume maior de dados, foco por teclado e revisão independente não comprovados |
| RNF-02 | NFT-02 | **Parcial:** 15/15 P95 de servidor abaixo de 500 ms em linha de base local com 1 usuário | [S3-QA-028](s3-qa-028-p95-consulta.md), [resultado.json](evidencias/s3-qa-028/resultado.json) e [amostras.csv](evidencias/s3-qa-028/amostras.csv) | TestServer local, sem carga de 20 usuários, sem Azure e sem medição mobile de 3 s |

A coluna de resultado distingue aprovação local de aceite formal. Nenhuma linha desta matriz declara aceite de release.

### 13.2 Rastreabilidade dos casos de uso

| Caso de uso | Testes principais |
|---|---|
| UC-01 — Realizar login | CT-AUT-01 a CT-AUT-22, E2E-01 |
| UC-02 — Consultar estoque por modelo | UT-07, IT-19, IT-20, E2E-03 |
| UC-03 — Visualizar grade | UT-06, IT-21, IT-22, E2E-03 |
| UC-04 — Registrar `Vendeu` | UT-09, IT-24, IT-25, E2E-03 |
| UC-05 — Registrar `Não tinha` | IT-26, IT-27, E2E-04 |
| UC-06 — Registrar `Desistiu` | UT-10, IT-28, IT-29, E2E-05 |
| UC-07 — Cadastrar produto | UT-01, UT-02, IT-01 a IT-03, E2E-02 |
| UC-08 — Cadastrar grade | UT-03, IT-04 a IT-06, E2E-02 |
| UC-09 — Registrar entrada | UT-04, IT-07, IT-08, E2E-02 |
| UC-10 — Ajustar saldo | UT-11, UT-12, IT-15 a IT-17, E2E-06 |
| UC-11 — Visualizar saldos zerados | UT-13, IT-30, IT-31, E2E-07 |
| UC-12 — Visualizar rupturas | UT-14, IT-32, IT-33, E2E-07 |
| UC-S1 — Gerar SKU | UT-03, IT-04 a IT-06 |
| UC-S2 — Decrementar saldo | UT-05, UT-09, IT-18, IT-24, IT-25 |
| UC-S3 — Criar ruptura | IT-26, IT-27, E2E-04 |
| UC-S4 — Rejeitar saída inválida | UT-05, IT-11, IT-12, IT-25 |
| UC-S5 — Registrar movimentação | IT-07, IT-09, IT-10, IT-15 |

### 13.3 Evidência de execução

Este plano mantém, na seção 13.4, somente resultados de casos efetivamente executados, com data, commit, resultado e link da evidência. Contagens e percentuais agregados não são copiados para o plano: permanecem no runner, no check do Pull Request e no [GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions). O estado executável está em [`tests/`](../../tests/). Validações que dependem de percepção visual, dispositivo ou ambiente publicado ficam no [registro consolidado](homologacoes/registro-validacoes-manuais.md).

### 13.4 Resultados registrados na Sprint 3

| Caso | Execução | Resultado | Evidência | Limites |
|---|---|---|---|---|
| IT-13 / RF-12 | 29/10/2026, base `95199c4`, PR #83 | **Aprovado:** 16 cenários HTTP de data e hora | [S3-QA-025](s3-qa-025-ultima-atualizacao.md) | Sem homologação manual em navegador |
| IT-14 / RN-03 | 14/10/2026, base `1dacf4d` | **Aprovado:** 12 tentativas de edição/exclusão recusadas; campos e histórico preservados | [MovimentacaoImutabilidadeHttpTests](../../tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs) e [registro consolidado](homologacoes/registro-validacoes-manuais.md) | Smoke HTTP local; sem Azure ou teste visual |
| IT-15 e IT-16 / RF-23 | 12/10/2026 e 22/10/2026, PR #58 e #69 | **Aprovado:** 7 cenários de autorização e 8 de motivo/persistência | [S3-QA-021](execucoes/sprint-3/s3-qa-021-autorizacao-ajuste.md) e [S3-QA-020](execucoes/sprint-3/s3-qa-020-motivo-ajuste.md) | Saldo negativo (IT-17) não foi executado |
| IT-18 / RN-07 | 08/10 a 11/10/2026, PR #53 e commit `91beacd` | **Aprovado em linha de base local:** 30 disputas com uma única venda e uma única saída; transação e rollback cobertos | [S3-BE-001](execucoes/sprint-3/s3-be-001-decremento-atomico.md), [S3-BE-002](execucoes/sprint-3/s3-be-002-venda-transacional.md), [S3-QA-003](execucoes/sprint-3/s3-qa-003-venda-simultanea.md) e [saída da concorrência](execucoes/sprint-3/s3-qa-003-resultados.txt) | Não representa dois navegadores nem teste de carga |
| S3-BE-007 / RF-21 | 13/10/2026, commit `bfa420c` | **Aprovado:** contrato de saldos zerados com 11 casos e suíte da baseline aprovada | [S3-BE-007](execucoes/sprint-3/s3-be-007-viewmodel-saldos-zerados.md) | Entrega o ViewModel, não a consulta ou a tela |
| S3-BE-013 / RF-22 | 14/10/2026, commit `88465f1` | **Aprovado:** contrato do histórico com os seis campos e suíte da baseline aprovada | [S3-BE-013](execucoes/sprint-3/s3-be-013-viewmodel-rupturas.md) | Sem teste unitário próprio de propriedades |
| IT-30 e IT-31 / RF-21 | 01/11/2026, PR #90 | **Aprovado:** composição, consulta e autorização; capturas em 320/390/768/1440 px | [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md) | Aparelho físico, leitor de tela e Safari não validados |
| IT-32 e IT-33 / RF-22 | 01/11/2026, PR #90 | **Aprovado:** agrupamento, contagem, restrição ao LOJISTA e capturas responsivas | [S3-FE-031](../05-ux/evidencias/s3-fe-031/README.md) | Volume maior, foco por teclado e revisão independente pendentes |
| S3-DB-026 / apoio a RF-21 e RF-22 | 29/10/2026, PR #87 | **Aprovado:** planos de consulta usam os novos índices; migration testada com `Up`, `Down` e preservação de dados | [S3-DB-026](s3-db-026-indices-consulta.md) | Planos SQL representativos; não mede tempo HTTP nem ganho em produção |
| E2E-01 | 02/11/2026, base `2e116c5`; registro integrado pelo PR #95 (`c6cfdad`) — o PR #92 foi fechado sem merge | **Aprovado:** login, logout e retorno à rota protegida | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Sem capturas de tela; ambiente publicado não validado |
| E2E-02 | 02/11/2026 reprovado; reteste em 07/11/2026 no head `eb26f7d`; PR #95 integrado em 08/11/2026 (`c6cfdad`) | **Aprovado no reteste:** grade 37–40 sem workaround, saldos 1/2/1/0 e entradas corretas | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) e [estado do reteste](evidencias/s3-qa-030/estado-final-banco-reexecucao-e2e02.json) | Aceite formal pendente de revisão independente e validação em ambiente publicado |
| E2E-03 | 02/11/2026 | **Aprovado em ambiente local:** duas vendas 3→2→1 e confirmação visível | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | `DEF-S3QA030-02` (“1 pares”) aberto; ambiente publicado bloqueado por `DEF-VEN-MOB-01` |
| E2E-04 | 02/11/2026 | **Aprovado:** 3 rupturas, saldo 0 preservado e nenhuma saída | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Sem capturas de tela |
| E2E-05 | 03/11/2026 | **Aprovado:** `Desistiu` sem movimentação ou ruptura e nova consulta liberada | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Sem capturas de tela |
| E2E-06 | 02/11/2026 | **Aprovado:** ajustes 1→3 e 2→0 rastreáveis; VENDEDOR recebeu `AccessDenied` | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Sem capturas de tela |
| E2E-07 | 02/11/2026 | **Aprovado:** saldos zerados e rupturas agrupados conforme a massa | [S3-QA-030](execucoes/sprint-3/s3-qa-030-regressao-e2e.md) | Sem capturas de tela |
| NFT-02 / RNF-02 | 29/10/2026, commit `91b9e8c`, PR #85 | **Aprovado em linha de base local:** 15/15 P95 de servidor < 500 ms | [S3-QA-028](s3-qa-028-p95-consulta.md), [resultado.json](evidencias/s3-qa-028/resultado.json) e [amostras.csv](evidencias/s3-qa-028/amostras.csv) | 1 usuário, TestServer local, sem Azure ou carga de homologação |

## 14. Automação existente

| Arquivo | Responsabilidade principal |
|---|---|
| [BasicRoutesTests.cs](../../tests/SquadEstoque.Web.Tests/BasicRoutesTests.cs) | inicialização, rotas básicas e acesso anônimo |
| [AuthenticationAuthorizationTests.cs](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs) | login, perfis, navegação, autorização, BCrypt, logout e regressão de segurança do login |
| [AdministrativeHttpTests.cs](../../tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs) | fluxos administrativos e autorização HTTP |
| [ConsultaEstoqueTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs) | busca, seleção, grade, saldos e estados visuais |
| [ConsultaOperacionalHttpTests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs) | consulta operacional e controles de atendimento |
| [EstoqueControllerHttpTests.cs](../../tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs) | venda, ruptura, autorização, antiforgery e renderização |
| [EstoqueDomainPersistenceTests.cs](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) | persistência, estoque, movimentações, concorrência, rollback e última atualização |
| [VendaTransacionalTests.cs](../../tests/SquadEstoque.Web.Tests/VendaTransacionalTests.cs) | transação da venda, rollback, conflito de escrita e mensagens seguras |
| [RollbackVendaTests.cs](../../tests/SquadEstoque.Web.Tests/RollbackVendaTests.cs) | falha controlada antes do commit sem saldo ou movimentação parcial |
| [VendaRapidaHttpTests.cs](../../tests/SquadEstoque.Web.Tests/VendaRapidaHttpTests.cs) | venda de uma unidade e movimentação rastreável |
| [UltimaAtualizacaoSaldoHttpTests.cs](../../tests/SquadEstoque.Web.Tests/UltimaAtualizacaoSaldoHttpTests.cs) | RF-12 via HTTP em entrada, venda, ajuste, ruptura e desistência |
| [AjusteMotivoHttpTests.cs](../../tests/SquadEstoque.Web.Tests/AjusteMotivoHttpTests.cs) | motivo, normalização e persistência do ajuste |
| [AjusteAuthorizationHttpTests.cs](../../tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs) | autorização do ajuste por perfil e antiforgery |
| [SaldosZeradosViewModelTests.cs](../../tests/SquadEstoque.Web.Tests/SaldosZeradosViewModelTests.cs) | composição, filtro, agrupamento e ordenação de saldos zerados |
| [SaldosZeradosConsultaTests.cs](../../tests/SquadEstoque.Web.Tests/SaldosZeradosConsultaTests.cs) | consulta de saldos zerados e preservação de estoque |
| [SaldosZeradosAuthorizationHttpTests.cs](../../tests/SquadEstoque.Web.Tests/SaldosZeradosAuthorizationHttpTests.cs) | autorização e estados da visão de saldos zerados |
| [RupturasControllerTests.cs](../../tests/SquadEstoque.Web.Tests/RupturasControllerTests.cs) | agrupamento, ordenação e restrição do histórico de rupturas |
| [RupturasHistoricoHttpTests.cs](../../tests/SquadEstoque.Web.Tests/RupturasHistoricoHttpTests.cs) | contagem, agregação, estado vazio e autorização do histórico |
| [RupturaPersistenceHttpTests.cs](../../tests/SquadEstoque.Web.Tests/RupturaPersistenceHttpTests.cs) | persistência da ruptura com SKU, vendedor e data |
| [MovimentacaoImutabilidadeHttpTests.cs](../../tests/SquadEstoque.Web.Tests/MovimentacaoImutabilidadeHttpTests.cs) | RN-03: edição e exclusão de movimentações recusadas |
| [MovimentacaoSomenteLeituraHttpTests.cs](../../tests/SquadEstoque.Web.Tests/MovimentacaoSomenteLeituraHttpTests.cs) | RN-03: rotas MVC somente leitura e preservação do histórico |
| [RelatoriosErroHttpTests.cs](../../tests/SquadEstoque.Web.Tests/RelatoriosErroHttpTests.cs) | falha de consulta nos relatórios sem confundir com estado vazio |
| [ConsultaP95Tests.cs](../../tests/SquadEstoque.Web.Tests/ConsultaP95Tests.cs) | medição opt-in de RNF-02 e cálculo de P95 por nearest-rank |
| [`tests/frontend/`](../../tests/frontend/) | comportamento de interface isolado e regressões frontend |

As classes podem crescer ou ser reorganizadas sem exigir atualização de contagens neste plano. Novos domínios só precisam ser citados aqui quando alterarem a estratégia ou a cobertura obrigatória.

## 15. Priorização por risco

| Prioridade | Critério | Casos |
|---|---|---|
| P0 — bloqueia release | perda/corrupção de estoque, acesso indevido ou senha insegura | AUT de escrita proibida, IT-11, IT-12, IT-15 a IT-18, IT-24 a IT-27, NFT-04, NFT-05 |
| P1 — obrigatório para MVP | jornada principal indisponível ou resultado incorreto | IT-01 a IT-10, IT-19 a IT-23, IT-28 a IT-33, E2E-01 a E2E-07 |
| P2 — importante | apresentação, compatibilidade e observabilidade | RF-12, RF-22, NFT-01, NFT-03, NFT-07 |

Ordem recomendada para reduzir risco atual:

1. hash bcrypt persistido e custo (NFT-04), ainda sem execução registrada;
2. auditoria completa das movimentações (IT-09);
3. correção dos defeitos cosméticos `DEF-S3QA030-02` e `DEF-S3QA030-03`;
4. carga de RNF-02 em homologação e medição mobile (NFT-02);
5. validação das jornadas E2E em ambiente publicado;
6. rejeição de saldo negativo no ajuste (IT-17) e demais lacunas de escrita.

## 16. Execução

### 16.1 Local

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Cobertura automatizada:

```bash
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj \
  --configuration Release \
  --collect:"XPlat Code Coverage"
```

O CI exige cobertura de linhas igual ou superior a 50% e publica o arquivo
`coverage.cobertura.xml`. O percentual vigente deve ser consultado no artefato da
execução correspondente, sem manter uma cópia manual no plano.

Medição opt-in de RNF-02, ignorada na suíte padrão:

```powershell
pwsh -File scripts/medir-rnf02.ps1 -OutputDirectory ./TestResults/rnf02-execucao-01
```

A medição local não substitui a carga de 20 usuários em homologação nem a validação mobile de 3 s.

Com `mise`:

```bash
mise --cd src/SquadEstoque.Web exec -- dotnet test ../../tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Filtrar uma classe:

```bash
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj \
  --filter FullyQualifiedName~AuthenticationAuthorizationTests
```

### 16.2 Cadência

| Momento | Execução mínima |
|---|---|
| Durante o desenvolvimento | testes unitários e de integração afetados |
| Antes do commit/PR | suíte automatizada completa |
| Pull Request | restore, build e suíte completa no GitHub Actions |
| Antes de release | suíte completa, E2E disponível, P0/P1 manual e não funcionais aplicáveis |
| Após deploy | smoke tests não destrutivos de login, HTTPS e rotas essenciais |

## 17. Critérios de entrada, suspensão e saída

### 17.1 Entrada

- requisito e critério de aceite identificados;
- ambiente disponível;
- massa controlada;
- build concluído;
- dependências restauradas;
- funcionalidade implementada quando o caso não for puramente documental.

### 17.2 Suspensão

Suspender uma execução quando:

- o ambiente impedir a maioria dos casos;
- a massa estiver corrompida ou não determinística;
- houver falha P0 que torne os resultados seguintes inválidos;
- a especificação tiver contradição que altere o resultado esperado.

A suspensão deve registrar causa, casos afetados e condição para retomada.

### 17.3 Saída para Pull Request

- build aprovado;
- todos os testes automatizados relacionados aprovados;
- nenhuma regressão na suíte completa;
- novo comportamento possui teste proporcional ao risco;
- validação manual registrada quando aplicável.

### 17.4 Saída para release

- zero defeito P0 aberto;
- zero defeito P1 sem aceite explícito do responsável pelo produto;
- todos os casos P0 e P1 aplicáveis aprovados;
- migrações validadas em cópia descartável;
- desempenho e compatibilidade dentro dos critérios;
- evidências e limitações conhecidas registradas.

## 18. Evidências e resultados

As evidências devem permanecer na fonte que as produz:

- testes automatizados: código em `tests/` e resultado no check do PR ou no GitHub Actions;
- revisão e integração: conversa, checks, commits e decisão no Pull Request;
- defeitos: issue ou cartão associado, quando aplicável;
- validações manuais, visuais, em dispositivo ou ambiente publicado: [registro consolidado de validações manuais](homologacoes/registro-validacoes-manuais.md);
- síntese do período: seção 13.4 deste plano e relatório da sprint em `docs/10-planejamento/`.

Não deve ser criado um relatório Markdown para cada teste automatizado, cartão ou execução local. Logs, arquivos TRX, saídas de console, percentuais e contagens agregadas pertencem à execução da CI e não devem ser copiados para a documentação. A seção 13.4 registra apenas a síntese por caso, com link para a evidência original.

Quando uma validação manual for necessária, registrar apenas data, commit, ambiente, escopo, resultado, limitações e o link da evidência externa relevante. Capturas anexadas somente ao Trello permanecem no cartão; entram no repositório apenas quando forem necessárias para homologação ou entrega acadêmica duradoura.

## 19. Manutenção do plano

- requisito novo deve entrar na matriz antes de ser considerado pronto;
- requisito removido deve ter os casos marcados como obsoletos, preservando histórico no Git;
- teste automatizado novo deve atualizar a situação do caso correspondente;
- resultado novo deve ser registrado na seção 13.4 com data, commit, resultado e link da evidência;
- não manter contagens ou percentuais agregados de execução no plano; o runner e a CI são as fontes operacionais;
- divergências entre SRS, UX e implementação devem ser resolvidas, não escondidas alterando a expectativa do teste;
- este plano deve ser revisado no planejamento de cada sprint e antes de cada release.

## 20. Registro de revisões

| Versão | Data | Tipo | Revisor | Resultado registrado |
|---|---|---|---|---|
| 1.0 | 2026-08-25 | Criação | Equipe do projeto | Estratégia, catálogos e matriz inicial versionados no Git |
| 1.1 | 2026-08-30 | Técnica | Codex, em revisão assistida solicitada pela equipe | Incluídos modelo obrigatório, separação por execução, estados controlados e cobertura transversal; rastreabilidade e referências conferidas |
| 1.1 | 2026-08-30 | Textual | Codex, em revisão assistida solicitada pela equipe | Terminologia de situação, execução, resultado e evidência uniformizada; regra contra execução presumida explicitada |
| 1.2 | 2026-08-30 | Integração documental | Codex, em revisão assistida solicitada pela equipe | Especificação detalhada de consulta, venda e ruptura do vendedor vinculada ao catálogo IT-18 a IT-29 |
| 1.3 | 2026-09-02 | Automação | Codex, em execução assistida solicitada pela equipe | Cobertura automatizada no CI; cadastro, edição, inativação, autorização POST, limpeza de senha, BCrypt e auditoria atualizados conforme testes executados |
| 1.4 | 2026-09-29 | Execução e rastreabilidade | Codex, em execução solicitada pela equipe | Resultados reais da venda com saldo, saldo zero, autorização, movimentação, vínculo Produto–SKU e validação mobile registrados; divergência do Azure e validação por toque mantidas abertas |
| 1.5 | 2026-09-29 | Ruptura | Codex, em execução solicitada pela equipe | IT-26 atualizado com a execução HTTP que comprova uma ruptura, saldos integralmente preservados e nenhuma alteração na tabela de movimentações |
| 2.0 | 2026-10-12 | Simplificação documental | Equipe do projeto | Evidências automáticas direcionadas à CI e validações manuais reunidas em registro único; relatórios repetitivos removidos |
| 2.1 | 2026-11-09 | Resultados da Sprint 3 | Equipe do projeto | Resultados de RN-03, RN-07, RF-12, RF-21, RF-22 e RNF-02 registrados por caso na seção 13.4; catálogos, automação, prioridades e referências atualizados; falhas e bloqueios mantidos visíveis |

Revisões assistidas não substituem o aceite do responsável de QA ou do produto quando esse aceite for exigido para release. A próxima alteração funcional ou de requisito deve gerar uma nova linha, com versão, data, tipo, revisor e resultado.
