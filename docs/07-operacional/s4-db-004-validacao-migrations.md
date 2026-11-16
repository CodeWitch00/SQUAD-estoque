# S4-DB-004 — Validação das migrations do estoque

> **Status:** PLANEJADO — NÃO EXECUTADO
>
> **Cartão:** S4-DB-004
>
> **Ambiente-alvo:** `<preencher: App Service asp-squad-estoque-free / outro>`
>
> **Responsável pela execução:** `<preencher>`
>
> **Data do planejamento:** 12/11/2026
>
> **Última atualização:** `<preencher>`

Este documento é um roteiro. Ele deve ser preenchido **somente após a execução
real**. Enquanto uma etapa não for executada, manter o status `NÃO EXECUTADO`.
Uma validação planejada não pode ser descrita como concluída.

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

| Métrica | Antes | Depois | Igual? |
| --- | --- | --- | --- |
| `Usuario` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `Produto` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `Sku` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `Movimentacao` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `Ruptura` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `Atendimento` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `EntradaFiscal` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `ItemEntradaFiscal` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `VinculoProdutoFornecedor` | `<preencher>` | `<preencher>` | `<sim/não>` |
| Soma de `Sku.SaldoAtual` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `integrity_check` | `<preencher>` | `<preencher>` | `<sim/não>` |
| `foreign_key_check` | `<preencher>` | `<preencher>` | `<sim/não>` |

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

| Verificação | Resultado |
| --- | --- |
| Backup restaurado em cópia | `<preencher>` |
| `integrity_check` | `<preencher>` |
| `foreign_key_check` | `<preencher>` |
| Contagens/somas iguais ao baseline | `<preencher>` |
| `/health` na cópia | `<preencher>` |
| Tempo de restauração | `<preencher>` |

---

## 16. Resultado da execução

| Etapa | Status | Evidência |
| --- | --- | --- |
| Teste automatizado: banco vazio | `IMPLEMENTADO (execução local/CI pendente)` | `tests/SquadEstoque.Web.Tests/EstoqueMigrationsBancoVazioTests.cs` |
| Teste automatizado: banco existente | `IMPLEMENTADO (execução local/CI pendente)` | `tests/SquadEstoque.Web.Tests/EstoqueMigrationsAtualizacaoTests.cs` |
| Teste automatizado: falha sem perda | `IMPLEMENTADO (execução local/CI pendente)` | `tests/SquadEstoque.Web.Tests/EstoqueMigrationsAtualizacaoTests.cs` |
| Backup do ambiente compartilhado | `NÃO EXECUTADO` | `<link>` |
| Ensaio em cópia do backup | `NÃO EXECUTADO` | `<link>` |
| Atualização do ambiente compartilhado | `NÃO EXECUTADO` | `<link>` |
| Verificação pós-atualização | `NÃO EXECUTADO` | `<link>` |
| Fluxos essenciais (ambiente compartilhado) | `NÃO EXECUTADO` | `<link>` |
| Teste de restauração | `NÃO EXECUTADO` | `<link>` |
| CI/PR | `NÃO EXECUTADO` | `<link>` |

Status possíveis: `NÃO EXECUTADO`, `IMPLEMENTADO (execução pendente)`, `APROVADO`, `FALHOU`, `BLOQUEADO`.

> **Nota:** os cenários automatizados usam a cadeia real de migrations em SQLite de arquivo (sem `EnsureCreated`), cobrem inicialização do zero, atualização a partir de versões anteriores com preservação de snapshot (produtos, SKUs, saldos e históricos), idempotência, constraints e falha de migration sem perda dos dados legados. A validação operacional no ambiente compartilhado (backup prévio, ensaio em cópia real, atualização e fluxos em produção) permanece dependente de janela/acesso e deve ser registrada com evidências quando executada.

---

## 17. Anexos

- [ ] Log de build/testes (`migrations.trx`).
- [ ] Saída de `__EFMigrationsHistory`.
- [ ] Backup: nome, tamanho, hash e caminho off-host.
- [ ] Saída de `integrity_check` e `foreign_key_check`.
- [ ] Baseline e pós-atualização.
- [ ] Logs de deploy/migration.
- [ ] Resposta de `/health`.
- [ ] Capturas dos fluxos essenciais.
- [ ] Resultado do teste de restauração.
- [ ] Link do PR e do cartão.

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
