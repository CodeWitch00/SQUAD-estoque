# Caminho e permissões do SQLite no container

O caminho vem de `Data Source` em `ConnectionStrings__EstoqueContext`. Em
Production, deve ser absoluto e persistente. `DataProtection__KeysPath` define
o diretório das chaves. O Compose aceita sobrescrever ambas por ambiente:

```bash
export ConnectionStrings__EstoqueContext='Data Source=/app/data/operacao/Estoque.db'
export DataProtection__KeysPath='/app/data/keys'
docker compose up --build -d
```

Mantenha os caminhos sob o volume montado em `/app/data`. Para usar outro ponto
de montagem, ajuste também `volumes` no Compose ou no serviço de hospedagem.
Alterar somente a connection string não transforma um diretório da imagem em
armazenamento persistente.

## Inicialização e permissões

Antes das migrations, o processo cria diretórios ausentes, grava e relê um
arquivo temporário e o remove. Abre o arquivo do banco para leitura/escrita,
abre uma transação SQLite de escrita e a desfaz sem alterar os dados.
As chaves passam pela mesma verificação de diretório.

No Linux, novos diretórios são criados com `0700` e o arquivo do banco com
`0600` (a umask pode restringir ainda mais). Permissões existentes não são
ampliadas. O processo roda com o `APP_UID` da imagem .NET, sem root. Apenas
`/app/data` e `/app/wwwroot/uploads` pertencem a esse usuário; os binários
continuam pertencendo a root. O Compose remove capabilities e impede
elevação de privilégios. Uploads mantêm a localização atual; sua persistência
não faz parte desta alteração.

Um volume nomeado novo recebe proprietário e modo do diretório da imagem.
Volumes existentes e bind mounts precisam ser provisionados pelo operador
com proprietário compatível com o UID/GID do processo. Consulte a identidade:

```bash
docker compose run --rm --no-deps --entrypoint id squad-estoque
```

No host Linux, atribua ao UID/GID informado somente o diretório de dados e
use modo `0700` para diretórios e `0600` para arquivos privados. Faça backup
antes de corrigir permissões em dados existentes. Não use `chmod 777`, não
execute o serviço como root e não altere permissões automaticamente no startup.

Falhas de acesso, montagem somente leitura, caminho ocupado por arquivo e
erros de abertura do SQLite interrompem o startup com:

```text
Armazenamento SQLite indisponivel em '<caminho>' para o usuario '<usuario>'.
Verifique a montagem do volume, espaco livre e permissoes de leitura, escrita e acesso ao diretorio.
```

A mensagem não imprime a connection string. Corrija a causa e reinicie o
serviço. A verificação não substitui monitoramento de espaço/permissões durante
a execução nem uma rotina de backup.

## Validação automatizada

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
bash scripts/test-sqlite-volume.sh
```

O roteiro Docker usa apenas um volume descartável com nome exclusivo. Constrói
a imagem, inicia com caminho aninhado, confirma UID diferente de zero, binários
sem escrita, modos `0700`/`0600`, banco criado e ausência de arquivos de sondagem.
Recria o container e compara o hash do banco e um marcador persistido. Confirma
falha com diagnóstico e código diferente de zero em volume somente leitura, arquivo sem escrita,
diretório root sem acesso e caminho inválido sob `/proc`. O cleanup remove
somente os recursos temporários criados pelo roteiro.

O job `sqlite-volume` do GitHub Actions executa esse roteiro em Linux e publica
o log no artefato `sqlite-volume-evidence`. Os testes .NET também verificam
preservação de dados, criação restrita, arquivo sem escrita, diretório inválido,
chaves indisponíveis e integração com o startup em Production e Development.
