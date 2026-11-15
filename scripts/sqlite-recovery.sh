#!/usr/bin/env bash
set -Eeuo pipefail

SQLITE_BIN="${SQLITE3_BIN:-sqlite3}"

usage() {
  cat <<'USAGE'
Uso:
  scripts/sqlite-recovery.sh backup AMBIENTE BANCO.sqlite DIRETORIO_DE_BACKUP
  scripts/sqlite-recovery.sh verify BANCO.sqlite [ID_DO_PRODUTO_CONHECIDO]
  scripts/sqlite-recovery.sh restore BACKUP.sqlite ARQUIVO_ISOLADO.sqlite [ID_DO_PRODUTO_CONHECIDO]

backup usa a API .backup do SQLite e cria um nome com ambiente e data UTC.
restore nunca sobrescreve o destino; use um caminho isolado que ainda não exista.
SQLITE3_BIN pode indicar o caminho para um binário sqlite3 compatível.
USAGE
}

fail() {
  printf 'ERRO: %s\n' "$1" >&2
  exit 1
}

require_sqlite() {
  command -v "$SQLITE_BIN" >/dev/null 2>&1 || \
    fail "sqlite3 não encontrado. Instale-o no host administrativo ou defina SQLITE3_BIN para um binário confiável."
}

absolute_file() {
  [[ -f "$1" ]] || fail "arquivo não encontrado: $1"
  realpath -- "$1"
}

check_product_id() {
  [[ "$1" =~ ^[[:xdigit:]]{8}-[[:xdigit:]]{4}-[[:xdigit:]]{4}-[[:xdigit:]]{4}-[[:xdigit:]]{12}$ ]] || \
    fail "ID conhecido deve estar no formato UUID."
}

verify_database() {
  local database="$1"
  local known_product_id="${2:-}"
  local integrity foreign_keys count

  integrity="$("$SQLITE_BIN" -batch -noheader -readonly "$database" 'PRAGMA integrity_check;')" || \
    fail "não foi possível consultar PRAGMA integrity_check."
  [[ "$integrity" == "ok" ]] || fail "integrity_check falhou: $integrity"

  foreign_keys="$("$SQLITE_BIN" -batch -noheader -readonly "$database" 'PRAGMA foreign_key_check;')" || \
    fail "não foi possível consultar PRAGMA foreign_key_check."
  [[ -z "$foreign_keys" ]] || fail "foreign_key_check encontrou violações: $foreign_keys"

  printf 'integrity_check=ok\nforeign_key_check=ok\n'

  if [[ -n "$known_product_id" ]]; then
    check_product_id "$known_product_id"
    count="$("$SQLITE_BIN" -batch -noheader -readonly "$database" \
      "SELECT COUNT(*) FROM Produto WHERE Id = '$known_product_id';")" || \
      fail "não foi possível consultar o Produto pelo ID informado."
    [[ "$count" == "1" ]] || fail "o Produto conhecido não foi encontrado exatamente uma vez."
    printf 'produto_conhecido=presente (%s)\n' "$known_product_id"
  fi
}

sqlite_backup_to_name() {
  local source="$1"
  local destination_directory="$2"
  local destination_name="$3"
  (
    cd -- "$destination_directory"
    "$SQLITE_BIN" -batch -readonly "$source" ".backup \"$destination_name\""
  )
}

backup_database() {
  local environment="$1"
  local source="$2"
  local backup_directory="$3"
  local timestamp backup_name partial_name partial_path final_path

  [[ "$environment" =~ ^[A-Za-z0-9][A-Za-z0-9._-]*$ ]] || \
    fail "ambiente deve conter somente letras, números, ponto, sublinhado ou hífen."
  source="$(absolute_file "$source")"
  mkdir -p -- "$backup_directory"
  backup_directory="$(realpath -- "$backup_directory")"

  timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
  backup_name="Estoque-${environment}-${timestamp}.sqlite3"
  final_path="$backup_directory/$backup_name"
  [[ ! -e "$final_path" ]] || fail "o backup de mesmo nome já existe: $final_path"

  umask 077
  partial_name=".${backup_name}.partial.$$"
  partial_path="$backup_directory/$partial_name"
  trap 'rm -f -- "$partial_path"' EXIT

  sqlite_backup_to_name "$source" "$backup_directory" "$partial_name" || \
    fail "a operação SQLite .backup falhou."
  verify_database "$partial_path" >/dev/null
  mv -- "$partial_path" "$final_path"
  trap - EXIT
  printf '%s\n' "$final_path"
}

restore_database() {
  local backup="$1"
  local destination="$2"
  local known_product_id="${3:-}"
  local destination_directory destination_name partial_name partial_path

  backup="$(absolute_file "$backup")"
  destination_directory="$(dirname -- "$destination")"
  destination_name="$(basename -- "$destination")"
  [[ -d "$destination_directory" ]] || fail "o diretório isolado precisa existir antes da restauração."
  [[ "$destination_name" =~ ^[A-Za-z0-9][A-Za-z0-9._-]*\.sqlite3$ ]] || \
    fail "o destino deve ser um novo arquivo .sqlite3 com nome seguro."
  destination_directory="$(realpath -- "$destination_directory")"
  destination="$destination_directory/$destination_name"
  [[ ! -e "$destination" ]] || fail "o destino já existe; nenhum arquivo foi substituído: $destination"

  if [[ -n "$known_product_id" ]]; then
    check_product_id "$known_product_id"
  fi
  verify_database "$backup" >/dev/null

  umask 077
  partial_name=".${destination_name}.partial.$$"
  partial_path="$destination_directory/$partial_name"
  trap 'rm -f -- "$partial_path"' EXIT

  sqlite_backup_to_name "$backup" "$destination_directory" "$partial_name" || \
    fail "a cópia isolada do backup falhou."
  verify_database "$partial_path" "$known_product_id"
  mv -- "$partial_path" "$destination"
  trap - EXIT
  printf 'restauracao_isolada=%s\n' "$destination"
}

main() {
  require_sqlite
  case "${1:-}" in
    backup)
      [[ $# -eq 4 ]] || { usage >&2; exit 2; }
      backup_database "$2" "$3" "$4"
      ;;
    verify)
      [[ $# -ge 2 && $# -le 3 ]] || { usage >&2; exit 2; }
      verify_database "$(absolute_file "$2")" "${3:-}"
      ;;
    restore)
      [[ $# -ge 3 && $# -le 4 ]] || { usage >&2; exit 2; }
      restore_database "$2" "$3" "${4:-}"
      ;;
    *)
      usage >&2
      exit 2
      ;;
  esac
}

main "$@"
