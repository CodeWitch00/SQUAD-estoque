# S3-QA-035 — Auditoria de integridade do schema e das migrations

Data: 04/11/2026. Base auditada: `dev/emmy`, commit `31cdb18f4db43bf333dababffa8bfcba43614943`.
Branch documental: `docs/s3-qa-035-emmy`. Destino do PR: `dev/emmy`. Revisão solicitada a `CodeWitch00` no PR; aprovação deve ser conferida no GitHub.

## Escopo e método

Inspeção estática das regras, entidades, configuração do EF Core, operações `Up`/`Down`, respectivos Designers e snapshot. Nenhum banco foi aberto, migrado ou alterado. A aplicação não foi iniciada: seu bootstrap executa `Database.Migrate()` em [Program.cs](../../src/SquadEstoque.Web/Program.cs). Não houve publicação nem alteração de workflow. A `main` não foi usada como base nem modificada.

Fontes: [SRS, RN-01 a RN-06](../02-requisitos/srs.md), [domínio](../01-negocio/dominio.md), [modelo físico](../03-modelagem/modelo_fisico.md), [dicionário](../03-modelagem/dicionario-de-dados.md), [contexto](../../src/SquadEstoque.Web/Data/EstoqueContext.cs), [entidades](../../src/SquadEstoque.Web/Models/Entities), [migrations e Designers](../../src/SquadEstoque.Web/Migrations/Estoque), [snapshot](../../src/SquadEstoque.Web/Migrations/Estoque/EstoqueContextModelSnapshot.cs).

## Resultado executivo

As quatro proteções centrais estão declaradas no modelo e nas migrations: SKU único por produto e numeração, e-mail único, saldo não negativo e quantidade de movimentação positiva. As referências obrigatórias do núcleo estão presentes e usam deleção restritiva. A extensão fiscal apresenta duas divergências relevantes: quantidade de item sem CHECK e exclusão em cascata dos itens da nota. A conclusão é de conformidade parcial do conjunto, sem correções neste cartão.

Esta auditoria comprova as declarações versionadas; não certifica o estado de um banco implantado, a aplicação efetiva das migrations nem a ativação de FKs em uma conexão externa.

## Constraints

| Regra | Proteção versionada | Resultado e limite |
| --- | --- | --- |
| RN-01: SKU único | PK `Sku.Id`; índice único `IX_Sku_ProdutoId_Numeracao`; ambas as colunas NOT NULL | Conforme para a combinação literal Produto + Numeração. Não existe código SKU textual separado. |
| E-mail único | `Usuario.Email` NOT NULL; índice único `IX_Usuario_Email` | Conforme para valores iguais. Não há collation NOCASE ou índice normalizado: não comprova unicidade ignorando maiúsculas e espaços. |
| RN-02: saldo não negativo | `SaldoAtual` NOT NULL; `chk_sku_saldo_nao_negativo`: `SaldoAtual >= 0` | Conforme; zero é permitido. |
| Quantidade positiva de movimentação | `Quantidade` NOT NULL; `chk_movimentacao_quantidade_positiva`: `Quantidade > 0` | Conforme; zero e negativos são incompatíveis com o CHECK. |
| Quantidade positiva de item fiscal | `ItemEntradaFiscal.Quantidade` NOT NULL; atributo `[Range(1, int.MaxValue)]` na entidade | Divergência D01: não há CHECK equivalente no contexto, migration ou snapshot. |

As quatro proteções centrais são introduzidas por `20260817120442_InitialSquadSchema` e permanecem no snapshot final. Há ainda índices únicos para `EntradaFiscal.ChaveAcesso`, `(CnpjFornecedor, CodigoFornecedor)` e, separadamente, `Movimentacao.AtendimentoId` e `Ruptura.AtendimentoId`. Os dois últimos vínculos são opcionais; não impõem exclusividade cruzada entre as duas tabelas.

O snapshot contém nove CHECKs: os dois numéricos centrais, `chk_usuario_perfil`, `chk_movimentacao_tipo` e cinco de atendimento (`conclusao`, `resultado`, `saldo`, `sku_resultado`, `status`). Os de atendimento exigem domínio de status/resultado, saldo consultado não negativo quando informado e consistência entre conclusão e resultado; SKU é exigido para resultado diferente de DESISTIU.

## Relacionamentos e deleção

O sentido abaixo é dependente → principal. Obrigatória significa FK NOT NULL; o fato de a propriedade de navegação C# aceitar null não torna uma FK Guid não anulável opcional.

| Dependente / FK | Principal | Obrigatória | ON DELETE |
| --- | --- | --- | --- |
| Sku.ProdutoId | Produto | Sim | RESTRICT |
| Movimentacao.SkuId | Sku | Sim | RESTRICT |
| Movimentacao.UsuarioId | Usuario | Sim | RESTRICT |
| Movimentacao.AtendimentoId | Atendimento | Não | RESTRICT |
| Ruptura.SkuId | Sku | Sim | RESTRICT |
| Ruptura.UsuarioId | Usuario | Sim | RESTRICT |
| Ruptura.AtendimentoId | Atendimento | Não | RESTRICT |
| Atendimento.UsuarioId | Usuario | Sim | RESTRICT |
| Atendimento.ProdutoId | Produto | Sim | RESTRICT |
| Atendimento.SkuId | Sku | Não; condicionado ao resultado | RESTRICT |
| EntradaFiscal.UsuarioId | Usuario | Sim | RESTRICT |
| ItemEntradaFiscal.EntradaFiscalId | EntradaFiscal | Sim | CASCADE |
| ItemEntradaFiscal.ProdutoId | Produto | Não | RESTRICT |
| ItemEntradaFiscal.SkuId | Sku | Não | RESTRICT |
| VinculoProdutoFornecedor.SkuId | Sku | Sim | RESTRICT |

Total: 15 FKs, 14 RESTRICT e uma CASCADE, coerentes entre contexto, migrations de introdução e snapshot. Os vínculos opcionais dos itens fiscais permitem a fase de conferência anterior à associação ao catálogo. A nulabilidade, isoladamente, não constitui falha de obrigatoriedade.

RESTRICT protege a exclusão do principal referenciado; não impede UPDATE ou DELETE direto na própria linha do histórico. Não foram encontrados triggers de imutabilidade nas migrations. A RN-03 prevê também proteção pela ausência de endpoints de edição/exclusão; essa proteção da aplicação não equivale a uma proibição no banco.

## Ordem das migrations e snapshot

| Ordem | Migration | Up e dependências | Down |
| --- | --- | --- | --- |
| 1 | `20260817120442_InitialSquadSchema` | Produto/Usuario antes de Sku; Sku antes de Movimentacao/Ruptura. Cria índices únicos e CHECKs centrais. | Remove dependentes antes dos principais. |
| 2 | `20260901000759_AddEnumDomainConstraints` | Acrescenta CHECKs de Perfil e Tipo às tabelas existentes. | Remove esses CHECKs. |
| 3 | `20261009181737_AddAtendimentoAuditavel` | Adiciona AtendimentoId anulável, cria Atendimento e depois índices/FKs de vínculo. | Remove FKs de vínculo antes de Atendimento e das colunas adicionadas. |
| 4 | `20261009200729_AddEntradaFiscalEImagemProduto` | Acrescenta ImagemUrl; cria EntradaFiscal e vínculo de fornecedor antes de ItemEntradaFiscal. | Remove itens/vínculos antes da nota e retira ImagemUrl. |
| 5 | `20261026220326_AddSkuUltimaAtualizacaoSaldo` | Acrescenta timestamp anulável; registros existentes não exigem preenchimento. | Remove somente o timestamp. |

Os cinco identificadores são crescentes e únicos; cada migration possui Designer vinculado a `EstoqueContext`. O snapshot final representa nove entidades e inclui Atendimento, as três entidades fiscais, ImagemUrl e UltimaAtualizacaoSaldoEm, além das constraints e relações acima. A inspeção do modelo alvo do último Designer não identificou divergência estrutural em relação ao snapshot ou ao contexto atual. Não se trata de validação dinâmica por `HasPendingModelChanges`.

Os `Down` declaram operações inversas em ordem compatível com as dependências. Reversão com perda de tabelas/colunas pode perder dados; não foi executada. A adição de CHECKs em tabelas existentes também exige dados prévios válidos; este relatório não certifica dados existentes.

## Divergências e encaminhamentos

| ID | Achado e evidência | Impacto | Encaminhamento fora deste cartão |
| --- | --- | --- | --- |
| D01 — alta | Quantidade de item fiscal sem CHECK; [migration fiscal](../../src/SquadEstoque.Web/Migrations/Estoque/20261009200729_AddEntradaFiscalEImagemProduto.cs), [entidade](../../src/SquadEstoque.Web/Models/Entities/EntradaFiscal.cs). O parser XML rejeita quantidade não positiva em [EntradasFiscaisController](../../src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs), mas essa validação não é constraint. | Escrita direta pode persistir zero/negativo na origem fiscal. | Tratar em atividade própria, verificando dados prévios antes de definir migration. |
| D02 — alta | `ItemEntradaFiscal.EntradaFiscalId` usa CASCADE no contexto, migration fiscal e snapshot. Não há condição de status na FK. | Excluir uma EntradaFiscal pode apagar seus itens, inclusive de nota confirmada, contrariando preservação restritiva do histórico fiscal. | Definir política de retenção de rascunhos e notas confirmadas e corrigir em atividade própria. |
| D03 — média | RESTRICT não impede alteração/exclusão direta de movimentações; ausência de trigger nas migrations. | Imutabilidade não é garantida integralmente pelo schema; depende também da aplicação e dos acessos ao banco. | Confirmar se a RN-03 deve ser reforçada no banco. |
| D04 — documental | Modelo físico menciona CHECKs de Ativo, defaults e índices auxiliares que não aparecem nas migrations auditadas; sua lista principal de tabelas não contempla toda a extensão fiscal/atendimento. | Documento pode ser interpretado como retrato exato de um schema que ele não representa integralmente. | Atualizar modelagem em cartão próprio. |

A unicidade de e-mail sem normalização é um limite a confirmar com a equipe, não uma violação comprovada de uma regra explícita de comparação sem distinção de caixa. FKs simples de Atendimento e ItemEntradaFiscal também não garantem que o SkuId pertença ao ProdutoId informado; essa consistência deve ser considerada em auditoria de regras cruzadas.

## Verificação e reprodução

Inspeção com `git ls-tree`, `Get-Content` e `rg`, sem execução de SQL ou conexão ao banco. Conferir os nomes na matriz contra contexto, `Up` e snapshot; comparar o modelo alvo do último Designer com o snapshot. Conferir os quinze relacionamentos e suas nulabilidades na migration que os introduziu.

Os testes existentes [EstoqueDomainPersistenceTests](../../tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs) cobrem SKU duplicado e saldo negativo usando `EnsureCreated`; [EnumDomainMigrationTests](../../tests/SquadEstoque.Web.Tests/EnumDomainMigrationTests.cs) e [SkuUltimaAtualizacaoMigrationTests](../../tests/SquadEstoque.Web.Tests/SkuUltimaAtualizacaoMigrationTests.cs) usam migrations em SQLite isolado. Foram lidos como evidência de cobertura disponível, sem execução local neste cartão. Não se deve confundir cobertura do modelo por `EnsureCreated` com validação de toda a cadeia de migrations.

Antes do commit: revisar links relativos, executar `git status --short`, conferir que o diff contém somente este relatório e executar `git diff --check`. No PR, acompanhar o workflow `.NET Build and Tests`, incluindo build, testes e baseline de cobertura. Resultado da execução e revisão devem ser consultados no próprio PR, pois ocorrem após o versionamento deste relatório.

## Evidência para o cartão

Usar o link permanente deste arquivo no commit documental e o PR direcionado a `dev/emmy`. Capturar a tela do PR com os checks e a solicitação de revisão para CodeWitch00. Resumo: quatro regras centrais declaradas; 15 FKs (14 restritivas, uma cascata); cinco migrations em sequência; divergências D01–D04 registradas; nenhuma alteração de banco ou da main. A movimentação do cartão permanece com a responsável.
