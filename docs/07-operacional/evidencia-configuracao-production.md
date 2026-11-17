# Evidencia — configuracao de Production

Data: 11/11/2026. Area: Operacoes. Branch: `dev/emmy`.

## Comportamento validado

- Production usa a configuracao padrao do ASP.NET Core e recebe os valores por variaveis de ambiente.
- Connection string retirada do `appsettings.json`; default local restrito a Development.
- `ConnectionStrings__EstoqueContext` e `DataProtection__KeysPath` obrigatorias em Production.
- Caminhos relativos e SQLite em memoria rejeitados em Production.
- Mensagens de erro identificam a chave sem imprimir o valor recebido.
- Exemplo operacional sem segredos: [configuracao-production.md](configuracao-production.md).

## Validacao manual com processo real

Executado com `ASPNETCORE_ENVIRONMENT=Production`, banco temporario isolado e porta livre:

| Cenario | Resultado observado |
| --- | --- |
| Duas variaveis obrigatorias configuradas | `GET /health`: `200 Healthy`; banco e diretorio de chaves criados |
| Connection string removida | Processo encerra com `Configuracao obrigatoria ausente: ConnectionStrings__EstoqueContext.` |
| Diretorio de chaves removido | Processo encerra com `Configuracao obrigatoria ausente: DataProtection__KeysPath.` |

## Revisao tecnica

Conferidos o diff, a precedencia da configuracao padrao, o ponto de validacao
antes das migrations e a ausencia de segredos/caminhos pessoais nos arquivos do escopo.
As factories removem a configuracao original do DbContext ao substituir o banco;
os testes de inicializacao usam bancos sem pooling para liberar arquivos no Windows.

## Validacao automatizada

Os nove cenarios de `ProductionConfigurationTests` cobrem inicializacao real,
configuracoes ausentes/vazias, SQLite invalido, caminhos relativos e banco em memoria.
A execucao focada de inicializacao aprovou os 15 casos, incluindo seed por ambiente.

Resultado local final:

| Verificacao | Resultado |
| --- | --- |
| `git diff --check` | Aprovado |
| `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj` | Aprovado; 0 avisos, 0 erros |
| `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj` | 327 aprovados, 0 falhas, 1 ignorado; total 328 |

O teste ignorado pertence a medicao optativa de performance ja existente na suite.
A revisao de CodeWitch00 e o resultado da CI devem ser conferidos no Pull Request.

## Ajustes da revisao em 14/11/2026

O relatorio S3-QA-035 foi retirado do escopo do PR #104 e preservado, sem alteracoes
de conteudo, no [PR #105](https://github.com/CodeWitch00/SQUAD-estoque/pull/105).

`AllowedHosts` passou a ser obrigatoria em Production. O fallback `*` foi removido
do `appsettings.json`. A lista deve conter hosts explicitos separados por `;`, sem
espacos, esquema, porta ou caminho. Curingas e enderecos que desabilitam o filtro
de hosts sao rejeitados antes das migrations.

O Compose usa somente hosts de loopback por padrao; acesso por dominio ou IP da
rede exige configurar a lista. O runbook Azure foi atualizado com a configuracao
que deve existir no App Service antes da publicacao. Nenhuma configuracao do
ambiente remoto foi alterada nesta atividade.

### Validacao manual do filtro de hosts

Processo real em Production, com banco temporario, porta livre e lista explicita
`localhost;127.0.0.1;[::1];estoque.example.test`:

| Cenario | Resultado observado |
| --- | --- |
| `GET /health` com Host `127.0.0.1` | HTTP 200 Healthy |
| `GET /health` com Host `estoque.example.test` | HTTP 200 Healthy |
| `GET /health` com Host fora da lista | HTTP 400 |
| `AllowedHosts` ausente | Inicializacao recusada com `Configuracao obrigatoria ausente: AllowedHosts.` |
| `AllowedHosts` igual a `*`, `localhost;*`, `0.0.0.0` ou `[::]` | Inicializacao recusada com mensagem clara identificando AllowedHosts |

### Validacao automatizada da revisao

A execucao focada aprovou 31 testes: configuracao de Production, inicializacao por
ambiente e primeiro acesso do lojista. Foram adicionados 14 casos de AllowedHosts
ausente ou invalido. O caso positivo verifica a lista de hosts permitidos e a
rejeicao de um Host fora da lista pelo middleware padrao do ASP.NET Core.

Resultado da revisao em 14/11/2026:

| Verificacao | Resultado |
| --- | --- |
| `git diff --check` | Aprovado |
| Build da aplicacao | Aprovado; 0 avisos, 0 erros |
| Suite completa | 341 aprovados, 0 falhas, 1 ignorado; total 342 |
| Testes focados de inicializacao | 31 aprovados, 0 falhas |
| Filtro de hosts em processo Production real | Hosts autorizados com HTTP 200; host fora da lista com HTTP 400 |

O unico teste ignorado continua sendo a medicao optativa de performance preexistente.
