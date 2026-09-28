# Validação da ação Não tinha na grade

Data: 28/09/2026. Branch: `dev/emmy`. Requisitos: RF-18, RN-05, RN-06, UC-05.

## Escopo e revisão local

A ação existente foi ajustada para bloquear seleção e ações enquanto o POST está
pendente, preservar o SKU selecionado e confirmar produto/numeração sem interpretar
o nome do cadastro como HTML. A rota é gerada pelo MVC. O payload contém somente
`skuId` e `__RequestVerificationToken`; produto e vendedor são resolvidos no servidor.
O endpoint, autorização e schema permanecem os existentes. A ação Desistiu já
existia na branch e não foi acrescentada nem expandida nesta entrega.

## Validação pelo navegador

Aplicação local, perfil VENDEDOR, banco separado em `tmp/nao-tinha`, dados fictícios.
Navegação e cliques assistidos por Playwright, com inspeção das capturas e do banco.
Não foi usado o ambiente compartilhado. Resoluções: 390 × 844 e 1366 × 900.

1. Entrar como vendedor e consultar o produto **Tênis Validação Ruptura**.
2. Abrir a grade e selecionar o número **39**, saldo **3**.
3. Acionar **Não tinha** e conferir confirmação com produto e número 39.
4. Inspecionar o POST `/Estoque/RegistrarNaoTinha`: um envio, apenas SKU/token.
5. Repetir no número **37**, saldo **0**: ação permitida, saldo preservado.
6. No número **38**, atrasar a requisição no navegador: os três seletores e as
   três ações existentes ficam bloqueados; o botão mostra **Registrando…**.
7. Liberar o POST: confirmação do número 38 e liberação da interface.
8. Conferir foco por Tab: contorno sólido de 3 px; botão com altura de 48 px.
   Largura do documento mobile de 390 px, sem rolagem horizontal.

Produto: `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb`.

| Numeração | SKU | Saldo antes/depois | Rupturas antes/depois |
| --- | --- | --- | --- |
| 37 | `cccccccc-cccc-cccc-cccc-cccccccccc37` | 0 / 0 | 0 / 1 |
| 38 | `cccccccc-cccc-cccc-cccc-cccccccccc38` | 1 / 1 | 0 / 1 |
| 39 | `cccccccc-cccc-cccc-cccc-cccccccccc39` | 3 / 3 | 0 / 1 |

Nenhuma movimentação criada. Os registros são vinculados ao vendedor autenticado.

## Verificações automatizadas

- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj`: aprovado, zero avisos/erros.
- `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj`: 94 aprovados.
- `node --test tests/frontend/consulta-nao-tinha.test.cjs`: 6 aprovados.
  Requer Node.js, pacote `playwright` resolvível pelo Node e Chromium instalado
  pelo Playwright. Na sessão foram utilizadas as dependências locais do Codex.
  Esses testes adicionais não são executados pelo comando `dotnet test`.
- Testes de interação: seleção obrigatória, saldos zero/positivo, payload mínimo,
  preservação de saldo e seleção, nome com HTML exibido como texto, bloqueio de
  reentrada após evento change, falha de rede, rejeição e SKU divergente.
- `git diff --check` no escopo: aprovado. A verificação geral apontou somente
  espaços/linhas vazias em três documentos preexistentes de validação de venda,
  preservados fora desta entrega.

## Capturas

- [Seleção mobile e foco](evidencias/nao-tinha/01-mobile-selecao.png)
- [Registro do número 39 com saldo positivo](evidencias/nao-tinha/02-mobile-registrado.png)
- [Registro do número 37 com saldo zero](evidencias/nao-tinha/03-mobile-saldo-zero.png)
- [Desktop e foco de teclado](evidencias/nao-tinha/04-desktop-foco.png)
- [Envio em processamento](evidencias/nao-tinha/05-mobile-processando.png)

## Pendências externas

Publicação da branch, vínculo ao cartão, abertura do PR e solicitação de revisão
ficaram com a integrante, conforme combinado. A revisão local do diff foi feita;
aprovação de outro integrante e CI remoto ainda não foram realizados. Nenhuma
operação na main foi executada nesta continuação.

O bloqueio evita duplicação durante a solicitação nesta página; não oferece
idempotência entre abas ou novas tentativas após perda da resposta. Em falha de
comunicação, a mensagem orienta conferir as rupturas antes de tentar novamente.
