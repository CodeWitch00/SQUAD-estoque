# Especificação de testes não funcionais - mobile, desempenho e segurança

## 1. Objetivo e escopo

Esta especificação detalha como avaliar os requisitos não funcionais do SQUAD
Estoque relativos a compatibilidade mobile, desempenho e segurança. Ela complementa
o [plano de testes](plano-de-testes.md) e não substitui os testes de autenticação e
perfis já documentados em
[especificacao-testes-autenticacao.md](especificacao-testes-autenticacao.md).

| Área | Requisitos rastreados |
|---|---|
| Mobile e usabilidade | RNF-01, RNF-06 e RNF-07 |
| Desempenho | RNF-02 e RF-13 |
| Segurança | RNF-04, RNF-05 e controles de autenticação relacionados a RF-01 a RF-04 |

Os testes de carga e de navegador real são executados em **homologação**, em base
exclusiva. Nunca usar senhas, cookies, dados de clientes ou inventário de produção
nas evidências.

## 2. Ambientes, massa e evidências

| Área | Ambiente e ferramenta | Evidência mínima |
|---|---|---|
| Mobile | Chrome Android e Safari iOS reais ou em nuvem; Playwright quando adotado | versão do navegador/SO, viewport, captura de tela e resultado de cada jornada |
| Desempenho | Homologação com configuração registrada; k6 ou NBomber | script, massa, data, duração, usuários virtuais, perfil de rede, P50/P95/P99 e taxa de erro |
| Segurança | CI/local para testes de integração; homologação para HTTPS | resultado automatizado, cabeçalhos HTTP e registro sem dados sensíveis |

Massa mínima: USR-LOJ-01, USR-VEN-01, PRD-01 e SKUs definidos no plano de testes.
Para carga, preparar uma massa representativa de consultas: nomes com ao menos dois
caracteres, resultados com 1, 10 e 50 itens e dados suficientes para evitar que todo
o teste seja atendido por uma única página ou cache trivial.

## 3. Casos de teste mobile

Os fluxos dependentes do módulo de consulta do vendedor permanecem planejados até
que essa tela esteja implementada. A responsividade de páginas existentes pode ser
validada desde já.

| ID | Requisitos | Entradas e procedimento | Resultado esperado | Situação |
|---|---|---|---|---|
| NFT-MOB-01 | RNF-01 | Abrir login, login de `VENDEDOR`, consulta de estoque e resultado do atendimento em Chrome Android atual, sem instalar aplicativo. | Toda a jornada funciona no navegador; não exige aplicativo, download ou recurso exclusivo de desktop. | Parcial — login existe; consulta do vendedor é dependente de implementação. |
| NFT-MOB-02 | RNF-01 | Repetir NFT-MOB-01 em Safari iOS atual. | Mesmo fluxo crítico funciona sem erro de layout ou interação. | Dependente do módulo de consulta. |
| NFT-MOB-03 | RNF-07 | Abrir as telas públicas e autenticadas nos viewports `360×800`, `390×844` e `430×932`; usar orientação vertical. | Não há rolagem horizontal, texto cortado, sobreposição, botão fora da área visível ou controle impossível de acionar. | Planejado |
| NFT-MOB-04 | RNF-07 | Em cada viewport, usar login, logout e as ações disponíveis de `LOJISTA` e `VENDEDOR`, incluindo mensagens de validação. | Campos, mensagens e botões permanecem legíveis e utilizáveis sem zoom obrigatório. | Planejado |
| NFT-MOB-05 | RNF-06 | Cinco vendedores sem treinamento recebem a tarefa: após login, localizar um modelo disponível. Registrar toques necessários, tempo e pedidos de ajuda. | Cada participante conclui a primeira consulta em no máximo dois toques após o login, sem ajuda. Falha de qualquer participante exige análise de UX. | Dependente do módulo de consulta. |

## 4. Casos de teste de desempenho

O requisito RNF-02 estabelece dois limites: resposta da consulta de estoque em
**P95 inferior a 500 ms** e resultado visível em até **3 s**, considerando a rede.
Como o SRS não define volume de usuários simultâneos nem massa de produção, a carga
abaixo é uma linha de base reproduzível, não uma estimativa de capacidade final. A
equipe deve registrar e aprovar a carga-alvo antes da liberação.

| ID | Requisitos | Entradas e procedimento | Resultado esperado | Situação |
|---|---|---|---|---|
| NFT-PERF-01 | RNF-02, RF-13 | Em homologação, executar 20 usuários virtuais: aquecimento de 1 min, rampa até 20 em 2 min e carga estável por 5 min. Cada usuário consulta termos válidos com ao menos dois caracteres, alternando resultados de 1, 10 e 50 itens. | Para a requisição de consulta, P95 < 500 ms; erros HTTP ou funcionais ≤ 1%; não há dados incorretos ou falhas não tratadas. | Dependente da rota de consulta do vendedor. |
| NFT-PERF-02 | RNF-02, RF-13 | No navegador mobile, repetir consultas representativas sob perfil de rede móvel documentado (latência e banda registradas), com cache frio na primeira execução. | O resultado fica visível em até 3 s; o tempo é medido do toque no comando de busca à renderização do resultado. | Dependente da rota/tela de consulta. |
| NFT-PERF-03 | RNF-02 | Reexecutar NFT-PERF-01 três vezes no mesmo ambiente e massa, sem outras cargas concorrentes conhecidas. | As três execuções atendem ao P95; relatório informa mediana e pior P95. Resultado isolado não é suficiente para aceite. | Dependente da rota de consulta do vendedor. |

Antes de executar, registrar no relatório: commit testado, CPU/memória da aplicação e
banco, tamanho da base, configuração de cache, usuários virtuais, taxa de chegada,
perfil de rede e intervalo de medição. Alterações nesses parâmetros invalidam a
comparação direta com execuções anteriores.

## 5. Casos de teste de segurança

Estes casos verificam controles diretamente ligados aos requisitos. Eles não
equivalem a uma auditoria completa ou teste de invasão.

| ID | Requisitos | Entradas e procedimento | Resultado esperado | Situação |
|---|---|---|---|---|
| NFT-SEG-01 | RF-04, RNF-04 | Inspecionar `SenhaHash` dos usuários de teste e validar a senha correta e uma incorreta usando BCrypt. | A senha não aparece em texto plano; o hash é bcrypt válido; o fator de custo é ≥ 12; somente a senha correta é aceita. | Existente — `Seeded_users_use_bcrypt_with_work_factor_12`. |
| NFT-SEG-02 | RF-01, RF-02 | Enviar credenciais inválidas, depois solicitar uma rota protegida; executar também tentativa de `VENDEDOR` em `/Produtos`. | Credenciais inválidas não criam sessão; anônimo é redirecionado ao login; `VENDEDOR` é redirecionado a acesso negado e não recebe dados protegidos. | Existente — consultar `AuthenticationAuthorizationTests.cs`; detalhamento em `AUT-04`, `AUT-09` e `AUT-12`. |
| NFT-SEG-03 | RF-01, RF-03 | Enviar `POST /Account/Login` e `POST /Account/Logout` sem token antiforgery ou com token inválido. | A operação é rejeitada; não autentica nem encerra uma sessão válida por requisição forjada. | Planejado |
| NFT-SEG-04 | RF-01 | Autenticar com `ReturnUrl` local (`/Produtos`) e, em execução separada, com URL externa (`https://exemplo.test`). | O destino local é aceito; URL externa é ignorada e o usuário segue ao destino padrão. | Planejado |
| NFT-SEG-05 | RNF-05 | Em homologação/produção, requisitar por HTTP uma rota pública e uma protegida; inspecionar a resposta final e cabeçalhos. | O acesso HTTP é redirecionado para HTTPS; a resposta final usa HTTPS. Quando HSTS estiver habilitado no ambiente publicado, o cabeçalho é enviado. | Planejado — não aprovar apenas pela presença de `UseHttpsRedirection` no código. |
| NFT-SEG-06 | RF-03 | Inspecionar, em HTTPS, o cookie emitido após login e testar acesso após logout. | Cookie não expõe a senha; sessão é invalidada no cliente após logout; atributos de transporte e acesso do cookie atendem à configuração publicada. | Parcial — o encerramento de sessão é automatizado; inspeção de cabeçalhos em HTTPS é pendente. |

## 6. Referências à automação existente e critérios de aceite

O arquivo
[`AuthenticationAuthorizationTests.cs`](../../tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs)
já cobre o formulário de login, credenciais inválidas, autorização de `LOJISTA` e
`VENDEDOR`, rota proibida, política BCrypt e logout. Ele é referência para
NFT-SEG-01, NFT-SEG-02 e parte de NFT-SEG-06, sem duplicar seus cenários.

Para liberar uma versão que altere essas áreas:

- os testes automatizados existentes devem passar na CI;
- NFT-MOB-03 e NFT-MOB-04 devem ser aprovados nos três viewports;
- quando a consulta do vendedor estiver disponível, NFT-PERF-01 a NFT-PERF-03 e
  NFT-MOB-01, NFT-MOB-02 e NFT-MOB-05 deixam de ser dependentes;
- NFT-SEG-01, NFT-SEG-03 a NFT-SEG-05 não podem falhar;
- evidências precisam omitir senhas, hashes completos, cookies e dados reais.
