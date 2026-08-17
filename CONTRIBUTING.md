# Guia de Contribuição — SQUAD Estoque

Este documento estabelece as diretrizes de desenvolvimento, arquitetura, qualidade e fluxo de trabalho para a equipe de desenvolvimento do projeto SQUAD Estoque.

---

## 1. Objetivo

Orientar a equipe sobre como dar continuidade ao desenvolvimento do SQUAD Estoque de forma simples, limpa, sustentável e colaborativa, garantindo consistência técnica, aderência aos requisitos de negócio e entregas frequentes com qualidade.

---

## 2. Princípios de Trabalho

O fluxo de desenvolvimento da equipe é orientado por práticas inspiradas em Extreme Programming (XP):

* Simplicidade antes de complexidade: implementar a solução mais simples que atenda aos requisitos de negócio.
* Entregas pequenas e frequentes: fatiar funcionalidades em incrementos pequenos e integrá-los com regularidade.
* Feedback rápido: validar código, regras e telas o mais cedo possível com testes manuais e revisões.
* Revisão contínua: todo código passa por revisão de pares via Pull Request antes de ser integrado.
* Propriedade coletiva do código: qualquer membro da equipe tem autonomia para melhorar o código existente, desde que respeitadas as diretrizes arquiteturais.
* Comunicação clara: alinhar escopo, regras e decisões técnicas de forma transparente na documentação e nos Pull Requests.
* Melhoria incremental: refatorar continuamente o código em pequenos passos seguros.
* Testes antes de integração: validar manualmente e com rigor todos os fluxos afetados antes de solicitar merge.
* Evitar overengineering: não antecipar abstrações, padrões complexos ou infraestruturas que não sejam estritamente necessárias.

---

## 3. Fluxo de Trabalho com Git

A equipe adota o fluxo baseado em branches curtas integradas à branch principal:

* Não trabalhar diretamente na branch main. A branch main deve permanecer sempre estável e pronta para execução.
* Criar branches com escopo reduzido a partir da main atualizada.
* Manter cada branch focada em uma única entrega de funcionalidade, correção ou ajuste de documentação.
* Abrir Pull Request (PR) assim que o incremento estiver pronto para revisão.
* Integrar com frequência para evitar divergências e conflitos longos.

### Padrão de Nomenclatura de Branches

* feat/nome-da-funcionalidade para novas funcionalidades.
* fix/descricao-da-correcao para correção de bugs.
* docs/descricao-do-ajuste para atualizações em documentação.
* refactor/descricao-da-melhoria para melhorias internas de código sem alteração de comportamento.

Exemplos:
* feat/consulta-rapida-estoque
* feat/venda-rapida-balcao
* feat/registro-ruptura
* fix/validacao-saida-estoque
* docs/ajuste-requisitos
* refactor/limpeza-view-produtos

---

## 4. Padrão de Commits

Os commits devem ser pequenos, atômicos e com mensagens claras, seguindo o padrão de commits convencionais:

* feat: inclusão de nova funcionalidade.
* fix: correção de bug ou inconsistência.
* docs: alterações exclusivamente em documentação.
* refactor: refatoração interna sem mudança de regra funcional.
* chore: ajustes operacionais, dependências ou governança.

Exemplos:
* docs: adicionar guia de contribuicao
* feat: implementar consulta rapida de estoque
* feat: registrar ruptura de SKU indisponivel
* fix: impedir saida com saldo insuficiente
* refactor: simplificar view de produtos

---

## 5. Diretrizes para Pull Requests

Todo Pull Request deve:

* Ter escopo reduzido, facilitando a revisão rápida por outro desenvolvedor.
* Conter descrição clara do que foi implementado ou corrigido.
* Explicar o passo a passo de como testar a alteração localmente.
* Indicar se houve impacto em regras de negócio existentes.
* Indicar explicitamente se houve criação ou alteração de migrations, banco de dados ou autenticação.
* Não misturar desenvolvimento funcional, grandes refatorações estéticas e documentação no mesmo PR sem necessidade.

---

## 6. Diretrizes de Arquitetura

O sistema é construído sobre ASP.NET Core MVC, Entity Framework Core e SQLite. A arquitetura do projeto é deliberadamente direta e enxuta para atender às necessidades do negócio sem complexidade acidental.

O padrão estrutural atual é composto por:
* Controllers MVC comunicando-se diretamente com o EstoqueContext.
* Models representando as entidades de banco de dados.
* ViewModels específicos para agregação e validação de dados de tela.
* Razor Views (.cshtml) para a interface com o usuário.

### Restrições Arquiteturais

Não introduzir no projeto sem discussão e decisão explícita prévia:
* Clean Architecture ou arquitetura em camadas artificiais (Application, Domain, Infrastructure).
* Repository Pattern e Unit of Work (o DbContext e DbSet do EF Core já exercem esses papéis).
* Services obrigatórios para operações CRUD simples.
* DTOs e mapeadores automáticos desnecessários.
* MediatR, CQRS ou mensageria.
* Criação de novos projetos na solução.
* Migração ou substituição do banco SQLite.
* Pacotes pesados de autenticação externa ou ASP.NET Identity completo (utiliza-se autenticação por Cookie nativa com hash seguro via BCrypt).

---

## 7. Banco de Dados e Migrations

* O banco de desenvolvimento é o SQLite (MvcMovie/Estoque.db).
* Arquivos de banco de dados (*.db, *.db-wal, *.db-shm) são locais e não devem ser adicionados ao Git.
* Não criar novas migrations sem necessidade concreta e justificada por alteração nas entidades.
* Toda criação de migration deve ser revisada no código gerado (Designer.cs e migration principal) antes de abrir PR.
* Qualquer operação de alteração de saldo deve garantir estrita consistência entre o campo SaldoAtual da entidade Sku e o histórico na entidade Movimentacao.

---

## 8. Regras de Negócio Críticas

Toda alteração de código deve respeitar rigorosamente as seguintes regras:

1. Produto: representa o modelo comercial de calçado (nome, marca, categoria, cor).
2. SKU: unidade mínima de estoque, formada pela combinação única de Produto + Numeração.
3. Unicidade de SKU: a combinação (ProdutoId, Numeracao) é única e garantida por constraint no banco de dados.
4. Saldo Não Negativo: o campo SaldoAtual de um SKU nunca pode ser menor que zero.
5. Histórico Imutável (Append-Only): a entidade Movimentacao é um livro-razão de entradas, saídas e ajustes. Não deve possuir rotas nem métodos de edição (UPDATE) ou exclusão (DELETE).
6. Validação de Saída: toda operação de saída de estoque deve validar o saldo disponível em transação atômica para impedir inconsistências e concorrência indevida.
7. Justificativa em Ajustes: qualquer ajuste manual de estoque deve conter obrigatoriamente um motivo descritivo informado pelo usuário.
8. Ruptura: representa a demanda não atendida quando um cliente solicita um SKU sem saldo disponível.
9. Desacoplamento da Ruptura: o registro de ruptura é um dado de inteligência comercial e não altera a quantidade física nem o saldo de estoque.

---

## 9. Funcionalidades Pendentes Prioritárias

O próximo incremento funcional prioritário do projeto é o Módulo operacional do vendedor.

### Escopo do Módulo do Vendedor
A ser desenvolvido de forma incremental:
1. Consulta rápida de estoque: busca ágil de produtos por modelo, marca, categoria ou cor, acessível com poucos toques no smartphone ou terminal.
2. Visualização da grade de numerações: listagem de todos os tamanhos de um produto com seus respectivos saldos atuais.
3. Indicação visual de disponibilidade por numeração:
   * Disponível (saldo maior que 1)
   * Último par (saldo igual a 1)
   * Indisponível (saldo zerado)
4. Venda rápida: registro imediato da saída de 1 par com baixa atômica de saldo no SKU correspondente.
5. Registro de ruptura: ação para registrar a falta do produto quando o cliente solicitar uma numeração sem saldo disponível.

### Validação Prévia Antes do Desenvolvimento
Antes de iniciar a implementação deste módulo, a equipe deve validar:
* O fluxo de navegação e layout das telas para telas mobile.
* As regras de autorização por perfil (vendedor autenticado).
* Os critérios de aceite de cada caso de uso (consulte squad/02-requisitos/casos-de-uso.md).
* Se o fluxo de ação na tela será unificado (ex: botões diretos de resultado "Vendeu" / "Não tinha") ou estruturado em etapas.

---

## 10. Fora do Escopo Imediato

Para manter o foco e evitar dispersão de esforço, os seguintes itens não devem ser misturados com o módulo operacional prioritário:
* Relatórios gerenciais avançados ou exportação de dados.
* Dashboards complexos com gráficos e agregações pesadas.
* Análise estatística de rupturas.
* Limpeza completa ou exclusão do domínio legado Movie.
* Renomeação técnica da pasta MvcMovie.
* Alterações ou trocas de arquitetura e framework.
* Troca do motor de banco de dados SQLite.

---

## 11. Qualidade e Testes

Antes de submeter código para revisão, execute o checklist prático de testes manuais:

* Executar dotnet build MvcMovie/MvcMovie.csproj e certificar que não há erros de compilação.
* Testar a tela de login (/Account/Login) com os usuários padrão (lojista@squad.com e vendedor@squad.com).
* Testar o controle de acesso e redirecionamento de rotas protegidas para cada perfil.
* Testar o cadastro, edição e listagem de produtos com grade de numerações (caso a funcionalidade tenha sido alterada).
* Testar registro de entrada de estoque e verificar atualização do saldo.
* Testar registro de saída de estoque e validar o bloqueio contra saldo insuficiente.
* Testar ajuste manual de estoque verificando a obrigatoriedade da justificativa.
* Testar o novo fluxo implementado no cenário de sucesso e nos cenários de erro/validação.
* Verificar se nenhuma regra de negócio existente foi violada.

---

## 12. Checklist Antes de Abrir Pull Request

* [ ] O escopo da branch é pequeno e bem delimitado.
* [ ] A branch foi criada a partir da main atualizada.
* [ ] A compilação (dotnet build) executa com sucesso sem erros.
* [ ] O fluxo foi testado manualmente nos perfis pertinentes (Lojista / Vendedor).
* [ ] As regras de negócio e validações foram verificadas.
* [ ] Não há arquivos de banco local (*.db, *.db-wal, *.db-shm) ou binários (bin/, obj/) incluídos.
* [ ] Não foram incluídas migrações acidentais ou alterações de schema não planejadas.
* [ ] Não foram introduzidas camadas ou abstrações desnecessárias.
* [ ] O README.md ou a documentação na pasta squad/ foram atualizados, se aplicável.

---

## 13. Checklist Antes de Realizar o Merge

* [ ] O Pull Request foi revisado e aprovado por ao menos outro desenvolvedor da equipe.
* [ ] Todos os critérios de aceite da funcionalidade foram atendidos.
* [ ] A compilação está íntegra.
* [ ] Os testes manuais mínimos foram executados com sucesso.
* [ ] Não há alterações fora do escopo do Pull Request.
