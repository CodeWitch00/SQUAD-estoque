# S2-UX-001 - Atendimento do vendedor

## Delimitação

Este recorte atualiza o trecho do protótipo entre a grade consultada e a próxima consulta. Ele parte do [protótipo do vendedor](prototipos/prototipo-vendedor.html) e do [mapa de navegação do MVP](mapa-navegacao-mvp.svg). É uma referência de interação: não cria Razor Pages, controllers, endpoints nem persistência.

## Regra de contexto

Os resultados aparecem na grade do modelo consultado com a pergunta curta **Qual número?**. Abrir o modelo e consultar a grade não cria atendimento. Ao tocar em uma numeração com saldo, o sistema salva o SKU, o saldo consultado e cria uma pendência vinculada ao vendedor; uma confirmação informa que o par pode ser buscado. Se o vendedor escolher outra numeração disponível na mesma consulta, o SKU da pendência existente é atualizado, sem criar duplicata. Uma numeração sem saldo pode ser selecionada para a ação **Não tinha**, mas o toque isolado não cria pendência. Ao retomar uma pendência, somente o calçado selecionado é exibido. **Vendeu** e **Não tinha** só são habilitados após a escolha da numeração. **Desistiu** fica disponível depois que existe uma pendência e encerra o atendimento sem movimentação ou ruptura. **Busca** abre uma nova consulta sem apagar a pendência, que permanece visível na lista.

As numerações com saldo aparecem primeiro e as indisponíveis ficam em um grupo separado abaixo. Cada cartão mostra somente o número e uma informação curta: quantidade de pares, **Último par** ou **Sem estoque**. A seleção usa marca de confirmação e borda, sem depender somente de cor. Metadados e explicações repetidas não ocupam a área principal da consulta.

Quando todas as numerações do modelo estão sem saldo, abrir a grade ou tocar em um tamanho não cria pendência nem mostra **Desistiu**. Caso o cliente tenha pedido uma numeração específica, o vendedor pode tocar nela e confirmar **Não tinha**; nesse momento o sistema registra e conclui a demanda diretamente, sem deixar uma pendência artificial.

## Estados e transições

| Estado de origem | Ação | Destino | Feedback e efeito representado |
| --- | --- | --- | --- |
| Grade consultada | Tocar em numeração com saldo | Pendência criada para o SKU | Salva numeração e saldo; informa que o vendedor pode buscar o par. |
| Pendência com SKU | Tocar em outra numeração disponível | Pendência atualizada | Atualiza o SKU e saldo da mesma pendência, sem duplicá-la. |
| Grade consultada | Tocar em numeração sem saldo | Grade com SKU visualmente selecionado | Não cria pendência; **Não tinha** pode registrar e concluir a demanda. |
| Grade com SKU selecionado | Vendeu | Venda registrada | Confirma o modelo e o SKU; apresenta saldo reduzido e retorna automaticamente para a busca após breve leitura da mensagem. |
| Grade com SKU selecionado | Vendeu com saldo zero | Saldo indisponível | Rejeita o registro, preserva o SKU no contexto e mantém a grade com o saldo vigente. |
| Grade com SKU selecionado | Vendeu com atualização concorrente | Erro de saldo/concorrência | Rejeita o registro e destaca a atualização do saldo na grade. |
| Grade com SKU selecionado | Não tinha | Ruptura registrada | Confirma o modelo e o SKU; informa que o saldo foi mantido e retorna automaticamente para a busca após breve leitura da mensagem. |
| Grade sem SKU | Desistiu | Consulta com confirmação | Exibe somente o modelo no aviso de encerramento; não representa movimentação ou ruptura. |
| Grade com SKU selecionado | Desistiu | Consulta com confirmação | Pode manter o modelo e o SKU no aviso de encerramento; não representa movimentação ou ruptura. |
| Qualquer estado da grade | Consultar outro modelo | Consulta vazia com pendência visível | Permite atender outro cliente sem apagar ou concluir artificialmente o atendimento atual. |
| Venda ou ruptura registrada | Retorno automático | Consulta vazia | Exibe brevemente a confirmação e libera a busca seguinte sem exigir outro toque. |

## Critérios de revisão visual

- Área de toque dos botões de resultado: altura mínima de 46 px.
- O produto permanece visível antes de cada ação; numeração e saldo permanecem visíveis antes de Vendeu e Não tinha.
- Saldo zero, erro e confirmação usam texto além de cor.
- Os textos operacionais são curtos e a interface mantém o foco em smartphone.

## Como revisar

Abra `prototipos/prototipo-vendedor.html` no navegador e use o painel **Navegação de revisão** para inspecionar, em especial, os estados 07 a 14. O fluxo navegável também permite buscar um modelo, abrir a grade, selecionar uma numeração e acionar cada resultado.
