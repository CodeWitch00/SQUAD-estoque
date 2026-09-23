# S2-BE-008 — Ação POST de venda rápida

## Entrega

Foi adicionada a ação `POST /Estoque/Vender` no `EstoqueController`. A ação:

- é protegida por `[Authorize(Roles = "VENDEDOR")]` herdado do controller;
- exige `[ValidateAntiForgeryToken]`;
- recebe somente `skuId`;
- rejeita identificador vazio ou SKU inexistente;
- busca o SKU e o saldo no servidor;
- reutiliza `context.RegistrarVendaRapidaAsync(skuId, usuarioId)`;
- registra exatamente uma unidade e a identidade do usuário autenticado;
- retorna mensagem JSON clara de sucesso ou erro.

Não foi criado o botão `Vendeu` nem alterado o refinamento visual da grade, conforme o escopo do cartão. O token antiforgery é emitido na página da consulta em um contêiner separado do formulário GET, para não ser enviado nos parâmetros da busca.

## Testes

Os testes HTTP cobrem:

- vendedor autenticado registra a venda, reduz o saldo em um par e grava o usuário;
- SKU inválido é rejeitado;
- token antiforgery ausente é rejeitado;
- lojista e usuário anônimo não executam a ação.

Comandos executados:

```sh
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
git diff --check
```

Resultado: build aprovado, `81` testes aprovados e `git diff --check` aprovado.

## Limites

O botão e a integração visual com a seleção do SKU pertencem aos cartões seguintes. A validação manual no navegador e a cobertura específica de saldo zero permanecem nos cartões de QA correspondentes.
