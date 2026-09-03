# Especificação de testes - autenticação, sessão e perfis

## 1. Objetivo e rastreabilidade

Esta especificação define os testes funcionais e de integração para autenticação,
manutenção de sessão e autorização por perfil do SQUAD Estoque. Ela complementa o
[plano de testes](plano-de-testes.md): descreve o comportamento verificável, mas não
reproduz a implementação dos testes automatizados.

| Artefato de origem | Itens cobertos |
|---|---|
| [SRS](../02-requisitos/srs.md) | RF-01, RF-02, RF-03, RF-04 e RNF-04 |
| [Casos de uso](../02-requisitos/casos-de-uso.md) | UC-01 — Realizar login |
| Implementação atual | Cookie de autenticação, validade de 12 horas e perfis `LOJISTA` e `VENDEDOR` |

**Fora do escopo:** cadastro de usuários, recuperação de senha, troca de senha e
integração com provedores externos de identidade.

## 2. Dados e condições de execução

Os testes devem usar uma base isolada, sem dados reais. A massa padrão é a mesma do
plano de testes:

| Identificador | E-mail | Senha | Perfil |
|---|---|---|---|
| USR-LOJ-01 | `lojista@squad.com` | `123` | `LOJISTA` |
| USR-VEN-01 | `vendedor@squad.com` | `123` | `VENDEDOR` |
| USR-INV-01 | `usuario.invalido@example.test` | `senha-incorreta` | Não cadastrado |

Para requisições `POST`, enviar um token antiforgery válido. Os valores de senha,
hash e cookie não devem ser registrados em logs, relatórios ou evidências anexadas.

### Rotas consideradas nesta especificação

| Rota | Acesso esperado |
|---|---|
| `GET` e `POST /Account/Login` | Anônimo; o `GET` redireciona quem já está autenticado |
| `POST /Account/Logout` | Sessão autenticada; encerra a autenticação |
| `GET /Produtos` | Somente `LOJISTA` |
| `GET /Movimentacoes` | Somente `LOJISTA` |
| `GET /Movimentacoes/Saida` | `LOJISTA` ou `VENDEDOR` |

## 3. Casos de teste

Legenda: **Existente** indica cobertura automatizada já presente; **Planejado**
indica comportamento especificado que ainda requer teste automatizado ou manual;
**Parcial** indica que há cobertura, mas falta verificar alguma condição de aceite.

| ID | Requisitos | Cenário e entradas | Resultado esperado | Situação |
|---|---|---|---|---|
| AUT-01 | RF-01, UC-01 | Usuário **anônimo** envia `GET /Account/Login`. | Resposta `200 OK`; formulário contém campos e-mail e senha, submissão `POST` e token antiforgery. | Existente |
| AUT-02 | RF-01, RF-02, UC-01 | `POST /Account/Login` com USR-LOJ-01 e token válido. | Credenciais são aceitas; é emitida sessão autenticada com perfil `LOJISTA`; resposta redireciona para `/Produtos`. | Existente |
| AUT-03 | RF-01, RF-02, RNF-06, UC-01 | `POST /Account/Login` com USR-VEN-01 e token válido. | Credenciais são aceitas; é emitida sessão autenticada com perfil `VENDEDOR`; resposta redireciona diretamente para `/Estoque/Consulta`. | Existente |
| AUT-04 | RF-01, UC-01 | `POST /Account/Login` com USR-INV-01 e token válido. | Login é recusado; a página é reapresentada com mensagem genérica de credenciais inválidas; nenhuma sessão autenticada é criada e `/Produtos` continua redirecionando ao login. | Existente — inclui mensagem, ausência de sessão e senha não renderizada novamente. |
| AUT-05 | RF-01, UC-01 | `POST /Account/Login` com `lojista@squad.com` e `senha-incorreta`. | Login é recusado sem informar se o e-mail existe; não é criada sessão autenticada. Conforme UC-01, o campo senha deve ser limpo. | Planejado |
| AUT-06 | RF-01, UC-01 | `POST /Account/Login` com e-mail e senha vazios, com token válido. | O formulário apresenta os erros de obrigatoriedade; nenhuma sessão autenticada é criada. A validação de cliente pode impedir o envio, mas o servidor também deve rejeitar a entrada. | Planejado |
| AUT-07 | RF-01, RF-02, UC-01 | Usuário já autenticado requisita `GET /Account/Login`. | Redirecionamento para a entrada do perfil (`/Estoque/Consulta` ou `/Produtos`); a página de login não é exibida novamente. | Existente |
| AUT-20 | RF-02, RNF-06, UC-01 | Usuário autenticado visualiza a navegação global. | `VENDEDOR` vê somente Consulta e logout; `LOJISTA` vê Produtos, Movimentações e logout. A marca retorna à entrada do próprio perfil. | Existente |
| AUT-08 | RF-01, UC-01 | `LOJISTA` anônimo solicita `/Produtos`, autentica-se com USR-LOJ-01 e informa `ReturnUrl=/Produtos`. | Após autenticar, o usuário é redirecionado para `/Produtos`. Um `ReturnUrl` externo não pode ser aceito. | Planejado |
| AUT-09 | RF-02 | Usuário **anônimo** solicita `GET /Produtos` e, em execução separada, `GET /Movimentacoes`. | Em ambos os casos, resposta `302` para `/Account/Login` com `ReturnUrl`; nenhum conteúdo protegido é retornado. | Existente |
| AUT-10 | RF-02 | `LOJISTA` autenticado solicita `GET /Produtos` e `GET /Movimentacoes`. | As duas rotas respondem `200 OK`, pois pertencem ao perfil `LOJISTA`. | Existente |
| AUT-11 | RF-02 | `VENDEDOR` autenticado solicita `GET /Movimentacoes/Saida`. | Resposta `200 OK`, pois a rota permite `VENDEDOR` e `LOJISTA`. | Existente |
| AUT-12 | RF-02 | `VENDEDOR` autenticado tenta acessar a rota proibida `GET /Produtos` diretamente. | Resposta `302` para `/Account/AccessDenied?ReturnUrl=%2FProdutos`; não retorna dados de produtos. | Existente |
| AUT-13 | RF-02 | `VENDEDOR` autenticado tenta acessar `GET /Movimentacoes/Ajuste` e enviar um `POST /Movimentacoes/Ajuste` válido. | Ambos são negados. No `POST`, saldo e histórico de movimentações permanecem inalterados. | Parcial — o POST é negado por `Vendedor_cannot_execute_administrative_post_endpoints`; falta afirmar GET e banco inalterado no mesmo caso. |
| AUT-14 | RF-03 | Com o mesmo cliente HTTP e cookie, `VENDEDOR` autenticado solicita repetidamente `/Movimentacoes/Saida`. | Todas as requisições autorizadas respondem sem novo login; o perfil permanece `VENDEDOR` durante a sessão. | Parcial — a autorização de uma requisição está coberta; falta a sequência de requisições. |
| AUT-15 | RF-03 | Com o mesmo cliente HTTP e cookie, `LOJISTA` autenticado navega por `/Produtos`, `/Movimentacoes` e `/Movimentacoes/Saida`. | As rotas permitidas respondem sem novo login; o perfil permanece `LOJISTA` durante a sessão. | Parcial — as rotas individuais estão cobertas; falta a jornada contínua. |
| AUT-16 | RF-03 | `LOJISTA` autenticado envia `POST /Account/Logout` com token antiforgery e depois solicita `GET /Produtos`. | Logout redireciona para `/Account/Login`; a sessão anterior deixa de autenticar o cliente e `/Produtos` volta a redirecionar ao login. | Existente |
| AUT-17 | RF-03 | Com relógio controlado, utilizar cookie após mais de 12 horas de validade. | A sessão expirada é recusada e uma rota protegida redireciona para login. | Planejado |
| AUT-18 | RF-04, RNF-04 | Inspecionar `SenhaHash` persistido para USR-LOJ-01 e USR-VEN-01. | Nenhum valor é a senha em claro; cada valor é um hash bcrypt válido com fator de custo igual ou superior a 12. | Existente |
| AUT-19 | RF-04, RNF-04 | Validar a senha `123` e a senha `senha-incorreta` contra o hash armazenado. | A senha correta é validada; a incorreta é rejeitada; a tentativa não altera o hash persistido. | Existente |

## 4. Referência aos testes automatizados existentes

Os testes abaixo já implementam parte desta especificação no projeto
`tests/SquadEstoque.Web.Tests`. Eles devem ser mantidos como referência; os casos
planejados devem ampliar a suíte sem copiar os mesmos fluxos.

| Arquivo | Testes automatizados | Casos desta especificação |
|---|---|---|
| `AuthenticationAuthorizationTests.cs` | `Login_page_contains_expected_form` | AUT-01 |
| `AuthenticationAuthorizationTests.cs` | `Lojista_can_login_and_access_produtos` | AUT-02 e parte de AUT-10 |
| `AuthenticationAuthorizationTests.cs` | `Lojista_can_access_movimentacoes` | AUT-10 |
| `AuthenticationAuthorizationTests.cs` | `Vendedor_can_login` | AUT-03 e AUT-07 |
| `AuthenticationAuthorizationTests.cs` | `Vendedor_can_access_saida` | AUT-11 |
| `AuthenticationAuthorizationTests.cs` | `Vendedor_is_redirected_to_access_denied_for_produtos` | AUT-12 |
| `AuthenticationAuthorizationTests.cs` | `Invalid_credentials_do_not_authenticate` | AUT-04 e parte de AUT-09 |
| `AuthenticationAuthorizationTests.cs` | `Invalid_login_does_not_render_submitted_password` | AUT-04 |
| `AuthenticationAuthorizationTests.cs` | `Seeded_users_use_bcrypt_with_work_factor_12` | AUT-18 e AUT-19 |
| `AuthenticationAuthorizationTests.cs` | `Logout_clears_authentication_session` | AUT-16 |
| `AuthenticationAuthorizationTests.cs` | `Vendedor_navigation_contains_only_operational_commands_and_logout` | AUT-20 |
| `AuthenticationAuthorizationTests.cs` | `Lojista_navigation_contains_only_administrative_commands_and_logout` | AUT-20 |
| `AdministrativeHttpTests.cs` | `Vendedor_cannot_execute_administrative_post_endpoints` | Parte de AUT-13 e autorização de todos os POSTs administrativos |
| `AdministrativeHttpTests.cs` | `Anonymous_user_cannot_execute_protected_post_endpoints` | Proteção anônima dos POSTs de produto e movimentação |

## 5. Critérios de aceite e observações

- Todos os casos **Existente** devem continuar verdes na integração contínua.
- Casos **Planejado** são necessários para declarar cobertura completa dos requisitos
  indicados; em especial AUT-05, AUT-06 e a parte restante de AUT-13.
- O UC-01 prevê uma entrada correspondente a cada perfil. A implementação direciona
  `VENDEDOR` para `/Estoque/Consulta` e `LOJISTA` para `/Produtos`, sem depender da
  Home provisória no fluxo pós-login.
- O UC-01 usa a grafia `LOGISTA` em seu fluxo, enquanto o requisito, o modelo e a
  implementação usam `LOJISTA`. Nesta especificação, `LOJISTA` é o termo adotado.
