# [S4-QA-012] Persistência do SQLite no Docker (reinício sem perda)

- **Cartão:** S4-QA-012 — Testar persistência no Docker - reinício sem perda do SQLite
- **Sprint/Reunião/Prazo:** Sprint 4 | Reunião-alvo 18/11/2026 | Prazo 30/11/2026 23:59 (America/Sao_Paulo)
- **Área:** QA e banco de dados
- **Data de execução:** 19/11/2026
- **Tipo de validação:** validação operacional, **sem alteração do repositório** `SQUAD-estoque`
- **Evidências brutas:** `docs/09-testing/evidencias/s4-qa-012/`, incluindo a reexecução ao vivo com prefixo `reexec-` (seção 7)

## 1. Objetivo

Comprovar que os dados do sistema permanecem após **parar, remover e recriar** o container usando o **mesmo volume nomeado**, e que um **volume novo** inicia um banco independente.

## 2. Ambiente e configuração inspecionada

| Item | Valor |
|---|---|
| Projeto | `SQUAD-estoque` (ASP.NET Core/.NET 10, SQLite, EF Core) |
| Compose | `SQUAD-estoque/compose.yaml` |
| Serviço / container | `squad-estoque` |
| Imagem | `squad-estoque:local` (build local do `Dockerfile`) |
| Volume nomeado | `squad-estoque-data` → montado em `/app/data` |
| Banco | `/app/data/Estoque.db` (`ConnectionStrings__EstoqueContext=Data Source=/app/data/Estoque.db`) |
| Chaves de proteção | `/app/data/keys` (`DataProtection__KeysPath=/app/data/keys`) |
| Porta / health | `http://localhost:8080` → `GET /health` = `Healthy` |
| Política de restart | `unless-stopped` (substituída ao remover/recriar via Compose) |

**Nota operacional (sem alteração do repo):** a aplicação em `Production` exige `AllowedHosts` explícito (sem curingas). Para viabilizar a execução local controlada sem editar arquivos versionados, foi usado um override Compose **temporário e externo** (`docs/09-testing/evidencias/s4-qa-012/compose.runtime.yaml`) definindo apenas `AllowedHosts=localhost;127.0.0.1`. Nenhum segredo, connection string ou arquivo versionado foi exposto/alterado. Para a criação inicial dos usuários demonstrativos pelo fluxo normal de inicialização (`Demo__SeedUsers` em banco vazio), utilizou-se um override externo transitório (`compose.seed.yaml`) que foi substituído antes da coleta final; credenciais demonstrativas não são registradas neste relatório.

## 3. Dados identificáveis criados (fluxos normais da aplicação)

Dados criados via interface/fluxos HTTP da aplicação (login, cadastro de produto, cadastro de grade com saldo, entrada de estoque, registro de ruptura via atendimento de vendedor), com identificadores únicos para rastreio:

| Entidade | Identificador / marcador | Detalhes relevantes |
|---|---|---|
| Usuário (seed, fluxo de inicialização) | `E24F424F-7400-4808-9FFD-CCBD82BEC3EE` | `Lojista Teste`, `lojista@squad.com`, Perfil `1` (LOJISTA) |
| Usuário (seed, fluxo de inicialização) | `D838185E-7A03-4410-9BEF-E71D68781349` | `Vendedor Teste`, `vendedor@squad.com`, Perfil `0` (VENDEDOR) |
| Produto | `514BBCC8-1A3B-4F19-B59A-5B62BB71E7B0` | Nome **`QA-PERSIST-20261119-S4QA012`**, Marca `QA-S4QA012`, Categoria `Persistencia`, Cor `TesteAzul`, Ativo `1` |
| SKU (saldo) | `2596EB02-0D95-4FF3-874E-51F6FC243B96` | Produto acima, Numeração `40`, **SaldoAtual `8`**, `UltimaAtualizacaoSaldoEm 2026-11-19 17:17:41.6231843`, Ativo `1` |
| Movimentação (histórico) | `FE61CB9F-E936-4B36-A4DF-7BBB852A252C` | Tipo `0` (ENTRADA), Qtd `5`, `2026-11-19 17:16:58.0482904`, Motivo `Saldo inicial no cadastro da grade`, Usuário LOJISTA |
| Movimentação (histórico) | `A5B88489-F024-4EAD-AB69-A1A64D537207` | Tipo `0` (ENTRADA), Qtd `3`, `2026-11-19 17:17:41.6231895`, Motivo `QA-PERSIST entrada extra 2026-11-19`, Usuário LOJISTA |
| Ruptura (histórico) | `DE08D9D8-1DB8-4DC7-A4A6-F78E7721EF11` | SKU/Produto acima, Usuário VENDEDOR, `2026-11-19 17:18:21.7396937`, Atendimento `7B01E1C1-5974-4B12-B05D-E5AA610E5DF3` |
| Atendimento (histórico) | `7B01E1C1-5974-4B12-B05D-E5AA610E5DF3` | Produto/SKU acima, Status `1`, Resultado `1`, Criado `2026-11-19 17:18:21.685946`, Concluído `2026-11-19 17:18:21.7398747` |

> Consultas de conferência omitiram campos sensíveis (ex.: `SenhaHash`). Nenhum `.env`/segredo foi impresso.

## 4. Execução e comandos (sanitizados)

Todos os comandos foram executados a partir de `SQUAD-estoque/` (raiz do compose), exceto os indicados. Arquivos de override referenciados residem em `../docs/09-testing/evidencias/s4-qa-012/` (fora do repositório).

### 4.1 Pré-checks e estado inicial
```powershell
docker version
docker compose version
docker ps -a
docker volume ls
# health: curl http://localhost:8080/health  -> Healthy
```

### 4.2 Criação dos marcadores (resumo dos fluxos)
- Login LOJISTA → `POST /Produtos/Create` (produto `QA-PERSIST-20261119-S4QA012`) → `POST /Produtos/AdicionarNumeracoes/{produtoId}` (numeração 40, quantidade 5, saldo inicial).
- Login LOJISTA → `POST /Movimentacoes/Entrada` (SKU acima, quantidade 3, motivo identificável) — entrada extra para histórico.
- Login VENDEDOR → `POST /Estoque/RegistrarNaoTinha` (SKU/produto acima) — geração de ruptura/atendimento.
- Snapshot do banco (somente leitura, sem `SenhaHash`): `sqlite3` via container `alpine:3.22` montando `squad-estoque-data:/data` → `docs/09-testing/evidencias/s4-qa-012/snapshot-antes.txt`; `PRAGMA integrity_check = ok`.

### 4.3 Parar e remover o container (sem remover o volume)
```powershell
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml down
# (sem -v / --volumes)
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml ps -a
docker volume inspect squad-estoque-data
# Conferência do conteúdo do volume com container removido:
docker run --rm -v squad-estoque-data:/data:ro alpine:3.22 sh -c "ls -la /data"
# Consulta dos marcadores (cópia read-only do .db para /tmp dentro do container efêmero):
# ProdutoMarcador=1, MovMarcador=2, RupMarcador=1, SaldoAtual(num 40)=8
```

### 4.4 Recriar o serviço e conferir
```powershell
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml up -d
curl http://localhost:8080/health   # -> Healthy
# Snapshot pós-recriação (mesmas consultas): docs/09-testing/evidencias/s4-qa-012/snapshot-depois.txt
# Comparação: diff antes x depois -> NO_DIFF
```

### 4.5 Prova de volume novo (banco independente)
```powershell
# Projeto isolado + volume nomeado distinto (squad-estoque-data-fresh), porta 8081
docker compose -p squad-estoque-fresh -f compose.yaml -f ..\S4-QA-012-evidencias\compose.fresh.yaml up -d
curl http://localhost:8081/health   # -> Healthy (serviço principal 8080 permaneceu Healthy)
# Consulta no volume fresh: integrity_check=ok, Usuarios=0, Produtos=0, Skus=0,
# Movimentacoes=0, Rupturas=0, ProdutoMarcador=0, tabelas/schema presentes.
docker compose -p squad-estoque-fresh -f compose.yaml -f ..\S4-QA-012-evidencias\compose.fresh.yaml down
docker volume rm squad-estoque-data-fresh
```

## 5. Resultados e evidências

| Critério do cartão | Evidência | Resultado |
|---|---|---|
| Dados identificáveis criados pelos fluxos normais | Seção 3; `snapshot-antes.txt` | **OK** — usuários, produto, SKU/saldo, 2 movimentações, 1 ruptura, 1 atendimento |
| Parar e remover container **sem** remover volume | `compose-down.txt`, `compose-ps-after-down.txt` (vazio) | **OK** — container/rede removidos, `down` sem `-v` |
| Volume preservado após remoção do container | `volume-inspect-after-down.txt`, `volume-ls-after-down.txt`, `volume-check-after-down.txt` | **OK** — volume `squad-estoque-data` existente com `Estoque.db` + `keys/`; marcadores presentes (1/2/1, saldo 8) |
| Recriar serviço e conferir dados preservados | `compose-up-recreate.txt`, `health-depois.txt`, `snapshot-depois.txt`, `diff-antes-depois.txt`, `logs-after-recreate.txt` | **OK** — `/health Healthy`; **`NO_DIFF`** entre antes e depois; `PRAGMA integrity_check=ok`; usuários, produto, saldo e históricos íntegros |
| Volume novo inicia banco independente | `fresh-up2.txt`, `fresh-db-check.txt`, `fresh-volume-inspect.txt`, `fresh-down.txt`, `fresh-volume-rm.txt` | **OK** — volume `squad-estoque-data-fresh` com schema/migrations e **sem** marcadores nem dados do volume original; removido após a prova |
| Comandos sem segredos | Este relatório e arquivos em `docs/09-testing/evidencias/s4-qa-012/` | **OK** — sem senhas, hashes, tokens ou `.env` expostos |

**Comparação antes × depois:** `docs/09-testing/evidencias/s4-qa-012/diff-antes-depois.txt` → `NO_DIFF` (usuários, produtos, SKUs/saldos, movimentações, rupturas e atendimentos idênticos após recriação).

## 6. Estado final

- Serviço `squad-estoque` **em execução** e saudável em `http://localhost:8080` (`/health = Healthy`), utilizando o volume original `squad-estoque-data` com os dados preservados.
- Volume de prova `squad-estoque-data-fresh` removido (limpeza concluída). Volumes remanescentes do projeto: `squad-estoque-data`.
- **Nenhum arquivo do repositório `SQUAD-estoque` foi alterado por esta validação** (alterações preexistentes de outros cartões permanecem intocadas). Artefatos desta atividade estão fora do repositório (`S4-QA-012-persistencia-docker.md` e pasta `docs/09-testing/evidencias/s4-qa-012/`).

## 7. Reexecução ao vivo (verificação independente)

Para confirmar que as evidências refletem o comportamento real do ambiente, o ciclo completo foi **reexecutado** em 19/11/2026 com o Docker Desktop em execução (engine `29.9.3`), a partir de `SQUAD-estoque/`, com os mesmos overrides externos (`compose.runtime.yaml` e `compose.fresh.yaml`). Nenhum arquivo versionado do repositório foi alterado por esta reexecução.

### 7.1 Antes / parada / volume preservado

```powershell
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml ps -a
curl http://localhost:8080/health                                             # Healthy
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml down   # sem -v
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml ps -a  # vazio
docker volume inspect squad-estoque-data                                      # volume preservado
# Consulta somente leitura no volume com o container removido
# (alpine:3.22 + sqlite3, copia de Estoque.db/-wal/-shm para /tmp):
#   integrity_check=ok; ProdutoMarcador=1, MovMarcador=2, RupMarcador=1, SaldoNum40=8
```

Evidências: `reexec-ps-antes.txt`, `reexec-health-antes.txt`, `reexec-down.txt`, `reexec-ps-after-down.txt`, `reexec-volume-inspect-after-down.txt`, `reexec-snapshot-volume-offline.txt`, `reexec-volume-check-after-down.txt`.

### 7.2 Recriação e conferência

```powershell
docker compose -f compose.yaml -f ..\S4-QA-012-evidencias\compose.runtime.yaml up -d
curl http://localhost:8080/health                                             # Healthy
# snapshot pos-recriacao com a mesma consulta e comparacao com o snapshot anterior
```

Evidências: `reexec-up.txt`, `reexec-health-depois.txt`, `reexec-ps-depois.txt`, `reexec-logs-after-recreate.txt`, `reexec-snapshot-antes.txt`, `reexec-snapshot-depois.txt`, `reexec-diff.txt` → **`NO_DIFF`**.

### 7.3 Volume novo (banco independente)

```powershell
docker compose -p squad-estoque-fresh -f compose.yaml -f ..\S4-QA-012-evidencias\compose.fresh.yaml up -d
curl http://localhost:8081/health                                             # Healthy (8080 permanece Healthy)
# SQLite no volume squad-estoque-data-fresh: integrity_check=ok, 9 tabelas de dominio/migrations,
#   Usuarios=0, Produtos=0, Skus=0, Movimentacoes=0, Rupturas=0, ProdutoMarcador=0
docker compose -p squad-estoque-fresh -f compose.yaml -f ..\S4-QA-012-evidencias\compose.fresh.yaml down
docker volume rm squad-estoque-data-fresh
```

Evidências: `reexec-fresh-up.txt`, `reexec-fresh-health.txt`, `reexec-fresh-ps.txt`, `reexec-fresh-db-check.txt`, `reexec-fresh-volume-inspect.txt`, `reexec-fresh-down.txt`, `reexec-fresh-volume-rm.txt`, `reexec-volumes-final.txt`.

**Resultado da reexecução:** todos os critérios confirmados — dados preservados (`NO_DIFF`), volume novo vazio e independente (removido ao final), serviço principal saudável e único volume remanescente `squad-estoque-data`. A consulta SQL utilizada está em `reexec-query.sql` (sem seleção de `SenhaHash`).

## 8. Conclusão

**Aprovado.** O teste comprova que os dados do SQLite (usuários, produto, saldo de SKU e históricos de movimentação/ruptura/atendimento) persistem após `docker compose down` (sem remoção de volume) seguido de recriação do serviço com o mesmo volume `squad-estoque-data`. Um volume novo inicia banco independente, sem os dados do volume original. A atividade atende aos critérios do cartão S4-QA-012 e está apta para registro/merge conforme o fluxo da equipe (validação sem alteração de código-fonte).
