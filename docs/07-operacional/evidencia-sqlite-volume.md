# Evidencia: caminho e permissoes do SQLite

Data: 16/11/2026. Branch: `dev/emmy`.
Base: `9c4946c` (main sincronizada antes da implementacao).

## Validacao local

- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj`: aprovado, zero avisos e zero erros.
- `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`: 351 aprovados, 1 ignorado, 0 falhas; 352 no total.
- Testes de armazenamento e startup: 17 aprovados, 0 falhas.
- `git diff --check`: aprovado.
- `bash -n scripts/test-sqlite-volume.sh`: aprovado.

Os testes confirmam criacao de diretorio aninhado, leitura e escrita, dados
preservados apos nova inicializacao, recusa de arquivo somente leitura,
diretorio ocupado por arquivo, chaves indisponiveis e falha no startup em
Production com diagnostico do caminho e do usuario do processo.

## Validacao Linux/container

O host local nao possui Docker instalado. A validacao real de container e
executada no job `sqlite-volume` do GitHub Actions pelo roteiro
`scripts/test-sqlite-volume.sh`. O resultado e publicado como artefato
`sqlite-volume-evidence`, com log de cada cenario:

1. Caminho configurado externamente, diretorios criados e `/health` saudavel.
2. Usuario diferente de root; binarios sem escrita; dados 0700 e banco 0600.
3. Banco e marcador preservados ao recriar o container.
4. Volume somente leitura recusado com diagnostico.
5. Arquivo existente sem escrita recusado com diagnostico.
6. Diretorio sem acesso ao usuario do processo recusado com diagnostico.
7. Caminho invalido sob `/proc` recusado com diagnostico.

O status final e o link da execucao devem ser consultados no PR junto ao
artefato; a existencia do roteiro, sozinha, nao comprova aprovacao no Docker.

## Escopo

Nenhum commit desta atividade foi criado ou enviado na main. Dados e chaves
usam configuracao externa, sem `chmod 777`. Diretorios existentes nao recebem
ampliacao automatica de permissoes. Movimentacoes e registro no Trello ficam
com a responsavel pelo cartao.

Detalhes operacionais: [sqlite-volume.md](sqlite-volume.md).
