# Manual técnico — Docker, publicação e recuperação

Referência: cartão **S4-DOC-021**, Sprint 4. Este manual reúne a operação da
aplicação ASP.NET Core MVC/.NET 10 com EF Core e SQLite. O fluxo de contribuição
permanece no [CONTRIBUTING.md](../../CONTRIBUTING.md).

Os comandos partem da raiz do repositório, salvo indicação contrária. Blocos
`bash` exigem Bash; blocos `powershell` exigem PowerShell. Substitua os marcadores
`<...>` antes de executar. Não copie credenciais, cookies, bancos ou chaves para
o Git, cartões, capturas ou logs públicos.

## Índice

1. [Ambientes e ferramentas](#1-ambientes-e-ferramentas)
2. [Construir e verificar](#2-construir-e-verificar)
3. [Configuração externa](#3-configuração-externa)
4. [Docker e Compose](#4-docker-e-compose)
5. [Banco e migrations](#5-banco-e-migrations)
6. [Publicação e atualização](#6-publicação-e-atualização)
7. [Saúde, logs e diagnóstico](#7-saúde-logs-e-diagnóstico)
8. [Backup e restauração](#8-backup-e-restauração)
9. [Retorno de versão](#9-retorno-de-versão)
10. [Aceite operacional e evidências](#10-aceite-operacional-e-evidências)

## 1. Ambientes e ferramentas

| Ambiente | Execução | Estado persistente |
| --- | --- | --- |
| Desenvolvimento | SDK .NET 10; `dotnet run` | Banco individual, nunca compartilhado pelo Git |
| Compose local | Docker Engine/Desktop com containers Linux e Compose v2 | Volume `squad-estoque-data` em `/app/data` |
| Azure compartilhado | App Service Linux; pacote de código .NET 10 | Banco e chaves em `/home/data`, conforme configuração do serviço |
| CI | Ubuntu; .NET 10, Node 22 e Docker conforme o job | Bancos de teste isolados |

O Azure é o ambiente acadêmico de homologação/demonstração. Ele **não usa a
imagem Docker**. A imagem no GHCR tem um fluxo separado, descrito em
[entrega-imagem.md](entrega-imagem.md).

Para desenvolvimento, instale Git e SDK .NET 10 conforme o guia de contribuição.
Para Docker, confirme que o daemon está iniciado, usando containers Linux:

```text
git --version
dotnet --version
docker version
docker compose version
```

Backup exige Bash, `realpath` e a CLI `sqlite3` no host administrativo. No
Windows, use Git Bash com o executável oficial do SQLite no `PATH`, ou defina
`SQLITE3_BIN` para seu caminho. No Ubuntu, use Bash e o pacote SQLite da
distribuição. O procedimento foi repetido em Windows/Git Bash; os resultados e
limites estão na [evidência desta entrega](evidencia-s4-doc-021.md).

## 2. Construir e verificar

Em PowerShell ou Bash, numa cópia limpa do commit a verificar:

```text
git rev-parse HEAD
git status --short
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-build
dotnet publish src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release --no-restore --output publish/manual-tecnico
```

O projeto de testes referencia a aplicação, portanto o restore/build acima
inclui ambos. Use uma pasta de publicação nova e vazia para cada pacote; não
reutilize uma pasta com arquivos de outra versão. Resultado esperado: comandos
com saída zero, suíte aprovada e `SquadEstoque.Web.dll` no diretório publicado.
Não publique bancos, chaves ou arquivos privados junto com o aplicativo.

Para desenvolvimento, execute `dotnet run --project
src/SquadEstoque.Web/SquadEstoque.Web.csproj`. O perfil local normalmente usa
`http://localhost:5186`; confira a porta no terminal. As contas demonstrativas
só são criadas em `Development`, com `Demo__SeedUsers=true` e tabela de usuários
vazia. Consulte o guia de contribuição para habilitá-las localmente.

## 3. Configuração externa

O ASP.NET Core lê variáveis de ambiente com `__` entre seções. A aplicação não
carrega arquivos `.env` por conta própria. Configure o processo, o Compose ou
as configurações do App Service; mantenha os valores privados no ambiente.

| Variável | Finalidade e regra verificada no código |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` no ambiente publicado. Evite `DOTNET_ENVIRONMENT` conflitante. |
| `ConnectionStrings__EstoqueContext` | Obrigatória em Production; arquivo SQLite gravável, persistente e com caminho absoluto. Memória e caminho relativo são recusados. |
| `DataProtection__KeysPath` | Diretório absoluto persistente e gravável; preserva chaves de proteção dos cookies. |
| `AllowedHosts` | Hosts explícitos separados por `;`, sem esquema, porta, caminho, espaços ou curingas. Inclua o host do health check. |
| `ASPNETCORE_HTTP_PORTS` / `ASPNETCORE_URLS` | Porta/endereço de escuta do processo. A imagem define HTTP 8080. Não confundir com o endereço HTTPS público. |
| `ReverseProxy__KnownProxies__0` | IP do proxy imediato confirmado pelo operador; entradas adicionais usam índices consecutivos. |
| `ASPNETCORE_HTTPS_PORT` | Porta HTTPS pública usada no redirecionamento, conforme a infraestrutura. |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Ausente ou `false`; o modo automático permissivo é recusado. |
| `Logging__LogLevel__Default` | Nível de log operacional; evite registrar dados sensíveis. |
| `Demo__SeedUsers` | Restrito a Development. Production nunca cria as contas demonstrativas. |

Os valores não secretos de exemplo e as regras estão em
[configuracao-production.md](configuracao-production.md). Em Production, a
validação ocorre antes das migrations. Uma configuração ausente encerra o
processo; não relaxe a validação para contornar o erro.

HTTPS termina na borda/proxy. Confirme certificado, HTTPS Only, preservação do
`Host` e proxy imediato confiável conforme [azure.md](azure.md). O cookie de
autenticação usa `Secure` em Production; o HTTP local do Compose permite testar
startup/saúde, mas não comprova login seguro de ponta a ponta.

Para banco novo em Production, use [primeiro-acesso-lojista.md](primeiro-acesso-lojista.md).
O comando `--bootstrap-lojista NOME EMAIL` lê a senha pela entrada padrão,
exige tabela `Usuario` vazia e não inicia HTTP. Não passe senha em argumento.

## 4. Docker e Compose

O [Dockerfile](../../Dockerfile) faz restore e publish em estágios separados,
copia o resultado para a imagem ASP.NET e executa como `$APP_UID`, sem root.
O [compose.yaml](../../compose.yaml) define o serviço `squad-estoque`, imagem
`squad-estoque:local`, porta 8080, reinício `unless-stopped`, capacidades
removidas e `no-new-privileges`.

```text
docker compose config --quiet
docker compose up --build -d
docker compose ps
docker compose logs --tail=100 squad-estoque
```

Se usar `.env.compose`, copie [.env.compose.example](../../.env.compose.example)
e passe `--env-file .env.compose` **em todos os comandos Compose da operação**:

```text
docker compose --env-file .env.compose config --quiet
docker compose --env-file .env.compose up --build -d
```

Somente variáveis referenciadas no YAML são interpoladas. Colocar uma variável
arbitrária no arquivo não a injeta automaticamente no container. Use
`config --quiet` para validar sem imprimir a configuração expandida.

Confirme a resposta em PowerShell:

```powershell
curl.exe --fail --silent --show-error http://localhost:8080/health
```

No Bash, use `curl --fail --silent --show-error http://localhost:8080/health`.
Resultado esperado: `Healthy`. Confira também os logs de migrations.

O volume nomeado contém `/app/data/Estoque.db` e `/app/data/keys`. Para parar
sem remover dados, use `docker compose stop squad-estoque` ou
`docker compose down`. Para atualizar após backup e aprovação do commit, use
`docker compose up --build -d`. **Não use `down -v`**, pois remove o volume.

O Compose atual **não monta `wwwroot/uploads` em volume**. Fotos enviadas ficam
no filesystem do container e podem ser perdidas na recriação. Backup do SQLite
não inclui as fotos nem as chaves; preserve esses arquivos separadamente e
decida sua persistência antes de usar containers como ambiente publicado.
Este manual não altera a infraestrutura. Consulte [sqlite-volume.md](sqlite-volume.md)
para permissões e [docker.md](docker.md) para o procedimento complementar.

## 5. Banco e migrations

[Program.cs](../../src/SquadEstoque.Web/Program.cs) prepara o armazenamento e
executa `Database.Migrate()` no startup, exceto em `Testing`. Não é necessário
executar `dotnet ef database update` como etapa adicional do deploy; não execute
esse comando simultaneamente à inicialização da aplicação.

O schema está em [Migrations/Estoque](../../src/SquadEstoque.Web/Migrations/Estoque/).
Antes de atualizar: identifique as migrations do commit, faça backup, confira
integridade e ensaie a atualização em cópia isolada. Use uma única instância
gravando no SQLite. A identidade do processo precisa escrever no diretório pai,
no banco e nas chaves; não use `chmod 777`.

Com a CLI SQLite, a consulta abaixo é somente leitura e permite comparar o
histórico com as migrations do commit (substitua o caminho):

```text
sqlite3 -readonly /caminho/isolado/Estoque.sqlite3 "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"
```

Um arquivo recém-criado sem schema não possui essa tabela. Não inicie uma
versão antiga sobre um schema novo presumindo compatibilidade. As verificações
de banco vazio e atualização estão documentadas em
[s4-db-004-validacao-migrations.md](s4-db-004-validacao-migrations.md).

## 6. Publicação e atualização

### Azure: pacote de código

O fluxo implementado em [dotnet.yml](../../.github/workflows/dotnet.yml) habilita
`deploy-azure` somente em `push` ou execução manual da `main`, com
`AZURE_CD_ENABLED=true`. Ele depende de `build-and-test`, publica o pacote,
recusa arquivos SQLite, autentica por OIDC e consulta `/health` após o deploy.
Os jobs de frontend e volume existem, mas **não são dependências desse job de
deploy**; confira todos os resultados antes do aceite. Abrir PR desta branch
não publica no Azure.

A responsável pelo ambiente deve seguir esta sequência:

1. Conferir SHA aprovado, todos os checks, configurações e janela de operação.
2. Preservar backup consistente fora do App Service, chaves e uploads; anotar
   versão anterior e migrations. Confirmar que a recuperação isolada funciona.
3. Integrar pelo fluxo autorizado e acompanhar a execução na `main`.
4. Conferir a etapa de deploy **e** a de saúde; um pacote aceito pelo Azure não
   equivale a aplicação validada.
5. Validar login dos perfis, rotas permitidas, registros conhecidos, fotos e
   persistência. Registrar SHA, horário, executor e resultado sem segredos.

Com GitHub CLI autenticado, estes comandos só consultam o estado:

```text
gh run list --repo CodeWitch00/SQUAD-estoque --workflow dotnet.yml --branch main --limit 5
gh run view <ID_DA_EXECUCAO> --repo CodeWitch00/SQUAD-estoque
```

A contingência manual autorizada está em [azure.md](azure.md). Gere o pacote em
pasta nova, sem bancos; no ZIP, a DLL fica na raiz, não dentro de uma pasta extra.
No Windows, compacte o **conteúdo** da pasta pelo PowerShell; no Ubuntu use
`zip` conforme o runbook. Apenas a responsável pelo ambiente envia o pacote.
O manual documenta esse fluxo; sua revisão não exige republicar a aplicação.

Exemplo PowerShell, após o publish da seção 2, para gerar um ZIP novo:

```powershell
$bancos = Get-ChildItem publish/manual-tecnico -Recurse -File |
    Where-Object { $_.Name -match '\.(db|sqlite|sqlite3)(-|$)' }
if ($bancos) { throw 'Pacote contém banco; interrompa a publicação.' }
$pacote = 'publish/squad-estoque-manual.zip'
if (Test-Path $pacote) { throw 'Escolha um nome novo; preserve o pacote anterior.' }
Compress-Archive -Path publish/manual-tecnico/* -DestinationPath $pacote
```

Essa checagem de extensões complementa a revisão dos arquivos; não detecta
todo tipo de segredo. O ZIP não deve conter configurações privadas, bancos,
chaves de Data Protection ou uploads do ambiente.

### Imagem versionada

O workflow [image-release.yml](../../.github/workflows/image-release.yml) exige
SHA completo igual à ponta atual de `main`. Primeiro execute a simulação
`publish=false`; após aprovação, `publish=true` publica em GHCR. Registre o
digest e fixe-o no ambiente consumidor. Não trate `squad-estoque:local` como
identificador imutável de uma versão. Consulte [entrega-imagem.md](entrega-imagem.md).

## 7. Saúde, logs e diagnóstico

Consulta pública, sem autenticação, no PowerShell:

```powershell
$urlSaude = 'https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net/health'
curl.exe --fail --silent --show-error --max-time 30 $urlSaude
```

No Bash, atribua `url_saude` à mesma URL e execute
`curl --fail --silent --show-error --max-time 30 "$url_saude"`.
Não desabilite a validação TLS. Para conferir HTTP→HTTPS, consulte a URL HTTP
com `curl -I` e confira `Location` antes de seguir o redirecionamento.

`AddHealthChecks()`/`MapHealthChecks("/health")` não registra teste específico
do banco. `200 Healthy` demonstra que o processo atende; não comprova login,
escrita, backup, fotos ou persistência depois de reinício.

| Sintoma | Diagnóstico e próximo passo |
| --- | --- |
| Startup encerra | Conferir chave/regra indicada pela validação, ambiente e permissões. |
| HTTP 400 | Conferir `Host` e `AllowedHosts`. |
| Loop HTTPS ou cookie não funciona | Conferir HTTPS público, proxy imediato, protocolo encaminhado e chaves persistentes. |
| SQLite bloqueado/indisponível | Conferir instância única, escrita concorrente, volume, espaço e permissões; não apagar o banco. |
| Migration falhou | Parar tentativas, preservar banco e logs e identificar migration/versão antes de decidir recuperação. |
| Fotos sumiram | Conferir armazenamento de uploads; restaurar apenas o banco não recupera arquivos. |
| Deploy verde, saúde vermelha | Ler startup e etapa de saúde; preservar o resultado original mesmo que uma consulta posterior passe. |

Logs locais ficam no terminal. Para Docker use `docker compose logs --tail=100
squad-estoque`. No Azure use **Monitoring → Log stream**; se necessário,
habilite Application logging conforme [runbook-producao.md](runbook-producao.md).
No Cloud Shell autorizado:

```bash
az webapp log tail --resource-group rg-squad-estoque-demo --name asp-squad-estoque-free
```

Consulte o job no Actions para erros de build/deploy. Revise e remova dados
sensíveis antes de compartilhar qualquer saída.

## 8. Backup e restauração

Volume persistente não é backup. Guarde cópias consistentes fora do volume e
fora do repositório, com acesso restrito. O script
[sqlite-recovery.sh](../../scripts/sqlite-recovery.sh) usa `.backup`, verifica
integridade e chaves estrangeiras e restaura em destino novo; não substitui o
banco ativo. Não copie o arquivo SQLite ativo com `cp`/`Copy-Item`.

Exemplo Bash no host que tem acesso administrativo ao arquivo. Os caminhos são
marcadores; escolha diretórios protegidos antes de executar:

```bash
origem='/caminho/persistente/Estoque.db'
destino='/caminho/protegido/backups'
isolado='/caminho/isolado/ensaio-recuperacao'
mkdir -p "$isolado"
backup=$(bash scripts/sqlite-recovery.sh backup Production "$origem" "$destino")
bash scripts/sqlite-recovery.sh verify "$backup"
bash scripts/sqlite-recovery.sh restore "$backup" "$isolado/Estoque-restaurado.sqlite3"
```

O nome do backup inclui ambiente e UTC. Exija `integrity_check=ok` e
`foreign_key_check=ok`. O destino deve ser um arquivo `.sqlite3` inexistente.
Opcionalmente passe o UUID de um Produto conhecido como último argumento de
`verify`/`restore`; não use o UUID da fixture como prova de dados reais.
Confira registros esperados e `__EFMigrationsHistory` antes de iniciar uma
versão compatível sobre a cópia.

No Azure, a cópia deve ser gerada consistentemente no acesso administrativo
autorizado e então transferida para armazenamento externo. A disponibilidade
da CLI SQLite/SSH precisa ser conferida pelo operador. Não exponha banco ou
ferramenta de backup em `wwwroot`. Para Docker, o arquivo está no volume
`squad-estoque-data`; o caminho dentro do container não é um caminho local do
Windows. Não suponha que a imagem ASP.NET tenha a CLI SQLite instalada.

O teste existente é seguro para repetir com dados fictícios:

```bash
bash scripts/test-sqlite-recovery.sh
```

Resultado esperado: backup/restauração aprovados, Produto conhecido preservado
e destino existente não sobrescrito. Esse teste não valida uma cópia real do
Azure. O procedimento completo e o controle da troca do banco estão em
[backup-restauracao-sqlite.md](backup-restauracao-sqlite.md).

## 9. Retorno de versão

Retornar o aplicativo **não desfaz migrations**. Antes da janela, retenha o
pacote ou digest anterior, SHA, configurações necessárias e backup consistente.

1. Identifique a falha e interrompa escritas. A responsável deve impedir que
   uma nova publicação automática concorra com a recuperação.
2. Compare migrations/schema. Sem mudança incompatível, a responsável pode
   republicar o pacote anterior validado pelo fluxo manual do Azure, ou usar o
   digest anterior no ambiente de containers. Não recrie o volume para trocar
   somente o aplicativo.
3. Se o schema for incompatível, valide primeiro a restauração isolada. A
   decisão de retornar também os dados deve explicitar quais operações feitas
   depois do backup serão perdidas; não faça downgrade automático do schema.
4. Com todos os escritores parados, preserve o estado atual para diagnóstico e
   retorno. Trate banco e eventuais arquivos WAL/SHM como um conjunto; não
   misture um banco restaurado com WAL/SHM remanescentes da versão que falhou.
5. Aplique a troca aprovada conforme o procedimento de recuperação, confira
   propriedade/permissões e inicie a versão compatível. Preserve chaves e
   uploads ou restaure as cópias correspondentes, conforme o incidente.
6. Confira `/health`, logs, login, registros e fotos. Registre o resultado e
   retenha as cópias até o aceite da responsável.

Não execute rollback na aplicação compartilhada apenas para testar o manual.
O ensaio de recuperação deve começar em ambiente isolado.

## 10. Aceite operacional e evidências

Antes de considerar uma publicação ou recuperação concluída, registre:

- ambiente, SHA/digest, data/hora e executor;
- execução do CI e resultado de cada job relevante;
- saúde HTTP/TLS e resultado dos fluxos funcionais;
- backup fora do serviço, verificação de integridade e ensaio isolado;
- persistência de banco, chaves e fotos após reinício/republicação;
- revisão por outro integrante e decisões sobre divergências.

O estado conferido nesta entrega, a evidência reproduzível e as pendências
estão em [evidencia-s4-doc-021.md](evidencia-s4-doc-021.md). Não marque como
executado um procedimento somente descrito aqui. Revisão da mantenedora,
recuperação real do Azure e aceite da entrega permanecem etapas explícitas.
