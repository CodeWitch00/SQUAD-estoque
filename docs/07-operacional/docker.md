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

Consulte [caminho e permissões do SQLite](sqlite-volume.md) para configuração
externa, provisionamento de permissões e validação automatizada do container.

## Construir e iniciar

Na raiz do repositório, com o Docker em execução:

```bash
docker compose up --build -d
```

Esse único comando constrói a imagem quando necessário, cria o volume nomeado
`squad-estoque-data` e inicia o serviço em modo destacado.

Se precisar sobrescrever uma configuração local sem versioná-la, copie
`.env.compose.example` para `.env.compose` antes de subir o container. O Compose
carrega esse arquivo de forma opcional; ele é ignorado pelo Git e serve apenas
para valores de ambiente. Não coloque segredos reais em nenhum arquivo
versionado.

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

## Primeiro acesso

O Compose executa em `Production` e não cria usuários demonstrativos. Em um
volume novo, cadastre o primeiro LOJISTA pelo
[procedimento de primeiro acesso](primeiro-acesso-lojista.md) antes do login.
O seed com contas conhecidas pertence somente ao ambiente `Development` local.
Não publique senhas no repositório ou na imagem; mantenha configurações
sensíveis no gerenciador da plataforma.

## Backup dos bancos

O volume protege contra substituição do container, mas não substitui backup. Um
backup consistente do SQLite deve ser criado com a API de backup do SQLite ou com
o comando `.backup`, e não copiando o arquivo enquanto há escrita ativa.

Use o procedimento versionado em
[backup-restauracao-sqlite.md](backup-restauracao-sqlite.md), que nomeia a cópia
por ambiente e data UTC e testa a restauração em um arquivo isolado. A cópia
bruta por `cp` foi removida deste guia porque pode ser inconsistente se alguém
a executar com o serviço ativo. Não versione bancos reais ou hashes de senha.

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

## Evidências do cartão S4-OPS-009

Os comandos executados, as saídas obtidas e o teste de persistência do volume
estão registrados em [evidencia-s4-ops-009.md](evidencia-s4-ops-009.md).
