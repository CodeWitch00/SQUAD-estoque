# Decisões de interface do MVP

Este registro reúne decisões de interação aprovadas. As fontes normativas atuais são RF-01 a RF-30, UC-01 a UC-15 e US-01 a US-15.

| ID | Decisão | Motivo e impacto |
| --- | --- | --- |
| DI-01 | Após o login, o VENDEDOR vai para `/Estoque/Consulta` e o LOJISTA para `/Produtos`; não há dashboard separado. | Destinos implementados conforme UC-01. |
| DI-02 | A consulta é a ação principal e imediata do vendedor. | RF-13, UC-02 e US-02; reduz passos no atendimento mobile. |
| DI-03 | Consulta e grade podem ocupar estados da mesma experiência mobile. | Mantém o limite de fricção; não altera os casos de uso nem cria funcionalidade. |
| DI-04 | “Vendeu”, “Não tinha” e “Desistiu” são ações contextuais do SKU consultado. | RF-16 a RF-20; evita uma operação sem SKU e mantém rastreabilidade. |
| DI-05 | “Vendeu” sempre movimenta exatamente 1 par. | RF-17 e UC-04. O formulário genérico de saída não substitui essa ação. |
| DI-06 | O vendedor pode iniciar outra consulta sem registrar resultado, mantendo o atendimento anterior pendente e auditável. | RF-20, RF-24 e RN-08 evitam bloqueio sem perder rastreabilidade. |
| DI-07 | Entrada, saída administrativa, ajuste e histórico pertencem ao LOJISTA. | Os controllers e as ações GET/POST de movimentação exigem o papel LOJISTA. |
| DI-08 | Saldos zerados e rupturas são visões diferentes. | Saldo zero é estado de estoque; ruptura só nasce da declaração “Não tinha” (RF-18, RF-21 e RF-22). |
| DI-09 | Os três estados usam texto além de cor. | “Disponível”, “Último par” e “Indisponível” precisam permanecer distinguíveis sem depender apenas da percepção cromática. |
| DI-10 | Protótipos são referências visuais e não indicam o estado de implementação. | Consulte o inventário vigente e o código para verificar telas, rotas e permissões. |
| DI-12 | Não haverá dashboard analítico, notificações, recompra automática ou funções de ERP. | Esses itens ultrapassam o MVP documentado. |

## Pendências que exigem validação na revisão

- Confirmar com produto os rótulos finais dos atalhos do início do lojista.
- Confirmar se “Vendeu” e “Não tinha” terão confirmação explícita ou feedback com opção de desfazer; nenhuma opção pode mudar as regras de persistência do MVP.
- Validar os fluxos em smartphone para vendedor e desktop/tablet para lojista.
- Validar acessibilidade por teclado, foco, contraste e mensagens de erro nos protótipos finais.
