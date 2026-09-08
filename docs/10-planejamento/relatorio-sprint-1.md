# Relatório curto da Sprint 1 — evidências consolidadas

**Projeto:** SQUAD Estoque  
**Período da Sprint 1:** 05/08/2026 a 09/09/2026  
**Atualização deste registro:** 08/09/2026  
**Finalidade:** apoiar a avaliação da Sprint 1, as dailies e a futura atualização da monografia.

Este documento é um índice de resultados e evidências. Não substitui nem reescreve capítulos da monografia e não contém credenciais, cookies, tokens, hashes ou dados pessoais.

## Resultado consolidado

| Frente | Resultado observado | Situação |
|---|---|---|
| UX e protótipos | Fluxos navegáveis de login, vendedor e lojista; inventário de telas, mapa de navegação e decisões de interface registrados. | Entregue |
| Baseline administrativa | Cadastro e manutenção de produtos, grade, movimentações e histórico submetidos a homologação manual documentada. | Entregue com ressalva registrada na homologação |
| Consulta do vendedor | Busca de produtos ativos por nome, marca, categoria ou cor; validação do termo; estado vazio; seleção e consulta da grade ativa. | Entregue |
| Autenticação e perfis | Acesso anônimo redirecionado, consulta permitida ao `VENDEDOR` e negada ao `LOJISTA`; navegação ajustada por perfil. | Entregue |
| Testes automatizados | Suíte xUnit de domínio, persistência, HTTP, autenticação, autorização, busca e grade. Execução local em 08/09/2026: **60 aprovados, 0 falhas e 0 ignorados**. | Aprovado localmente |
| Integração contínua | Workflow executa restore, build, testes, coleta de cobertura e exige mínimo de 50% de cobertura de linhas. | Configurado; conferir execução remota após o push |

## Evidências organizadas

### Protótipos e UX

- [Login navegável](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/prototipos/login.html)
- [Fluxo do vendedor](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/prototipos/vendedor/prototipo-vendedor.html)
- [Fluxo do lojista](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/prototipos/lojista/prototipo-lojista.html)
- [Inventário de telas e mapa de navegação](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/inventario-telas-e-mapa-navegacao.md)
- [Mapa visual de navegação](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/mapa-navegacao-mvp.svg)
- [Decisões de interface](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/decisoes/decisoes-de-interface.md)
- [Evidências de UX](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/05-ux/evidencias/README.md)

### Planejamento e especificação dos testes

- [Plano geral de testes](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/plano-de-testes.md)
- [Especificação de autenticação, sessão e perfis](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacao-testes-autenticacao.md)
- [Especificação dos testes administrativos](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacao-teste-administrativo.md)
- [Especificação dos testes do vendedor](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacao-testes-vendedor.md)
- [Especificação dos testes não funcionais](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/especificacao-testes-nao-funcionais.md)

### Testes automatizados

- [Autenticação e autorização](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AuthenticationAuthorizationTests.cs)
- [Busca e estados da consulta](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaEstoqueTests.cs)
- [Contrato HTTP da consulta](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EstoqueControllerHttpTests.cs)
- [Grade operacional](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/ConsultaOperacionalHttpTests.cs)
- [Fluxos administrativos HTTP](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/AdministrativeHttpTests.cs)
- [Domínio e persistência](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/EstoqueDomainPersistenceTests.cs)
- [Infraestrutura isolada de integração](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/tests/SquadEstoque.Web.Tests/SquadEstoqueWebApplicationFactory.cs)

Comando reproduzível:

```bash
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

Resultado registrado em 08/09/2026: **60 testes aprovados em aproximadamente 5 segundos; nenhuma falha ou teste ignorado**.

### CI

- [Definição do workflow .NET](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/.github/workflows/dotnet.yml)
- [Execuções no GitHub Actions](https://github.com/CodeWitch00/SQUAD-estoque/actions/workflows/dotnet.yml)

O workflow está configurado para `push` em qualquer branch e `pull_request` direcionado à `main`. O link de Actions é a evidência oficial para confirmar commit, horário, duração, testes e cobertura da execução remota; o resultado local não deve ser apresentado como execução da CI.

### Validações manuais

- [Roteiro administrativo executado](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/roteiro-executado-baseline-administrativa-2026-08-30.md)
- [Homologação da baseline administrativa](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/relatorio-homologacao-baseline-administrativa-2026-08-30.md)
- [Validação responsiva da consulta do vendedor](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/validacao-consulta-vendedor-2026-08-31.md)
- [Validação da navegação por perfil](https://github.com/CodeWitch00/SQUAD-estoque/blob/main/docs/09-testing/validacao-navegacao-por-perfil-2026-09-03.md)

As validações são datadas e representam o escopo existente no momento de cada execução. A validação responsiva de 31/08 registra uma limitação do navegador headless e recomenda conferência visual final em navegador gráfico.

## Registro para dailies

Para as dailies, este relatório pode ser usado no formato “feito, evidência, pendência”:

- **Feito:** protótipos, baseline administrativa, consulta operacional, grade, perfis, automação e workflow de CI.
- **Evidência:** links das seções anteriores e resultado local da suíte.
- **Pendências de conferência:** execução remota mais recente da CI e validação visual final em navegador/dispositivo gráfico.

O calendário e a organização das reuniões estão em [Dailies da Sprint](Dailies-Sprint.md).

## Referência para futura atualização da monografia

Na próxima revisão da monografia, usar este arquivo apenas como índice de evidências para atualizar resultados, metodologia de testes e validação do incremento. Confirmar datas e resultados novamente antes da redação final. Os documentos acadêmicos permanecem separados em [docs/08-monografia](https://github.com/CodeWitch00/SQUAD-estoque/tree/main/docs/08-monografia).

## Limites deste relatório

- Não comprova uma execução remota da CI sem o respectivo job no GitHub Actions.
- Não marca como concluídos fluxos não demonstrados pelas evidências listadas.
- Não inclui massa do banco local nem informações de acesso.
- Não substitui os planos, roteiros, relatórios detalhados ou documentos da monografia.
