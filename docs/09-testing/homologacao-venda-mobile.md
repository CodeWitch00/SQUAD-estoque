# Homologação da venda rápida no mobile

**Estado:** em execução; aceite mobile pendente  
**Ambiente:** aplicação publicada no Azure, `https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net`  
**Execução disponível:** Firefox 156.0 em Linux, viewport de 390 × 844 px, orientação vertical, em modo sem interface. Não é um smartphone físico nem um navegador com entrada por toque.  
**Conta e massa:** a conta de teste `vendedor@squad.com` autenticou em uma consulta HTTP de leitura. A busca por `Tênis` retornou o produto `Tênis run` (Olympics, Preto), com apenas a numeração 37, saldo 0 e estado **Indisponível**. Não foram identificados SKUs com saldo maior que 1 ou saldo 1 nas buscas por `Tênis`, `Sapato`, `Sandália`, `Bota`, `Preto` e `Squad`. Nenhuma venda foi enviada e nenhum dado foi alterado.

## Registro por passo

| Passo | Esperado | Obtido | Estado |
| --- | --- | --- | --- |
| 1. Abrir o login no viewport móvel | Formulário inteiro legível, campos e botão acessíveis, sem sobreposição ou rolagem horizontal | A página respondeu HTTP 200. A captura de 390 × 844 px mostra e-mail, senha e Entrar visíveis; não há sobreposição ou rolagem horizontal aparente na captura | Parcial: verificação visual, sem interação |
| 2. Autenticar vendedor e consultar produto | Login leva à consulta; busca e seleção exibem grade, saldo e estado de cada numeração | Login e consulta HTTP funcionaram. A grade publicada de `Tênis run` exibiu tamanho 37, saldo 0, **Indisponível**. Não foi executada interação no navegador mobile | Parcial |
| 3. Vender SKU com saldo maior que 1 | Venda confirmada; saldo diminui exatamente 1; grade e mensagem exibem o saldo atualizado | Não executado: SKU preparado não identificado e a página publicada não apresenta **Vendeu** | Bloqueado |
| 4. Vender o último par (saldo 1) | Uma venda confirmada; saldo final 0; grade passa a **Indisponível**, com mensagem de sucesso e saldo atualizado | Não executado: SKU preparado não identificado e a página publicada não apresenta **Vendeu** | Bloqueado |
| 5. Tentar vender SKU já com saldo 0 | A interface mostra **Indisponível**, bloqueia **Vendeu**, conserva saldo 0 e não registra movimentação; verificar a mensagem apresentada ou sua ausência | Saldo 0 e **Indisponível** apareceram na resposta HTML. A página publicada não contém ação **Vendeu**, seleção de SKU ou mensagem de venda; nenhuma tentativa foi enviada | Bloqueado |
| 6. Iniciar nova consulta após cada resultado | Busca fica disponível para outro atendimento, sem reenvio da venda anterior | Não executado: a página publicada não contém **Nova consulta** nem fluxo de resultado | Bloqueado |
| 7. Conferir foco e alvo de toque | Foco visível e ordem útil; campos, cards, ações e nova consulta acionáveis com o dedo | Não executado em aparelho com toque | Pendente |
| 8. Conferir layout durante todos os estados | Sem sobreposição, cortes ou rolagem horizontal em busca, grade, mensagens e nova consulta | Apenas o login foi visto na captura; os demais estados não foram abertos | Pendente |

**Evidência obtida:** [login em 390 × 844 px](evidencias/homologacao-venda-mobile/login-firefox-390x844.png).

## Observação para a tentativa com saldo zero

Na implementação da branch examinada, selecionar um SKU com saldo 0 mostra **Indisponível** e deixa o botão **Vendeu** desabilitado. O fluxo normal da interface, portanto, não envia uma tentativa de venda e não apresenta mensagem de erro nesse caso. A rejeição HTTP de uma venda sem saldo é uma verificação separada da interface e já possui teste de integração em outra atividade. Esta diferença deve ser considerada ao conferir o critério de “mensagem” no cartão, antes de declarar a homologação aprovada.

## Divergência do ambiente publicado

Após autenticar a conta de teste, a resposta HTML de `/Estoque/Consulta?termo=T%C3%AAnis&produtoId=91978fba-8983-4163-9be8-a5464e8449fe` continha a grade antiga: não havia controles `Vendeu`, `skuSelecionado` ou `Nova consulta`. O código examinado na branch `dev/rayana` contém esses controles. A versão publicada no Azure precisa ser identificada e atualizada antes de executar a homologação do fluxo completo. Não foi possível atribuir a diferença a um commit implantado porque a aplicação não expôs uma versão na página consultada.

## Condições para concluir

1. Registrar aparelho físico, sistema e navegador utilizados, ou declarar formalmente que a equipe aceita somente emulação.
2. Disponibilizar no ambiente de homologação uma versão que contenha as ações de venda e nova consulta, identificando o commit implantado.
3. Identificar os produtos, numerações, IDs dos SKUs e saldos iniciais para os três cenários; reservar os dados para evitar vendas simultâneas da equipe.
4. Executar os passos pendentes na interface e registrar saldo, estado visual, mensagem, foco, toque e continuidade obtidos, com capturas antes e depois.
5. Confirmar que os dados de teste foram preparados sem afetar estoque real.

Nenhum código da aplicação foi alterado nesta validação.
