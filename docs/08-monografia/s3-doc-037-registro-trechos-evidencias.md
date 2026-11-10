# S3-DOC-037 — Registro de trechos e evidências da atualização da monografia

> **Status:** registro de trabalho em revisão.
> **Não é declaração de conclusão do projeto, de publicação ou de homologação.**
> **Não substitui a revisão por outro integrante nem a declaração de conclusão parcial assinada.**

| Campo | Valor |
| --- | --- |
| Cartão | `[S3-DOC-037] Atualizar monografia — resultados e arquitetura implementada` |
| Sprint | Sprint 3 |
| Reunião-alvo | 09/11/2026 |
| Prazo formal | 11/11/2026, 23:59 (America/Sao_Paulo) |
| Área principal | Documentação |
| Branch sugerida | `docs/s3-doc-037-integrante` |
| Baseline de código conferida | `main` em `c6cfdad` (08/11/2026) |
| Data desta revisão | 09/11/2026 |
| Responsável | integrante que assumir o cartão |

## 1. Objetivo e limite do artefato

Este registro documenta a atualização incremental da monografia do Sistema SQUAD com a
arquitetura e os resultados efetivamente comprovados até a Sprint 3. O trabalho foi
limitado a:

- alterar somente trechos afetados pela implementação real;
- incorporar integridade, relatórios e testes com evidências verificáveis;
- diferenciar **Concluído**, **Parcial** e **Pendente**;
- preservar a estrutura acadêmica e o histórico das versões anteriores;
- não antecipar publicação, homologação ou resultados finais.

A revisão S3 recebida já continha a maior parte das atualizações de arquitetura,
modelo de dados, integridade, matriz de rastreabilidade, resultados e limitações.
Esta entrega faz a auditoria de consistência, corrige referências internas,
explicita a conciliação entre as numerações de regras de negócio e registra as
evidências utilizadas. Os status de implementação não foram alterados. Nesta
etapa, além da conciliação numérica, da referência interna e da paginação, a
baseline foi atualizada para a `main` `c6cfdad` e a contagem da suíte passou de
306 para 310 testes, incorporando as evidências da S3-QA-029 e da S3-QA-030.

## 2. Arquivos e versionamento

| Arquivo | Papel | Situação |
| --- | --- | --- |
| `docs/08-monografia/3ºsemestre/MONOGRAFIA FINAL SQUAD.docx` | Versão original anterior à revisão S3 | Preservado, não editado |
| `docs/08-monografia/3ºsemestre/MONOGRAFIA FINAL SQUAD - REVISAO S3.docx` | Revisão S3 recebida | Preservado sem alteração; SHA-256 `95BD8A1CCA498EF73888D5173799CEEDA72A58298507640906FEB69395C41FF8` |
| `docs/08-monografia/3ºsemestre/MONOGRAFIA FINAL SQUAD - REVISAO S3 - DOC-037.docx` | Entrega deste cartão | Criado a partir da revisão S3; SHA-256 `C526A64CAFA9C2261C8F0A7A0D168A04A48DD8592214D6A3BFE276783AD3DCB7` |
| `docs/08-monografia/s3-doc-037-registro-trechos-evidencias.md` | Este registro | Em elaboração/revisão |

A opção por criar um arquivo versionado, em vez de sobrescrever a revisão S3,
preserva o histórico e permite comparar as duas versões. A revisão S3 permanece
byte a byte igual à recebida. Após a revisão por outro integrante, a equipe pode
decidir manter apenas a versão DOC-037 como referência corrente.

## 3. Método de trabalho

1. Extração de parágrafos e tabelas das duas versões com `python-docx`.
2. Comparação textual entre a versão original e a revisão S3 com `difflib`.
3. Auditoria de títulos, legendas de Quadro/Figura, referências internas e
   códigos RF/RN.
4. Conferência de cada número citado na monografia contra o arquivo de evidência
   correspondente.
5. Correção apenas de inconsistências documentais e inserção da nota de
   conciliação numérica.
6. Reextração da versão DOC-037 para confirmar as alterações.
7. Verificação de hash para garantir que a revisão S3 não foi sobrescrita.
8. Abertura da versão DOC-037 no Word para conferir a página real de cada
   legenda e atualizar as listas de figuras e quadros.

## 4. Trechos alterados nesta entrega

| Local | Situação anterior na revisão S3 | Alteração aplicada | Motivo | Rastreabilidade | Status |
| --- | --- | --- | --- | --- | --- |
| Cap. 4, após a RN12 | Lista original RN01–RN12 sem correspondência explícita com o SRS | Inserção da “Nota de conciliação numérica”, em dois parágrafos | Evitar que o mesmo código RN tenha sentido diferente nos Capítulos 5 e 6 | Lista original do Cap. 4; Seção 6 do SRS | Concluído (documentação) |
| Cap. 5, Tecnologias Utilizadas | “como descrito na seção 5.3.2.” | “como descrito na subseção Concorrência.” | Os títulos não possuem numeração automática; a referência numérica era ambígua | Estrutura de títulos do próprio documento | Concluído (documentação) |
| Cap. 5, Integridade Operacional | “descrita na seção 5.3.2:” | “descrita na subseção Atomicidade:” | Mesma razão | Estrutura de títulos do próprio documento | Concluído (documentação) |
| Cap. 5, Coerência entre Requisitos e Modelagem | “discutido na seção 5.3.3:” | “discutido na subseção Integridade Operacional:” | Mesma razão | Estrutura de títulos do próprio documento | Concluído (documentação) |
| Cap. 6, Estratégia de Validação | “na seção 6.6 e no Quadro 9.” | “na subseção Limitações e no Quadro 9.” | Mesma razão | Estrutura de títulos do próprio documento | Concluído (documentação) |
| Listas de figuras e quadros | Paginação manual desatualizada (ex.: Quadro 9 em p. 66) | Números de página atualizados a partir da paginação real do Word (Quadro 9 em p. 76) | A inserção da nota e o acréscimo de conteúdo na revisão S3 alteraram a paginação | Verificação via Word, página de cada legenda | Concluído (formatação) |

A nota de conciliação registra a correspondência entre os dois conjuntos:

| Regra original (Cap. 4) | Regra do SRS usada nos Cap. 5–6 | Observação |
| --- | --- | --- |
| RN03 — Validação de Estoque | RN-02 — Saldo nunca negativo | Correspondência direta |
| RN04 — Atomicidade das Movimentações | RN-07 — Atomicidade nas saídas | Correspondência direta |
| RN05 — Imutabilidade do Histórico | RN-03 — Movimentação imutável | Correspondência direta |
| RN06 — Ajuste Manual de Estoque | RN-04 — Ajuste manual restrito | Correspondência direta |
| RN07 — Cadastro Único de Produtos | RN-01 — SKU como unidade mínima de estoque | Correspondência parcial: a unicidade do SKU é o aspecto comum |
| RN01, RN02, RN08, RN09, RN10, RN11 e RN12 | RF-09, RF-10, RF-13, RF-14, RF-21 e RF-22; restrições de modelagem | No SRS, esses temas não aparecem como regras de negócio numeradas |
| — | RN-05, RN-06, RN-08, RN-09, RN-10 e RN-11 | Sem correspondente direto na lista original |

## 5. Conteúdo já incorporado na revisão S3 e preservado

| Seção | Conteúdo incorporado | Evidência | Status |
| --- | --- | --- | --- |
| Resumo e abstract | 310 aprovados, 0 falhas, 1 ignorado; 30/30 vendas simultâneas; 15/15 P95 < 500 ms | `s3-fe-031/README.md`; `s3-qa-003-venda-simultanea.md`; `s3-qa-028-p95-consulta.md` | Concluído (local) para suíte e concorrência; Parcial para P95 |
| Metodologia | Etapa de implementação e validação com evidências locais | Revisão S3 | Concluído (documentação) |
| Arquitetura | MVC, ASP.NET Core, Razor, EF Core, SQLite, autenticação por cookie | `docs/04-arquitetura/arquitetura.md`; código-fonte | Concluído (local) |
| Modelo de dados | Nove entidades, constraints, índices únicos e parciais | Migrations; `s3-db-026-indices-consulta.md` | Concluído (local) |
| Integridade e concorrência | Transação explícita, atualização condicional, CHECK e imutabilidade | `Data/SaidaEstoque.cs`; `s3-qa-003-venda-simultanea.md` | Concluído (local) |
| Rastreabilidade | Quadro 8 com requisito, implementação, evidência e status | Testes automatizados; documentos de evidência | Concluído (local) para as linhas verificadas; Parcial nas linhas pendentes |
| Evidências experimentais | Quadro 9 com resultados e limites | Fontes indicadas no próprio quadro | Misto, conforme a linha |
| Limitações e considerações finais | Pendências de publicação, carga, mobile, backup e homologação fiscal | Registro de validações; `s3-qa-028`; `s3-fe-031` | Concluído (documentação) |

## 6. Registro de evidências

| Afirmação na monografia | Valor conferido | Fonte | Ambiente/data | Status | Limite |
| --- | --- | --- | --- | --- | --- |
| Suíte automatizada | 310 aprovados, 0 falhas, 1 ignorado (311 no total) | Execução local em `main` `c6cfdad`; GitHub Actions run 42512546507 | Windows, .NET SDK 10.0.403, 09/11/2026 | Concluído (local) | O resultado oficial por commit é o do GitHub Actions; não é homologação |
| Regressão de segurança do login (S3-QA-029) | 3 testes de regressão: senha inválida de usuário existente, POST de login sem antiforgery e POST de logout sem antiforgery | `AuthenticationAuthorizationTests.cs`, commit `748572f` | Windows, .NET SDK 10.0.403, 09/11/2026 | Concluído (local) | Cobre o servidor; não substitui teste de segurança ofensiva nem homologação |
| Regressão E2E vendedor e lojista (S3-QA-030) | 7/7 casos aprovados após o reteste do E2E-02; aceite formal não declarado | `docs/09-testing/execucoes/sprint-3/s3-qa-030-regressao-e2e.md` | Windows, .NET SDK 10.0.403, 02, 03 e 07/11/2026 | Concluído (local) / aceite pendente | Sem capturas em todos os casos, ambiente publicado e revisão independente |
| Venda simultânea | 30/30 repetições com uma venda aceita, uma rejeitada e saldo final zero | `docs/09-testing/execucoes/sprint-3/s3-qa-003-venda-simultanea.md` | Windows, SDK 10.0.401, base `3609f8b`, 11/10/2026 | Concluído (local) | Não é teste manual em dois navegadores nem teste de carga |
| P95 da consulta | 15/15 P95 de servidor < 500 ms; pior 163,848 ms; 1.500/1.500 respostas válidas | `docs/09-testing/s3-qa-028-p95-consulta.md` | Windows, SDK 10.0.400, 29/10/2026, um usuário | Parcial | Carga de 20 usuários e critério de 3 s em rede móvel real permanecem pendentes |
| Índices de consulta | Planos trocam `SCAN` por `SEARCH`; migration `AddConsultaIndexes` | `docs/09-testing/s3-db-026-indices-consulta.md` | SQLite 3.45.1, ambiente local | Concluído (local) | Os planos não medem tempo HTTP nem garantem ganho em produção |
| Relatórios do lojista | Larguras 320, 390, 768 e 1440 px sem rolagem horizontal; erro HTTP 503 | `docs/05-ux/evidencias/s3-fe-031/README.md` | Edge headless, 01/11/2026 | Parcial | Dispositivo físico e leitor de tela não testados |
| CI | Build, testes, cobertura mínima de 50% e deploy condicional | `.github/workflows/dotnet.yml` | Configuração do repositório | Concluído (configuração) | Deploy e homologação do ambiente publicado não concluídos |
| Publicação e homologação no Azure | Baseline `c6cfdad` verificada localmente e publicada pela esteira; homologação manual pendente | Registro de validações manuais; GitHub Actions run 42512546507 | — | Pendente (homologação) | O ambiente publicado exibia versão antiga da grade |
| Backup e restauração | Não testados | — | — | Pendente | — |
| Carga de 20 usuários | Não executada | `s3-qa-028-p95-consulta.md` | — | Pendente | — |
| RNF-02 em rede móvel real | Não executado | `s3-qa-028-p95-consulta.md` | — | Pendente | — |
| Dispositivo físico e leitor de tela | Não realizados | `s3-fe-031/README.md` | — | Pendente | — |
| Homologação fiscal da NF-e | Não realizada | `EntradaFiscalHttpTests` | — | Pendente | — |
| Declaração de conclusão parcial assinada | Não localizada no repositório | Avaliação P2 | — | Pendente | — |

### 6.1 Verificação independente desta revisão

Em 09/11/2026, a suíte foi executada novamente durante esta revisão com
`dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --nologo`,
SDK 10.0.403. Resultado: **306 aprovados, 0 falhas, 1 ignorado (307 no total)**,
duração de 1 m 19 s. O teste ignorado é
`ConsultaP95Tests.RNF02_consulta_p95_must_be_below_500ms`, executado separadamente.
Essa execução confirma o número citado na monografia, mas não substitui o
resultado oficial do GitHub Actions por commit nem a homologação.

## 7. Síntese de status

### Concluído

- Arquitetura MVC/ASP.NET Core/Razor/EF Core/SQLite implementada e documentada.
- Modelo de dados com nove entidades, constraints e índices.
- Transação explícita, atualização condicional e imutabilidade das movimentações.
- Suíte automatizada com 306 aprovados, 0 falhas e 1 ignorado na execução de referência.
- Concorrência de venda simultânea verificada em 30/30 repetições.
- Relatórios de saldos zerados e rupturas implementados e verificados em quatro larguras.
- CI com build, testes e cobertura mínima configurada.

### Parcial

- P95 local de 15/15 cenários abaixo de 500 ms, com um usuário e banco local.
- Relatórios responsivos validados por automação, sem dispositivo físico ou leitor de tela.
- Rastreabilidade com linhas concluídas e linhas ainda pendentes de homologação.
- Entrada fiscal implementada e testada localmente, sem homologação fiscal.

### Pendente

- Publicação e homologação da baseline `9111713` no Azure.
- Reteste de persistência após a publicação.
- Backup e restauração do arquivo SQLite.
- Teste de carga com 20 usuários, aquecimento, rampa e estabilidade.
- Medição do RNF-02 em rede móvel real, incluindo o critério de 3 segundos percebidos.
- Validação em dispositivo físico e com leitor de tela.
- Homologação fiscal da NF-e.
- Declaração de conclusão parcial assinada.

## 8. O que não foi alterado e o que não pode ser afirmado

- A versão original `MONOGRAFIA FINAL SQUAD.docx` não foi editada.
- A revisão S3 recebida não foi sobrescrita.
- Os quadros de requisitos funcionais, requisitos não funcionais e regras de
  negócio do Capítulo 4 preservam a organização original.
- Nenhum resultado ou status de implementação foi modificado nesta entrega; as
  alterações são de conciliação numérica, referência interna e atualização de
  paginação.
- A monografia não afirma ganho de receita, redução de perdas ou melhoria de
  eficiência em loja.
- A monografia não declara publicação, homologação, aceite de terceiros ou
  conclusão final do projeto.
- A execução local de testes não substitui o resultado oficial do GitHub Actions
  por commit nem a revisão por outro integrante.

## 9. Checagens de revisão acadêmica

- Estrutura preservada: elementos pré-textuais, sete capítulos, referências,
  glossário e apêndices.
- Legendas de Quadro 1 a 9 e de Figura 1 a 11 presentes e coerentes com o corpo.
- Listas de figuras e quadros conferidas contra a paginação real do Word: 11
  figuras e 9 quadros, com os 20 números de página atualizados.
- Referências internas convertidas de numeração de seção para nome de subseção,
  evitando ambiguidade em um documento sem numeração automática de títulos.
- Citações e referências bibliográficas preservadas; não foi necessária nova
  referência para as correções desta entrega.
- Status **Concluído**, **Parcial** e **Pendente** usados de forma consistente
  no Quadro 9, na matriz de rastreabilidade e neste registro.
- Nenhuma linguagem de resultado final, publicação ou homologação foi introduzida.

## 10. Reprodução e verificação

Na raiz do repositório:

```powershell
# Suíte completa
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj

# Build da aplicação
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore

# Medição local do P95 (RNF-02)
pwsh -File scripts/medir-rnf02.ps1 -OutputDirectory ./TestResults/rnf02-execucao-01

# Concorrência isolada (S3-QA-003)
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build --configuration Release --filter FullyQualifiedName~Concurrent_saidas --logger "trx;LogFileName=concorrencia.trx" --results-directory TestResults/s3-qa-003

# Verificação de whitespace
git diff --check
```

A execução local pode diferir do CI. Para a monografia, a fonte oficial por commit
é o resultado do GitHub Actions, não uma execução isolada.

Conferência dos arquivos:

```powershell
Get-FileHash "docs/08-monografia/3ºsemestre/MONOGRAFIA FINAL SQUAD - REVISAO S3.docx" -Algorithm SHA256
Get-FileHash "docs/08-monografia/3ºsemestre/MONOGRAFIA FINAL SQUAD - REVISAO S3 - DOC-037.docx" -Algorithm SHA256
```

Conferência da paginação das listas (Word):

1. Abrir a versão DOC-037 no Microsoft Word e atualizar os campos do documento.
2. Para cada legenda de Figura 1–11 e Quadro 1–9, registrar a página exibida em
   `Range.Information(3)`.
3. Comparar o número obtido com o número lançado na lista de figuras e na lista
   de quadros.
4. Corrigir apenas as entradas divergentes e salvar.

Nesta revisão, as 20 entradas foram conferidas por esse procedimento. A lista
recebida tinha paginação anterior à inserção da nota e ao acréscimo de conteúdo
da revisão S3; a maior divergência era o Quadro 9, listado na página 66 e
localizado na página 76. As listas não são campos automáticos: os números são
texto manual e precisam ser revalidados sempre que o corpo do documento mudar.

## 11. Pendências e próximos passos

1. Revisão da versão DOC-037 por outro integrante.
2. Publicação de uma versão identificada no Azure e homologação da baseline.
3. Reteste de persistência após a publicação.
4. Teste de carga com 20 usuários e medição do RNF-02 em rede móvel real.
5. Validação em dispositivo físico e com leitor de tela.
6. Teste de backup e restauração do SQLite.
7. Homologação fiscal da NF-e.
8. Assinatura da declaração de conclusão parcial.
9. Criação do commit e do Pull Request na branch `docs/s3-doc-037-integrante`,
   somente após a revisão.
10. Decisão da equipe sobre manter a revisão S3 ao lado da versão DOC-037 ou
    substituí-la após o aceite.

## 12. Histórico de versões

| Versão | Data | Origem | Alteração |
| --- | --- | --- | --- |
| Original | 06/11/2026 | `MONOGRAFIA FINAL SQUAD.docx` | Versão anterior preservada |
| Revisão S3 | 08/11/2026 | `MONOGRAFIA FINAL SQUAD - REVISAO S3.docx` | Atualizações de arquitetura, resultados, integridade e limitações |
| DOC-037 | 09/11/2026 | `MONOGRAFIA FINAL SQUAD - REVISAO S3 - DOC-037.docx` | Nota de conciliação RN, correção de referências internas e atualização da paginação das listas de figuras e quadros |
