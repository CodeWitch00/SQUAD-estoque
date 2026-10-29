# S3-QA-028 — P95 da consulta de estoque

Rastreabilidade: [RNF-02 no SRS](../02-requisitos/srs.md) e
[especificação não funcional](especificacao-testes-nao-funcionais.md), NFT-PERF-01 a 03.
Dependência: consulta e grade implementadas em `dev/emmy` (base `91b9e8c`).

## Protocolo reproduzível

Executar em checkout da branch `test/s3-qa-028-emmy`, com SDK .NET 10,
sem build, suíte de testes ou outra carga concorrente conhecida durante a medição:

```powershell
pwsh -File scripts/medir-rnf02.ps1 -OutputDirectory ./TestResults/rnf02-execucao-01
```

O script compila em Release, executa somente `ConsultaP95Tests` e retorna erro se
a execução ou o critério falhar. Não sobrescreve um relatório anterior. Para uma
nova tentativa, usar outra pasta e conservar os resultados anteriores, inclusive
reprovados. Em CI ou outra máquina, registrar CPU, memória disponível, SDK,
commit e atividades concorrentes; não comparar diretamente ambientes diferentes.

| Parâmetro | Valor fixado |
|---|---|
| Aplicação | ASP.NET Core, ambiente Testing, Release, TestServer no mesmo processo |
| Banco | SQLite em arquivo temporário exclusivo por rodada, pooling desligado; schema EF EnsureCreated |
| Massa | 10.000 produtos ativos; 30.000 SKUs; 1 vendedor; zero atendimentos, vendas e rupturas |
| Dados | IDs e nomes determinísticos; 3 SKUs (37/38/39, saldos 0/1/2) por produto; atualização UTC fixa |
| Cenários | `unico`: 1 produto; `dezgrupo`: 10; `cinquentagrupo`: 50; `inexistente`: 0; `unico` + produto 1: grade com 3 SKUs |
| Rodadas | 3; cada rodada recria banco, aplicação e sessão |
| Aquecimento | 20 consultas por cenário, intercaladas na ordem da tabela, antes da coleta |
| Medição | 100 consultas por cenário por rodada, na mesma ordem intercalada; 1 usuário, sem pausa artificial |
| Total | 300 aquecimentos preservados e 1.500 amostras medidas |
| Cache | Nenhum cache adicional; JIT, EF, Razor e cache do SO aquecidos pelo protocolo; sem limpeza entre requisições |
| Autenticação | Login por cookie e antiforgery real, fora do intervalo medido |
| Timeout | 30 s por requisição; exceção é registrada e reprova a execução |

Os nomes de grupo foram escolhidos para produzir cardinalidades verificáveis sem
correspondência aproximada entre grupos. Cada resposta valida HTTP 200 e a
quantidade de produtos; a busca vazia valida a mensagem e a grade valida três SKUs.
Não há alterações no código de produção nem nos dados de outros ambientes.

## Relógios e critério

`ServerMs`: Stopwatch em middleware exclusivo do teste, envolvendo o pipeline
da aplicação, incluindo autenticação, controller, EF e renderização Razor até o
retorno do pipeline. Não é apenas duração SQL.

`ClientMs`: Stopwatch do envio até a leitura completa do HTML pelo HttpClient do
TestServer, antes das validações do conteúdo. Inclui o transporte em memória e
leitura; não inclui rede real, debounce, JavaScript, recursos externos ou pintura
do navegador. Não representa o tempo percebido por um usuário mobile.

Para cada cenário **e cada rodada**, ordenar todas as 100 amostras medidas em
ordem crescente. O P95 por nearest-rank é a posição `ceil(0,95 × N)` (posição 95
para N=100, índice 94). Não aparar, filtrar nem substituir tempos altos. O CSV
armazena seis casas decimais; o JSON preserva a precisão usada no cálculo.
Aquecimento fica em seção própria, definido antes da execução, sem seleção por
latência. Se faltar relógio de servidor, o relatório conserva o tempo do cliente
como substituto no agregado e reprova a amostra, sem apresentá-la como medição
válida do servidor.

Passa somente com **todos os 15 P95 de servidor estritamente menores que 500 ms**,
zero erro HTTP/funcional e aquecimento válido. `500 ms` reprova. As verificações
ocorrem depois da gravação das amostras e do resumo; um teste vermelho mantém as
evidências. Falha de preparação (login/schema/seed) gera erro de teste e não deve
ser interpretada como aprovação ou medição completa.

## Limites e validação complementar

Esta é uma linha de base local de RNF-02 com um usuário. Não conclui o aceite de
capacidade de NFT-PERF-01/03: ainda é necessário executar em homologação a carga
de 20 usuários, aquecimento de 1 min, rampa de 2 min e estabilidade de 5 min,
repetida três vezes. Não extrapolar os tempos locais para Azure ou produção.

Para NFT-PERF-02, no navegador mobile registrar aparelho, navegador, viewport,
latência e banda da rede. Com cache frio na primeira consulta, medir do comando
de busca até o resultado pintado, inclusive mensagens sem resultado; anexar trace
e comparar com 3 s. Registrar separadamente a requisição e o tempo visual.
Essa validação de rede/navegador permanece pendente neste cartão de medição local.

## Evidência da execução

Os arquivos [resultado.json](evidencias/s3-qa-028/resultado.json) e
[amostras.csv](evidencias/s3-qa-028/amostras.csv) contêm ambiente, horários,
commit da aplicação, todos os valores de aquecimento e medição e os 15 P95.
O relatório abaixo registra os valores efetivamente obtidos.

### Resultado local — 29/10/2026

**APROVADO nesta linha de base:** 15/15 P95 < 500 ms; 1.500/1.500 respostas válidas; 300/300 aquecimentos válidos.

Ambiente: Windows 10.0.19045, SDK 10.0.400, runtime .NET 10.0.11, x64, 4 processadores lógicos, CPU Intel64 Family 6 Model 60 Stepping 3, memória disponível ao runtime 8.261.124.096 bytes. Banco e aplicação na mesma máquina; sem carga de teste concorrente conhecida. Nenhum limite de CPU/memória ou isolamento do sistema operacional foi imposto.

Coleta: 15:14:33 a 15:16:18 (America/Sao_Paulo). Aplicação medida: commit `91b9e8caa270fca1b32051676406f30a8252a073`. Cada rodada inclui preparação fora das amostras cronometradas.

| Rodada | Cenário | N | Erros | P95 servidor (ms) | P95 cliente (ms) | Máximo servidor (ms) |
|---|---|---:|---:|---:|---:|---:|
| 1 | um | 100 | 0 | 21.707 | 21.830 | 47.895 |
| 1 | dez | 100 | 0 | 125.270 | 125.412 | 177.806 |
| 1 | cinquenta | 100 | 0 | 163.848 | 164.938 | 210.638 |
| 1 | vazio | 100 | 0 | 21.189 | 21.403 | 53.362 |
| 1 | grade | 100 | 0 | 27.005 | 27.131 | 43.092 |
| 2 | um | 100 | 0 | 16.298 | 16.391 | 18.035 |
| 2 | dez | 100 | 0 | 92.677 | 92.784 | 114.008 |
| 2 | cinquenta | 100 | 0 | 107.741 | 107.845 | 125.773 |
| 2 | vazio | 100 | 0 | 17.262 | 17.355 | 19.298 |
| 2 | grade | 100 | 0 | 20.353 | 20.462 | 28.745 |
| 3 | um | 100 | 0 | 13.733 | 13.810 | 19.489 |
| 3 | dez | 100 | 0 | 96.132 | 96.255 | 106.910 |
| 3 | cinquenta | 100 | 0 | 102.815 | 102.900 | 152.732 |
| 3 | vazio | 100 | 0 | 14.428 | 14.511 | 19.781 |
| 3 | grade | 100 | 0 | 18.358 | 18.452 | 24.361 |

| Cenário | Mediana dos 3 P95 servidor (ms) | Pior P95 servidor (ms) |
|---|---:|---:|
| cinquenta | 107.741 | 163.848 |
| dez | 96.132 | 125.270 |
| grade | 20.353 | 27.005 |
| um | 16.298 | 21.707 |
| vazio | 17.262 | 21.189 |

Validação: build da aplicação aprovado (zero erros); suíte funcional com 280 aprovados, zero falhos e 1 ignorado (desempenho opt-in); execução Release isolada com 2/2 testes aprovados (medição e cálculo do percentil). `git diff --check` aprovado. Aviso NU1900 local: consulta de vulnerabilidades do NuGet indisponível, inclusive na tentativa de restauração com rede liberada.

Conferência manual da evidência: CSV com 1.800 linhas de dados, 1.500 classificadas como medição; ordenação independente dos valores por rodada/cenário reproduziu todos os 15 P95 do JSON com tolerância de 0,000001 ms. Sintaxe do script PowerShell validada. Não foi executado teste visual/mobile; o limite de 3 s e a carga de homologação continuam sem aceite nesta evidência.
