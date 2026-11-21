# Inventário de telas e mapa de navegação do MVP

**Escopo:** P1 : VENDEDOR e LOJISTA  
**Fontes normativas:** [SRS](../02-requisitos/srs.md) (RF-01 a RF-30), [casos de uso](../02-requisitos/casos-de-uso.md) (UC-01 a UC-15) e [user stories](../02-requisitos/user-stories.md) (US-01 a US-15)

Este inventário é o mapa de navegação vigente. Protótipos e capturas anteriores
são referências visuais e não substituem as rotas e permissões abaixo.

## 1. Critério de classificação

| Estado | Critério usado neste inventário |
| --- | --- |
| **Existente** | Há action GET e Razor View utilizável na baseline. Uma regra implementada apenas no domínio não torna a tela existente. |
| **Pendente** | A interface é exigida por RF, UC ou US, mas não há action/view correspondente. |
| **Legado fora do fluxo** | Há rota ou view no repositório, mas ela não pertence aos fluxos normativos vigentes. |
| **Apoio técnico** | Estado de interface necessário a segurança/erro, sem constituir funcionalidade de negócio independente. |

“Vendeu”, “Não tinha” e “Desistiu” são inventariados como estados de interação da grade, e não como páginas administrativas independentes. Podem ser modal, confirmação ou feedback na própria grade, desde que mantenham os destinos e efeitos descritos abaixo.

## 2. Inventário de telas do fluxo do MVP

| ID | Tela/estado | Rota prevista ou atual | Perfil permitido | Estado | Responsabilidade e rastreabilidade |
| --- | --- | --- | --- | --- | --- |
| AUT-01 | Login | `/Account/Login` | Público; VENDEDOR; LOJISTA | **Existente** | E-mail, senha, erros de validação e criação da sessão. RF-01 a RF-04; UC-01; US-01. |
| VEN-01 | Início do vendedor | `/Estoque/Consulta` | VENDEDOR | **Existente** | Destino após login; abre diretamente a consulta operacional, sem menu administrativo. RF-02, RF-03, RF-13; UC-01, UC-02; US-02. |
| VEN-02 | Consulta de produtos | `/Estoque/Consulta` | VENDEDOR | **Existente** | Busca por parte do nome do modelo, valida mínimo de dois caracteres e apresenta resultados e estado vazio. RF-13; UC-02; US-02. |
| VEN-03 | Grade consultada | `/Estoque/Consulta` | VENDEDOR | **Existente** | Mostra numerações, saldo, atualização e estados de disponibilidade do SKU selecionado. RF-12, RF-14, RF-15; UC-03; US-03. |
| VEN-04 | Resultado “Vendeu” | `POST /Estoque/Vender` | VENDEDOR | **Existente** | Registra a venda de um par para o SKU do atendimento, atualiza saldo e informa sucesso ou conflito de saldo. RF-10, RF-11, RF-16, RF-17, RF-20; UC-04; US-04, US-07. |
| VEN-05 | Resultado “Não tinha” | `POST /Estoque/RegistrarNaoTinha` | VENDEDOR | **Existente** | Registra ruptura explícita para o SKU do atendimento sem alterar saldo. RF-16, RF-18, RF-20; UC-05; US-05, US-07. |
| VEN-06 | Resultado “Desistiu” | `POST /Estoque/Desistiu` | VENDEDOR | **Existente** | Encerra o atendimento sem venda, ruptura ou movimentação e permite nova consulta. RF-16, RF-19, RF-20; UC-06; US-06, US-07. |
| LOJ-01 | Início do lojista | `/Produtos` | LOJISTA | **Existente** | Destino após login e ponto de entrada para as áreas administrativas disponíveis; não há dashboard separado. RF-02; UC-01; US-08 a US-15. |
| LOJ-02 | Produtos | `/Produtos` | LOJISTA | **Existente** | Lista catálogo e grades; origina cadastro, detalhe e edição. Apoia RF-05 a RF-09; UC-07, UC-08; US-08, US-09. |
| LOJ-03 | Novo produto e grade | `/Produtos/Create` | LOJISTA | **Existente** | Cadastra identificação, foto opcional e numerações; gera SKUs e saldos iniciais e rejeita duplicidade. RF-05 a RF-09, RF-30; UC-07, UC-08, UC-15; US-08, US-09, US-15. |
| LOJ-04 | Detalhe do produto/grade | `/Produtos/Details/{id}` | LOJISTA | **Existente** | Exibe a grade do produto e origina entrada, saída administrativa e ajuste. Os estados do estoque e a consulta operacional do VENDEDOR são descritos em VEN-03. Apoia RF-14, RF-15, UC-08, UC-09, UC-10, US-09, US-10 e US-11. |
| LOJ-05 | Editar produto | `/Produtos/Edit/{id}` | LOJISTA | **Existente** | Edita identificação e permite trocar ou remover foto opcional; os SKUs existentes são preservados. RF-05, RF-30; UC-07, UC-15; US-08, US-15. |
| LOJ-06 | Entrada de estoque | `/Movimentacoes/Entrada[?skuId=...]`, `/Movimentacoes/EntradaLote` | LOJISTA | **Existente** | Registra entrada por SKU ou lote, atualiza saldo e histórico. RF-09, RF-10; UC-09; US-10. |
| LOJ-07 | Saída administrativa | `/Movimentacoes/Saida[?skuId=...]` | LOJISTA | **Existente** | Registra saída com quantidade informada e valida saldo. Não substitui “Vendeu”, ação operacional de um par ligada ao atendimento. RF-10, RF-11. |
| LOJ-08 | Ajuste manual | `/Movimentacoes/Ajuste[?skuId=...]` | LOJISTA | **Existente** | Define novo saldo, exige motivo e registra ajuste imutável. RF-10, RF-11, RF-23; UC-10; US-11. |
| LOJ-09 | Histórico de movimentações | `/Movimentacoes` | LOJISTA | **Existente** | Lista data/hora, produto, numeração, tipo, quantidade, responsável e motivo; sem editar/excluir. RF-10; apoio ao controle de estoque. |
| LOJ-10 | Saldos zerados | `/SaldosZerados` | LOJISTA | **Existente** | Lista somente SKUs com saldo zero, agrupados por modelo. RF-21; UC-11; US-12. |
| LOJ-11 | Histórico de rupturas | `/Rupturas` | LOJISTA | **Existente** | Agrupa ocorrências “Não tinha” por modelo e numeração. RF-22; UC-12; US-13. |
| LOJ-12 | Atendimentos | `/Atendimentos` | LOJISTA | **Existente** | Consulta atendimentos pendentes e concluídos, responsáveis, resultados e alertas. RF-25; UC-13. |
| LOJ-13 | Entradas fiscais | `/EntradasFiscais`, `/EntradasFiscais/Importar`, `/EntradasFiscais/Conferir/{id}` | LOJISTA | **Existente** | Importa XML manualmente como rascunho, confere itens e confirma estoque de forma transacional. RF-26 a RF-29; UC-14; US-14. |

## 3. Origem, destino e permissão

Esta matriz descreve o fluxo-alvo. “Voltar” deve preservar a origem quando a tela puder ser aberta por mais de um caminho.

| Origem | Ação | Destino | Permissão | Estado na baseline |
| --- | --- | --- | --- | --- |
| Não autenticado / rota protegida | Abrir sistema ou autenticar | AUT-01 Login | Público | Existente |
| AUT-01 Login | Credenciais válidas de vendedor | VEN-01 `/Estoque/Consulta` | VENDEDOR | Existente |
| AUT-01 Login | Credenciais válidas de lojista | LOJ-01 `/Produtos` | LOJISTA | Existente |
| AUT-01 Login | Credenciais inválidas | AUT-01 com mensagem, senha limpa | Público | Existente; confirmar limpeza da senha no teste de interface |
| VEN-01 | Focar/digitar busca | VEN-02 Consulta/resultados | VENDEDOR | Existente |
| VEN-02 | Selecionar modelo | VEN-03 Grade consultada | VENDEDOR | Existente |
| VEN-02 | Nenhum resultado | VEN-02 com estado vazio | VENDEDOR | Existente |
| VEN-03 | Selecionar SKU e “Vendeu” | VEN-04 confirmação/feedback | VENDEDOR | Existente |
| VEN-04 | Venda confirmada ou dispensar resultado | VEN-02 nova consulta | VENDEDOR | Existente |
| VEN-04 | Saldo zerado/concorrência | VEN-03 com erro e saldo atualizado | VENDEDOR | Existente |
| VEN-03 | Selecionar SKU e “Não tinha” | VEN-05 confirmação/feedback | VENDEDOR | Existente |
| VEN-05 | Ruptura registrada ou dispensar resultado | VEN-02 nova consulta | VENDEDOR | Existente |
| VEN-03 | “Desistiu” | VEN-06 e retorno à consulta | VENDEDOR | Existente |
| VEN-03 | Consultar outro modelo sem informar resultado | VEN-02 com pendência visível | VENDEDOR | Não bloqueia e não apaga o atendimento anterior |
| LOJ-01 | Produtos | LOJ-02 Produtos | LOJISTA | Existente |
| LOJ-02 | Novo produto com grade | LOJ-03 Novo produto e grade | LOJISTA | Existente |
| LOJ-03 | Salvar | LOJ-02 Produtos | LOJISTA | Existente |
| LOJ-02 | Detalhes | LOJ-04 Detalhe/grade | LOJISTA | Existente |
| LOJ-02 | Editar | LOJ-05 Editar produto | LOJISTA | Existente |
| LOJ-04 | Entrada | LOJ-06 Entrada com SKU pré-selecionado | LOJISTA | Existente |
| LOJ-04 | Saída | LOJ-07 Saída com SKU pré-selecionado | LOJISTA | Existente |
| LOJ-04 | Ajuste | LOJ-08 Ajuste com SKU pré-selecionado | LOJISTA | Existente |
| LOJ-06 / LOJ-07 / LOJ-08 | Salvar operação | LOJ-04 Detalhe/grade atualizado | LOJISTA | Existente |
| LOJ-01 | Movimentações | LOJ-09 Histórico | LOJISTA | Existente |
| LOJ-09 | Nova entrada / saída / ajuste | LOJ-06 / LOJ-07 / LOJ-08 | LOJISTA | Existente |
| LOJ-01 | Saldos zerados | LOJ-10 Saldos zerados | LOJISTA | Existente |
| LOJ-01 | Rupturas | LOJ-11 Histórico de rupturas | LOJISTA | Existente |
| LOJ-01 | Atendimentos | LOJ-12 Atendimentos | LOJISTA | Existente |
| LOJ-02 / LOJ-03 | Importar NF-e | LOJ-13 Entradas fiscais | LOJISTA | Existente; atalho também disponível no cadastro de produto |
| Qualquer tela autenticada | Sair | AUT-01 Login | VENDEDOR ou LOJISTA | Existente |
| Rota sem permissão | Autorização negada | SUP-01 Acesso negado | Usuário autenticado sem papel exigido | Existente |

## 4. Estado da implementação consultada

A matriz acima descreve os fluxos presentes na implementação atual:

- Após o login, o VENDEDOR entra em `/Estoque/Consulta` e o LOJISTA em `/Produtos`; não há dashboard separado.
- Consulta, grade, atualização de saldo e resultados Vendeu, Não tinha e Desistiu estão implementados no fluxo do VENDEDOR.
- Cadastro, entrada, saída administrativa, ajuste e consultas gerenciais pertencem ao LOJISTA.
- A saída administrativa em `/Movimentacoes/Saida` é distinta da venda operacional de um par em `/Estoque/Vender`; o backend restringe a saída administrativa ao LOJISTA.
- A lista de capturas de interface pendentes continua em [evidencias/README.md](evidencias/README.md). A ausência de captura final é uma lacuna de evidência visual, não indica que as rotas acima estejam pendentes.

## 5. Telas de apoio

| ID | Rota | Classificação | Decisão para os protótipos e testes P1 |
| --- | --- | --- | --- |
| SUP-01 | `/Account/AccessDenied` | Apoio técnico existente | Manter como estado de autorização; testar retorno seguro ao início do perfil. |
| SUP-02 | `/Home/Error` | Apoio técnico existente | Não entra no caminho feliz; cobrir apenas em teste técnico de erro. |

## 6. Cobertura das fontes

| Fonte | Telas/estados que a materializam |
| --- | --- |
| RF-01 a RF-04 / UC-01 / US-01 | AUT-01, redirecionamento a VEN-01 ou LOJ-01 e sessão autenticada. RF-04 é regra interna, validada sem tela própria. |
| RF-05, RF-06, RF-07, RF-08 / UC-07, UC-08 / US-08, US-09 | LOJ-02, LOJ-03, LOJ-04. A geração e unicidade do SKU são efeitos do salvamento, não telas. |
| RF-09, RF-10, RF-11, RF-12 / UC-09, UC-10 / US-10, US-11 | LOJ-04, LOJ-06, LOJ-07, LOJ-08, LOJ-09 e VEN-03; a última atualização aparece por SKU quando disponível. |
| RF-13, RF-14, RF-15 / UC-02, UC-03 / US-02, US-03 | VEN-01, VEN-02 e VEN-03. |
| RF-16 a RF-20, RF-24 / UC-04, UC-05, UC-06 / US-04, US-05, US-06, US-07 | VEN-04, VEN-05, VEN-06, retorno não bloqueante a VEN-02 e manutenção auditável das pendências. |
| RF-21 / UC-11 / US-12 | LOJ-10. |
| RF-22 / UC-12 / US-13 | LOJ-11. |
| RF-23 / UC-10 / US-11 | LOJ-08 e retorno a LOJ-04. |
| RF-25 / UC-13 | LOJ-12. |
| RF-26 a RF-29 / UC-14 / US-14 | LOJ-13 e confirmação de estoque. |
| RF-30 / UC-15 / US-15 | Foto opcional em LOJ-03, LOJ-05 e LOJ-13. |

## 7. Base para o plano de testes da P1

| Fluxo prioritário | Verificações mínimas |
| --- | --- |
| Login por perfil | Credenciais válidas e inválidas; destino correto por papel; retorno à rota protegida; sessão; logout; acesso negado entre perfis. |
| Consulta e grade | Limite de 2 caracteres; modelo encontrado/não encontrado; grade completa; saldo e última atualização; três estados visuais; uso em smartphone. |
| Vendeu | SKU obrigatório; decremento de exatamente 1; movimentação com usuário/data; saldo 0; duas vendas concorrentes do último par; nova consulta sem bloqueio. |
| Não tinha | SKU obrigatório; ruptura com vendedor/data; saldo inalterado; nenhuma ruptura automática para saldo zero. |
| Desistiu | Nenhuma movimentação ou ruptura; retorno imediato à consulta. |
| Produtos e grade | Campos obrigatórios; geração de SKU; duplicidade de numeração; produto salvo; grade criada com saldo inicial zero. |
| Entrada, saída e ajuste | Permissão; saldo atualizado; histórico; rejeição de saldo negativo; motivo obrigatório no ajuste; inexistência de editar/excluir movimentação. |
| Saldos zerados | Somente saldo 0; agrupamento por modelo; estado vazio. |
| Rupturas | Modelo, numeração e contagem corretos; agrupamento; estado vazio; acesso exclusivo do lojista. |
