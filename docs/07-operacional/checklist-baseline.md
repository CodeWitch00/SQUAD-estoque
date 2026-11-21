# Checklist de Baseline Técnica

**Data da validação:** 17/08/2026

> Registro histórico da baseline de agosto de 2026. As permissões observadas
> abaixo não representam o estado atual: a autorização foi corrigida e deve ser
> conferida no código, nos testes HTTP e nos registros de validação posteriores.

## Estrutura validada

- Aplicação em `src/SquadEstoque.Web/`
- Documentação em `docs/`
- Projeto `SquadEstoque.Web.csproj`

## Validações realizadas

- Restore com `mise`
- Build com `mise`
- Execução local
- Rotas GET básicas
- Validação manual mínima de login e permissões em 17/08/2026

## Resultados

- Restore passou
- Build passou com 0 erros e 2 avisos `NU1900`
- Aplicação iniciou em `http://localhost:5186`
- `/` respondeu `200`
- `/Account/Login` respondeu `200`
- `/Produtos` respondeu `302` para login
- `/Movimentacoes` respondeu `302` para login
- Login do lojista com `lojista@squad.com` funcionou
- Lojista acessou Produtos
- Lojista acessou Movimentações
- Login do vendedor com `vendedor@squad.com` funcionou
- Vendedor acessou `/Movimentacoes/Saida` com sucesso
- O comportamento de `/Produtos` com o vendedor foi validado conforme a regra atual observada
- Não houve erro na tela durante a validação manual
- Não houve erro no terminal durante a validação manual

## Observações

- O arquivo local de banco `Estoque.db` é ignorado pelo Git
- Na data desta baseline, o teste manual completo dos fluxos ainda estava pendente.

## Checklist pendente

- [x] Validar login manual de lojista e vendedor
- [x] Validar Produtos autenticado como lojista
- [x] Validar Movimentações autenticado como lojista
- [x] Confirmar acesso do vendedor ao fluxo de saída
- [x] Confirmar comportamento do acesso do vendedor a Produtos
- [x] Confirmar ausência de erros na tela e no terminal durante o teste manual
- [x] Validar permissões básicas por perfil
- [ ] Validar fluxo de entrada, saída e ajuste
- [ ] Validar debug no VS Code
