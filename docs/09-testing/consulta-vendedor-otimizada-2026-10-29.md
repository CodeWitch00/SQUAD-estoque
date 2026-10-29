# Otimização da consulta do vendedor — 29/10/2026

## Escopo e comportamento

A retomada de atendimento projeta somente identificação, produto e campos da grade usados pelos ViewModels. O filtro de SKUs ativos e a ordenação por numeração agora são executados no SQLite, sem materializar entidades completas. A busca normaliza e divide o termo uma vez por requisição; as palavras dos campos são divididas apenas quando a correspondência aproximada precisa delas, uma vez por produto.

Preservados: filtros de produtos e SKUs, busca por nome/marca/categoria/cor, acentos, correspondência aproximada, ordenação, mensagens, autorização, proprietário/status do atendimento, numeração selecionada, saldos e data da última atualização. Nenhuma migration ou alteração de schema.

## Comparação reproduzível

Base anterior: `95199c4` (main remota consultada em 29/10/2026). Implementação e evidências na `dev/emmy`; a branch main local não foi atualizada.

Ambiente: Windows, SDK .NET 10.0.400, runtime 10.0.11, EF Core/SQLite, build Debug. Medição isolada com `DOTNET_TieredCompilation=0`, 5 execuções de aquecimento e 30 medidas por massa. Valores são medianas; alocações medidas com `GC.GetAllocatedBytesForCurrentThread` (as operações SQLite deste teste concluem sincronamente). O intervalo inclui o controller e as mesmas verificações do ViewModel, não inclui criação do banco nem seed. Não representa latência HTTP, rede móvel ou SLA de produção.

O teste `ConsultaQueryPerformanceTests` usa SQLite em memória, IDs fixos, um vendedor, um atendimento pendente e um produto com 4 ou 200 SKUs. Metade dos SKUs está ativa; saldos alternam entre 0, 1 e 2; data de atualização fixa. Antes/depois usam a mesma massa e verificam os mesmos valores retornados. Na versão anterior, somente as duas verificações específicas da nova projeção SQL foram omitidas; elas ficam fora do intervalo cronometrado. O binário foi recompilado ao alternar versões.

| Massa | Antes | Depois | Variação |
| --- | ---: | ---: | ---: |
| 4 SKUs / 2 ativos: tempo | 1,091 ms | 0,665 ms | -39,0% |
| 4 SKUs / 2 ativos: bytes alocados | 60.896 | 69.952 | +14,9% |
| 200 SKUs / 100 ativos: tempo | 3,776 ms | 2,161 ms | -42,8% |
| 200 SKUs / 100 ativos: bytes alocados | 631.240 | 403.208 | -36,1% |
| Consultas por retomada, ambas as massas | 2 | 2 | constante |
| Colunas retornadas pela consulta da grade | 22 | 13 | -40,9% |
| Linhas da grade, massa de 200 SKUs | 200 | 100 | -50,0% |

A projeção tem custo fixo adicional de alocação na massa pequena; não há redução de memória nesse cenário. Tempos locais variam entre execuções. A melhoria estrutural é a redução dos campos e das linhas transferidas, sem consultas por SKU. A busca com produto selecionado permanece com três consultas (pendências, produtos e grade). O ganho da normalização da busca não foi cronometrado separadamente.

Reprodução da versão otimizada, no PowerShell:

```powershell
$env:DOTNET_TieredCompilation='0'
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --filter FullyQualifiedName~ConsultaQueryPerformanceTests --logger "console;verbosity=detailed"
Remove-Item Env:DOTNET_TieredCompilation
```

Saídas com SQL: [antes](evidencias/consulta-otimizada/antes.log) e [depois](evidencias/consulta-otimizada/depois.log).

## Validação

- Baseline: 277 testes aprovados, sem falhas ou ignorados.
- Build Web: aprovado, zero avisos e erros ([log](evidencias/consulta-otimizada/build.log)).
- Suíte final: 279 aprovados, zero falhas e ignorados ([log](evidencias/consulta-otimizada/suite.log)).
- Novos testes: igualdade da grade ativa e ordenada, saldos/datas/seleção, ausência de rastreamento, quantidade constante de consultas, projeção SQL, busca sem acento com seleção de produto e rejeição de atendimento concluído ou de outro vendedor.
- `git diff --check`: aprovado.

Validação manual em banco local exclusivo, com dados fictícios, pela publicação local em `http://localhost:5199`, perfil VENDEDOR:

| Cenário | Resultado observado |
| --- | --- |
| Acesso sem sessão | Redirecionamento para login |
| Busca `tenis runner` | Encontrou `Tênis Runner` |
| Abrir produto | Grade com 38/1 (Último par), 39/2 (Disponível), 37/0 (Indisponível); SKU 40 inativo ausente |
| Retomar atendimento pendente | Produto e número 37 preservados; venda desabilitada com saldo zero |
| Busca `zzinexistente` | Mensagem `Produto não encontrado.` preservada |

Capturas: [grade](evidencias/consulta-otimizada/grade.png) e [retomada](evidencias/consulta-otimizada/retomada.png).

Não foram efetuadas vendas, rupturas ou movimentações no Trello. A validação visual usou banco criado por `EnsureCreated` para isolar esta tarefa de um erro preexistente ao aplicar migrations em banco vazio; o schema compartilhado não foi alterado.

## Evidência para o cartão

Consulta do vendedor otimizada na dev/emmy. Retomada usa projeção de 13 colunas (antes 22), filtra SKUs ativos no banco e mantém duas consultas com 4 ou 200 SKUs. Na massa de 200 SKUs, a mediana caiu de 3,776 para 2,161 ms (-42,8%) e as alocações de 631.240 para 403.208 bytes (-36,1%). Build aprovado; 279 testes aprovados; busca, grade e retomada verificadas no navegador. Logs e capturas anexos neste documento. Na massa de 4 SKUs, as alocações aumentaram 14,9%; desempenho de produção não foi medido.
