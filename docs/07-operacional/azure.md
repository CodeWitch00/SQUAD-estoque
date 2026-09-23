# Publicação e validação no Azure

Este runbook registra o ambiente acadêmico compartilhado e o procedimento
manual já executado. Ele não contém senhas, bancos ou outros segredos.

## Ambiente

```text
Aplicativo: asp-squad-estoque-free
Plano: plan-squad-estoque-f1
Nível: F1 Gratuito
Região: North Central US
Sistema operacional: Linux
Runtime: .NET 10
Publicação: Código por pacote ZIP
Instâncias: 1
```

Acesso: [SQUAD Estoque no Azure](https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net)

O ambiente serve para homologar a versão integrada da `main`, compartilhar uma
única massa fictícia entre os desenvolvedores e preparar a demonstração final.
Não cadastre clientes, dados pessoais ou informações reais.

## Persistência configurada

As configurações de aplicativo usadas no App Service são:

```text
ASPNETCORE_ENVIRONMENT=Production
WEBSITES_ENABLE_APP_SERVICE_STORAGE=true
ConnectionStrings__EstoqueContext=Data Source=/home/data/Estoque.db
ConnectionStrings__LegacyMovieContext=Data Source=/home/data/LegacyMovie.db
DataProtection__KeysPath=/home/data/keys
Demo__SeedUsers=true
```

Os bancos e as chaves de cookies ficam em `/home/data`, fora de
`/home/site/wwwroot`, onde o pacote é implantado. O seed demonstrativo permanece
temporariamente habilitado somente para a equipe e será revisto no cartão
`S4-BE-003` antes da demonstração final ou de qualquer uso real.

## Publicação manual

Na raiz de uma cópia limpa e atualizada da `main`:

```bash
dotnet restore src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release --no-restore
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-build

rm -rf publish/azure
dotnet publish src/SquadEstoque.Web/SquadEstoque.Web.csproj \
  --configuration Release \
  --no-restore \
  --output publish/azure

find publish/azure -type f \( -name '*.db' -o -name '*.db-wal' -o \
  -name '*.db-shm' -o -name '*.sqlite' -o -name '*.sqlite3' \)

rm -f publish/squad-estoque-azure.zip
cd publish/azure
zip -r ../squad-estoque-azure.zip .
cd ../..
```

O comando `find` não pode listar bancos. Envie o ZIP ao Azure Cloud Shell e
execute:

```bash
az webapp deploy \
  --resource-group rg-squad-estoque-demo \
  --name asp-squad-estoque-free \
  --src-path ~/squad-estoque-azure.zip \
  --type zip
```

A publicação deve partir somente da `main` validada. Apenas a responsável pelo
ambiente executa o deploy.

## Estado validado em 21/09/2026

| Verificação | Resultado |
| --- | --- |
| Commit publicado | `ead5edb` |
| Build Release | Aprovado |
| Suíte automatizada | 81 aprovados, 0 falhas, 0 ignorados |
| Pacote sem SQLite | Aprovado |
| Deploy ZIP | `RuntimeSuccessful`, 1 instância, 0 falhas |
| HTTPS e `/health` | Aprovado, resposta `Healthy` |
| Login dos dois perfis | Pendente de validação registrada |
| Persistência após reinício | Pendente |
| Persistência após nova publicação | Pendente |
| Backup e restauração | Pendente |

## Validação após cada publicação

1. Acesse `/health` por HTTPS e confirme `Healthy`.
2. Entre como `LOJISTA` e valide Produtos e Movimentações.
3. Entre como `VENDEDOR` e valide Consulta e o fluxo permitido ao perfil.
4. Confirme que rotas não autorizadas continuam bloqueadas.
5. Crie um registro fictício identificável.
6. Reinicie o App Service e confirme que o registro permaneceu.
7. Em uma publicação posterior, confirme novamente a persistência.
8. Faça e teste um backup antes de considerar o ambiente pronto para CD.

O plano F1 pode suspender a aplicação por inatividade e não possui garantia de
disponibilidade. Antes da apresentação, valide a URL, a assinatura Azure for
Students, as cotas do plano e uma cópia local de contingência.
