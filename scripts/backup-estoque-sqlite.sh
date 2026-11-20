#!/usr/bin/env bash
# S4-DB-004 — Backup consistente do SQLite do estoque.
#
# Uso:
#   ./scripts/backup-estoque-sqlite.sh /home/data/Estoque.db /home/data/backups
#
# Regras:
# - usa a operação .backup do SQLite, que é consistente mesmo com o banco em uso;
# - não copia o arquivo "a quente" com cp;
# - não exclui nenhum arquivo;
# - valida o backup com PRAGMA integrity_check e calcula o SHA-256.
#
# Este script é um esqueleto operacional. Ele não substitui a conferência de
# versão, o congelamento de escrita nem o registro de evidências no runbook.

set -euo pipefail

DB_PATH="${1:-/home/data/Estoque.db}"
BACKUP_DIR="${2:-/home/data/backups}"
STAMP="$(date -u +%Y%m%d-%H%M%S)"
BACKUP_PATH="${BACKUP_DIR}/Estoque-${STAMP}.db"

if ! command -v sqlite3 >/dev/null 2>&1; then
    echo "sqlite3 não encontrado. Use o binário temporário documentado no runbook." >&2
    exit 1
fi

if [ ! -f "$DB_PATH" ]; then
    echo "Banco não encontrado: $DB_PATH" >&2
    exit 1
fi

mkdir -p "$BACKUP_DIR"

# .backup é a forma recomendada de copiar um SQLite em uso.
sqlite3 "$DB_PATH" ".backup '${BACKUP_PATH}'"

INTEGRITY="$(sqlite3 "$BACKUP_PATH" 'PRAGMA integrity_check;')"
if [ "$INTEGRITY" != "ok" ]; then
    echo "Falha no integrity_check do backup: $INTEGRITY" >&2
    exit 1
fi

HASH="$(sha256sum "$BACKUP_PATH" | awk '{print $1}')"
SIZE="$(wc -c < "$BACKUP_PATH" | tr -d ' ')"

echo "backup=${BACKUP_PATH}"
echo "bytes=${SIZE}"
echo "sha256=${HASH}"
echo "integrity_check=${INTEGRITY}"
echo
echo "Próximos passos obrigatórios:"
echo "1. baixar uma cópia para fora do App Service;"
echo "2. registrar caminho, tamanho, hash e horário no runbook;"
echo "3. testar a restauração em uma cópia, nunca no banco compartilhado;"
echo "4. não excluir o backup nem o banco antigo."
