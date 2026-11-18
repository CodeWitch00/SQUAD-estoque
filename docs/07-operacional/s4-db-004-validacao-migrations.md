# S4-DB-004 — Validação das migrations do estoque

> **Status:** EXECUTADO LOCALMENTE — ambiente compartilhado BLOQUEADO (sem acesso/janela)
>
> **Cartão:** S4-DB-004
>
> **Ambiente-alvo:** App Service compartilhado do SQUAD Estoque (Azure)
>
> **Ambiente executado:** Windows 11 + WSL Ubuntu 26.04, .NET SDK 10.0.404, EF Core 10.0.11, SQLite 3.46.1
>
> **Data do planejamento:** 12/11/2026
>
> **Data da execução local:** 18/11/2026 (UTC)
>
> **Última atualização:** 18/11/2026

Este documento era um roteiro e foi preenchido com os resultados da execução
real. As etapas executáveis sem acesso ao Azure foram concluídas e possuem
evidência versionada em `docs/09-testing/evidencias/s4-db-004/`. As etapas que
dependem de janela, acesso SSH/Kudu e congelamento da massa compartilhada
permanecem explicitamente como `BLOQUEADO` — nenhuma delas foi simulada ou
declarada como concluída.

---

## 1. Objetivo

Comprovar que o `EstoqueContext`:

1. inicializa um banco SQLite novo usando somente as migrations versionadas;
2. atualiza um banco existente sem apagar produtos, SKUs, saldos ou históricos;
3. pode ser atualizado no ambiente compartilhado com backup prévio, verificação
   de dados e validação dos fluxos essenciais;
4. possui procedimento documentado para identificar uma falha de migration e
   recuperar o banco a partir do backup.

---

## 2. Escopo

### Dentro do escopo

- testes automatizados de banco vazio e de banco existente;
- ensaio de atualização sobre uma cópia do backup real;
- backup consistente antes de qualquer alteração de schema no ambiente
  compartilhado;
- verificação de contagens, somas, amostras, integridade e foreign keys;
- validação dos fluxos essenciais após a atualização;
- documentação de falha e recuperação;
- teste de restauração do backup em uma cópia.

### Fora do escopo

- criar, editar ou remover migrations;
- executar `Down`/rollback de migration no ambiente compartilhado;
- apagar arquivos SQLite, volumes ou backups;
- usar `EnsureCreated` como prova de schema;
- alterar schema manualmente em produção;
- publicar branch individual no Azure compartilhado;
- corrigir divergências estruturais já conhecidas sem cartão próprio.

Se for encontrado um defeito real de migration, registrar o bloqueio e abrir a
correção em cartão separado, conforme `CONTRIBUTING.md`.

---

## 3. Ambiente e versões

| Item | Valor | Como confirmar |
| --- | --- | --- |
| App Service | `<preencher>` | Portal/`az webapp show` |
| Resource group | `<preencher>` | Portal/`az webapp show` |
| URL | `<preencher>` | `README.md` / runbook do Azure |
| Plano | `<preencher>` | Portal |
| Sistema operacional | `<preencher>` | Portal |
| Runtime | `<preencher>` | Portal |
| Caminho do banco | `/home/data/Estoque.db` | Configuração do App Service |
| Connection string | `Data Source=/home/data/Estoque.db` | Configuração do App Service |
| Commit implantado | `<preencher>` | GitHub Actions/deploy |
| Migration atual do banco | `<preencher>` | `__EFMigrationsHistory` |
| Versão anterior conhecida | `<preencher>` | Pacote/commit de rollback |
| Janela de manutenção | `<preencher>` | Acordo com a equipe |
| Responsável pelo backup | `<preencher>` | Escala da equipe |
| Responsável pelo deploy | `<preencher>` | Escala da equipe |

### Versões versionadas no código

| Ordem | Migration |
| --- | --- |
| 1 | `20260817120442_InitialSquadSchema` |
| 2 | `20260901000759_AddEnumDomainConstraints` |
| 3 | `20261009181737_AddAtendimentoAuditavel` |
| 4 | `20261009200729_AddEntradaFiscalEImagemProduto` |
| 5 | `20261026220326_AddSkuUltimaAtualizacaoSaldo` |
| 6 | `20261029210004_AddConsultaIndexes` |

A versão realmente implantada deve ser confirmada no banco; não presumir que o
ambiente já está na última migration.

### 3.1 Ambiente da execução local (ensaio)

| Item | Valor |
| --- | --- |
| Sistema operacional | Windows 11 + WSL Ubuntu 26.04.1 LTS |
| SDK .NET | 10.0.404 |
| EF Core CLI (`dotnet-ef`) | 10.0.11 (manifest versionado em `.config/dotnet-tools.json`) |
| SQLite CLI (WSL) | 3.46.1 2024-08-13 |
| Banco de partida do ensaio | Cópia sintética representativa, criada com as próprias migrations até `20261009200729_AddEntradaFiscalEImagemProduto` |
| Migration de partida | `20261009200729_AddEntradaFiscalEImagemProduto` (4 de 6) |
| Migrations pendentes no ensaio | `20261026220326_AddSkuUltimaAtualizacaoSaldo` e `20261029210004_AddConsultaIndexes` |
| Modo da aplicação | `ASPNETCORE_ENVIRONMENT=Production`, `Demo__SeedUsers=false` |
| Porta do ensaio | `http://127.0.0.1:5199` |

> O ensaio usa massa sintética porque não há acesso ao banco compartilhado.
> Isso valida o **caminho de atualização**, o backup, a verificação e a
> restauração, mas **não substitui** a execução no ambiente compartilhado.

---

## 4. Pré-condições

- [ ] Cartão aprovado e escopo confirmado.
- [ ] Branch e PR corretos, sem misturar outras alterações.
- [ ] CI verde no commit que será implantado.
- [ ] Versão anterior conhecida e disponível para rollback.
- [ ] Janela de manutenção combinada com a equipe.
- [ ] Escrita na massa compartilhada congelada durante a janela.
- [ ] Acesso ao SSH/Kudu do App Service.
- [ ] `sqlite3` disponível ou binário temporário oficial preparado.
- [ ] Espaço em disco suficiente em `/home/data`.
- [ ] `Demo__SeedUsers` conferido; não confundir seed com perda de dados.
- [ ] Nenhuma outra pessoa publicando ou reiniciando o App Service.
- [ ] Backup do ambiente compartilhado criado e verificado.
- [ ] Cópia do backup baixada para fora do App Service.
- [ ] Ensaio em cópia do backup concluído.

---

## 5. Evidências a produzir

| Evidência | Como obter | Onde guardar |
| --- | --- | --- |
| Histórico de migrations | `SELECT * FROM __EFMigrationsHistory` | `docs/09-testing/evidencias/s4-db-004/` |
| Backup (nome, tamanho, hash) | `sha256sum` + `ls -lh` | Runbook + pasta de evidências |
| Integridade do backup | `PRAGMA integrity_check` | Log/arquivo de saída |
| Baseline de dados | `scripts/verificar-migrations-estoque.sql` | `baseline.txt` |
| Pós-atualização | Mesmo script após o deploy | `pos-atualizacao.txt` |
| Logs de migration | `az webapp log tail` / download | `deploy.log` |
| Health check | `curl -fsS .../health` | `health.txt` |
| Fluxos essenciais | Capturas de tela/respostas HTTP | `fluxos/` |
| Testes automatizados | `dotnet test` com logger TRX | `migrations.trx` |
| Restauração testada | Saída do drill em cópia | `restore-drill.txt` |

Não versionar arquivos `.db`, `.db-wal`, `.db-shm`, `.sqlite` ou `.sqlite3`.
Não registrar segredos, senhas ou dados reais.

---

## 6. Diagnóstico inicial (somente leitura)

### 6.1 No App Service

```bash
az webapp ssh -g <resource-group> -n <app-name>
```

Dentro do SSH:

```bash
ls -lh /home/data
ls -lh /home/data/backups

sqlite3 /home/data/Estoque.db \
  "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;"

sqlite3 /home/data/Estoque.db "PRAGMA integrity_check;"
sqlite3 /home/data/Estoque.db "PRAGMA foreign_key_check;"
```

Se `sqlite3` não existir:

```bash
command -v sqlite3 || echo "sqlite3 ausente; usar o binário temporário documentado"
```

O container do App Service pode não ter `sqlite3` nem `python3`. O procedimento
anterior baixou um binário oficial temporário do SQLite em `/tmp`. Registrar a
fonte e a verificação do binário usado.

### 6.2 No repositório

```bash
dotnet ef migrations list \
  --project src/SquadEstoque.Web/SquadEstoque.Web.csproj \
  --context EstoqueContext

dotnet ef migrations has-pending-model-changes \
  --project src/SquadEstoque.Web/SquadEstoque.Web.csproj \
  --context EstoqueContext
```

Resultado esperado: nenhuma alteração pendente de modelo e todas as migrations
versionadas reconhecidas.

---

## 7. Backup antes da atualização

> O `Program.cs` executa `Database.Migrate()` no startup. Portanto, o backup
> precisa ser feito **antes do deploy**, não depois que a aplicação subir.

```bash
STAMP="$(date -u +%Y%m%d-%H%M%S)"
mkdir -p /home/data/backups

sqlite3 /home/data/Estoque.db \
  ".backup '/home/data/backups/Estoque-${STAMP}.db'"

sqlite3 /home/data/backups/Estoque-${STAMP}.db "PRAGMA integrity_check;"
sha256sum /home/data/backups/Estoque-${STAMP}.db
ls -lh /home/data/backups/Estoque-${STAMP}.db
```

Alternativa reproduzível:

```bash
./scripts/backup-estoque-sqlite.sh /home/data/Estoque.db /home/data/backups
```

### 7.1 Baixar uma cópia para fora do App Service

Usar o mesmo método já utilizado no backup de 24/09/2026. Uma opção é o Kudu:

```text
https://<app-name>.scm.azurewebsites.net/api/vfs/data/backups/Estoque-<STAMP>.db
```

Registrar:

| Campo | Valor |
| --- | --- |
| Arquivo de backup | `Estoque-<STAMP>.db` |
| Data/hora UTC | `<preencher>` |
| Tamanho em bytes | `<preencher>` |
| SHA-256 | `<preencher>` |
| `integrity_check` | `<preencher>` |
| Caminho da cópia off-host | `<preencher>` |
| Responsável | `<preencher>` |

> Não excluir o backup nem o banco antigo. Em caso de falha, eles são a
> evidência e a fonte de recuperação.

---

## 8. Baseline de dados

Executar as consultas de `scripts/verificar-migrations-estoque.sql` antes da
atualização e salvar a saída.

Em banco que ainda está em versão anterior, comentar as tabelas que ainda não
existem. O script assume o schema mais recente.

Registrar no mínimo:

- contagem por tabela;
- `COUNT(*)`, `SUM(SaldoAtual)`, `MIN(SaldoAtual)` e `MAX(SaldoAtual)` de `Sku`;
- IDs e valores de 3 a 5 registros por tabela;
- `PRAGMA integrity_check`;
- `PRAGMA foreign_key_check`;
- lista de índices existentes;
- presença de tabelas `ef_temp_*`;
- conteúdo de `__EFMigrationsHistory`.

Valores medidos no ensaio local (massa sintética com 4 migrations de partida):

| Métrica | Antes | Depois | Igual? |
| --- | ---: | ---: | --- |
| `Usuario` | 1 | 1 | Sim |
| `Produto` | 1 | 1 | Sim |
| `Sku` | 2 | 2 | Sim |
| `Movimentacao` | 1 | 1 | Sim |
| `Ruptura` | 1 | 1 | Sim |
| `Atendimento` | 1 | 1 | Sim |
| `EntradaFiscal` | 1 | 1 | Sim |
| `ItemEntradaFiscal` | 1 | 1 | Sim |
| `VinculoProdutoFornecedor` | 1 | 1 | Sim |
| Soma de `Sku.SaldoAtual` | 8 | 8 | Sim |
| `integrity_check` | `ok` | `ok` | Sim |
| `foreign_key_check` | sem linhas | sem linhas | Sim |

Evidências: `baseline.txt`, `copia-antes.txt` e `pos-atualizacao.txt` em
`docs/09-testing/evidencias/s4-db-004/`. As colunas acrescentadas
(`Sku.UltimaAtualizacaoSaldoEm`, `Produto.ImagemUrl`,
`Movimentacao.AtendimentoId`, `Ruptura.AtendimentoId`) permanecem `NULL` nos
registros antigos, e os quatro índices esperados estão presentes após a upgrade.

---

## 9. Ensaio em cópia do backup

> Esta é a etapa mais importante para “sem perda de dados”. Ela usa os dados
> reais do ambiente em uma cópia descartável, antes de tocar no banco
> compartilhado.

1. Baixar o backup para uma máquina local ou para `/tmp`.
2. Copiar para um caminho de ensaio, por exemplo `/tmp/Estoque-copia.db`.
3. Rodar a versão nova apontando para a cópia:

```bash
ASPNETCORE_ENVIRONMENT=Production \
Demo__SeedUsers=false \
ConnectionStrings__EstoqueContext="Data Source=/tmp/Estoque-copia.db" \
dotnet run \
  --project src/SquadEstoque.Web/SquadEstoque.Web.csproj \
  --urls http://localhost:5199
```

4. Conferir nos logs se as migrations pendentes foram aplicadas.
5. Abrir `/health`.
6. Executar o script de verificação na cópia.
7. Comparar contagens, somas e amostras com o baseline.
8. Exercitar os fluxos essenciais na cópia.
9. Guardar os logs e a saída das consultas.

Se a cópia já estiver na última migration, o ensaio prova apenas idempotência.
Nesse caso, usar um backup mais antigo ou o teste automatizado com massa
sintética para provar o caminho de atualização.

> Não apagar a cópia nem o backup. Se o ensaio falhar, ela será necessária para
> diagnosticar o problema.

### 9.1 Resultado do ensaio executado

1. Banco de partida criado com `dotnet ef database update
   20261009200729_AddEntradaFiscalEImagemProduto` e populado com dados legados
   (`baseline-migrate.log`, `baseline.txt`).
2. Backup gerado pelo script versionado:
   `Estoque-20261118-231629.db`, 176128 bytes,
   SHA-256 `b2147d04a2965db8758331444ad1104c315d23ecfdf302dfc6e9687e5f937561`,
   `integrity_check=ok` (`backup.log`).
3. Cópia do backup em `Estoque-copia2.db` (nenhum arquivo SQLite foi excluído).
4. Aplicação iniciada em Production contra a cópia; o startup aplicou exatamente
   as duas migrations pendentes e liberou o lock (`app-copia2.log`).
5. `/health` respondeu `200 Healthy`; `/`, `/Account/Login`, `/Produtos`,
   `/Movimentacoes`, `/Rupturas`, `/EntradasFiscais`, `/Atendimentos`,
   `/Estoque/Consulta` e `/SaldosZerados` responderam HTTP 200
   (`fluxos-copia.log`).
6. Verificação pós-upgrade executada com o mesmo script somente leitura
   (`pos-atualizacao.txt`); contagens, somas e amostras conferem com o baseline.

**Resultado: APROVADO no ensaio local.** O ambiente compartilhado não foi
alterado; a etapa 10 continua bloqueada.

---

## 10. Atualização do ambiente compartilhado

1. Confirmar que o backup foi criado, verificado e copiado para fora do App
   Service.
2. Confirmar que o ensaio em cópia foi aprovado.
3. Congelar alterações na massa compartilhada.
4. Publicar a versão nova (GitHub Actions ou deploy manual de contingência).
5. Acompanhar os logs:

```bash
az webapp log tail -g <resource-group> -n <app-name>
```

6. Confirmar `/health`.
7. Executar a verificação pós-atualização da seção 11.

Lembrar: a migration roda no startup. Se o processo falhar, a aplicação pode
ficar indisponível; seguir imediatamente para as seções 13 e 14.

---

## 11. Verificação pós-atualização

```bash
curl -fsS https://<app-name>.azurewebsites.net/health
```

No SSH:

```bash
sqlite3 /home/data/Estoque.db \
  "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;"

sqlite3 /home/data/Estoque.db "PRAGMA integrity_check;"
sqlite3 /home/data/Estoque.db "PRAGMA foreign_key_check;"

sqlite3 /home/data/Estoque.db \
  "SELECT name FROM sqlite_master WHERE name LIKE 'ef_temp_%';"

sqlite3 /home/data/Estoque.db \
  "SELECT name FROM sqlite_master WHERE type = 'index' AND name IN (
     'IX_Produto_Marca',
     'IX_Produto_Categoria',
     'IX_Sku_ProdutoId_SaldoZero',
     'IX_Ruptura_SkuId_CriadoEm'
   ) ORDER BY name;"
```

Checklist:

- [ ] `/health` responde `Healthy` com HTTP 200.
- [ ] `__EFMigrationsHistory` contém as 6 migrations esperadas.
- [ ] Contagens e somas batem com o baseline.
- [ ] Registros amostrados continuam com os mesmos valores.
- [ ] `PRAGMA integrity_check` retorna `ok`.
- [ ] `PRAGMA foreign_key_check` não retorna linhas.
- [ ] Não existem tabelas `ef_temp_*`.
- [ ] Não existe lock preso em `__EFMigrationsLock`.
- [ ] Colunas novas estão nulas/default nos registros antigos:
  - `Sku.UltimaAtualizacaoSaldoEm`
  - `Produto.ImagemUrl`
  - `Movimentacao.AtendimentoId`
  - `Ruptura.AtendimentoId`
- [ ] Índices e constraints esperados existem.

---

## 12. Fluxos essenciais após a atualização

| Fluxo | Perfil | Resultado esperado | Status |
| --- | --- | --- | --- |
| `/health` | anônimo | `Healthy`, HTTP 200 | `<preencher>` |
| Login | LOJISTA | sessão criada e redirecionamento correto | `<preencher>` |
| Login | VENDEDOR | sessão criada e redirecionamento correto | `<preencher>` |
| Produtos: listagem e detalhes | LOJISTA | dados e numeração preservados | `<preencher>` |
| Movimentações: histórico | LOJISTA | registros anteriores preservados | `<preencher>` |
| Consulta: busca, grade e saldo | VENDEDOR | saldo e última atualização corretos | `<preencher>` |
| Saída/venda | conforme perfil | saldo decrementado e movimentação criada | `<preencher>` |
| Ruptura | conforme perfil | registrada sem alterar saldo | `<preencher>` |
| Entrada fiscal | LOJISTA | nota/itens preservados | `<preencher>` |
| Rota não autorizada | ambos | bloqueio mantido | `<preencher>` |
| Reinício do App Service | — | dados continuam presentes | `<preencher>` |

Para validar escrita no ambiente compartilhado, criar um registro fictício
claramente identificável e registrar o ID. Não usar dados reais.

---

## 13. Como identificar uma falha de migration

| Sintoma | Onde olhar | Interpretação | Ação |
| --- | --- | --- | --- |
| App não sobe ou retorna 500 na primeira requisição | Logs do App Service, `Microsoft.EntityFrameworkCore.Migrations` e `SqliteException` | Migration falhou no startup | Parar, preservar o banco, avaliar restauração |
| `/health` falha após deploy | Logs e `__EFMigrationsHistory` | App sem schema completo ou sem subir | Não insistir; seguir recuperação |
| Migration esperada ausente | `SELECT * FROM __EFMigrationsHistory` | Migration não aplicada ou revertida | Comparar com a versão esperada |
| Tabelas `ef_temp_*` | `sqlite_master` | Reconstrução de tabela interrompida | Não rodar de novo às cegas; restaurar ou concluir com aprovação |
| `integrity_check` diferente de `ok` | SQLite | Corrupção ou interrupção grave | Restaurar backup |
| `foreign_key_check` com linhas | SQLite | Violação de FK após reconstrução | Investigar e restaurar se necessário |
| `database is locked` | Logs e SQLite | Escrita concorrente ou lock de migration | Garantir instância única, parar escrita e repetir |
| `CHECK constraint failed` | Logs da migration | Dado legado viola constraint nova | Identificar linhas; corrigir com aprovação ou abortar |
| `no such column` / `no such table` | Logs | Schema parcial ou versão errada | Restaurar e reaplicar na cópia |
| Contagens/somas diferentes do baseline | Consultas de verificação | Possível perda de dados | Parar imediatamente e restaurar |
| `__EFMigrationsLock` preso | Tabela de lock | Processo anterior interrompido | Confirmar que não há processo ativo antes de limpar, com aprovação |

### 13.1 Diagnóstico rápido

```bash
sqlite3 /home/data/Estoque.db \
  "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"

sqlite3 /home/data/Estoque.db \
  "SELECT name FROM sqlite_master WHERE name LIKE 'ef_temp_%';"

sqlite3 /home/data/Estoque.db "PRAGMA integrity_check;"
sqlite3 /home/data/Estoque.db "PRAGMA foreign_key_check;"
sqlite3 /home/data/Estoque.db "SELECT * FROM __EFMigrationsLock;"
```

Se a falha for em dado legado, identificar as linhas inválidas antes de decidir
entre corrigir os dados (com aprovação) e restaurar o backup. Não editar dados
de produção sem autorização.

---

## 14. Como recuperar a partir do backup

> Regra do cartão: não excluir arquivos SQLite. Usar `mv`/`cp`, nunca `rm`.

1. Parar a aplicação para impedir novas escritas:

```bash
az webapp stop -g <resource-group> -n <app-name>
```

2. Preservar o estado atual (banco e arquivos `-wal`/`-shm`):

```bash
STAMP="$(date -u +%Y%m%d-%H%M%S)"
mv /home/data/Estoque.db "/home/data/Estoque.falha-${STAMP}.db"
[ -f /home/data/Estoque.db-wal ] && mv /home/data/Estoque.db-wal "/home/data/Estoque.falha-${STAMP}.db-wal"
[ -f /home/data/Estoque.db-shm ] && mv /home/data/Estoque.db-shm "/home/data/Estoque.falha-${STAMP}.db-shm"
```

3. Coletar evidências antes de restaurar: logs, `__EFMigrationsHistory`,
   `sqlite_master`, `integrity_check`, `foreign_key_check` e listagem de
   arquivos.

4. Restaurar o backup:

```bash
cp /home/data/backups/Estoque-<STAMP>.db /home/data/Estoque.db
sqlite3 /home/data/Estoque.db "PRAGMA integrity_check;"
```

5. Validar a restauração: contagens, somas e amostras contra o baseline.

6. **Importante:** se a versão nova já está publicada, iniciar o App Service
   executaria `Database.Migrate()` novamente. Por isso, antes de iniciar,
   republicar a versão anterior conhecida como boa.

7. Iniciar a aplicação e validar:

```bash
az webapp start -g <resource-group> -n <app-name>
curl -fsS https://<app-name>.azurewebsites.net/health
```

8. Validar login, Produtos, Movimentações, Consulta e persistência.

9. Comunicar o ocorrido, o RPO (desde o backup) e o RTO (tempo de restauração).

10. Reproduzir a falha na cópia preservada, corrigir em cartão próprio e só
    então tentar novamente, com novo backup.

---

## 15. Teste de restauração (drill)

> Este teste fecha a pendência registrada em
> `docs/07-operacional/validacao-persistencia-azure.md`, que ainda diz
> “restauração não testada”.

1. Restaurar o backup em um caminho temporário, nunca no caminho de produção.
2. Executar `PRAGMA integrity_check`.
3. Executar `PRAGMA foreign_key_check`.
4. Conferir contagens e somas contra o baseline.
5. Opcionalmente, subir a aplicação contra a cópia restaurada e validar
   `/health` e os fluxos de leitura.
6. Registrar o resultado e o tempo gasto.

Execução local em 18/11/2026 (evidência: `restore-drill.txt`):

| Verificação | Resultado |
| --- | --- |
| Backup restaurado em cópia | APROVADO — `Estoque-restaurado.db` (destino temporário, nunca produção) |
| `integrity_check` | `ok` |
| `foreign_key_check` | sem linhas |
| Contagens/somas iguais ao baseline | APROVADO — `diff` sem diferenças; 1\|1\|2\|8\|1\|1\|1\|1\|1\|1 |
| Histórico de migrations do restaurado | 4 migrations (idêntico ao baseline) |
| `/health` na cópia | Não executado neste drill; `/health` foi validado na cópia em ensaio (item 9.1) |
| Tempo de restauração | 72 ms (cópia local de 176128 bytes) |

---

## 16. Resultado da execução

| Etapa | Status | Evidência |
| --- | --- | --- |
| Build (`Release`) | APROVADO | `build.log` — 0 avisos, 0 erros |
| Suíte completa | APROVADO | `test.log` e `migrations.trx` — 348 aprovados, 0 falhas, 1 ignorado (349) |
| Teste automatizado: banco vazio | APROVADO | `migrations.trx` — 1 teste aprovado |
| Teste automatizado: banco existente | APROVADO | `migrations.trx` — 5 versões de partida aprovadas |
| Teste automatizado: falha sem perda | APROVADO | `migrations.trx` — 1 teste aprovado |
| Diagnóstico de migrations no repositório | APROVADO | `ef-migrations-list.log` (6 migrations) e `ef-pending-model-changes.log` (sem alterações pendentes) |
| Backup antes da atualização (ensaio) | APROVADO | `backup.log` — SHA-256 e `integrity_check=ok` |
| Ensaio em cópia do backup | APROVADO | `app-copia2.log`, `baseline.txt`, `pos-atualizacao.txt`, `fluxos-copia.log` |
| Verificação pós-atualização (ensaio) | APROVADO | `pos-atualizacao.txt` — contagens/somas iguais, índices presentes, sem `ef_temp_*` |
| Fluxos essenciais (ensaio) | APROVADO | `fluxos-copia.log` — `/health` 200 Healthy e rotas respondendo |
| Teste de restauração | APROVADO | `restore-drill.txt` — restauração idêntica ao baseline em 72 ms |
| Backup do ambiente compartilhado | BLOQUEADO | Sem `az` CLI/acesso SSH ao App Service neste ambiente |
| Atualização do ambiente compartilhado | BLOQUEADO | Depende de janela, congelamento e autorização (seções 10 e 14) |
| Verificação pós-atualização (compartilhado) | BLOQUEADO | Depende da etapa anterior |
| Fluxos essenciais (compartilhado) | BLOQUEADO | Depende da etapa anterior |
| CI/PR | EM ANDAMENTO | PR com resultados será aberto; status do workflow a conferir no GitHub |

Status possíveis: `NÃO EXECUTADO`, `IMPLEMENTADO (execução pendente)`, `APROVADO`, `FALHOU`, `BLOQUEADO`.

> **Nota:** os cenários automatizados usam a cadeia real de migrations em SQLite de arquivo (sem `EnsureCreated`), cobrem inicialização do zero, atualização a partir de versões anteriores com preservação de snapshot (produtos, SKUs, saldos e históricos), idempotência, constraints e falha de migration sem perda dos dados legados. Todos os 7 cenários foram executados com sucesso na suíte completa. O ensaio operacional foi realizado localmente em cópia do backup, incluindo restauração. A validação no **ambiente compartilhado** (backup do banco real, congelamento, deploy, atualização e fluxos em produção) continua dependente de janela/acesso e **não foi executada**; nenhuma evidência deste documento deve ser lida como confirmação dessa etapa.

---

## 17. Anexos

- [x] Log de build/testes (`build.log`, `test.log`, `migrations.trx`).
- [x] Saída de `__EFMigrationsHistory` (`baseline.txt`, `pos-atualizacao.txt`).
- [x] Backup: nome, tamanho, hash e integridade (`backup.log`). Caminho off-host não se aplica ao ensaio local.
- [x] Saída de `integrity_check` e `foreign_key_check` (`pos-atualizacao.txt`, `restore-drill.txt`).
- [x] Baseline e pós-atualização (`baseline.txt`, `pos-atualizacao.txt`).
- [x] Logs de migration do ensaio (`app-copia2.log`).
- [x] Resposta de `/health` (`fluxos-copia.log`).
- [x] Fluxos essenciais (`fluxos-copia.log`).
- [x] Resultado do teste de restauração (`restore-drill.txt`).
- [ ] Link do PR e do cartão (PR a ser aberto nesta entrega).
- [ ] Evidências do ambiente compartilhado (BLOQUEADO: janela/acesso).

Nenhum arquivo `.db`, `.db-wal`, `.db-shm`, `.sqlite` ou `.sqlite3` deve ser
versionado.

---

## 18. Decisões pendentes

1. “Produção” é o App Service compartilhado ou existe outro ambiente?
2. Qual migration está aplicada no banco hoje?
3. Quem está autorizado a fazer backup, deploy e restauração?
4. Qual é a janela de manutenção e o tempo de congelamento?
5. Podemos restaurar o backup em uma cópia e rodar a aplicação contra ela?
6. “Não excluir arquivos SQLite” vale também para artefatos temporários de
   teste e tabelas `ef_temp_*`?
7. Quais fluxos são obrigatórios para o aceite?
8. Podemos criar um registro fictício no ambiente compartilhado para validar
   escrita?
9. O cartão espera scripts operacionais ou apenas testes + documentação?
10. Se houver defeito de migration, o cartão apenas registra o bloqueio ou já
    prevê a correção em cartão separado?

### 18.1 Situação após a execução local

| Pergunta | Situação |
| --- | --- |
| 1. Ambiente de produção | Confirmado como App Service compartilhado pelo `README.md` e `docs/07-operacional/azure.md`; não acessado nesta execução. |
| 2. Migration aplicada hoje | Desconhecida no banco compartilhado; no ensaio foi usada a versão 4 de 6. Confirmar `__EFMigrationsHistory` antes de qualquer deploy. |
| 3. Autorização para backup/deploy/restauração | Não confirmada; pré-condição para a etapa compartilhada. |
| 4. Janela e congelamento | Não combinados; pré-condição para a etapa compartilhada. |
| 5. Restaurar em cópia e rodar a aplicação | Respondido localmente: SIM, executado e aprovado (seções 9.1 e 15). |
| 6. Exclusão de SQLite/tabelas `ef_temp_*` | Nenhum arquivo SQLite foi excluído; não havia `ef_temp_*` após a atualização. Limpeza restrita a logs temporários. |
| 7. Fluxos obrigatórios | Saúde, login, consulta, listagens e histórico foram exerciseados no ensaio (item 9.1); confirmar a lista oficial para o compartilhado. |
| 8. Registro fictício no compartilhado | Não executado; depende de autorização na janela. |
| 9. Escopo do cartão | Scripts operacionais + testes + documentação, conforme revisão da revisora. |
| 10. Defeito de migration | Nenhum defeito encontrado; se aparecer, registrar bloqueio e abrir cartão separado. |
