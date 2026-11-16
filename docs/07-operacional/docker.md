# Execução do SQUAD Estoque com Docker

Este documento descreve um container neutro, sem vínculo com um provedor de
hospedagem. A mesma imagem pode ser usada localmente e adaptada futuramente para
uma plataforma que aceite imagens Docker.

## O que o Docker isola

O `Dockerfile` produz uma imagem com a aplicação publicada e o runtime do .NET 10.
Um container é uma execução dessa imagem. A imagem é imutável: alterações feitas
na camada interna do container desaparecem quando ele é substituído.

O banco SQLite e as chaves que protegem os cookies não ficam nessa camada. O
`compose.yaml` monta o volume nomeado `squad-estoque-data` em `/app/data`:

```text
container /app
├── SquadEstoque.Web.dll       imagem imutável
└── data/                      volume persistente
    ├── Estoque.db
    └── keys/                  chaves de proteção dos cookies
```

Recriar ou atualizar o container preserva o volume. Excluir o volume remove o
banco e as chaves.

## Construir e iniciar

Na raiz do repositório, com o Docker em execução:

```bash
docker compose up --build -d
```

Esse comando constrói a imagem (quando necessário), cria o volume nomeado do
SQLite e sobe o container em modo destacado.

A aplicação fica disponível em:

```text
http://localhost:8080
```

Verifique a aplicação e acompanhe os logs:

```bash
docker compose ps
docker compose logs -f squad-estoque
curl --fail http://localhost:8080/health
```

Se quiser definir variáveis locais sem versioná-las, copie
`.env.compose.example` para `.env.compose` antes de subir o container. O arquivo
`.env.compose` é ignorado pelo Git e pode ser usado para sobrescrever
configurações ou informar segredos apenas por ambiente/arquivo não versionado.

## Parar e atualizar

Parar sem apagar os dados:

```bash
docker compose down
```

Depois de atualizar o código, publique uma nova imagem e substitua o container:

```bash
docker compose up --build -d
```

O volume `squad-estoque-data` será reutilizado e as migrations pendentes serão
aplicadas durante a inicialização.

## Dados demonstrativos e segredos

O Compose versionado não embute segredos. Configurações sensíveis ou específicas
de ambiente devem ser fornecidas por variáveis do ambiente de execução ou por um
arquivo local não versionado (`.env.compose`).

Para cenários de demonstração local, a aplicação pode criar usuários de exemplo
quando `Demo__SeedUsers=true` em um banco vazio. Defina esse valor apenas em
ambiente controlado e nunca use credenciais demonstrativas em publicação real.

Antes de uma publicação real:

- garanta `Demo__SeedUsers=false` ou omita a criação de usuários demonstrativos;
- provisione usuários com senhas fortes por um procedimento seguro;
- não publique senhas no repositório ou na imagem;
- mantenha connection strings e demais segredos fora do versionamento.

## Backup dos bancos

O volume protege contra substituição do container, mas não substitui backup. Um
backup consistente do SQLite deve ser criado com a API de backup do SQLite ou com
o comando `.backup`, e não copiando o arquivo enquanto há escrita ativa.

Exemplo para um ambiente controlado, após parar a aplicação:

```bash
docker compose stop squad-estoque
docker run --rm \
  -v squad-estoque-data:/data:ro \
  -v "$PWD/backups:/backup" \
  alpine:3.22 \
  cp /data/Estoque.db /backup/Estoque.db
docker compose start squad-estoque
```

A pasta `backups/` é ignorada pelo Git. Não versione bancos que contenham dados
reais ou hashes de senha.

## Publicação futura

Uma plataforma futura precisa fornecer:

1. execução de imagem Docker;
2. volume persistente montado em `/app/data`, enquanto SQLite for usado;
3. uma única instância gravando no SQLite;
4. variáveis de ambiente para as connection strings;
5. HTTPS na borda ou em um proxy reverso;
6. monitoramento do endpoint `/health`;
7. rotina externa e testada de backup.

Se a plataforma não oferecer volume persistente, ela não é adequada para este
SQLite. Nesse caso, a equipe precisará escolher outra hospedagem ou aprovar uma
mudança de banco antes da publicação.
