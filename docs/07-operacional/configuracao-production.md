# Configuracao de Production por variaveis de ambiente

A aplicacao usa a configuracao padrao do ASP.NET Core (`WebApplication.CreateBuilder`).
Variaveis de ambiente sobrescrevem os arquivos `appsettings`; use `__` para separar
secoes. Nao existe carregador de `.env` na aplicacao. Configure os valores no
servico que executa o processo, no App Service ou no Compose.

## Exemplo sem segredos

Exemplo Linux para um volume persistente montado em `/var/lib/squad-estoque`:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__EstoqueContext='Data Source=/var/lib/squad-estoque/Estoque.db'
export DataProtection__KeysPath=/var/lib/squad-estoque/keys
export ASPNETCORE_URLS=http://0.0.0.0:8080
export AllowedHosts=estoque.example.test
export Logging__LogLevel__Default=Information
dotnet SquadEstoque.Web.dll
```

Os caminhos sao exemplos de infraestrutura, sem informacoes pessoais. No Windows,
use caminhos absolutos do volume do servico. Provisione o diretorio pai do banco e
permissoes de leitura/escrita para a identidade que executa a aplicacao. O diretorio
de chaves e criado pela aplicacao; preserve-o entre reinicios e publicacoes.
Nao registre valores reais de connection strings, tokens ou senhas em commits ou logs.

| Variavel | Regra em Production |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production`; remova `DOTNET_ENVIRONMENT` conflitante, pois ele tem precedencia no host padrao |
| `ConnectionStrings__EstoqueContext` | Obrigatoria; SQLite com arquivo persistente e caminho absoluto; banco em memoria e caminho relativo sao rejeitados |
| `DataProtection__KeysPath` | Obrigatoria; diretorio persistente com caminho absoluto para as chaves dos cookies |
| `ASPNETCORE_URLS` | Endereco/porta do processo, conforme a infraestrutura |
| `AllowedHosts` | Obrigatoria; lista explicita de hosts separados por `;`, sem espacos, esquema, porta ou caminho. Curingas como `*`, `0.0.0.0` e `[::]` sao rejeitados |
| `Logging__LogLevel__Default` | Nivel de log, conforme a operacao |

O caminho de dados SQLite e definido em `Data Source` da connection string.
Nenhum banco e escolhido implicitamente em Production. A configuracao e validada
antes de criar diretorios, abrir o banco ou executar migrations. Erros identificam
apenas a chave e a regra violada, sem imprimir o valor recebido.

`appsettings.json` contem somente logging e seed desabilitado. Nao existe
fallback permissivo de `AllowedHosts` no arquivo base: Production nao inicia
sem a lista explicita. Exemplo para dois hosts:
`AllowedHosts='estoque.example.test;lojista.example.test'`. Enderecos IPv6 devem
usar colchetes, como `[::1]`. Requisicoes com Host fora da lista recebem HTTP 400
pelo filtro padrao do ASP.NET Core.
O banco relativo de desenvolvimento fica exclusivamente em
`appsettings.Development.json`. `Demo__SeedUsers` continua restrito a Development;
para um banco de producao vazio, siga o [primeiro acesso do LOJISTA](primeiro-acesso-lojista.md).
O [Compose](../../compose.yaml) ja fornece banco e chaves em um volume persistente,
com `AllowedHosts` restrito a `localhost;127.0.0.1;[::1]` por padrao. Para acesso
por dominio ou IP da rede, defina `AllowedHosts` no ambiente do Compose com os
hosts realmente utilizados. O [runbook Azure](azure.md) possui o exemplo do App Service.
Fotos de produtos continuam em `wwwroot/uploads/produtos`; sua persistencia segue
os procedimentos de armazenamento do ambiente e nao e alterada por esta configuracao.

## Validacao

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

`ProductionConfigurationTests` executa o processo real com variaveis de ambiente:
verifica falha por ausencia/valor vazio das tres configuracoes, SQLite invalido,
banco em memoria, caminhos relativos, AllowedHosts permissivo/invalido e ausencia
de vazamento do valor invalido.
O caso positivo inicia Kestrel em porta livre, aplica migrations em banco isolado
e exige `200 Healthy` em `/health` para hosts autorizados e HTTP 400 para um host
fora da lista.

Para repetir manualmente, execute o pacote com o exemplo acima e consulte `/health`.
Em seguida, remova `ConnectionStrings__EstoqueContext` e reinicie: o processo deve
encerrar com `Configuracao obrigatoria ausente: ConnectionStrings__EstoqueContext.`
Restaure-a, remova `DataProtection__KeysPath` e confira a mensagem correspondente.
Restaure-a, remova `AllowedHosts` e confira `Configuracao obrigatoria ausente: AllowedHosts.`.
Repita com `AllowedHosts='*'`: a aplicacao deve recusar a configuracao permissiva.
A mudanca deve ser publicada junto com as tres variaveis ja configuradas no ambiente;
inclua o hostname usado pelos health checks e pelo proxy, quando aplicavel.
