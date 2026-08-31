# Validação da consulta do vendedor — 31/08/2026

## Escopo

Cartão S1-FE-018, com referência aos componentes e estados definidos em
S1-UX-002 e S1-UX-010.

Rota validada: `GET /Estoque/Consulta`.

## Validação manual responsiva

Foi usada a página Razor realmente servida pela aplicação em ambiente local,
autenticada como `VENDEDOR`, com três produtos ativos de teste. A inspeção foi
feita para os pontos de referência abaixo.

| Cenário | Smartphone — 390 px | Desktop — 1366 px |
| --- | --- | --- |
| Campo e botão | Empilhados, largura total e altura mínima de 50 px | Campo e botão lado a lado; campo pode encolher sem ultrapassar o painel |
| Resultados | Uma coluna, ícone de 44 px e texto com quebra segura | Uma coluna central limitada a 760 px, ícone de 48 px |
| Rolagem horizontal | Não identificada; grid usa `minmax(0, 1fr)`, itens têm `min-width: 0` e textos usam `overflow-wrap` | Não identificada; conteúdo permanece limitado pelo contêiner |
| Teclado e foco | Campo recebe foco inicial, Enter envia a busca e alvos têm pelo menos 44 px | Ordem natural: busca, botão e produtos; foco visível em todos os controles |
| Estados | Inicial, validação, nenhum resultado, resultados e seleção conferidos | Os mesmos estados e mensagens foram conferidos |

O Firefox headless disponível no ambiente não conseguiu produzir as capturas
por falha do compositor gráfico (`RenderCompositorSWGL`). Por isso, esta
validação foi registrada a partir do HTML real servido, dos breakpoints e da
inspeção das regras CSS; recomenda-se apenas uma conferência visual final em um
navegador gráfico antes da apresentação pública.

## Regressão automatizada

- Build Debug: aprovado, sem erros.
- Testes: 29 aprovados, 0 falhas.
- Casos novos: autenticação e autorização; estado inicial; termo curto; busca
  por nome, marca, categoria e cor; somente ativos; ordenação; nenhum resultado;
  seleção de produto.
- Aviso do ambiente: `NU1900`, pois a consulta de vulnerabilidades do NuGet não
  alcançou `https://api.nuget.org`; não houve erro de compilação ou teste.

## Limites preservados

A tela não consulta nem exibe SKU, numeração, saldo, grade, venda ou ruptura.
A seleção apenas destaca o produto dentro do resultado atual.
