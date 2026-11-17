# Runbook de produção — configurar, iniciar e diagnosticar

Guia rápido para a aplicação ASP.NET Core MVC com SQLite. O ambiente hospedado
atualmente é o Azure App Service Linux com publicação por pacote. O Docker
Compose disponível no repositório é a opção local de execução em container.
Publicação de Production em uma plataforma de containers permanece uma opção
futura; ela não é o mecanismo usado pelo App Service atual.

## Pré-requisitos

- .NET 10 SDK para compilar ou executar localmente; Docker Compose v2 para a
  execução em container; acesso autorizado ao App Service para operar o Azure.
- Em Production, um caminho persistente gravável para o SQLite e para as chaves
  de Data Protection. Use uma única instância escrevendo no arquivo SQLite.
- Nenhum segredo, senha, banco com dados reais ou arquivo de chaves deve ser
  versionado ou anexado a logs e capturas.

## Configuração de Production

Configure no ambiente que inicia a aplicação:

| Variável | Valor/regra |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__EstoqueContext` | Obrigatória; SQLite em arquivo persistente com caminho absoluto. Caminho relativo e banco em memória são recusados. |
| `DataProtection__KeysPath` | Obrigatória; diretório persistente com caminho absoluto. |
| `AllowedHosts` | Obrigatória; hosts reais separados por `;`, sem esquema, porta, caminho, espaços ou curingas. |

No App Service deste projeto, confira os valores e o hostname no [runbook
Azure](azure.md). Para configurar um host, informe somente o nome DNS, por
exemplo `app.exemplo.com`. Se a aplicação ou o health check usar mais de um
hostname, inclua todos na lista. Não defina `DOTNET_ENVIRONMENT` com valor
conflitante: no `WebApplication` ele pode determinar um ambiente diferente do
esperado.

## Build e testes

Na raiz do repositório:

```bash
dotnet restore src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release --no-restore
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-build
```

O workflow do GitHub Actions executa build, testes e cobertura; consulte a
execução do commit na aba **Actions** antes de atualizar o ambiente.

## Executar localmente — Development

O `appsettings.Development.json` aponta para `Estoque.db` relativo ao diretório
de execução. No primeiro início, a aplicação aplica as migrations
automaticamente. O seed demonstrativo fica desativado por padrão.

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

Acesse `http://localhost:5186` e consulte `/health`. Para habilitar usuários
demonstrativos apenas no banco local vazio:

```bash
Demo__SeedUsers=true ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

Nunca use essa opção em Production.

## Iniciar em Production a partir de um publish

Configure as quatro variáveis da tabela de Production e ajuste caminhos e host
para o ambiente. Exemplo Linux sem credenciais:

```bash
dotnet publish src/SquadEstoque.Web/SquadEstoque.Web.csproj \
  --configuration Release --output ./publish/app

export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__EstoqueContext='Data Source=/var/lib/squad-estoque/Estoque.db'
export DataProtection__KeysPath=/var/lib/squad-estoque/keys
export AllowedHosts=app.exemplo.com
dotnet ./publish/app/SquadEstoque.Web.dll
```

O processo precisa de permissão de leitura e escrita no diretório do banco e
das chaves. Antes de iniciar uma versão que pode aplicar migrations, preserve
um backup consistente do SQLite de acordo com o ambiente. Siga o procedimento
de [backup e restauração](backup-restauracao-sqlite.md); as limitações e o
estado do backup do Azure também estão em [azure.md](azure.md). A
aplicação aplica migrations pendentes durante o startup, antes de servir
requisições; não execute `database update` em paralelo com a aplicação. Em
`Testing`, a inicialização automática é desativada e os testes usam banco
isolado.

No Azure, o fluxo normal é integrar a versão validada à `main` e acompanhar o
workflow de deploy e a verificação `/health`. O procedimento de publicação
manual de contingência está em [azure.md](azure.md). Não publique um pacote
com banco SQLite.

## Primeiro acesso do LOJISTA

Production não cria contas demonstrativas. Em um banco sem usuários, use o
procedimento de [primeiro acesso do LOJISTA](primeiro-acesso-lojista.md), que
lê a senha pela entrada padrão e grava somente o hash. Confirme o banco de
destino antes de executar o comando. Não passe a senha como argumento nem a
grave em script, histórico, log ou captura.

## Diagnóstico

| Sintoma | Verificação inicial |
| --- | --- |
| Aplicação encerra antes do startup | Confira `ASPNETCORE_ENVIRONMENT`, `ConnectionStrings__EstoqueContext`, `DataProtection__KeysPath` e `AllowedHosts`. A mensagem indica a chave/regra ausente ou inválida; não publique valores de configuração. |
| HTTP 400 | Confira se o `Host` da URL está incluído em `AllowedHosts`, sem esquema, porta ou espaços. |
| SQLite não abre ou migrations falham | Confira caminho absoluto, volume montado, permissões e espaço livre. Faça backup antes de tentar novamente após falha de migration. |
| Login/cookies deixam de funcionar depois de reinício ou deploy | Confira se `DataProtection__KeysPath` aponta para armazenamento persistente e se o diretório mantém as chaves. |
| `/health` não retorna `Healthy` | Consulte os logs de inicialização e confirme se o processo subiu e terminou as migrations. No Azure, confira também o resultado de **Check application health** no Actions. |

**Logs locais:** a saída de `dotnet run` ou do processo publicado aparece no
terminal em que ele foi iniciado.

**Azure App Service:** no Portal, abra o recurso `asp-squad-estoque-free` e
acesse **Monitoring → Log stream**. Se não houver saída, habilite primeiro
**Monitoring → App Service logs → Application logging: File System** e salve.
No Azure Cloud Shell também é possível tentar:

```bash
az webapp log tail --resource-group rg-squad-estoque-demo --name asp-squad-estoque-free
```

O streaming pela CLI pode não funcionar em alguns App Services Linux; nesse
caso use **Log stream** no Portal. Consulte também a [documentação de logs do
App Service](https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs).
Para logs de build ou deploy, consulte a execução correspondente em
**GitHub → Actions**.

**Container local:** o Compose atual executa em `Production`, mantém banco e
chaves no volume `squad-estoque-data` e aplica migrations no startup:

```bash
docker compose up --build -d
docker compose ps
docker compose logs --tail=100 -f squad-estoque
curl --fail http://localhost:8080/health
```

Para substituir o container sem excluir os dados, use `docker compose down`
e depois `docker compose up --build -d`. Não use `docker compose down -v`:
isso remove o volume persistente. Para backup, restauração e detalhes do
container, siga [docker.md](docker.md).
