# Validação manual - login responsivo

Data: 2026-09-16

Escopo: tela de login (`/Account/Login`) nas larguras desktop e smartphone.

## Evidências visuais

- `login-desktop.png`: tela em desktop, com foco inicial visível no campo de e-mail.
- `login-mobile-390x844.png`: tela no viewport CSS de 390 x 844, sem rolagem horizontal, com campos e botão inteiramente visíveis.

## Cenários executados

| Cenário | Ambiente | Resultado |
| --- | --- | --- |
| Renderização e foco inicial | Desktop | Aprovado: foco visível no campo de e-mail. |
| Layout responsivo | Smartphone 390 x 844 | Aprovado: conteúdo empilhado, sem overflow horizontal e controles acessíveis ao toque. |
| Campos obrigatórios vazios | Smartphone 390 x 844 | Aprovado: mensagens "Informe o e-mail" e "Informe a senha" são exibidas junto aos respectivos campos. |
| Credenciais inválidas | Teste de autenticação | Aprovado: mensagem de credenciais inválidas e ausência de autenticação. |
| Login válido por perfil | Teste de autenticação | Aprovado: cookie e redirecionamento para a área correspondente ao perfil. |

Não foram utilizadas ou exibidas credenciais reais nas capturas.

## Validação complementar informada pela usuária

Data do relato: 2026-09-18. Revisão testada: `origin/dev/emmy` no commit `9d4f790`.

A usuária informou que todos os cenários do roteiro manual foram aprovados em desktop
e nos viewports móveis de 360 × 800, 390 × 844 e 430 × 932:

| Cenário | Resultado informado |
| --- | --- |
| Layout inicial, foco e ausência de cortes, sobreposição ou rolagem horizontal | Aprovado |
| Campos vazios e mensagens de obrigatoriedade | Aprovado |
| Senha incorreta e mensagem de credenciais inválidas | Aprovado |
| Login válido de LOJISTA e destino em Produtos | Aprovado |
| Login válido de VENDEDOR e destino em `/Estoque/Consulta` | Aprovado |
| Navegação por Tab, foco visível, botão de mostrar senha e acionamento dos controles | Aprovado |

O navegador, sua versão e o tipo de dispositivo móvel não foram informados. As
capturas acima pertencem à validação de 2026-09-16; não foram recebidas novas
capturas da execução complementar.
