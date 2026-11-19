#!/usr/bin/env bash
set -euo pipefail
set +x
cd "$(git rev-parse --show-toplevel)"
revision=$(git rev-parse HEAD)
image="squad-estoque:qa-${revision:0:12}"
temp=$(mktemp -d)
container="estoque-qa-$$"
volume="$container-data"
stage=contexto
cleanup() {
  local result=$?
  if (( result != 0 )); then
    printf 'FALHA etapa=%s exit=%s\n' "$stage" "$result"
  fi
  docker rm -f "$container" >/dev/null 2>&1 || true
  docker volume rm "$volume" >/dev/null 2>&1 || true
  rm -rf -- "$temp"
}
trap cleanup EXIT

printf 'Commit=%s\n' "$revision"
docker version --format 'Docker Server={{.Server.Version}}'
stage=build
mkdir "$temp/context"
git archive "$revision" | tar -x -C "$temp/context"
docker build --pull --no-cache --progress=plain -t "$image" "$temp/context"
docker image inspect --format 'Tag={{index .RepoTags 0}} ID={{.Id}} Plataforma={{.Os}}/{{.Architecture}}' "$image"

stage=inspecao
inspection=0
docker run --rm --network none --entrypoint sh "$image" -ec '
  test -z "$(find /app -type f \( -iname "*.db" -o -iname "*.db-*" -o -iname "*.sqlite*" \) -print -quit)"
  test -z "$(find /app -type f \( -iname ".env*" -o -iname "secrets.json" -o -iname "*.pem" -o -iname "*.key" -o -iname "*.pfx" -o -iname "*.p12" -o -iname "key-*.xml" \) -print -quit)"
  ! grep -Eiq "\"(password|senha|token|clientsecret|apikey)\"[[:space:]]*:[[:space:]]*\"[^\"]+\"|password[[:space:]]*=" /app/appsettings*.json
' && echo 'OK imagem sem arquivos SQLite ou de segredo em /app e sem valores sensíveis nos appsettings examinados' || {
  echo 'FALHA inspeção de arquivos ou appsettings da imagem'
  inspection=1
}
docker image inspect --format '{{range .Config.Env}}{{println .}}{{end}}' "$image" > "$temp/image-env"
if grep -Eiq '^[^=]*(password|senha|token|secret|api_?key|connectionstrings)[^=]*=' "$temp/image-env"; then
  echo 'FALHA variável de configuração sensível na imagem'
  inspection=1
else
  echo 'OK metadados ENV sem variáveis de credencial ou connection string'
fi
# O seed de Development é compilado junto com o aplicativo. Registre apenas a ocorrência.
if git show "$revision:src/SquadEstoque.Web/Program.cs" | grep -E 'HashPassword\("[^"]+",' > /dev/null; then
  echo 'RESSALVA fonte publicada contém senha fixa para contas de demonstração de Development'
fi

stage=bootstrap
settings=(-e ASPNETCORE_ENVIRONMENT=Production -e AllowedHosts=127.0.0.1
  -e 'ConnectionStrings__EstoqueContext=Data Source=/app/data/qa/Estoque.db'
  -e DataProtection__KeysPath=/app/data/keys)
docker volume create "$volume" >/dev/null
runtime=(--cap-drop ALL --security-opt no-new-privileges:true -v "$volume:/app/data" "${settings[@]}")
email=lojista-qa@example.test
password=$(openssl rand -hex 24)
printf '%s\n' "$password" | docker run --rm -i "${runtime[@]}" "$image" \
  --bootstrap-lojista 'Lojista QA' "$email" > "$temp/bootstrap.log" 2>&1
grep -F 'Primeiro LOJISTA criado.' "$temp/bootstrap.log"
stage=inicializacao
docker run -d --name "$container" "${runtime[@]}" -p 127.0.0.1::8080 "$image" >/dev/null
address=$(docker port "$container" 8080/tcp)
base="http://$address"
printf 'Porta publicada=%s -> 8080/tcp\n' "$address"
for attempt in {1..60}; do
  if [[ $(curl -fsS "$base/health" 2>/dev/null) == Healthy ]]; then
    break
  fi
  if [[ $(docker inspect -f '{{.State.Running}}' "$container") != true ]]; then
    break
  fi
  sleep 1
done
docker logs "$container" > "$temp/app.log" 2>&1
grep -E 'Hosting environment:|Now listening on:|^warn:|^fail:' "$temp/app.log" || true
grep -q 'Hosting environment: Production' "$temp/app.log"
grep -q 'Now listening on:.*:8080' "$temp/app.log"
! grep -q '^fail:' "$temp/app.log" || exit 1
test "$(curl -fsS "$base/health")" = Healthy
echo 'OK GET /health = Healthy'
docker exec "$container" test -s /app/data/qa/Estoque.db
echo 'OK SQLite criado no volume externo'

stage=acesso_anonimo
status=$(curl -sS -o /dev/null -w '%{http_code}' "$base/Produtos")
printf 'GET /Produtos anônimo HTTP=%s\n' "$status"
test "$status" = 302

stage=login
curl -fsS -c "$temp/cookies" -o "$temp/login.html" "$base/Account/Login"
token=$(sed -n 's/.*name="__RequestVerificationToken"[^>]*value="\([^"]*\)".*/\1/p' "$temp/login.html")
test -n "$token"
status=$(printf '%s' "$password" | curl -sS -b "$temp/cookies" -c "$temp/cookies" \
  -o /dev/null -w '%{http_code}' --data-urlencode "Email=$email" \
  --data-urlencode 'Senha@-' --data-urlencode "__RequestVerificationToken=$token" "$base/Account/Login")
unset password
printf 'POST /Account/Login HTTP=%s\n' "$status"
test "$status" = 302

stage=catalogo
status=$(curl -sS -b "$temp/cookies" -o "$temp/products" -w '%{http_code}' "$base/Produtos")
printf 'GET /Produtos autenticado HTTP=%s\n' "$status"
test "$status" = 200
grep -q 'Catálogo de Produtos' "$temp/products"
grep -q 'Nenhum produto cadastrado no momento.' "$temp/products"
echo 'OK catálogo carregado com banco novo e sem produtos'

stage=resultado
test "$inspection" = 0
