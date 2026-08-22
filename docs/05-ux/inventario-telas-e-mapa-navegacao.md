# Inventário de telas e mapa de navegação do MVP

**Escopo:** P1 : VENDEDOR e LOJISTA  
**Fontes normativas:** [SRS](../02-requisitos/srs.md) (RF-01 a RF-23), [casos de uso](../02-requisitos/casos-de-uso.md) (UC-01 a UC-12) e [user stories](../02-requisitos/user-stories.md) (US-01 a US-13)  
**Mapa visual:** [mapa-navegacao-mvp.svg](mapa-navegacao-mvp.svg)

## 1. Critério de classificação

| Estado | Critério usado neste inventário |
| --- | --- |
| **Existente** | Há action GET e Razor View utilizável na baseline. Uma regra implementada apenas no domínio não torna a tela existente. |
| **Pendente** | A interface é exigida por RF, UC ou US, mas não há action/view correspondente. |
| **Legado fora do fluxo** | Há rota ou view no repositório, mas ela não pertence aos fluxos RF-01 a RF-23, UC-01 a UC-12 ou US-01 a US-13. |
| **Apoio técnico** | Estado de interface necessário a segurança/erro, sem constituir funcionalidade de negócio independente. |

“Vendeu”, “Não tinha” e “Desistiu” são inventariados como estados de interação da grade, e não como páginas administrativas independentes. Podem ser modal, confirmação ou feedback na própria grade, desde que mantenham os destinos e efeitos descritos abaixo.

## 2. Inventário de telas do fluxo do MVP

| ID | Tela/estado | Rota prevista ou atual | Perfil permitido | Estado | Responsabilidade e rastreabilidade |
| --- | --- | --- | --- | --- | --- |
| AUT-01 | Login | `/Account/Login` | Público; VENDEDOR; LOJISTA | **Existente** | E-mail, senha, erros de validação e criação da sessão. RF-01 a RF-04; UC-01; US-01. |
| VEN-01 | Início do vendedor | a definir na implementação | VENDEDOR | **Pendente** | Entrada do perfil já com acesso imediato à consulta, sem menu administrativo. RF-02, RF-03, RF-13; UC-01, UC-02; US-02. |
| VEN-02 | Consulta de produtos | a definir na implementação | VENDEDOR | **Pendente** | Busca por parte do nome do modelo; não consultar com menos de 2 caracteres; apresentar vazio e “produto não encontrado”. RF-13; UC-02; US-02. |
| VEN-03 | Grade consultada | a definir na implementação | VENDEDOR | **Pendente** | Modelo selecionado, todas as numerações, saldos e última atualização, com estados Disponível, Último par e Indisponível. RF-12, RF-14, RF-15; UC-03; US-03. |
| VEN-04 | Resultado “Vendeu” | ação da grade | VENDEDOR | **Pendente** | Selecionar SKU, confirmar em um toque, decrementar exatamente 1, registrar saída e mostrar saldo atualizado ou erro de saldo. RF-10, RF-11, RF-16, RF-17, RF-20; UC-04; US-04, US-07. |
| VEN-05 | Resultado “Não tinha” | ação da grade | VENDEDOR | **Pendente** | Selecionar SKU, criar ruptura explícita e confirmar sem alterar saldo. RF-16, RF-18, RF-20; UC-05; US-05, US-07. |
| VEN-06 | Resultado “Desistiu” | ação da grade | VENDEDOR | **Pendente** | Encerrar o atendimento, não persistir movimentação e retornar à consulta. RF-16, RF-19, RF-20; UC-06; US-06, US-07. |
| LOJ-01 | Início do lojista | a definir na implementação | LOJISTA | **Pendente** | Entrada do perfil e acesso às áreas Produtos, Movimentações, Saldos zerados e Rupturas. RF-02; UC-01; US-08 a US-13. |
| LOJ-02 | Produtos | `/Produtos` | LOJISTA | **Existente** | Lista catálogo e grades; origina cadastro, detalhe e edição. Apoia RF-05 a RF-09; UC-07, UC-08; US-08, US-09. |
| LOJ-03 | Novo produto e grade | `/Produtos/Create` | LOJISTA | **Existente** | Cadastra nome, marca, categoria, cor e numerações; gera SKUs com saldo zero e rejeita numeração repetida no formulário. RF-05, RF-06, RF-07, RF-08 e apoio a RF-09; UC-07, UC-08; US-08, US-09. |
| LOJ-04 | Detalhe do produto/grade | `/Produtos/Details/{id}` | LOJISTA | **Existente** | Exibe saldos e os três estados por SKU; origina entrada, saída e ajuste. Apoia RF-14, RF-15, UC-08, UC-09, UC-10, US-09, US-10 e US-11. Não exibe a última atualização exigida por RF-12. |
| LOJ-05 | Editar produto | `/Produtos/Edit/{id}` | LOJISTA | **Existente** | Mantém nome, marca, categoria e cor. É manutenção já existente; não adiciona capacidade além do cadastro do produto do MVP. Apoio a RF-05 e US-08. |
| LOJ-06 | Entrada de estoque | `/Movimentacoes/Entrada[?skuId=...]` | LOJISTA | **Existente** | Seleciona SKU e quantidade, atualiza saldo e registra movimentação. RF-09, RF-10; UC-09; US-10. |
| LOJ-07 | Saída de estoque | `/Movimentacoes/Saida[?skuId=...]` | **LOJISTA no fluxo-alvo** | **Existente com divergência** | Saída administrativa, com quantidade e validação de saldo. RF-10, RF-11. Não substitui “Vendeu”, que é uma ação operacional de exatamente 1 par ligada à consulta. |
| LOJ-08 | Ajuste manual | `/Movimentacoes/Ajuste[?skuId=...]` | LOJISTA | **Existente** | Define novo saldo, exige motivo e registra ajuste imutável. RF-10, RF-11, RF-23; UC-10; US-11. |
| LOJ-09 | Histórico de movimentações | `/Movimentacoes` | LOJISTA | **Existente** | Lista data/hora, produto, numeração, tipo, quantidade, responsável e motivo; sem editar/excluir. RF-10; apoio ao controle de estoque. |
| LOJ-10 | Saldos zerados | a definir na implementação | LOJISTA | **Pendente** | Lista somente SKUs com saldo zero, agrupados por modelo. RF-21; UC-11; US-12. |
| LOJ-11 | Histórico de rupturas | a definir na implementação | LOJISTA | **Pendente** | Agrupa “Não tinha” por modelo e numeração e mostra a quantidade de ocorrências. RF-22; UC-12; US-13. |

## 3. Origem, destino e permissão

Esta matriz descreve o fluxo-alvo. “Voltar” deve preservar a origem quando a tela puder ser aberta por mais de um caminho.

| Origem | Ação | Destino | Permissão | Estado na baseline |
| --- | --- | --- | --- | --- |
| Não autenticado / rota protegida | Abrir sistema ou autenticar | AUT-01 Login | Público | Existente |
| AUT-01 Login | Credenciais válidas de vendedor | VEN-01 Início do vendedor | VENDEDOR | Destino pendente; hoje vai para Home provisória |
| AUT-01 Login | Credenciais válidas de lojista | LOJ-01 Início do lojista | LOJISTA | Destino pendente; hoje vai para Home provisória |
| AUT-01 Login | Credenciais inválidas | AUT-01 com mensagem, senha limpa | Público | Existente; confirmar limpeza da senha no teste de interface |
| VEN-01 | Focar/digitar busca | VEN-02 Consulta/resultados | VENDEDOR | Pendente |
| VEN-02 | Selecionar modelo | VEN-03 Grade consultada | VENDEDOR | Pendente |
| VEN-02 | Nenhum resultado | VEN-02 com estado vazio | VENDEDOR | Pendente |
| VEN-03 | Selecionar SKU e “Vendeu” | VEN-04 confirmação/feedback | VENDEDOR | Pendente |
| VEN-04 | Venda confirmada ou dispensar resultado | VEN-02 nova consulta | VENDEDOR | Pendente |
| VEN-04 | Saldo zerado/concorrência | VEN-03 com erro e saldo atualizado | VENDEDOR | Pendente |
| VEN-03 | Selecionar SKU e “Não tinha” | VEN-05 confirmação/feedback | VENDEDOR | Pendente |
| VEN-05 | Ruptura registrada ou dispensar resultado | VEN-02 nova consulta | VENDEDOR | Pendente |
| VEN-03 | “Desistiu” | VEN-06 e retorno à consulta | VENDEDOR | Pendente |
| VEN-03 | Nova consulta sem informar resultado | VEN-02 | VENDEDOR | Pendente; obrigatório não bloquear |
| LOJ-01 | Produtos | LOJ-02 Produtos | LOJISTA | Tela existente; origem pendente |
| LOJ-02 | Novo produto com grade | LOJ-03 Novo produto e grade | LOJISTA | Existente |
| LOJ-03 | Salvar | LOJ-02 Produtos | LOJISTA | Existente |
| LOJ-02 | Detalhes | LOJ-04 Detalhe/grade | LOJISTA | Existente |
| LOJ-02 | Editar | LOJ-05 Editar produto | LOJISTA | Existente |
| LOJ-04 | Entrada | LOJ-06 Entrada com SKU pré-selecionado | LOJISTA | Existente |
| LOJ-04 | Saída | LOJ-07 Saída com SKU pré-selecionado | LOJISTA | Existente |
| LOJ-04 | Ajuste | LOJ-08 Ajuste com SKU pré-selecionado | LOJISTA | Existente |
| LOJ-06 / LOJ-07 / LOJ-08 | Salvar operação | LOJ-04 Detalhe/grade atualizado | LOJISTA | Existente |
| LOJ-01 | Movimentações | LOJ-09 Histórico | LOJISTA | Tela existente; origem pendente |
| LOJ-09 | Nova entrada / saída / ajuste | LOJ-06 / LOJ-07 / LOJ-08 | LOJISTA | Existente |
| LOJ-01 | Saldos zerados | LOJ-10 Saldos zerados | LOJISTA | Pendente |
| LOJ-01 | Rupturas | LOJ-11 Histórico de rupturas | LOJISTA | Pendente |
| Qualquer tela autenticada | Sair | AUT-01 Login | VENDEDOR ou LOJISTA | Existente |
| Rota sem permissão | Autorização negada | SUP-01 Acesso negado | Usuário autenticado sem papel exigido | Existente |

## 4. Desvios entre a baseline e o fluxo-alvo

1. **Não há início por perfil.** `AccountController` envia VENDEDOR e LOJISTA para `/`, e `Home/Index` é pública e provisória. Isso não cumpre o redirecionamento por perfil do UC-01.
2. **A consulta operacional inteira está pendente.** Não existem controller/view para VEN-02 a VEN-06, apesar de Produto, SKU e Ruptura já existirem no modelo.
3. **A grade existente não é a grade do vendedor.** `/Produtos/Details/{id}` mostra saldos e estados, porém todo `ProdutosController` exige LOJISTA. O bloco da view que testa `User.IsInRole("VENDEDOR")` é inalcançável na configuração atual.
4. **“Saída” não é “Vendeu”.** `/Movimentacoes/Saida` aceita quantidade livre, pode ser aberta sem consulta anterior e redireciona o vendedor para a Home provisória. VEN-04 exige um SKU consultado, decremento fixo de 1 e continuidade do atendimento.
5. **Permissão da saída genérica está ampla.** O controller permite `LOJISTA,VENDEDOR`, embora o fluxo-alvo reserve a operação administrativa ao LOJISTA e dê ao VENDEDOR apenas “Vendeu”. A equipe deve corrigir essa autorização quando VEN-04 for implementada.
6. **Saldos zerados e rupturas não têm endpoints ou views.** São pendências diretas de RF-21/UC-11/US-12 e RF-22/UC-12/US-13.
7. **A navegação global contém itens fora do MVP.** “Privacy” está no menu e rodapé; deve sair do fluxo principal. Produtos e Movimentações aparecem apenas para LOJISTA, o que está correto para as áreas administrativas.
8. **A última atualização do SKU não aparece.** A grade existente mostra o saldo, mas não data/hora de atualização; RF-12 continua pendente para a grade operacional.

## 5. Telas de apoio e legado fora do fluxo

| ID | Rota | Classificação | Decisão para os protótipos e testes P1 |
| --- | --- | --- | --- |
| SUP-01 | `/Account/AccessDenied` | Apoio técnico existente | Manter como estado de autorização; testar retorno seguro ao início do perfil. |
| SUP-02 | `/Home/Error` | Apoio técnico existente | Não entra no caminho feliz; cobrir apenas em teste técnico de erro. |
| LEG-01 | `/` (`Home/Index` atual) | Legado/provisório | Substituir pelos inícios por perfil; não usar como referência visual. |
| LEG-02 | `/Home/Privacy` | Legado de template | Fora do MVP e do mapa principal. |
| LEG-03 | `/Movies` | Legado de tutorial | Fora do MVP; inclui lista de filmes. |
| LEG-04 | `/Movies/Details/{id}` | Legado de tutorial | Fora do MVP. |
| LEG-05 | `/Movies/Create` | Legado de tutorial | Fora do MVP. |
| LEG-06 | `/Movies/Edit/{id}` | Legado de tutorial | Fora do MVP. |
| LEG-07 | `/Movies/Delete/{id}` | Legado de tutorial | Fora do MVP. |
| LEG-08 | `/HelloWorld` | Legado de tutorial | Fora do MVP. |
| LEG-09 | `/HelloWorld/Welcome` | Legado de tutorial | Fora do MVP. |

As rotas de Movies e HelloWorld não possuem `[Authorize]`; embora estejam fora do fluxo, essa exposição deve constar no plano técnico de remoção do legado, sem ampliar o escopo funcional do MVP.

## 6. Cobertura das fontes

| Fonte | Telas/estados que a materializam |
| --- | --- |
| RF-01 a RF-04 / UC-01 / US-01 | AUT-01, redirecionamento a VEN-01 ou LOJ-01 e sessão autenticada. RF-04 é regra interna, validada sem tela própria. |
| RF-05, RF-06, RF-07, RF-08 / UC-07, UC-08 / US-08, US-09 | LOJ-02, LOJ-03, LOJ-04. A geração e unicidade do SKU são efeitos do salvamento, não telas. |
| RF-09, RF-10, RF-11, RF-12 / UC-09, UC-10 / US-10, US-11 | LOJ-04, LOJ-06, LOJ-07, LOJ-08, LOJ-09 e estados de erro; RF-12 está pendente e deve aparecer em VEN-03. |
| RF-13, RF-14, RF-15 / UC-02, UC-03 / US-02, US-03 | VEN-01, VEN-02 e VEN-03. |
| RF-16, RF-17, RF-18, RF-19, RF-20 / UC-04, UC-05, UC-06 / US-04, US-05, US-06, US-07 | VEN-04, VEN-05, VEN-06 e retorno não bloqueante a VEN-02. |
| RF-21 / UC-11 / US-12 | LOJ-10. |
| RF-22 / UC-12 / US-13 | LOJ-11. |
| RF-23 / UC-10 / US-11 | LOJ-08 e retorno a LOJ-04. |

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


