# SQUAD Estoque

Sistema web de controle de estoque para lojas de calçados. O produto permite controlar produtos, grades de numeração, saldos por SKU, movimentações e rupturas, com experiências específicas para lojistas e vendedores.

Tecnologias principais: ASP.NET Core MVC, .NET 10, Entity Framework Core, SQLite, Razor Views, Bootstrap, xUnit e GitHub Actions.

---

## 1. Para começar a desenvolver

Siga esta ordem:

1. prepare o Ubuntu ou Windows pelo [Manual de preparação do ambiente](docs/07-operacional/manual-setup-ambiente.md);
2. leia as regras no [Guia de desenvolvimento e contribuição](CONTRIBUTING.md);
3. siga o ciclo prático no [Guia do fluxo de desenvolvimento XP](docs/07-operacional/guia-fluxo-desenvolvimento-xp.md);
4. consulte os [requisitos](docs/02-requisitos/) e a [arquitetura](docs/04-arquitetura/arquitetura.md) antes de alterar o comportamento;
5. escolha um cartão no Trello, atribua-se e defina a Daily de entrega;
6. desenvolva em uma branch individual e abra um Pull Request.

Não trabalhe diretamente na `main`.

---

## 2. Estado atual do produto

### Implementado

- entidades de Usuário, Produto, SKU, Movimentação e Ruptura;
- persistência SQLite com Entity Framework Core e migrations;
- autenticação por cookies e hash BCrypt;
- autorização pelos perfis `LOJISTA` e `VENDEDOR`;
- cadastro e manutenção de produtos e grades de SKU;
- entrada, saída e ajuste manual de estoque;
- histórico append-only de movimentações;
- validação contra saldo negativo;
- testes automatizados de rotas, domínio, persistência, autenticação e autorização;
- CI com build e testes no GitHub Actions.

### Próximo incremento

O foco atual é o Módulo Operacional do Vendedor:

1. consulta por modelo, marca, categoria ou cor;
2. grade de numerações com saldo atual;
3. estados `Disponível`, `Último par` e `Indisponível`;
4. venda rápida de um par;
5. registro de ruptura;
6. fluxo responsivo para smartphone.

### Validações e riscos pendentes

- homologação manual dos fluxos completos;
- definição final do fluxo de telas do vendedor;
- teste de concorrência na venda do último par;
- validação contínua no Ubuntu e Windows;
- revisão de dependências e avisos de vulnerabilidade;
- remoção futura e controlada do legado `Movie`.

---

## 3. Arquitetura

O projeto usa MVC tradicional e simples:

```text
Browser
   |
   v
Controllers MVC
   |          \
   v           v
EstoqueContext  Razor Views
   |
   v
SQLite
```

Estrutura principal:

```text
SQUAD-estoque/
├── .github/workflows/             Pipeline de CI
├── docs/                          Negócio, requisitos, arquitetura e operação
├── src/SquadEstoque.Web/          Aplicação ASP.NET Core MVC
├── tests/SquadEstoque.Web.Tests/  Testes automatizados
├── README.md                      Entrada do projeto
└── CONTRIBUTING.md                Regras oficiais de contribuição
```

Decisões obrigatórias:

- Controllers acessam diretamente o `EstoqueContext`;
- Models representam entidades;
- ViewModels atendem necessidades específicas de tela;
- Razor Views cuidam da apresentação;
- SQLite é o banco oficial;
- Cookie Authentication e BCrypt são mantidos;
- não introduzir Clean Architecture, Repository Pattern, Unit of Work, MediatR ou camadas artificiais.

Detalhes estão no [Documento de arquitetura](docs/04-arquitetura/arquitetura.md).

---

## 4. Execução rápida

### Pré-requisitos

- Git;
- SDK .NET 10;
- navegador atualizado.

O `mise` é opcional. Consulte o [manual completo de ambiente](docs/07-operacional/manual-setup-ambiente.md) para instalação no Ubuntu e Windows.

### Restaurar

Na raiz do repositório:

```bash
dotnet restore src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

### Compilar

```bash
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

### Testar

```bash
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
```

### Executar

```bash
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

Endereço esperado:

```text
http://localhost:5186
```

Se o terminal apresentar outra porta, use o endereço informado durante a inicialização.

---

## 5. Usuários locais de desenvolvimento

Quando a tabela de usuários está vazia no ambiente de desenvolvimento, a aplicação cria:

| Perfil | E-mail | Senha | Acesso |
| --- | --- | --- | --- |
| Lojista | `lojista@squad.com` | `123` | Produtos, grades, entradas, saídas, ajustes e histórico. |
| Vendedor | `vendedor@squad.com` | `123` | Operações autorizadas para o atendimento. |

Essas credenciais são exclusivamente locais e não devem ser usadas em produção.

---

## 6. Regras de negócio essenciais

- Produto representa o modelo comercial do calçado.
- SKU é a combinação única de Produto e Numeração.
- O saldo de um SKU nunca pode ser negativo.
- Movimentações são append-only e não podem ser editadas ou excluídas.
- Toda alteração de saldo deve possuir movimentação correspondente.
- Saídas devem validar saldo e ocorrer de forma transacional.
- Ajustes manuais exigem justificativa.
- Ruptura registra demanda não atendida e não altera o estoque.
- `LOJISTA` possui acesso administrativo.
- `VENDEDOR` possui acesso operacional limitado.
- Senhas devem permanecer armazenadas com hash BCrypt.

Consulte o [domínio](docs/01-negocio/dominio.md) e a [especificação de requisitos](docs/02-requisitos/srs.md) antes de modificar essas regras.

---

## 7. Qualidade e testes

O projeto segue a pirâmide de testes:

```text
                 Poucos
          Testes de ponta a ponta
        ---------------------------
          Testes de integração
      -------------------------------
       Muitos testes unitários rápidos
```

- Base: muitos testes unitários para regras isoláveis.
- Meio: testes de integração para MVC, autenticação, EF Core e persistência.
- Topo: poucos testes de ponta a ponta para fluxos críticos.
- Complemento: validação manual de responsividade, identidade visual e usabilidade.

O projeto possui 22 testes automatizados na baseline atual. O número pode crescer; a referência correta é sempre o resultado de `dotnet test` e do GitHub Actions.

Consulte o [guia XP](docs/07-operacional/guia-fluxo-desenvolvimento-xp.md) para a estratégia completa.

---

## 8. Fluxo resumido de contribuição

```text
Cartão em A Fazer
  -> responsável e Daily
  -> branch individual
  -> baby steps e testes
  -> Pull Request
  -> GitHub Actions
  -> revisão por outro integrante
  -> merge na main
  -> validação integrada
  -> cartão em Feito
```

Padrão de branch:

```text
tipo/sprint-categoria-identificador-integrante
```

Exemplos:

```text
feat/s1-be-001-felipe
feat/s1-fe-002-emmy
test/s1-qa-003-nicolas
docs/s1-doc-004-rayana
```

As instruções completas estão no [CONTRIBUTING.md](CONTRIBUTING.md) e no [guia XP](docs/07-operacional/guia-fluxo-desenvolvimento-xp.md).

---

## 9. Banco de dados local

O banco da aplicação é `src/SquadEstoque.Web/Estoque.db` e não deve ser versionado. Também são ignorados:

```text
*.db
*.db-wal
*.db-shm
*.sqlite
*.sqlite3
```

O schema é versionado pelas migrations em [Migrations/Estoque](src/SquadEstoque.Web/Migrations/Estoque/) e pelo mapeamento no [EstoqueContext.cs](src/SquadEstoque.Web/Data/EstoqueContext.cs).

Não crie migration sem necessidade real, cartão relacionado e revisão do código gerado.

---

## 10. Mapa da documentação

| Assunto | Documento |
| --- | --- |
| Preparação no Ubuntu e Windows | [manual-setup-ambiente.md](docs/07-operacional/manual-setup-ambiente.md) |
| Regras de contribuição | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Fluxo XP, Git, PR e conflitos | [guia-fluxo-desenvolvimento-xp.md](docs/07-operacional/guia-fluxo-desenvolvimento-xp.md) |
| Domínio do estoque | [dominio.md](docs/01-negocio/dominio.md) |
| Requisitos | [docs/02-requisitos](docs/02-requisitos/) |
| SRS | [srs.md](docs/02-requisitos/srs.md) |
| Casos de uso | [casos-de-uso.md](docs/02-requisitos/casos-de-uso.md) |
| Histórias de usuário | [user-stories.md](docs/02-requisitos/user-stories.md) |
| Modelagem de dados | [docs/03-modelagem](docs/03-modelagem/) |
| Arquitetura | [arquitetura.md](docs/04-arquitetura/arquitetura.md) |
| UX, telas e navegação | [docs/05-ux](docs/05-ux/) |
| Diagramas UML | [docs/06-uml](docs/06-uml/) |
| Baseline técnica | [checklist-baseline.md](docs/07-operacional/checklist-baseline.md) |
| Monografia | [docs/08-monografia](docs/08-monografia/) |
| Código da aplicação | [src/SquadEstoque.Web](src/SquadEstoque.Web/) |
| Testes | [tests/SquadEstoque.Web.Tests](tests/SquadEstoque.Web.Tests/) |
| CI | [dotnet.yml](.github/workflows/dotnet.yml) |

---

## 11. Onde pedir alinhamento

Pare antes de codificar quando:

- o requisito estiver ambíguo;
- cartão e documentação se contradisserem;
- for necessária mudança de arquitetura ou tecnologia;
- surgir uma migration não prevista;
- houver risco de perda de dados;
- autenticação ou permissões não estiverem claras;
- o cartão crescer além do escopo combinado;
- os testes existentes já estiverem falhando.

Registre o impedimento no cartão com contexto, evidência, impacto e decisão necessária.
