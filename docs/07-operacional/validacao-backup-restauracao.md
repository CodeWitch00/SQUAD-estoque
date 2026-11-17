# Evidência do teste de backup e restauração SQLite

## Execução local

Data: 15/11/2026. Ambiente: host local, fixture SQLite temporária. O teste foi
executado na branch `main` com `sqlite3` 3.45.1.

Comando:

```bash
scripts/test-sqlite-recovery.sh
```

Resultado observado:

```text
sqlite3_version=3.45.1
fixture_produto_id=11111111-1111-4111-8111-111111111111
backup_criado=Estoque-Testing-20261115T162735Z.sqlite3
integrity_check=ok
foreign_key_check=ok
produto_conhecido=presente (11111111-1111-4111-8111-111111111111)
dado_conhecido_restaurado=Produto conhecido de teste
destino_existente_sobrescrito=nao
resultado=APROVADO (backup e restauração isolada da fixture)
```

O script criou um banco descartável, fez backup pelo SQLite, restaurou para um
arquivo novo isolado e verificou integridade, chaves estrangeiras, Produto
conhecido e proteção contra sobrescrita. A pasta temporária foi removida ao
final.

## Limite da evidência

Esta execução comprova o procedimento automatizado com dados conhecidos. Não
comprova a restauração da cópia real do App Service nem o armazenamento
persistentemente montado no Azure. A última cópia Azure registrada foi criada
em 24/09/2026 e baixada para armazenamento local; sua restauração isolada ainda
precisa ser executada com o arquivo real disponível e sem substituir o banco
ativo. Consulte também [azure.md](azure.md) e
[validacao-persistencia-azure.md](validacao-persistencia-azure.md).
