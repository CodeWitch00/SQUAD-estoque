# S2-BE-007 — Regra compartilhada de saída

## Entrega

`Data/SaidaEstoque.cs` centraliza leitura, validação, decremento e movimentação
na mesma transação SQLite. O POST administrativo existente chama
`context.RegistrarSaidaAsync(skuId, quantidade, usuarioId, motivo)`.

Para o futuro atendimento, `context.RegistrarVendaRapidaAsync(skuId, usuarioId)`
delega à mesma operação com quantidade constante 1. O chamador deverá obter o
usuário da identidade autenticada e aplicar autorização e antiforgery na futura
action. Não deve aceitar usuário ou quantidade enviados pelo navegador.

Um resultado com `Erro` deve ser exibido como rejeição; falhas de persistência
cancelam a transação e são propagadas ao chamador. A saída administrativa mantém
mensagens, permissões, quantidade variável e redirecionamentos existentes.

Não foram criadas action ou interface “Vendeu”, migrations, alterações de schema,
novas dependências ou camadas. Os testes usam bancos SQLite isolados em memória
com `EnsureCreated`; não foi executado comando de migration.

## Validação local — Windows

- SDK .NET 10.0.401, configuração Release.
- Build da aplicação: aprovado, zero avisos e zero erros.
- Suíte completa: 82 aprovados, zero falhas e zero ignorados.
- 12 casos adicionados: venda de um par com saldo 1 e 5; saldo zero; SKU ausente;
  quantidades zero e negativa; rollback por usuário inexistente; saldo rastreado
  desatualizado; saída administrativa com quantidade, motivo e redirecionamento;
  ausência de identidade; formulário HTTP autenticado para lojista e vendedor.
- Os testes HTTP incluem login por cookie, token antiforgery, envio do formulário,
  redirecionamento e consulta do saldo e movimento persistidos.
- `git diff --check`: aprovado.

Comandos reproduzíveis a partir da raiz do repositório:

```sh
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --logger "trx;LogFileName=s2-be-007.trx" --collect:"XPlat Code Coverage"
```

## Limites e pendências

Os testes HTTP são automatizados; não representam validação manual no navegador.
O roteiro manual específico não aparece nas capturas recebidas. Revisão por outro
integrante, homologação manual e merge permanecem pendentes. Concorrência entre
duas conexões simultâneas não foi validada neste cartão.

O cartão foi identificado pelas capturas fornecidas. O quadro ao vivo não foi
consultado nem modificado. A instrução permanente em `AGENTS.md` protege cartões
em “Em execução”; registrar evidências ou mover este cartão exige autorização
específica de Felipe. Os prompts do manual de governança não foram executados.

As regras documentais sobre prazos de Daily e Sprint divergem; não houve alteração
de prazo. Essa divergência não afeta a extração da regra de saída.
