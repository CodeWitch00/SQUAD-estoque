#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
image="squad-estoque:volume-test"
prefix="sqlite-volume-test-$$"
volume="$prefix-data"
container="$prefix-app"
cleanup() {
  docker rm -f "$container" >/dev/null 2>&1 || true
  docker volume rm "$volume" >/dev/null 2>&1 || true
}
trap cleanup EXIT
docker build -t "$image" .
docker volume create "$volume" >/dev/null
settings=(-e ASPNETCORE_ENVIRONMENT=Production -e AllowedHosts=localhost
  -e 'ConnectionStrings__EstoqueContext=Data Source=/app/data/nested/Estoque.db;Pooling=False'
  -e DataProtection__KeysPath=/app/data/keys)
start() {
  docker run -d --name "$container" --cap-drop ALL --security-opt no-new-privileges:true \
    -p 127.0.0.1::8080 -v "$volume:/app/data" "${settings[@]}" "$image" >/dev/null
  for attempt in {1..60}; do
    if docker logs "$container" 2>&1 | grep -q 'Now listening on:'; then
      test "$(curl --fail --silent --show-error -H 'Host: localhost' \
        "http://$(docker port "$container" 8080/tcp)/health")" = Healthy
      return
    fi
    if [ "$(docker inspect -f '{{.State.Running}}' "$container")" != true ]; then
      docker logs "$container"; return 1
    fi
    sleep 1
  done
  docker logs "$container"; return 1
}
start
docker exec "$container" sh -exc '
  id
  stat -c "%a %U:%G %n" /app/data /app/data/nested /app/data/nested/Estoque.db /app/wwwroot/uploads
  test "$(id -u)" != 0
  test ! -w /app/SquadEstoque.Web.dll
  test ! -w /app/SquadEstoque.Web.runtimeconfig.json
  test -w /app/wwwroot/uploads
  test "$(stat -c %a /app/data)" = 700
  test "$(stat -c %a /app/data/nested)" = 700
  test "$(stat -c %a /app/data/nested/Estoque.db)" = 600
  test -s /app/data/nested/Estoque.db
  test -z "$(find /app/data -name ".storage-check-*")"
  echo persistido > /app/data/evidencia
'
checksum=$(docker exec "$container" sha256sum /app/data/nested/Estoque.db)
docker rm -f "$container" >/dev/null
start
test "$checksum" = "$(docker exec "$container" sha256sum /app/data/nested/Estoque.db)"
test "$(docker exec "$container" cat /app/data/evidencia)" = persistido
echo 'PASS: caminho configurado, criacao de diretorio, usuario nao root, permissoes e persistencia apos recriar container'
docker rm -f "$container" >/dev/null

expect_failure() {
  local label="$1"; shift
  local status=0 logs
  logs=$(docker run --rm --cap-drop ALL --security-opt no-new-privileges:true \
    "${settings[@]}" "$@" "$image" 2>&1) || status=$?
  if [ "$status" = 0 ] || ! grep -q 'Armazenamento SQLite indisponivel' <<< "$logs"; then
    printf '%s\n' "$logs"; echo "FAIL: $label"; return 1
  fi
  printf '%s\n' "$logs" | grep 'Armazenamento SQLite indisponivel' | head -n 1
  echo "PASS: $label (exit=$status)"
}
expect_failure 'volume somente leitura' -v "$volume:/app/data:ro"
docker run --rm -v "$volume:/app/data" --entrypoint chmod "$image" 400 /app/data/nested/Estoque.db
expect_failure 'arquivo existente sem escrita' -v "$volume:/app/data"
docker run --rm -v "$volume:/app/data" --entrypoint chmod "$image" 600 /app/data/nested/Estoque.db
expect_failure 'diretorio sem acesso ao usuario do processo' \
  --mount type=tmpfs,destination=/app/data,tmpfs-mode=0700
expect_failure 'caminho invalido sob /proc' \
  -e 'ConnectionStrings__EstoqueContext=Data Source=/proc/estoque-test/Estoque.db'
echo 'PASS: todos os cenarios de volume SQLite'
