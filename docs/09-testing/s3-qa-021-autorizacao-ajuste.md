# S3-QA-021 — Autorização do ajuste de estoque

Validação local realizada em 12/10/2026, no Windows, com SDK .NET 10.0.401.
Base: `1dacf4d1751df9143fcb8980930f96161001fafb`, obtida de `origin/main`.

## Escopo e evidências

O arquivo `tests/SquadEstoque.Web.Tests/AjusteAuthorizationHttpTests.cs` acrescenta
sete casos HTTP para `/Movimentacoes/Ajuste`, conforme RN-04 e RF-23 do SRS.
Cada caso usa uma aplicação e um SQLite em memória próprios, autenticação real
por cookies e redirecionamentos desabilitados.

| Perfil / cenário | Resultado verificado |
| --- | --- |
| LOJISTA / GET | HTTP 200, formulário de ajuste e banco inalterado. |
| LOJISTA / POST válido | Redirecionamento aos detalhes do produto, saldo de 2 para 5 e uma movimentação AJUSTE de 3 unidades, com motivo e identidade do lojista. |
| VENDEDOR / GET | Redirecionamento para AccessDenied; saldos e histórico intactos. |
| VENDEDOR / POST válido | Redirecionamento para AccessDenied; saldos e histórico intactos. |
| Anônimo / GET | Redirecionamento para Login; saldos e histórico intactos. |
| Anônimo / POST válido | Redirecionamento para Login; saldos e histórico intactos. |
| LOJISTA / POST sem token | HTTP 400; saldos e histórico intactos. |

Os POSTs negados enviam SKU existente, saldo diferente do atual, motivo válido e
token antiforgery da própria sessão. O teste exige o destino de autorização
correto, evitando aceitar uma falha de validação como prova de controle de acesso.
Há movimentação anterior persistida: a comparação verifica todos os saldos e
todos os campos das movimentações, inclusive a preservação do histórico existente.

Nenhum Controller, política, migration ou dependência foi alterado.

## Execução

Na raiz do repositório:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore --filter FullyQualifiedName~AjusteAuthorizationHttpTests -p:UseSharedCompilation=false
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore --configuration Release -p:UseSharedCompilation=false
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore --configuration Release -p:UseSharedCompilation=false --collect:"XPlat Code Coverage"
```

- Antes da alteração: 203 testes aprovados, nenhuma falha.
- Testes novos: 7 aprovados, nenhuma falha.
- Suíte final Release: 210 aprovados, nenhuma falha ou teste ignorado.
- Build Release: aprovado, zero erros.
- Cobertura de linhas: 54,86% (2.412 / 4.396), acima do limite de 50% do workflow.
- Verificação de espaços do arquivo novo: nenhum erro.
- Aviso NU1900: consulta aos dados de vulnerabilidade do NuGet indisponível.
  Os pacotes necessários estavam disponíveis e a compilação e execução concluíram.

## Limites e divergências registradas

- A validação foi automatizada no Windows; navegador manual, Ubuntu e GitHub
  Actions não foram executados nesta entrega local.
- O checklist das capturas pede branch por cartão; o CONTRIBUTING atual, seção
  4.2, pede a branch pessoal. A cópia isolada foi preparada na branch local
  `test/s3-qa-021-felipe` antes dessa diferença ser identificada. Em 12/10/2026,
  Felipe autorizou explicitamente publicar este PR nessa branch por cartão,
  preservando o histórico divergente e o trabalho existente em `dev/felipe`.
- O domínio RN-02 descreve ajuste por delta, enquanto o código atual recebe
  `NovoSaldoApurado`. A regra de autorização RN-04 é consistente. Estes testes
  exercitam o contrato existente; não decidem nem alteram a semântica do ajuste.
- O Trello não foi acessado nem alterado. Revisão da mantenedora e CI remoto
  permanecem pendentes; este registro não declara o cartão concluído.
