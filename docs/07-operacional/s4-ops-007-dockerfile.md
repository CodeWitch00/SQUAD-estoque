# S4-OPS-007 — Dockerfile multi-stage

O Dockerfile existente foi complementado com três etapas explícitas:

- `build`: SDK .NET 10, cópia do projeto e restore antes da cópia do código;
- `publish`: publicação Release sem novo restore e sem apphost;
- `runtime`: runtime ASP.NET Core, somente a saída de publicação, usuário
  não privilegiado definido pela imagem base e porta HTTP 8080.

O ambiente padrão da imagem é `Production`. A conexão SQLite, o caminho das
chaves e os hosts permitidos são fornecidos externamente. Não há credenciais
nem bancos adicionados pelo Dockerfile. O diretório `/app/data` permanece
gravável pelo usuário da aplicação e deve receber um volume persistente.

## Reproduzir o build

Na raiz do repositório, com Docker usando containers Linux, em Ubuntu ou
Windows com Docker Desktop:

```text
docker build --no-cache --progress=plain -t squad-estoque:s4-ops-007 .
docker build --progress=plain -t squad-estoque:s4-ops-007 .
docker image inspect squad-estoque:s4-ops-007 --format "{{.Size}}"
docker compose up --build -d
curl --fail http://localhost:8080/health
```

O primeiro build não usa cache; no segundo, a etapa de restore deve aparecer
como `CACHED`. A medida de tamanho é em bytes, conforme reportada pelo Docker.
As tags .NET `10.0` acompanham atualizações de manutenção: reproduzir o processo
não garante um digest idêntico em datas diferentes.

Use o guia [Docker](docker.md) para persistência, primeiro acesso e backup.
Não execute esses comandos contra dados compartilhados de produção.

## Validação automatizada

Em Ubuntu, execute a partir da raiz:

```bash
bash scripts/validate-docker.sh
```

O script constrói a imagem sem cache, repete o build e verifica o cache do
restore. Inspeciona porta, tamanho, usuário, ambiente e ausência de SDK,
código-fonte, bancos, arquivos `.env` e certificados na imagem final. Depois
inicia um container com volume novo, aguarda `/health` responder `Healthy`,
confirma Production nos logs e a criação do SQLite pelo usuário não root.
O container e o volume de teste criados pelo script são removidos ao terminar.

O workflow `Docker build and smoke test` executa o mesmo script e publica
`docker-evidence`, contendo SHA validado, versões, logs de build/cache,
metadados e tamanho da imagem, resposta de saúde e logs de inicialização.
A suíte .NET e os testes de frontend continuam no workflow existente.

## Limites e entregas relacionadas

- O filtro do contexto pertence ao S4-OPS-008, PR #110. Este cartão não duplica
  o `.dockerignore`. Use esse filtro antes de construir a partir de uma pasta
  de desenvolvimento que contenha arquivos locais; a CI usa checkout limpo.
- O endurecimento das permissões de dados/binários pertence ao PR #108.
  Este cartão preserva as permissões existentes e verifica execução sem root.
- A inspeção por nomes não detecta segredos embutidos no código ou JSON base.
- A validação automatizada usa Ubuntu e imagem Linux. Docker local no Windows
  não está instalado no ambiente de execução usado para esta entrega; esse
  teste local deve ser registrado separadamente, sem considerar a CI como
  evidência de execução no computador do integrante.
- Revisão da mantenedora, merge e atualização do Trello são etapas posteriores.
