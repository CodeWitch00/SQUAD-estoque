#!/usr/bin/env bash
set -Eeuo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
RECOVERY_SCRIPT="$SCRIPT_DIR/sqlite-recovery.sh"
SQLITE_BIN="${SQLITE3_BIN:-sqlite3}"
PRODUCT_ID="11111111-1111-4111-8111-111111111111"
TEMP_ROOT="$(mktemp -d)"
trap 'rm -rf -- "$TEMP_ROOT"' EXIT

command -v "$SQLITE_BIN" >/dev/null 2>&1 || {
  printf 'ERRO: sqlite3 não encontrado.\n' >&2
  exit 1
}

mkdir -p "$TEMP_ROOT/backups" "$TEMP_ROOT/isolado"
"$SQLITE_BIN" "$TEMP_ROOT/origem.sqlite3" <<SQL
PRAGMA foreign_keys = ON;
CREATE TABLE Produto (Id TEXT PRIMARY KEY, Nome TEXT NOT NULL);
INSERT INTO Produto (Id, Nome) VALUES ('$PRODUCT_ID', 'Produto conhecido de teste');
SQL

printf 'sqlite3_version=%s\n' "$($SQLITE_BIN --version | awk '{print $1}')"
printf 'fixture_produto_id=%s\n' "$PRODUCT_ID"

BACKUP_PATH="$("$RECOVERY_SCRIPT" backup Testing "$TEMP_ROOT/origem.sqlite3" "$TEMP_ROOT/backups")"
printf 'backup_criado=%s\n' "$(basename -- "$BACKUP_PATH")"

RESTORED_PATH="$TEMP_ROOT/isolado/Estoque-restaurado.sqlite3"
"$RECOVERY_SCRIPT" restore "$BACKUP_PATH" "$RESTORED_PATH" "$PRODUCT_ID"

RESTORED_NAME="$("$SQLITE_BIN" -batch -noheader -readonly "$RESTORED_PATH" \
  "SELECT Nome FROM Produto WHERE Id = '$PRODUCT_ID';")"
[[ "$RESTORED_NAME" == 'Produto conhecido de teste' ]] || {
  printf 'ERRO: os dados conhecidos não foram preservados.\n' >&2
  exit 1
}
printf 'dado_conhecido_restaurado=%s\n' "$RESTORED_NAME"

if "$RECOVERY_SCRIPT" restore "$BACKUP_PATH" "$RESTORED_PATH" "$PRODUCT_ID" >/dev/null 2>&1; then
  printf 'ERRO: o restore sobrescreveu um destino existente.\n' >&2
  exit 1
fi
printf 'destino_existente_sobrescrito=nao\n'
printf 'resultado=APROVADO (backup e restauração isolada da fixture)\n'
