# Checklist de Baseline Técnica

**Data da validação:** 17/08/2026

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
- `/Movies` respondeu `200` como legado mantido
- Login do lojista com `lojista@squad.com` funcionou
- Lojista acessou Produtos
- Lojista acessou Movimentações
- Login do vendedor com `vendedor@squad.com` funcionou
- Vendedor acessou `/Movimentacoes/Saida` com sucesso
- O comportamento de `/Produtos` com o vendedor foi validado conforme a regra atual observada
- Não houve erro na tela durante a validação manual
- Não houve erro no terminal durante a validação manual

## Observações

- Os arquivos locais de banco `Estoque.db` e `LegacyMovie.db` são ignorados pelo Git
- O teste manual completo dos fluxos ainda está pendente
- O legado `Movie` ainda existe e não deve ser removido nesta fase

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
