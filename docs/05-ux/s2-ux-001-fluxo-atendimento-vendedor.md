# S2-UX-001 - Atendimento do vendedor

## Delimitação

Este recorte atualiza o trecho do protótipo entre a grade consultada e a próxima consulta. Ele parte do [protótipo do vendedor](prototipos/prototipo-vendedor.html) e do [mapa de navegação do MVP](mapa-navegacao-mvp.svg). É uma referência de interação: não cria Razor Pages, controllers, endpoints nem persistência.

## Regra de contexto

Os resultados aparecem na grade do modelo consultado. **Vendeu** e **Não tinha** só são habilitados após a escolha de uma numeração: suas confirmações identificam modelo, SKU (base + numeração) e saldo exibido. **Desistiu** permanece disponível assim que a grade é exibida, pois encerra o atendimento sem movimentação, ruptura ou SKU. A continuidade também permanece disponível: **Nova consulta sem registrar resultado** não produz resultado nem exige SKU.

## Estados e transições

| Estado de origem | Ação | Destino | Feedback e efeito representado |
| --- | --- | --- | --- |
| Grade sem SKU | Tocar em uma numeração | Grade com SKU selecionado | A seleção mostra numeração, saldo e estado do estoque; Vendeu e Não tinha são habilitados. |
| Grade com SKU selecionado | Vendeu | Venda registrada | Confirma o modelo e o SKU; apresenta saldo reduzido em um par e oferece Nova consulta. |
| Grade com SKU selecionado | Vendeu com saldo zero | Saldo indisponível | Rejeita o registro, preserva o SKU no contexto e mantém a grade com o saldo vigente. |
| Grade com SKU selecionado | Vendeu com atualização concorrente | Erro de saldo/concorrência | Rejeita o registro e destaca a atualização do saldo na grade. |
| Grade com SKU selecionado | Não tinha | Ruptura registrada | Confirma o modelo e o SKU; informa que o saldo foi mantido e oferece Nova consulta. |
| Grade sem SKU | Desistiu | Consulta com confirmação | Exibe somente o modelo no aviso de encerramento; não representa movimentação ou ruptura. |
| Grade com SKU selecionado | Desistiu | Consulta com confirmação | Pode manter o modelo e o SKU no aviso de encerramento; não representa movimentação ou ruptura. |
| Qualquer estado da grade | Nova consulta sem registrar resultado | Consulta vazia | Inicia outro atendimento sem gravar resultado. |
| Venda ou ruptura registrada | Nova consulta | Consulta vazia | Encerra a confirmação e libera a busca seguinte. |

## Critérios de revisão visual

- Área de toque dos botões de resultado: altura mínima de 46 px.
- O produto permanece visível antes de cada ação; numeração e saldo permanecem visíveis antes de Vendeu e Não tinha.
- Saldo zero, erro e confirmação usam texto além de cor.
- Os textos operacionais são curtos e a interface mantém o foco em smartphone.

## Como revisar

Abra `prototipo-vendedor.html` no navegador e use o painel **Navegação de revisão** para inspecionar, em especial, os estados 07 a 14. O fluxo navegável também permite buscar um modelo, abrir a grade, selecionar uma numeração e acionar cada resultado.
