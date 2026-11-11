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
