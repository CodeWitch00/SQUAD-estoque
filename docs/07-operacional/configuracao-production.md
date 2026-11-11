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
| `AllowedHosts` | Configure os nomes de host aceitos no ambiente |
| `Logging__LogLevel__Default` | Nivel de log, conforme a operacao |

O caminho de dados SQLite e definido em `Data Source` da connection string.
Nenhum banco e escolhido implicitamente em Production. A configuracao e validada
antes de criar diretorios, abrir o banco ou executar migrations. Erros identificam
apenas a chave e a regra violada, sem imprimir o valor recebido.

`appsettings.json` contem somente logging, filtro de hosts e seed desabilitado.
O banco relativo de desenvolvimento fica exclusivamente em
`appsettings.Development.json`. `Demo__SeedUsers` continua restrito a Development;
para um banco de producao vazio, siga o [primeiro acesso do LOJISTA](primeiro-acesso-lojista.md).
O [Compose](../../compose.yaml) ja fornece as duas configuracoes obrigatorias em
um volume persistente. O [runbook Azure](azure.md) possui o exemplo do App Service.
Fotos de produtos continuam em `wwwroot/uploads/produtos`; sua persistencia segue
os procedimentos de armazenamento do ambiente e nao e alterada por esta configuracao.

## Validacao

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

`ProductionConfigurationTests` executa o processo real com variaveis de ambiente:
verifica falha por ausencia/valor vazio das duas configuracoes, SQLite invalido,
banco em memoria, caminhos relativos e ausencia de vazamento do valor invalido.
O caso positivo inicia Kestrel em porta livre, aplica migrations em banco isolado
e exige `200 Healthy` em `/health`.

Para repetir manualmente, execute o pacote com o exemplo acima e consulte `/health`.
Em seguida, remova `ConnectionStrings__EstoqueContext` e reinicie: o processo deve
encerrar com `Configuracao obrigatoria ausente: ConnectionStrings__EstoqueContext.`
Restaure-a, remova `DataProtection__KeysPath` e confira a mensagem correspondente.
A mudanca deve ser publicada junto com as variaveis ja configuradas no ambiente.
