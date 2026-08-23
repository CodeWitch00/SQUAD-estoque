# Guia do Fluxo de Desenvolvimento XP : SQUAD Estoque

Este guia mostra como transformar um cartão do Trello em uma entrega integrada à `main`. Ele foi escrito para desenvolvedores iniciantes e deve ser seguido por Rayana, Felipe, Emmy e Nicolas.

As regras resumidas de contribuição estão no [CONTRIBUTING.md](../../CONTRIBUTING.md). A instalação das ferramentas está no [Manual de preparação do ambiente](manual-setup-ambiente.md).

---

## 1. Fluxo completo

```text
Escolher cartão em A Fazer
        |
        v
Atribuir responsável e Daily de entrega
        |
        v
Consultar requisito e arquitetura
        |
        v
Atualizar main e criar branch individual
        |
        v
Implementar em baby steps
        |
        v
Compilar, testar e revisar o próprio diff
        |
        v
Abrir Pull Request
        |
        v
GitHub Actions + revisão por outro integrante
        |
        v
Corrigir, aprovar e fazer merge
        |
        v
Validar main e mover cartão para Feito
```

Não pule etapas. Se houver dúvida de requisito, arquitetura, segurança ou banco de dados, pare antes de implementar.

---

## 2. Onde pesquisar antes de codificar

| Dúvida | Fonte principal |
| --- | --- |
| O que o produto resolve | [README.md](../../README.md) |
| Como preparar Ubuntu ou Windows | [manual-setup-ambiente.md](manual-setup-ambiente.md) |
| Como contribuir | [CONTRIBUTING.md](../../CONTRIBUTING.md) |
| Conceitos e regras do estoque | [dominio.md](../01-negocio/dominio.md) |
| Requisitos funcionais e não funcionais | [srs.md](../02-requisitos/srs.md) |
| Fluxos esperados | [casos-de-uso.md](../02-requisitos/casos-de-uso.md) |
| Valor para cada perfil | [user-stories.md](../02-requisitos/user-stories.md) |
| Estrutura MVC permitida | [arquitetura.md](../04-arquitetura/arquitetura.md) |
| Dados e relacionamentos | [dicionario-de-dados.md](../03-modelagem/dicionario-de-dados.md) |
| Inventário de telas e fluxos de UX | [docs/05-ux](../05-ux/) |
| Diagramas UML | [docs/06-uml](../06-uml/) |
| Estado validado da base | [checklist-baseline.md](checklist-baseline.md) |
| Código da aplicação | [src/SquadEstoque.Web](../../src/SquadEstoque.Web/) |
| Exemplos de testes | [tests/SquadEstoque.Web.Tests](../../tests/SquadEstoque.Web.Tests/) |
| Pipeline automatizado | [dotnet.yml](../../.github/workflows/dotnet.yml) |

Ordem de confiança para o estado atual:

1. código e testes;
2. requisitos e decisões arquiteturais aprovados;
3. README e guias operacionais;
4. cartão do Trello.

Se duas fontes se contradisserem, registre a divergência no cartão e peça alinhamento. Não escolha silenciosamente a versão mais conveniente.

---

## 3. Escolher e preparar o cartão

### No Backlog

O cartão pode permanecer sem responsável. Deve indicar a Sprint, o objetivo, o escopo, os critérios de aceite, as dependências e a origem do requisito.

### Ao mover para A Fazer

O integrante que escolher o cartão deve:

1. atribuir o cartão a si mesmo;
2. definir como prazo a Daily em que pretende apresentar a entrega;
3. confirmar que o prazo não ultrapassa o encerramento da Sprint;
4. ler o requisito e os arquivos relacionados;
5. verificar dependências;
6. confirmar que a tarefa cabe em uma branch pequena;
7. esclarecer dúvidas antes de começar.

### Definition of Ready

Não comece enquanto não puder responder:

- Qual resultado deve ser entregue?
- O que está dentro e fora do escopo?
- Como o resultado será validado?
- Qual regra ou requisito está relacionado?
- Existe dependência pendente?
- Qual é a Daily de entrega?

Se o cartão estiver grande, divida-o antes de implementar. Não use um cartão genérico para desenvolver um módulo inteiro.

---

## 4. Atualizar a main e criar a branch

Execute a partir da raiz do repositório:

```bash
git status --short
git switch main
git pull --ff-only
```

O primeiro comando deve estar limpo. Se houver alterações locais, não as descarte. Identifique a origem antes de trocar de branch.

### Padrão obrigatório

```text
tipo/sprint-categoria-identificador-integrante
```

Exemplos:

```text
feat/s1-be-001-felipe
feat/s1-fe-002-emmy
test/s1-qa-003-nicolas
docs/s1-doc-004-rayana
fix/s2-be-017-felipe
```

Tipos permitidos:

| Tipo | Uso |
| --- | --- |
| `feat` | Funcionalidade nova. |
| `fix` | Correção de comportamento. |
| `test` | Teste ou melhoria de cobertura. |
| `docs` | Documentação. |
| `refactor` | Melhoria interna sem mudar comportamento. |
| `chore` | Configuração ou manutenção técnica. |

Categorias recomendadas:

| Categoria | Uso |
| --- | --- |
| `be` | Backend, Controller, regra ou persistência. |
| `fe` | Razor View, CSS, responsividade ou interação. |
| `qa` | Testes e validações. |
| `doc` | Documentação técnica ou acadêmica. |
| `db` | Entidade, mapeamento ou migration aprovada. |
| `ci` | Pipeline e automação. |
| `ux` | Fluxo e usabilidade. |

Criação da branch:

```bash
git switch -c feat/s1-be-001-felipe
```

Use sempre letras minúsculas, números e hífens. O identificador deve corresponder ao cartão do Trello.

---

## 5. Implementar em baby steps

Baby step é uma alteração pequena que pode ser entendida, testada e revertida com segurança.

Exemplo para uma consulta de estoque:

1. criar ou ajustar o ViewModel;
2. implementar a consulta mínima no Controller;
3. escrever teste da regra ou do comportamento;
4. criar a View com o resultado básico;
5. tratar ausência de resultado;
6. ajustar responsividade;
7. executar regressão.

Não crie novas camadas, Services, Repositories ou DTOs apenas para parecer mais organizado. O projeto usa MVC direto com Controllers acessando o `EstoqueContext`.

Durante o trabalho:

- mova o cartão para `Em execução`;
- registre impedimentos;
- faça uma alteração coerente por vez;
- compile cedo;
- execute os testes afetados com frequência;
- evite reformatação de arquivos não relacionados;
- não misture limpeza ampla com funcionalidade.

---

## 6. Estratégia de testes em pirâmide

O projeto segue a pirâmide de testes:

```text
                 Poucos
          Testes de ponta a ponta
        ---------------------------
          Testes de integração
      -------------------------------
       Muitos testes unitários rápidos
```

### Base: muitos testes unitários

Use para regras isoláveis, validações, estados e cálculos. Devem ser rápidos, determinísticos e independentes de interface, rede ou banco real.

Exemplos:

- classificação `Disponível`, `Último par` e `Indisponível`;
- validação de quantidade;
- obrigatoriedade do motivo de ajuste;
- regras de ViewModels e Models.

### Meio: quantidade moderada de testes de integração

Use para validar a colaboração entre MVC, autenticação, EF Core e persistência. O projeto utiliza xUnit e `WebApplicationFactory`.

Exemplos:

- login e autorização por perfil;
- rotas protegidas;
- persistência de Produto, SKU, Movimentação e Ruptura;
- entrada, saída e ajuste de saldo;
- ruptura sem alteração de estoque;
- concorrência na venda do último par.

### Topo: poucos testes de ponta a ponta

Reserve para fluxos críticos completos. Devem existir em menor quantidade porque são mais lentos, frágeis e caros de manter.

Exemplos:

- vendedor consulta uma numeração e registra venda;
- vendedor registra ruptura quando não há saldo;
- lojista realiza um fluxo administrativo essencial.

### Validação manual complementar

Validação manual não substitui a pirâmide, mas complementa aspectos como:

- responsividade no Ubuntu e Windows;
- uso em smartphone e desktop;
- identidade visual;
- mensagens e usabilidade;
- navegação por teclado.

Evite uma pirâmide invertida composta por muitos testes lentos de interface e poucos testes rápidos. Todo bug corrigido deve receber teste automatizado quando houver uma forma útil e proporcional de reproduzi-lo.

---

## 7. Compilar e testar localmente

Na raiz do repositório:

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Com `mise`:

```bash
mise --cd src/SquadEstoque.Web exec -- dotnet build SquadEstoque.Web.csproj
mise --cd src/SquadEstoque.Web exec -- dotnet test ../../tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Além dos testes automatizados, valide manualmente os fluxos afetados e registre no Pull Request:

- perfil utilizado;
- rota ou tela;
- entrada testada;
- resultado observado;
- ambiente utilizado: Ubuntu ou Windows;
- capturas de tela, quando houver mudança visual.

---

## 8. Commits semânticos

Formato:

```text
tipo: descrição curta no infinitivo
```

Exemplos:

```text
feat: implementar consulta por marca
fix: impedir venda com saldo insuficiente
test: cobrir autorização do vendedor
docs: atualizar fluxo operacional
refactor: simplificar montagem da grade
chore: ajustar pipeline de testes
```

Antes do commit:

```bash
git status --short
git diff
git diff --check
```

Depois:

```bash
git add <arquivos-do-cartao>
git commit -m "feat: implementar consulta por marca"
```

Não use `git add .` sem conferir o status. Não inclua banco local, segredos, `bin/`, `obj/` ou mudanças de outro cartão.

---

## 9. Sincronizar e prevenir conflitos

Antes de abrir o Pull Request, obtenha o estado mais recente da `main`:

```bash
git fetch origin
git merge origin/main
```

Para desenvolvedores iniciantes, o merge explícito é preferível por ser mais fácil de compreender e recuperar. Não use rebase, force push ou comandos destrutivos sem domínio e alinhamento.

Para prevenir conflitos:

- comece sempre da `main` atualizada;
- mantenha branches curtas;
- faça integração frequente;
- evite duas pessoas alterando o mesmo arquivo sem combinar;
- não reformate arquivos inteiros sem necessidade;
- avise a equipe ao modificar migrations, layout compartilhado ou configuração.

---

## 10. Resolver conflitos com segurança

Ao executar o merge, o Git pode indicar os arquivos em conflito.

### Passo 1: identificar

```bash
git status
```

### Passo 2: abrir cada arquivo

O Git marca as versões assim:

```text
<<<<<<< HEAD
conteúdo da sua branch
=======
conteúdo recebido da main
>>>>>>> origin/main
```

### Passo 3: decidir o conteúdo correto

Não escolha automaticamente “aceitar atual” ou “aceitar recebido”. Compare as duas mudanças e preserve o comportamento necessário. Se não compreender o código, peça ajuda ao autor da outra alteração.

### Passo 4: remover os marcadores e validar

```bash
git add <arquivo-resolvido>
git commit
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Para cancelar um merge ainda não concluído:

```bash
git merge --abort
```

Nunca apague arquivos ou use `git reset --hard` para resolver um conflito sem autorização e certeza sobre o impacto.

---

## 11. Enviar a branch e abrir o Pull Request

```bash
git push -u origin feat/s1-be-001-felipe
```

O Pull Request deve apontar para a `main` e informar:

- cartão do Trello;
- objetivo;
- alterações principais;
- requisito atendido;
- como testar;
- testes automatizados executados;
- validações manuais;
- impacto em banco, autenticação ou migration;
- capturas para mudanças visuais;
- limitações ou riscos conhecidos.

Modelo resumido:

```markdown
## Cartão
[S1-BE-001] Título do cartão

## Objetivo
Resultado entregue.

## Como validar
1. Execute ...
2. Acesse ...
3. Confirme ...

## Evidências
- Build: aprovado
- Testes: aprovados
- Validação manual: descrição

## Impactos
- Banco/migration: não
- Autenticação: não
- Documentação: atualizada/não aplicável
```

---

## 12. GitHub Actions

O pipeline [dotnet.yml](../../.github/workflows/dotnet.yml) executa restore, build e testes em Pull Requests para a `main`.

Se o CI falhar:

1. abra a execução no GitHub;
2. identifique a primeira etapa com erro;
3. leia a mensagem completa;
4. reproduza localmente;
5. corrija a causa, não apenas o sintoma;
6. faça novo commit e push;
7. aguarde a nova execução.

Não faça merge com CI vermelho. Avisos de dependência ou vulnerabilidade devem ser registrados e avaliados, mesmo quando não interromperem o pipeline.

---

## 13. Pair Programming e Code Review

### Pair Programming

Use quando a tarefa tiver regra crítica, risco de concorrência, autenticação, migration, conflito difícil ou quando um integrante precisar de apoio.

Papéis:

- Driver: escreve o código e explica o passo atual;
- Navigator: acompanha requisito, riscos, testes e próximos passos.

Troquem os papéis durante a sessão. O cartão continua com um responsável, mas deve registrar quem participou do pareamento.

### Code Review

O revisor deve verificar:

- aderência ao cartão;
- regra de negócio;
- arquitetura MVC;
- segurança e autorização;
- legibilidade e simplicidade;
- cobertura segundo a pirâmide de testes;
- regressões possíveis;
- documentação e instruções de validação;
- ausência de arquivos ou mudanças indevidas.

Comentários devem ser objetivos e explicar o impacto. Diferencie bloqueio obrigatório de sugestão opcional.

O autor deve responder, corrigir e solicitar nova revisão. O autor não aprova a própria entrega.

---

## 14. Merge, limpeza e conclusão do cartão

Depois de aprovação e CI verde:

1. faça o merge pelo GitHub;
2. apague a branch remota quando o trabalho estiver integrado;
3. atualize a `main` local;
4. valide o resultado integrado;
5. adicione ao cartão o link do Pull Request e as evidências;
6. mova o cartão para `Feito`.

Atualização local:

```bash
git switch main
git pull --ff-only
git branch -d feat/s1-be-001-felipe
```

Não mova o cartão para `Feito` apenas porque o código funciona na branch do autor.

---

## 15. Quando parar e consultar a equipe

Pare e peça alinhamento quando:

- o requisito estiver ambíguo;
- código e documentação se contradisserem;
- o cartão crescer além do combinado;
- for necessário alterar arquitetura ou adicionar tecnologia;
- surgir migration não prevista;
- houver risco de perda de dados;
- autenticação ou permissões não estiverem definidas;
- testes anteriores já estiverem falhando;
- a solução exigir remover o legado `Movie`;
- houver conflito que você não compreende.

Registre no cartão:

```text
Contexto:
Evidência:
Impacto:
Decisão necessária:
```

---

## 16. Checklist final do desenvolvedor

- [ ] Cartão atribuído e com Daily definida.
- [ ] Requisito consultado.
- [ ] Branch no padrão oficial.
- [ ] Escopo pequeno e respeitado.
- [ ] Código simples e aderente ao MVC atual.
- [ ] Regras de negócio preservadas.
- [ ] Testes escritos conforme a pirâmide.
- [ ] Build e testes locais aprovados.
- [ ] Validação manual registrada quando aplicável.
- [ ] Commits semânticos e pequenos.
- [ ] Pull Request completo.
- [ ] Revisão por outro integrante.
- [ ] GitHub Actions verde.
- [ ] Merge concluído e validado na `main`.
- [ ] Evidências anexadas ao cartão.
- [ ] Cartão movido para `Feito`.
