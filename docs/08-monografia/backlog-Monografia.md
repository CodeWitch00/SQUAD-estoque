Sprint I — preparar

criar estrutura;
identificar lacunas;
definir evidências;
preparar as seções.

Sprint II — registrar

documentar implementação à medida que ela acontece;
guardar evidências;
registrar decisões.

Sprint III — consolidar

resultados dos testes;
problemas encontrados;
decisões finais;
comparação entre planejado e implementado.

Sprint IV — finalizar

produção;
resultados finais;
conclusão;
revisão;
apresentação.

# Backlog da Monografia

## Objetivo

Organizar as atividades necessárias para complementar a monografia com a implementação real do sistema, testes, resultados, produção e evidências do desenvolvimento.

A documentação técnica já existente não será refeita.

O objetivo deste backlog é registrar e documentar aquilo que somente poderá ser produzido durante ou após a implementação do sistema.

---

# Regra de trabalho

A monografia será desenvolvida **em paralelo ao desenvolvimento do sistema**, mas não fará parte do backlog de desenvolvimento do produto.

As atividades acadêmicas serão organizadas separadamente.

### Regra principal

> Documentar somente aquilo que realmente foi realizado no projeto.

Não antecipar resultados.

Não registrar como implementado algo que ainda não foi desenvolvido.

Não registrar como aprovado um teste que ainda não foi executado.

---

# Estrutura

```text
Desenvolvimento do sistema
        │
        ├── Implementação
        ├── Testes
        ├── Integração
        └── Produção
                 │
                 ↓
        Evidências reais
                 │
                 ↓
        Documentação da monografia
```

A implementação gera informações que posteriormente serão incorporadas à monografia.

---

# Sprint I — Preparação da documentação de implementação

## Objetivo

Preparar a estrutura da monografia que será preenchida com informações reais durante o desenvolvimento.

### MB-001 — Revisar estrutura da monografia

* Revisar capítulos existentes.
* Identificar onde a implementação será acrescentada.
* Identificar quais partes ainda dependem da execução do sistema.
* Não alterar desnecessariamente a documentação técnica já existente.

### MB-002 — Criar seção de implementação

Preparar a seção que posteriormente descreverá:

* arquitetura implementada;
* organização do projeto;
* tecnologias utilizadas;
* estrutura do código;
* integração entre componentes;
* principais decisões de implementação.

Não preencher informações que ainda não foram confirmadas.

### MB-003 — Criar seção de desenvolvimento

Preparar espaço para documentar:

* processo de desenvolvimento;
* aplicação da metodologia XP;
* organização da equipe;
* fluxo de desenvolvimento;
* Git;
* branches;
* Pull Requests;
* Code Review;
* testes.

### MB-004 — Definir padrão de evidências

Definir quais evidências deverão ser guardadas durante o projeto.

Exemplos:

* commits;
* Pull Requests;
* branches;
* cartões do Trello;
* testes;
* cobertura;
* pipeline;
* screenshots;
* sistema funcionando;
* deploy;
* URL de produção, quando aplicável.

### MB-005 — Criar estrutura de evidências

Organizar no repositório uma estrutura para armazenar ou referenciar as evidências.

Exemplo:

```text
/docs/
    monografia/
    evidencias/
        desenvolvimento/
        testes/
        producao/
        git/
        trello/
```

---

# Sprint II — Documentação da implementação

## Objetivo

Documentar aquilo que já foi efetivamente implementado.

### MB-006 — Documentar tecnologias utilizadas

Registrar as tecnologias realmente utilizadas no projeto.

Não utilizar a lista planejada se ela tiver sido alterada.

Registrar a tecnologia efetivamente utilizada.

### MB-007 — Documentar estrutura do sistema

Registrar a estrutura final implementada.

Exemplos:

* backend;
* frontend;
* banco;
* APIs;
* organização dos módulos;
* integração entre componentes.

### MB-008 — Documentar implementação das funcionalidades

Para cada funcionalidade concluída:

* identificar requisito relacionado;
* descrever implementação;
* registrar comportamento;
* registrar decisões relevantes;
* adicionar evidências quando necessário.

### MB-009 — Registrar decisões técnicas

Documentar decisões importantes tomadas durante o desenvolvimento.

Para cada decisão:

```text
Problema
↓
Alternativas consideradas
↓
Decisão
↓
Motivo
↓
Resultado
```

### MB-010 — Registrar aplicação da XP

Documentar como a metodologia foi realmente aplicada.

Exemplos:

* pequenas tarefas;
* desenvolvimento incremental;
* testes;
* integração contínua;
* Code Review;
* pair programming, se utilizado;
* feedback;
* comunicação da equipe.

Não afirmar que uma prática foi utilizada se ela não tiver sido realmente utilizada.

---

# Sprint III — Resultados da implementação e testes

## Objetivo

Consolidar a parte experimental da monografia.

Essa é a etapa em que a implementação já fornece material suficiente para documentar resultados reais.

### MB-011 — Documentar estratégia de testes executada

Registrar:

* tipos de testes realizados;
* casos críticos;
* testes automatizados;
* testes de integração;
* testes E2E, quando realizados;
* testes de concorrência;
* critérios utilizados.

### MB-012 — Documentar resultados dos testes

Registrar os resultados reais.

Para cada grupo de testes:

```text
Teste
↓
Resultado esperado
↓
Resultado obtido
↓
Status
```

### MB-013 — Documentar problemas encontrados

Registrar problemas relevantes identificados durante o desenvolvimento.

Para cada problema:

* descrição;
* causa;
* correção;
* resultado após correção.

### MB-014 — Documentar evolução da implementação

Registrar alterações relevantes entre o planejamento e a implementação final.

Exemplo:

```text
Planejado
↓
Implementado
↓
Alteração realizada
↓
Motivo
```

Isso é importante porque a implementação real pode não ser exatamente igual ao planejamento inicial.

### MB-015 — Inserir evidências técnicas

Adicionar evidências relevantes:

* testes;
* cobertura;
* execução;
* telas;
* API;
* banco;
* pipeline;
* Git.

---

# Sprint IV — Produção e consolidação final

## Objetivo

Finalizar a documentação com o sistema efetivamente concluído.

### MB-016 — Documentar produção

Depois que o sistema estiver realmente publicado:

Registrar:

* ambiente utilizado;
* processo de deploy;
* configuração relevante;
* funcionamento em produção;
* evidências;
* data da publicação.

Não registrar antes do deploy real.

### MB-017 — Documentar validação final

Registrar:

* testes finais;
* validação do sistema;
* principais resultados;
* problemas restantes, se houver;
* situação final do sistema.

### MB-018 — Documentar resultado final do projeto

Consolidar:

* o que foi desenvolvido;
* principais funcionalidades;
* resultados obtidos;
* limitações;
* dificuldades;
* decisões relevantes.

### MB-019 — Atualizar conclusão

A conclusão deve refletir o resultado real do projeto.

Não deve ser escrita apenas com base no que estava planejado.

Deve considerar:

* objetivo inicial;
* sistema desenvolvido;
* resultados;
* limitações;
* experiência obtida.

### MB-020 — Revisar consistência da monografia

Verificar:

* requisitos citados realmente foram implementados;
* funcionalidades descritas realmente existem;
* tecnologias descritas correspondem ao sistema;
* testes descritos foram realmente executados;
* resultados apresentados possuem evidências;
* figuras e tabelas estão atualizadas;
* referências estão corretas;
* não existem informações contraditórias.

### MB-021 — Revisar evidências

Verificar se as principais afirmações da monografia possuem evidências correspondentes.

### MB-022 — Revisão final da monografia

Realizar:

* revisão textual;
* revisão técnica;
* revisão das figuras;
* revisão das tabelas;
* revisão das referências;
* padronização;
* correção de erros.

### MB-023 — Preparar material da apresentação

Separar:

* problema;
* objetivo;
* solução;
* arquitetura;
* funcionalidades;
* demonstração;
* testes;
* produção;
* resultados;
* limitações.

### MB-024 — Preparar demonstração

Validar previamente:

* ambiente;
* sistema;
* banco;
* login;
* funcionalidades principais;
* produção;
* roteiro da demonstração.

---

# Relação entre desenvolvimento e monografia

As atividades não devem ser colocadas no backlog do produto.

A relação será:

| Desenvolvimento            | Documentação                  |
| -------------------------- | ----------------------------- |
| Implementar funcionalidade | Documentar implementação      |
| Executar testes            | Documentar resultados         |
| Corrigir bug               | Documentar problema e solução |
| Fazer Code Review          | Registrar evidência           |
| Fazer deploy               | Documentar produção           |
| Validar sistema            | Documentar resultado final    |

---

# Regra para não duplicar trabalho

A documentação técnica existente permanece como base.

Não reescrever toda a documentação.

A monografia deve receber principalmente:

```text
O que foi planejado
        ↓
O que foi implementado
        ↓
Como foi implementado
        ↓
Como foi testado
        ↓
Quais foram os resultados
        ↓
Como foi colocado em produção
```

---

# Fluxo de atualização

Durante o projeto:

```text
Implementação
     ↓
Registrar evidência
     ↓
Atualizar backlog da monografia
     ↓
Documentar
```

Não deixar tudo para o final.

---

# Critério de conclusão de uma atividade da monografia

Uma atividade documental será considerada concluída quando:

```text
[ ] Informação confirmada
[ ] Fonte/evidência disponível
[ ] Texto produzido
[ ] Revisado tecnicamente
[ ] Inserido na monografia
[ ] Referências/figuras atualizadas quando necessário
```

---

# Critério final da documentação

Antes da entrega:

```text
[ ] Implementação documentada
[ ] Arquitetura final documentada
[ ] Tecnologias utilizadas documentadas
[ ] Metodologia aplicada documentada
[ ] Testes documentados
[ ] Resultados dos testes documentados
[ ] Problemas e correções relevantes documentados
[ ] Produção documentada
[ ] Evidências organizadas
[ ] Conclusão atualizada
[ ] Limitações registradas
[ ] Monografia revisada
[ ] Apresentação preparada
[ ] Demonstração validada
```

---

# Princípio central

> **A monografia deve contar a história real do projeto, e não apenas repetir o planejamento inicial.**

O planejamento mostra o que pretendíamos fazer.

A implementação mostra o que realmente fizemos.

Os testes mostram se funcionou.

A produção mostra que conseguimos colocar o sistema para rodar.

A monografia final deve conectar essas quatro partes.
