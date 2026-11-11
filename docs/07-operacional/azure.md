# Publicação e validação no Azure

Este runbook registra o ambiente acadêmico compartilhado, a publicação manual
de contingência e o fluxo automatizado ativo. Ele não contém senhas,
bancos ou outros segredos.

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

As configurações esperadas no App Service são (confirme os valores no Portal):

```text
ASPNETCORE_ENVIRONMENT=Production
WEBSITES_ENABLE_APP_SERVICE_STORAGE=true
ConnectionStrings__EstoqueContext=Data Source=/home/data/Estoque.db
DataProtection__KeysPath=/home/data/keys
```

O banco do estoque e as chaves de cookies ficam em `/home/data`, fora de
`/home/site/wwwroot`, onde o pacote é implantado. A configuração legada
`Demo__SeedUsers` foi removida no Portal em 10/11/2026. O código publicado
também não executa seed demonstrativo em `Production`, mesmo se a variável
voltar a ser definida. Usuários demonstrativos já existentes no banco não são
removidos automaticamente;
revise-os separadamente antes de usar o ambiente fora da demonstração. Para um
banco novo, siga o [primeiro acesso do LOJISTA](primeiro-acesso-lojista.md).

## Publicação automatizada

O workflow `.github/workflows/dotnet.yml` executa build e os testes antes de
publicar. O job de deploy só é elegível em `push` ou execução manual da `main`,
rejeita pacotes que contenham arquivos SQLite, autentica no Azure com OIDC e
confere `/health` depois da publicação.

O deploy só é executado quando a variável de repositório
`AZURE_CD_ENABLED` está definida como `true`; ela foi conferida assim em
10/11/2026. A identidade federada
`id-squad-estoque-github` confia somente em `main` e recebe `Website Contributor`
com escopo no App Service. Os identificadores usados pelo login OIDC ficam nos
secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` e `AZURE_SUBSCRIPTION_ID`; os
valores não devem ser registrados neste documento.

Cada atualização da `main` aciona build, testes e deploy automaticamente,
desde que a validação passe. Consulte a execução mais recente na aba
**Actions** antes de declarar a publicação concluída.

## Publicação manual de contingência

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

## Estado do ambiente — conferência em 10/11/2026

| Verificação | Resultado |
| --- | --- |
| Build, testes e deploy do commit `dac6eaf` | Aprovados no [workflow da `main`](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/42901865609) |
| Variável `AZURE_CD_ENABLED` | `true`, conferida em 10/11/2026 |
| `Demo__SeedUsers` | Removida do App Service em 10/11/2026; confirmar novamente no Portal se a configuração mudar |
| HTTPS e `/health` após o deploy | Aprovados pelo passo `Check application health` do workflow acima |
| Login dos dois perfis | Pendente de validação registrada |
| Persistência após reinício | Aprovada em 24/09/2026; ver [registro](validacao-persistencia-azure.md) |
| Persistência após nova publicação | Pendente |
| Backup consistente baixado | Aprovado; restauração ainda não testada |

## Validação após cada publicação

1. Acesse `/health` por HTTPS e confirme `Healthy`.
2. Entre como `LOJISTA` e valide Produtos e Movimentações.
3. Entre como `VENDEDOR` e valide Consulta e o fluxo permitido ao perfil.
4. Confirme que rotas não autorizadas continuam bloqueadas.
5. Crie um registro fictício identificável.
6. Reinicie o App Service e confirme que o registro permaneceu.
7. Em uma publicação posterior, confirme novamente a persistência.
8. Mantenha uma cópia consistente do SQLite fora do App Service e teste a
   restauração antes de depender do backup em uma recuperação.

O plano F1 pode suspender a aplicação por inatividade e não possui garantia de
disponibilidade. Antes da apresentação, valide a URL, a assinatura Azure for
Students, as cotas do plano e uma cópia local de contingência.
