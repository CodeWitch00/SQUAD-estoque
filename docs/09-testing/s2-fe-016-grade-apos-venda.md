# S2-FE-016 — Grade após a venda

Validação em 23/09/2026, como VENDEDOR, em ambiente local isolado.
Base: `9439f9a3619621ac039266a13acc51d9ec6e553f` (main).
Sprint 2; prazo informado: 07/10/2026. Requisitos: RF-14, RF-15, RF-17,
RN-02; casos de uso UC-03 e UC-04.

## Alteração

O fluxo já atualizava a grade por um GET após o POST. Se esse GET falhasse,
uma venda confirmada podia continuar sendo exibida com saldo antigo.
Agora o cartão do SKU vendido usa diretamente o saldo persistido devolvido
no JSON do POST. Atualiza quantidade, estado, cor, descrição acessível e resumo
da seleção, sem modificar os demais cartões nem perder o produto consultado.
Não existe decremento calculado no navegador.

Durante o POST, os resultados do atendimento e a seleção ficam bloqueados.
Saldo zero mantém Vendeu desabilitado. Em erro, tenta consultar a grade sem
cache; se também falhar, bloqueia novas vendas até uma nova consulta e informa
que o resultado precisa ser conferido. A validação de saldo, a transação,
autorização e antiforgery do servidor permanecem preservados.

## Validação no navegador

Aplicação real em localhost, cookies e antiforgery reais, bancos SQLite novos
e exclusivos desta validação. Nenhum acesso ao banco compartilhado ou Azure.
Produto fictício Runner S2-FE-016, grade inicial: 37=0, 38=1, 39=3, 40=5.

| Cenário | Resultado observado |
| --- | --- |
| Desktop, 1280 × 900, vender nº 39 com 3 pares | Saldo 2, Disponível, seleção preservada e confirmação visível. |
| Mobile, 390 × 844, vender nº 39 com 2 pares | Saldo 1, Último par, cartão amarelo e resumo atualizados. |
| Mobile, vender o último par do nº 39 | Saldo 0, Indisponível, cartão cinza e Vendeu desabilitado. |
| Outras numerações | 37=0, 38=1 e 40=5 durante toda a sequência. |
| Persistência | Três saídas de quantidade 1, todas para nº 39; saldo final 0. |
| Nova consulta | Link retornou à busca vazia, pronta para outro produto. |
| Conexão interrompida, nº 40 selecionado | Servidor local encerrado antes do clique; saldo visual continuou 5, mensagem de resultado não confirmado e Vendeu bloqueado após falha do POST e do GET. |
| Responsividade | Sem rolagem horizontal em 390 px; textos, seleção e ações legíveis. |

As capturas registram a aplicação real após as vendas:

- [Saldo disponível no desktop](evidencias/s2-fe-016/desktop-saldo-disponivel.png)
- [Último par no mobile](evidencias/s2-fe-016/mobile-ultimo-par.png)
- [Saldo zero no mobile](evidencias/s2-fe-016/mobile-saldo-zero.png)

## Testes automatizados

- Baseline da main: 88 casos; 87 passaram e 1 falhou no Windows porque comparava
  HTML com quebra de linha LF fixa. Após tornar essa comparação independente
  de CRLF/LF, todos os 88 passaram, sem alteração do comportamento da aplicação.
- O teste HTTP existente da venda foi ampliado para os saldos 3→2, 2→1 e 1→0,
  conferindo JSON, persistência, autoria e renderização posterior da grade.
- Suíte final: **90 aprovados, 0 falhas, 0 ignorados**; build Release aprovado.
- Verificação de sintaxe JavaScript e `git diff --check`: aprovadas.
- Windows, SDK .NET 10.0.401. NU1900 local: consulta de vulnerabilidades do
  NuGet indisponível por restrição de rede; não equivale a auditoria aprovada.

Comandos a partir da raiz, também utilizáveis no Ubuntu:

```sh
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
git diff --check
```

## Limites e decisões

Os testes HTTP não executam JavaScript; as transições visuais foram verificadas
no navegador conforme descrito acima. Não se afirma homologação no Azure,
teste de concorrência entre vendedores ou garantia de entrega única após
perda de resposta de rede.

O CONTRIBUTING orienta dev/felipe. Essa branch continha duas entregas antigas
fora da main; Felipe autorizou uma branch específica para manter este PR
restrito ao cartão: `feat/s2-fe-016-felipe`. A captura exibe etiqueta Sprint I,
mas descrição Sprint 2; esta evidência segue a descrição e o prazo informado.

Trello não foi alterado. Revisão da mantenedora, merge e validação integrada
continuam pendentes e não são substituídos por este relatório.
