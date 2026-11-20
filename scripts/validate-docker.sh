#!/usr/bin/env bash
set -euo pipefail

# Execute from the repository root. All runtime data are disposable test data.
evidence=artifacts/docker
image="squad-estoque:validation-$$"
container="squad-estoque-validation-$$"
volume="squad-estoque-validation-$$"
mkdir -p "$evidence"
cleanup() {
  docker logs "$container" > "$evidence/container.log" 2>&1 || true
  docker rm -f "$container" >/dev/null 2>&1 || true
  docker volume rm "$volume" >/dev/null 2>&1 || true
}
trap cleanup EXIT

git rev-parse HEAD > "$evidence/commit.txt"
docker version > "$evidence/docker-version.txt"
docker build --no-cache --progress=plain -t "$image" . 2>&1 | tee "$evidence/build.log"
docker build --progress=plain -t "$image" . 2>&1 | tee "$evidence/build-cache.log"
# Confirm the restore layer was cached, rather than just a later layer.
awk '/RUN dotnet restore/{step=$1} step && $1==step && /CACHED/{found=1} END{exit !found}' "$evidence/build-cache.log"
docker image inspect "$image" > "$evidence/image.json"
docker image inspect "$image" --format 'Image={{.Id}} SizeBytes={{.Size}} User={{.Config.User}} Ports={{json .Config.ExposedPorts}}' | tee "$evidence/image-summary.txt"
test "$(docker image inspect "$image" --format '{{json .Config.ExposedPorts}}')" = '{"8080/tcp":{}}'

docker run --rm --entrypoint sh "$image" -ec '
  test "$(id -u)" != 0
  test "$ASPNETCORE_ENVIRONMENT" = Production
  test -f /app/SquadEstoque.Web.dll
  test ! -d /source
  test ! -d /usr/share/dotnet/sdk
  test -z "$(find /app -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.db" -o -name "*.db-*" -o -name "*.sqlite" -o -name "*.sqlite3" -o -name ".env" -o -name ".env.*" -o -name "*.pfx" -o -name "*.pem" \) -print -quit)"
  printf "Runtime-only image, Production, non-root UID=%s, no source/database/environment/certificate files found\n" "$(id -u)"
' | tee "$evidence/image-content.txt"

docker volume create "$volume" >/dev/null
docker run -d --name "$container" -p 127.0.0.1::8080 \
  --mount "type=volume,source=$volume,target=/app/data" \
  -e 'ConnectionStrings__EstoqueContext=Data Source=/app/data/Estoque.db' \
  -e DataProtection__KeysPath=/app/data/keys \
  -e AllowedHosts=127.0.0.1 \
  "$image" >/dev/null
port=$(docker port "$container" 8080/tcp | sed 's/.*://')
curl --fail --silent --show-error --retry 30 --retry-all-errors --retry-delay 2 \
  --max-time 5 "http://127.0.0.1:$port/health" | tee "$evidence/health.txt"
test "$(cat "$evidence/health.txt")" = Healthy
docker exec "$container" sh -ec 'test "$(id -u)" != 0; test -s /app/data/Estoque.db; id' | tee "$evidence/runtime-user.txt"
docker logs "$container" > "$evidence/container.log" 2>&1
grep -F 'Hosting environment: Production' "$evidence/container.log"
printf '\nDocker validation passed.\n'
