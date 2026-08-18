# Guia de Desenvolvimento e Contribuição : SQUAD Estoque

Este é o guia oficial para dar continuidade ao SQUAD Estoque de forma simples, limpa, sustentável e colaborativa. Toda contribuição deve preservar as regras de negócio, a arquitetura existente e a estabilidade da `main`.


## 1. Comece aqui

Antes de desenvolver:

1. leia o cartão escolhido no Trello;
2. confirme objetivo, escopo, critérios de aceite, Sprint e dependências;
3. atribua o cartão a você ao movê-lo para `A Fazer`;
4. consulte o requisito relacionado na [documentação do projeto](docs/);
5. atualize sua `main` local;
6. crie uma branch curta para o cartão;
7. implemente em pequenos passos;
8. compile, teste e revise a alteração;
9. abra um Pull Request;
10. aguarde CI verde e revisão por outro integrante;
11. faça o merge somente depois da aprovação;
12. atualize o cartão e suas evidências.

Não implemente funcionalidades sem cartão compreendido e requisito verificável. Em caso de dúvida ou conflito entre cartão, documentação e código, pare e registre a divergência antes de continuar.

---

## 2. Regras essenciais

- Não trabalhar diretamente na `main`.
- Não fazer merge de código sem Pull Request e revisão de outro integrante.
- Não misturar funcionalidades independentes na mesma branch.
- Não ampliar o escopo do cartão durante a implementação sem alinhamento.
- Não introduzir arquitetura, dependência ou tecnologia nova sem decisão da equipe.
- Não versionar banco local, credenciais, segredos, `bin/`, `obj/` ou arquivos privados.
- Não remover o legado `Movie` nesta fase.
- Não quebrar regras de negócio existentes para acelerar uma entrega.
- Não mover cartão para `Feito` antes do merge e da validação final.
- Manter código, testes, documentação e Trello coerentes com o que foi realmente entregue.

---

## 3. Fluxo oficial: Trello até o merge

### 3.1 Backlog

O cartão ainda não precisa de responsável. Deve possuir, no mínimo:

- objetivo claro;
- escopo delimitado;
- critérios de aceite verificáveis;
- Sprint planejada;
- requisito ou regra de negócio relacionada;
- dependências conhecidas.

### 3.2 A Fazer

Ao escolher o cartão:

- atribua o cartão a você;
- defina como prazo a Daily em que a entrega será apresentada, sem ultrapassar o encerramento da Sprint;
- leia as referências indicadas;
- confirme que as dependências foram resolvidas;
- valide se a tarefa cabe em uma branch e em um Pull Request pequenos;
- combine qualquer dúvida de UX, regra ou arquitetura antes de codificar.

### 3.3 Em execução

Mova o cartão quando o trabalho realmente começar. Durante a execução:

- mantenha a branch focada no cartão;
- faça commits pequenos e coerentes;
- registre impedimentos no cartão;
- adicione evidências e decisões relevantes;
- divida o cartão se ele revelar mais de uma entrega independente;
- sincronize com a `main` com frequência para reduzir conflitos.

### 3.4 Teste

Mova o cartão para `Teste` quando:

- a implementação estiver concluída;
- os critérios de aceite tiverem sido verificados;
- os testes relevantes passarem localmente;
- a validação manual aplicável estiver documentada;
- o Pull Request estiver aberto e pronto para revisão;
- não houver arquivos indevidos ou mudanças fora do escopo.

### 3.5 Feito

Mova o cartão para `Feito` somente quando:

- o Pull Request tiver sido aprovado;
- o GitHub Actions estiver verde;
- o merge na `main` tiver sido concluído;
- a entrega estiver validada na `main`;
- as evidências estiverem vinculadas ao cartão;
- a documentação tiver sido atualizada, quando necessário.

---

## 4. Preparação do ambiente

Para instalação detalhada das ferramentas, configuração no Ubuntu e no Windows, VS Code e solução de problemas, consulte o [Manual de preparação do ambiente](docs/06-operacional/manual-setup-ambiente.md).

Para o passo a passo completo de cartão, branch, commits, Pull Request, revisão e conflitos, consulte o [Guia do fluxo de desenvolvimento XP](docs/06-operacional/guia-fluxo-desenvolvimento-xp.md).
### 4.1 Pré-requisitos

- Git;
- SDK .NET 10;
- navegador atualizado;
- editor com suporte a C#;
- acesso ao repositório e ao Trello da equipe.

O projeto usa:

- ASP.NET Core MVC;
- Entity Framework Core;
- SQLite;
- Razor Views;
- Bootstrap e jQuery;
- autenticação por cookies;
- BCrypt;
- xUnit e `WebApplicationFactory`.

O `mise` é opcional e pode instalar a versão definida no arquivo [mise.toml](src/SquadEstoque.Web/mise.toml). Quem já possui o SDK .NET 10 pode usar diretamente o comando `dotnet`.

### 4.2 Clonar e entrar no projeto

Ubuntu, Windows PowerShell e terminais compatíveis com Git:

```bash
git clone <URL-DO-REPOSITORIO>
cd SQUAD-estoque
dotnet --version
```

A versão exibida deve ser compatível com .NET 10.

### 4.3 Restaurar dependências

Execute na raiz do repositório:

```bash
dotnet restore src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Com `mise` disponível:

```bash
mise --cd src/SquadEstoque.Web exec -- dotnet restore SquadEstoque.Web.csproj
mise --cd src/SquadEstoque.Web exec -- dotnet restore ../../tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

### 4.4 Compilar

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

### 4.5 Executar a aplicação

```bash
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

Endereço local esperado:

```text
http://localhost:5186
```

Se a porta mudar, use o endereço informado no terminal.

### 4.6 Executar os testes

```bash
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

Para reproduzir uma validação próxima ao CI:

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --configuration Release
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-build
```

### 4.7 Ubuntu e Windows

Os comandos `dotnet` acima devem funcionar tanto no Ubuntu quanto no PowerShell do Windows. Ao alterar configuração, scripts ou onboarding:

- não use caminho absoluto da sua máquina;
- use caminhos relativos ao repositório;
- não presuma separador de diretório exclusivo de um sistema;
- valide no outro ambiente quando a mudança puder afetá-lo;
- registre no Pull Request o que foi validado em cada sistema.

As configurações de execução e depuração do VS Code ficam na pasta [.vscode](src/SquadEstoque.Web/.vscode/). Para utilizá-las, abra [src/SquadEstoque.Web](src/SquadEstoque.Web/) como pasta de trabalho no VS Code. Pela raiz do repositório, prefira os comandos de terminal deste guia.

---

## 5. Git, branches e commits

### 5.1 Atualizar a main

Antes de criar a branch:

```bash
git switch main
git pull --ff-only
```

Não inicie uma tarefa a partir de uma branch antiga ou de outra funcionalidade.

### 5.2 Criar uma branch

```bash
git switch -c feat/s1-be-001-felipe
```

Padrão obrigatório:

```text
tipo/sprint-categoria-identificador-integrante
```

Prefixos permitidos:

| Prefixo | Uso |
| --- | --- |
| `feat/` | Nova funcionalidade. |
| `fix/` | Correção de comportamento. |
| `test/` | Inclusão ou melhoria de testes. |
| `docs/` | Alteração somente documental. |
| `refactor/` | Melhoria interna sem mudar comportamento. |
| `chore/` | Manutenção, configuração ou governança técnica. |

Use letras minúsculas, números e hífens. O identificador deve corresponder ao cartão do Trello. Exemplos:

- `feat/s1-be-001-felipe`;
- `feat/s1-fe-002-emmy`;
- `test/s1-qa-003-nicolas`;
- `docs/s1-doc-004-rayana`;
- `fix/s2-be-017-felipe`.

### 5.3 Commits

Faça commits pequenos, atômicos e compreensíveis:

```text
feat: implementar busca de produtos por marca
fix: impedir venda com saldo insuficiente
test: cobrir autorização do vendedor
docs: atualizar fluxo de consulta de estoque
refactor: simplificar view model da grade
chore: ajustar validação do pipeline
```

Não use mensagens vagas como `ajustes`, `alterações`, `final` ou `funcionando`.

---

## 6. Arquitetura que deve ser preservada

A aplicação usa MVC tradicional e deliberadamente simples:

- Controllers MVC acessam diretamente o `EstoqueContext`;
- Models representam as entidades persistidas;
- ViewModels atendem necessidades específicas de tela e validação;
- Razor Views cuidam da apresentação;
- Entity Framework Core realiza a persistência;
- SQLite é o banco oficial do projeto.

Não introduza sem decisão técnica explícita:

- Clean Architecture ou novas camadas `Application`, `Domain` e `Infrastructure`;
- Repository Pattern ou Unit of Work sobre o EF Core;
- Services obrigatórios para CRUD simples;
- DTOs, interfaces ou mapeadores sem necessidade concreta;
- MediatR, CQRS, mensageria ou microserviços;
- ASP.NET Identity completo;
- outro banco de dados;
- novo projeto na solução.

O pacote `Microsoft.EntityFrameworkCore.SqlServer` ainda aparece no projeto, mas SQL Server não faz parte da arquitetura aprovada. Não use esse provedor nem altere o banco oficial sem decisão registrada da equipe.

---

## 7. Regras de negócio obrigatórias

Toda mudança deve preservar:

1. **Produto:** representa o modelo comercial do calçado.
2. **SKU:** representa a combinação de Produto e Numeração.
3. **Unicidade:** `ProdutoId + Numeracao` deve ser único.
4. **Saldo não negativo:** `Sku.SaldoAtual` nunca pode ficar abaixo de zero.
5. **Movimentação imutável:** registros de entrada, saída e ajuste são append-only; não devem possuir edição ou exclusão.
6. **Consistência do saldo:** alterações em `SaldoAtual` devem corresponder a uma `Movimentacao` válida.
7. **Saída segura:** toda saída deve validar o saldo e ocorrer de forma transacional.
8. **Ajuste justificado:** ajuste manual exige motivo.
9. **Ruptura explícita:** ruptura representa demanda não atendida e depende de ação do usuário.
10. **Ruptura desacoplada:** registrar ruptura não altera o saldo físico.
11. **Perfis:** `LOJISTA` possui acesso administrativo e `VENDEDOR` possui acesso operacional limitado.
12. **Senha:** senha persistida deve utilizar hash BCrypt; nunca texto puro.

Antes de modificar uma regra, consulte:

- [Domínio do negócio](docs/01-negocio/dominio.md);
- [Especificação de requisitos](docs/02-requisitos/srs.md);
- [Casos de uso](docs/02-requisitos/casos-de-uso.md);
- [Histórias de usuário](docs/02-requisitos/user-stories.md).

---

## 8. Prioridade funcional atual

O próximo incremento é o Módulo Operacional do Vendedor, desenvolvido em pequenos passos:

1. consulta por modelo, marca, categoria ou cor;
2. visualização da grade e dos saldos por numeração;
3. indicação de `Disponível`, `Último par` e `Indisponível`;
4. venda rápida de um par;
5. registro explícito de ruptura;
6. experiência responsiva para smartphone.

Antes de implementar, confirme o fluxo de navegação, os critérios de aceite e as permissões do perfil `VENDEDOR`.

Ficam fora do escopo imediato:

- dashboards e relatórios gerenciais avançados;
- exportação e análise estatística;
- remoção completa do legado `Movie`;
- grande renomeação estrutural;
- troca de arquitetura, framework ou banco de dados.

---

## 9. Padrões de implementação

### Controllers

- mantenha actions pequenas e legíveis;
- valide entrada e autorização no servidor;
- não confie apenas em validação JavaScript;
- use operações assíncronas do EF Core quando aplicável;
- proteja alterações de saldo com transação adequada;
- não coloque regras de apresentação no Controller.

### Models e ViewModels

- preserve invariantes de negócio e validações;
- use ViewModel quando a tela precisar combinar ou validar dados que não correspondem diretamente a uma entidade;
- não exponha campos sensíveis ou editáveis desnecessariamente;
- evite abstrações sem uso real.

### Razor Views e interface

- não implemente regra de negócio na View;
- use Tag Helpers e validação já adotados pelo projeto;
- mantenha navegação por teclado e textos compreensíveis;
- valide em telas mobile e desktop;
- preserve a identidade visual do projeto;
- apresente mensagens claras para sucesso, ausência de resultado e erro.

### Segurança

- use autenticação por cookies já configurada;
- aplique autorização por perfil nas actions e Controllers;
- não coloque senhas, tokens ou connection strings privadas no código;
- não registre credenciais em logs, cartões ou Pull Requests;
- preserve proteção contra requisições POST forjadas onde aplicável.

---

## 10. Testes e validação

Toda mudança deve ter validação proporcional ao risco.

O projeto segue a pirâmide de testes:

```text
                 Poucos
          Testes de ponta a ponta
        ---------------------------
          Testes de integração
      -------------------------------
       Muitos testes unitários rápidos
```

- mantenha uma base ampla de testes unitários para regras isoláveis;
- use uma quantidade moderada de testes de integração para MVC, autenticação, EF Core e persistência;
- mantenha poucos testes de ponta a ponta, reservados aos fluxos críticos;
- complemente com validação manual de responsividade, identidade visual e usabilidade.

Evite uma pirâmide invertida com muitos testes lentos de interface e pouca cobertura rápida. A estratégia detalhada está no [Guia do fluxo de desenvolvimento XP](docs/06-operacional/guia-fluxo-desenvolvimento-xp.md#6-estratégia-de-testes-em-pirâmide).

### Exigir teste automatizado quando houver

- regra de negócio;
- persistência;
- autenticação ou autorização;
- comportamento de Controller;
- correção de bug reproduzível;
- cenário de sucesso ou falha que possa ser automatizado com valor.

### Usar validação manual documentada quando envolver

- responsividade;
- navegação e usabilidade;
- identidade visual;
- comportamento difícil de automatizar no escopo atual.

Uma tarefa visual não dispensa build e testes de regressão. Uma tarefa documental não exige teste automatizado, mas exige revisão de conteúdo, links, ortografia e coerência.

### Validação manual mínima quando aplicável

- login como lojista e vendedor;
- autorização das rotas afetadas;
- cenário principal da funcionalidade;
- entrada inválida e mensagem apresentada;
- ausência de resultado;
- comportamento em mobile e desktop;
- confirmação de que nenhuma regra existente foi quebrada.

Usuários locais de desenvolvimento:

| Perfil | E-mail | Senha |
| --- | --- | --- |
| Lojista | `lojista@squad.com` | `123` |
| Vendedor | `vendedor@squad.com` | `123` |

Essas credenciais são somente para desenvolvimento local.

---

## 11. Banco de dados e migrations

- O banco oficial é SQLite.
- Arquivos `*.db`, `*.db-wal`, `*.db-shm`, `*.sqlite` e `*.sqlite3` são locais e não devem ser versionados.
- Não crie migration sem alteração real e aprovada das entidades.
- Não edite uma migration já compartilhada para esconder uma nova mudança de schema.
- Revise o arquivo principal, o `Designer.cs` e o snapshot gerados.
- Informe no cartão e no Pull Request qualquer alteração de schema.
- Teste criação e atualização do banco local antes da revisão.
- Nunca use o banco local como evidência única ou fonte de verdade.

Exemplo de criação, somente quando o cartão exigir alteração aprovada de schema:

```bash
dotnet ef migrations add NomeDescritivo --project src/SquadEstoque.Web/SquadEstoque.Web.csproj --context EstoqueContext --output-dir Migrations/Estoque
```

Aplicação local da migration:

```bash
dotnet ef database update --project src/SquadEstoque.Web/SquadEstoque.Web.csproj --context EstoqueContext
```

Se `dotnet ef` não estiver disponível, registre o impedimento e alinhe a instalação da ferramenta. Não improvise alteração manual do banco ou do schema.

---

## 12. Pull Request e revisão por pares

Todo Pull Request deve informar:

- cartão relacionado;
- objetivo da mudança;
- arquivos ou áreas principais alteradas;
- como testar;
- testes automatizados executados e resultado;
- validações manuais realizadas;
- impacto em regra de negócio, autenticação, banco ou migration;
- capturas de tela para alteração visual;
- riscos, limitações ou trabalho futuro conhecido.

### Responsabilidade de quem implementa

- manter o escopo pequeno;
- revisar o próprio diff;
- responder aos comentários;
- corrigir falhas do CI;
- não fazer merge antes da aprovação.

### Responsabilidade de quem revisa

- compreender o cartão e os critérios de aceite;
- verificar regra de negócio e arquitetura;
- avaliar segurança e possíveis regressões;
- conferir testes e instruções de validação;
- solicitar correções de maneira clara;
- aprovar somente quando a entrega estiver pronta.

O autor não deve ser o único revisor da própria entrega.

---

## 13. Definition of Ready

Um cartão está pronto para desenvolvimento quando:

- [ ] possui objetivo e resultado esperado;
- [ ] possui escopo e limites claros;
- [ ] possui critérios de aceite verificáveis;
- [ ] indica Sprint e Daily de entrega;
- [ ] aponta requisito ou regra aplicável;
- [ ] possui dependências identificadas;
- [ ] cabe em uma mudança pequena e revisável;
- [ ] não depende de decisão funcional ou arquitetural pendente;
- [ ] foi atribuído ao integrante que o escolheu.

Se algum item essencial estiver ausente, refine o cartão antes de implementar.

---

## 14. Definition of Done

Uma entrega está concluída quando:

- [ ] os critérios de aceite foram atendidos;
- [ ] a arquitetura e as regras de negócio foram preservadas;
- [ ] o código compila sem erro;
- [ ] os testes automatizados relevantes passam;
- [ ] a validação manual aplicável foi registrada;
- [ ] não há segredos, bancos ou artefatos locais no diff;
- [ ] documentação e requisitos foram atualizados, quando necessário;
- [ ] o Pull Request foi revisado por outro integrante;
- [ ] o GitHub Actions está verde;
- [ ] o merge foi feito na `main`;
- [ ] a entrega foi conferida após o merge;
- [ ] o cartão contém as evidências e foi movido para `Feito`.

---

## 15. Checklist rápido antes de abrir o Pull Request

```bash
git status --short
git diff --check
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

Confirme:

- [ ] branch pequena e relacionada a um cartão;
- [ ] commits claros;
- [ ] diff revisado pelo autor;
- [ ] nenhum arquivo fora do escopo;
- [ ] nenhum banco, segredo, `bin/` ou `obj/` incluído;
- [ ] cenários afetados validados;
- [ ] instruções de teste prontas para o revisor.

---

## 16. Quando parar e pedir alinhamento

Não prossiga por suposição quando:

- o requisito estiver ambíguo;
- documentação e código se contradisserem;
- a tarefa exigir mudança arquitetural;
- for necessário trocar ou adicionar tecnologia relevante;
- a alteração afetar perfis, autenticação ou segurança sem critério definido;
- uma migration não estiver prevista no cartão;
- o cartão crescer além do escopo inicial;
- houver risco de perda de dados;
- a correção exigir remover o legado `Movie`;
- os testes existentes falharem antes da sua mudança.

Registre o impedimento no cartão e leve uma descrição objetiva para a equipe: contexto, evidência, impacto e decisão necessária.

---

## 17. Fontes oficiais do projeto

- Estado e execução: [README.md](README.md);
- Fluxo de contribuição: [CONTRIBUTING.md](CONTRIBUTING.md);
- Domínio: [dominio.md](docs/01-negocio/dominio.md);
- Requisitos: [docs/02-requisitos](docs/02-requisitos/);
- Modelagem: [docs/03-modelagem](docs/03-modelagem/);
- Arquitetura: [arquitetura.md](docs/04-arquitetura/arquitetura.md);
- Diagramas e fluxos: [docs/05-uml](docs/05-uml/);
- Baseline operacional: [checklist-baseline.md](docs/06-operacional/checklist-baseline.md);
- Preparação do ambiente: [manual-setup-ambiente.md](docs/06-operacional/manual-setup-ambiente.md);
- Aplicação: [src/SquadEstoque.Web](src/SquadEstoque.Web/);
- Testes: [tests/SquadEstoque.Web.Tests](tests/SquadEstoque.Web.Tests/);
- Integração contínua: [dotnet.yml](.github/workflows/dotnet.yml).

Quando o comportamento implementado mudar de maneira aprovada, atualize também a documentação afetada no mesmo incremento ou em cartão diretamente vinculado.
