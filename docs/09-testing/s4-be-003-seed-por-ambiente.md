# S4-BE-003 — Seed por ambiente e primeiro LOJISTA

Validação local em 10/11/2026 na branch `dev/rayana`, com bancos SQLite
temporários exclusivos por teste. Nenhum banco compartilhado foi alterado.

## Resultado

| Ambiente | `Demo:SeedUsers` | Usuários criados na inicialização |
| --- | --- | --- |
| Development | `true` | 2 demonstrativos |
| Development | `false` | 0 |
| Production | `true` | 0 |
| Production | `false` | 0 |
| Testing | `true` | 0 pelo startup; fábrica de testes mantém 2 usuários em SQLite isolado |

O teste do comando `--bootstrap-lojista` em `Production` confirmou senha fraca
recusada, criação de um único LOJISTA com hash BCrypt, senha e hash ausentes da
saída capturada, e segunda execução recusada. O mesmo comando foi recusado em
`Development`. Os testes existentes de login, cookies e autorização continuam
na suíte completa.

## Comandos executados

```bash
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build \
  --filter 'FullyQualifiedName~StartupEnvironmentTests|FullyQualifiedName~PrimeiroLojistaBootstrapTests' \
  --logger 'console;verbosity=quiet'
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build \
  --logger 'console;verbosity=quiet'
git diff --check
```

Build: **0 erros, 0 avisos**. Testes específicos: **8 aprovados, 0 falhos**.
Suíte completa: **318 aprovados, 1 ignorado, 0 falhos**. O teste ignorado é a
medição de desempenho opt-in já existente. `git diff --check` aprovado.

Limites: a configuração do Azure ainda precisa ser alterada no App Service
antes de publicar esta versão. Contas demonstrativas já existentes em um banco
de produção não são removidas por este cartão. O comando operacional no Azure
e a execução em Docker não foram realizados nesta validação local.
