# Backup e restauração do SQLite

## Escopo e limites

Este procedimento gera uma cópia consistente pelo comando `.backup` do SQLite,
confere a cópia e restaura primeiro em um arquivo isolado. O script não substitui
automaticamente o banco em uso. Não execute o procedimento contra produção sem
uma janela operacional aprovada e uma cópia de retorno preservada.

O App Service usa `/home/data/Estoque.db`. O arquivo deve permanecer fora de
`/home/site/wwwroot` e o backup precisa ser baixado para armazenamento
administrativo protegido, separado do App Service. Uma pasta `backups` no mesmo
volume não protege contra perda desse volume. O registro de 24/09/2026 informa
que o App Service não tinha `sqlite3` nem `python3`; nesse ambiente, use um
binário SQLite confiável fornecido temporariamente pelo operador. Não instale a
ferramenta no aplicativo nem a publique em `wwwroot`; remova-a após o uso.

O teste automatizado deste procedimento usa um banco temporário fictício. Ele
comprova o fluxo do script e a preservação de um dado conhecido; não equivale a
restaurar a cópia real do Azure. Essa restauração depende de obter aquela cópia
e executá-la em ambiente isolado.

## Preparação e backup

Execute em host administrativo com Bash, `sqlite3` e espaço protegido para a
cópia. O nome contém o ambiente e a data/hora UTC, sem credenciais. O script
restringe permissões dos arquivos novos ao usuário operador (`umask 077`).

```bash
chmod +x scripts/sqlite-recovery.sh
mkdir -p "$HOME/backups-squad"
scripts/sqlite-recovery.sh backup Production \
  /caminho/seguro/Estoque.db "$HOME/backups-squad"
```

O comando imprime o caminho criado, no padrão
`Estoque-Production-AAAAMMDDTHHMMSSZ.sqlite3`. A origem pode estar recebendo
escritas: `.backup` usa a API de backup do SQLite para obter uma cópia
consistente; não use `cp` nem copie manualmente o arquivo ativo. Preserve a
cópia em armazenamento protegido e fora do repositório. Não inclua senhas,
tokens ou outras credenciais nos nomes ou no histórico de comandos.

Para o App Service, obtenha `/home/data/Estoque.db` por um canal administrativo
autorizado e salve-o diretamente no host de backup; não deixe a única cópia no
App Service. A disponibilidade do SSH/Kudu para esse fluxo depende do estado da
plataforma. Se a ferramenta SQLite for necessária dentro da sessão, mantenha-a
temporária e fora do diretório publicado. Faça a cópia consistente com `.backup`
e então transfira o resultado para fora do ambiente.

## Conferência e restauração isolada

Antes de qualquer substituição, crie um diretório novo e restaure para um nome
que ainda não exista. Informe um ID de Produto conhecido quando houver um dado
de referência confiável:

```bash
mkdir -p /caminho/isolado/recuperacao-20261115
scripts/sqlite-recovery.sh verify \
  "$HOME/backups-squad/Estoque-Production-AAAAMMDDTHHMMSSZ.sqlite3"
scripts/sqlite-recovery.sh restore \
  "$HOME/backups-squad/Estoque-Production-AAAAMMDDTHHMMSSZ.sqlite3" \
  /caminho/isolado/recuperacao-20261115/Estoque-restaurado.sqlite3 \
  11111111-1111-4111-8111-111111111111
```

`verify` exige `PRAGMA integrity_check = ok` e nenhuma linha em
`PRAGMA foreign_key_check`. O ID de exemplo acima pertence somente à fixture do
teste; substitua-o por um ID conhecido da cópia que está sendo recuperada, ou
omita o argumento opcional se não houver referência apropriada. O comando
`restore` repete as verificações na cópia isolada e recusa sobrescrever um
destino existente. Depois, confira também os registros esperados e a versão do
esquema/migrations antes de considerar o banco compatível com a aplicação.

Não inicie a versão atual da aplicação contra uma cópia antiga sem considerar
as migrations: o startup aplica migrations pendentes automaticamente. Restaure
com a versão compatível e planeje a atualização do esquema separadamente.

## Substituição controlada e retorno

Só depois de validar a cópia isolada e aprovar a janela:

1. Anote a versão publicada e confirme o caminho configurado do banco.
2. Pare o App Service no Portal Azure e confirme que a aplicação não está
   atendendo requisições. Para Compose, use `docker compose stop squad-estoque`.
3. Preserve o banco atual como retorno, com nome que inclua ambiente e UTC,
   mantendo-o fora do diretório publicado. Confirme que essa cópia existe e não
   está vazia.
4. Coloque o arquivo restaurado no caminho configurado, no mesmo volume
   persistente, e confira proprietário/permissões de leitura e escrita do
   processo da aplicação. Não apague a cópia de retorno.
5. Inicie a aplicação e verifique `/health`, logs de startup/migrations, login
   e os registros funcionais esperados.
6. Só encerre a janela após confirmar o serviço e os dados; retenha as cópias
   pelo prazo operacional definido.

Se a aplicação falhar, pare-a novamente, preserve a cópia que falhou para
diagnóstico, devolva o banco anterior a partir da cópia de retorno e reinicie a
versão compatível. Verifique `/health` e logs. Não repita o startup às cegas se
uma migration falhou; identifique primeiro a versão e a causa. Mantenha o
backup original e a cópia de retorno até confirmar a recuperação.

O script deliberadamente não executa parada, troca de arquivo, deploy ou
rollback: essas ações dependem do ambiente e exigem controle operacional.

## Teste reproduzível

Na raiz do repositório, com `sqlite3` no `PATH`, execute:

```bash
scripts/test-sqlite-recovery.sh
```

O teste cria uma fixture temporária, gera backup identificado como `Testing`,
restaura-a em diretório isolado, valida integridade e chaves estrangeiras,
confere um Produto conhecido e confirma que um destino já existente não é
sobrescrito. O diretório temporário é removido ao final. O registro da execução
e seus limites estão em [validacao-backup-restauracao.md](validacao-backup-restauracao.md).
