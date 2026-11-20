# Evidência: orquestração local do container — S4-OPS-009

Data da validação: 17/11/2026, por volta das 21:46 a 21:53 (America/Sao_Paulo).
Branch: `ops/s4-ops-009-integrante-v2`.
Base: `main` em `5d160a3e55804f889e5edf7d5f0e8c91cf3ed401` (`ci: add controlled
versioned image workflow`).

## Correção do PR anterior

O PR #109 foi aberto com a branch `developer-nicolas`, cujo head era `eea7249`.
A API do GitHub registra `changed_files: 1` e `additions: 665`: o único arquivo
alterado era o relatório de validação da Sprint 2 sobre navegação por perfil.
As alterações de `compose.yaml`, `Dockerfile` e documentação operacional
estavam em outra branch local (`ops/s4-ops-009-integrante`) e nunca entraram no
PR. Além disso, o PR tinha `mergeable_state: dirty`.

Este PR é aberto a partir de uma branch criada na `main` atualizada, com escopo
limitado ao cartão e sem os arquivos de monografia e de migrations que estavam
na branch antiga.

## Ambiente

| Ferramenta | Versão |
| --- | --- |
| Docker | 29.9.3, build 5457199 |
| Docker Compose | v5.7.0 |
| .NET SDK | 10.0.404 |
| Sistema operacional | Windows (Docker Desktop com engine Linux) |

## Alterações entregues

| Arquivo | Alteração |
| --- | --- |
| `compose.yaml` | Serviço único, `restart: unless-stopped`, `Production`, porta `8080:8080` e volume nomeado em `/app/data`; configurações locais podem ser passadas por `docker compose --env-file .env.compose`. |
| `.env.compose.example` | Modelo comentado de variáveis locais; nenhum segredo versionado. |
| `.gitignore` | Ignora `.env` e `.env.*`, com exceção explícita de `.env.compose.example`. |
| `docs/07-operacional/docker.md` | Inicialização por um comando, uso do arquivo local opcional e link para esta evidência. |
| `docs/07-operacional/evidencia-s4-ops-009.md` | Este registro. |

O `Dockerfile` **não foi alterado**. A `main` já contém o build em camadas com
`--no-restore` e as permissões persistentes do SQLite, entregues pelo PR #108.
O commit antigo `15bb1ef` removia justamente o `--no-restore`; ele não foi
trazido para esta branch.

## Comandos e resultados

### Configuração do Compose

```text
$ docker compose config
exit=0
```

Saídas relevantes:

```yaml
services:
  squad-estoque:
    restart: unless-stopped
    cap_drop: [ALL]
    security_opt: [no-new-privileges:true]
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      AllowedHosts: localhost;127.0.0.1;[::1]
      ConnectionStrings__EstoqueContext: Data Source=/app/data/Estoque.db
      DataProtection__KeysPath: /app/data/keys
    volumes:
      - type: volume
        source: squad-estoque-data
        target: /app/data
volumes:
  squad-estoque-data:
    name: squad-estoque-data
```

Com um `.env.compose` local, valide usando
`docker compose --env-file .env.compose config`; esse arquivo participa da
interpolação dos valores `${VARIAVEL:-padrao}` definidos em `compose.yaml`.
Para iniciar com essas configurações, use
`docker compose --env-file .env.compose up --build -d`. Sem `--env-file`,
continuam valendo os defaults definidos em `compose.yaml`. O arquivo local é
ignorado pelo Git.

### Isolamento da execução local

A máquina já possuía um container `squad-estoque` em execução na porta 8080 e o
volume `squad-estoque-data`, ambos criados pelo projeto na validação anterior.
Para não alterar esse ambiente, a subida do Compose foi validada com um arquivo
de override **temporário e não versionado**, trocando apenas a porta publicada
(18080) e o nome do volume (`squad-estoque-data-s4ops009-evidencia`). O
`compose.yaml` versionado foi integralmente mantido; ele foi conferido pelo
comando `docker compose config` acima e pelo job `sqlite-volume` do CI.

```text
$ docker compose -f compose.yaml -f compose.evidence.yaml up --build -d
exit=0
```

A imagem `squad-estoque:local` foi construída em camadas, com
`dotnet publish --configuration Release --no-restore` e diretórios `/app/data` e
`/app/wwwroot/uploads` criados com proprietário `1654:1654` e modo `0700`.

### Aplicação acessível e ambiente

```text
$ curl http://localhost:18080/health
Healthy

$ docker compose ps
NAME                               IMAGE               STATUS         PORTS
s4-ops-009-clean-squad-estoque-1   squad-estoque:local Up 5 seconds   0.0.0.0:18080->8080/tcp
```

Trecho dos logs do container:

```text
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      INSERT INTO "__EFMigrationsHistory" ...
      VALUES ('20261029210004_AddConsultaIndexes', '10.0.11');
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:8080
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
      Application started. Press Ctrl+C to shut down.
```

### Política de reinício e identidade do processo

```text
$ docker inspect --format '{{.Name}} restart={{.HostConfig.RestartPolicy.Name}} user={{.Config.User}}' ...
/s4-ops-009-clean-squad-estoque-1 restart=unless-stopped user=1654
```

### Banco no volume e persistência

Antes da recriação do container:

```text
$ docker compose exec -T squad-estoque sh -c 'id -u; ls -la /app/data; sha256sum /app/data/Estoque.db'
1654
drwx------ 3 app app 4096 ... .
-rw------- 1 app app 188416 ... Estoque.db
drwx------ 2 app app 4096 ... keys
58c6f3b4e357c93076daab2a0c56ff35aa12a7d405980796aa6755385ffd6712  /app/data/Estoque.db
```

Foi gravado um marcador no volume e o container foi removido e recriado:

```text
$ docker compose ... down
exit=0
$ docker compose ... up -d
exit=0
$ curl http://localhost:18080/health
Healthy
```

Depois da recriação:

```text
$ docker compose exec -T squad-estoque sh -c 'ls -la /app/data; sha256sum /app/data/Estoque.db; cat /app/data/evidencia-persistencia.txt'
drwx------ 3 app app 4096 ... .
-rw------- 1 app app 188416 ... Estoque.db
-rw-r--r-- 1 app app 22 ... evidencia-persistencia.txt
drwx------ 2 app app 4096 ... keys
8cec6587a83beaebb187a1259589e7f03d4f3023b762600f6982e013c80704da  /app/data/Estoque.db
s4ops009-persistencia
```

O banco continuou presente com o mesmo tamanho e o marcador sobreviveu à
substituição do container. O hash do arquivo mudou após a reabertura pelo
SQLite, o que é esperado quando o mecanismo atualiza o cabeçalho/estado do
arquivo; o fato relevante é que o arquivo não foi recriado na camada efêmera.

O volume nomeado usado no teste:

```text
$ docker volume inspect squad-estoque-data-s4ops009-evidencia --format 'name={{.Name}} driver={{.Driver}}'
name=squad-estoque-data-s4ops009-evidencia driver=local
```

Ao final, o container e o volume temporários foram removidos. O container
`squad-estoque` preexistente e o volume `squad-estoque-data` não foram
alterados.

### Serviços

```text
$ docker compose config --services
squad-estoque
```

Apenas o serviço utilizado pela aplicação está definido.

### Segredos fora do versionamento

`compose.yaml` usa apenas interpolação com valores padrão (`${VARIAVEL:-padrao}`)
e o arquivo `.env.compose.example` contém somente linhas comentadas. Nenhuma
senha, chave ou token real foi adicionado. O `.gitignore` passa a ignorar
`.env` e `.env.*`.

### Roteiro oficial do SQLite

```text
$ bash scripts/test-sqlite-volume.sh
PASS: caminho configurado, criacao de diretorio, usuario nao root, permissoes e persistencia apos recriar container
PASS: volume somente leitura (exit=139)
PASS: arquivo existente sem escrita (exit=139)
PASS: diretorio sem acesso ao usuario do processo (exit=139)
PASS: caminho invalido sob /proc (exit=139)
PASS: todos os cenarios de volume SQLite
```

O código `139` nos cenários de falha é o aborto do processo .NET ao lançar a
exceção de armazenamento indisponível no Linux; o roteiro considera esses
resultados esperados.

No Windows, o roteiro precisa de `MSYS_NO_PATHCONV=1` e
`MSYS2_ARG_CONV_EXCL='*'` para que o Git Bash não converta os caminhos `/app/...`
em caminhos do Windows. Sem essa precaução, a aplicação recebe um caminho
inválido e falha antes de testar o volume. Essa é uma limitação do shell local
de validação, não da configuração versionada; no Ubuntu do GitHub Actions o
roteiro roda sem variáveis adicionais.

### Build, testes e diff

```text
$ git diff --check
exit=0

$ dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
Aprovado!  – Com falha: 0, Aprovado: 352, Ignorado: 1, Total: 353
```

O teste ignorado é `ConsultaP95Tests.RNF02_consulta_p95_must_be_below_500ms`,
conforme já registrado nas evidências de desempenho da Sprint 3.

## Limitações conhecidas

- A validação local do Compose usou override de porta/volume por causa do
  container preexistente; a porta padrão `8080` e o volume
  `squad-estoque-data` permanecem os valores do arquivo versionado.
- Não há serviços adicionais, migration, alteração de autenticação ou mudança
  de regra de negócio neste PR.
- O merge depende de CI verde e da revisão da mantenedora `CodeWitch00`, conforme
  `CONTRIBUTING.md`.
