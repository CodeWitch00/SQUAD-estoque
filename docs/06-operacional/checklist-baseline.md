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

## Resultados

- Restore passou
- Build passou com 0 erros e 2 avisos `NU1900`
- Aplicação iniciou em `http://localhost:5186`
- `/` respondeu `200`
- `/Account/Login` respondeu `200`
- `/Produtos` respondeu `302` para login
- `/Movimentacoes` respondeu `302` para login
- `/Movies` respondeu `200` como legado mantido

## Observações

- Os arquivos locais de banco `Estoque.db` e `LegacyMovie.db` são ignorados pelo Git
- O teste manual completo dos fluxos ainda está pendente
- O login manual com `lojista@squad.com` e `vendedor@squad.com` ainda deve ser validado
- O legado `Movie` ainda existe e não deve ser removido nesta fase

## Checklist pendente

- [ ] Validar login manual
- [ ] Validar Produtos autenticado
- [ ] Validar Movimentações autenticado
- [ ] Validar permissões por perfil
- [ ] Validar fluxo de entrada, saída e ajuste
- [ ] Validar debug no VS Code
