# SQUAD Estoque

Sistema web de controle de estoque para loja de calçados, desenvolvido em **ASP.NET Core MVC** com **Entity Framework Core** e **SQLite**.

---

## 📌 Estado Atual do Produto

### Implementado
* **Domínio de estoque:** modelos de Produto, SKU, Movimentação, Ruptura e Usuário.
* **Persistência de dados:** banco de dados SQLite com controle de schema via Migrations do Entity Framework Core.
* **Autenticação e autorização:** autenticação por cookies nativa do ASP.NET Core com hash seguro de senhas via BCrypt e controle de acesso por perfis (`LOJISTA` e `VENDEDOR`).
* **Catálogo e grades:** cadastro de produtos, geração e controle de grades de SKUs, edição e alternância de status ativo/inativo.
* **Movimentações de estoque:** registro de entradas, saídas com validação de saldo em transação atômica, ajustes manuais com justificativa e histórico de movimentações em modelo append-only.

### Funcionalidades Pendentes Prioritárias
* **Módulo operacional do vendedor:**
  * Consulta rápida de estoque por modelo, marca, categoria ou cor.
  * Visualização da grade de numerações por produto com saldo atual.
  * Indicação visual de disponibilidade por numeração (Disponível, Último par, Indisponível).
  * Venda rápida de 1 par com baixa imediata de estoque.
  * Registro de ocorrência de ruptura quando o SKU solicitado não possuir saldo.

### Pendente de Validação
* Teste manual completo e homologação dos fluxos operacionais de ponta a ponta.
* Validação do fluxo de depuração e execução assistida no VS Code.
* Definição final e alinhamento do fluxo de telas do módulo do vendedor.

---

## 🛠️ Tecnologias Confirmadas

* **Framework Web:** ASP.NET Core MVC (.NET 10 — `net10.0`)
* **Linguagem:** C#
* **ORM:** Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite` 10.0.11)
* **Banco de Dados:** SQLite
* **Camada de Apresentação:** Razor Views (.cshtml)
* **Client-side / UI:** Bootstrap v5.3.3, jQuery v3.7.1, jQuery Validation v1.21.0

---

## 📁 Estrutura Principal do Repositório

```text
estoque/
├── src/SquadEstoque.Web/   # Aplicação ASP.NET Core MVC (código-fonte, views, controllers, migrations e assets)
└── docs/      # Documentação de negócio, requisitos, modelagem, arquitetura e diagramas UML
```

---

## 🚀 Como Executar Localmente

### 1. Pré-requisitos
* `mise` instalado. A versão `10` do SDK do .NET é definida em `src/SquadEstoque.Web/mise.toml`.

### 2. Comandos para Execução

1. Acesse a pasta do repositório:
   ```bash
   cd estoque
   ```

2. Restaure os pacotes de dependências:
   ```bash
   mise --cd src/SquadEstoque.Web exec -- dotnet restore SquadEstoque.Web.csproj
   ```

3. Compile a aplicação:
   ```bash
   mise --cd src/SquadEstoque.Web exec -- dotnet build SquadEstoque.Web.csproj
   ```

4. Execute o projeto:
   ```bash
   mise --cd src/SquadEstoque.Web exec -- dotnet run --project SquadEstoque.Web.csproj
   ```

Se o SDK do .NET 10 já estiver disponível diretamente no `PATH`, os mesmos comandos também podem ser executados com `dotnet`, sem o prefixo do `mise`.

---

## 🌐 Porta e Acesso Local

* **Endereço esperado:** `http://localhost:5186` (conforme configurado em `src/SquadEstoque.Web/Properties/launchSettings.json` e observado em execução local anterior).

---

## 👤 Usuários de Desenvolvimento (Seed Automático)

Na inicialização em ambiente de desenvolvimento, caso a tabela `Usuario` esteja vazia, a aplicação insere automaticamente os seguintes usuários para testes:

| Perfil | E-mail | Senha | Acesso / Permissões |
| :--- | :--- | :--- | :--- |
| **Lojista** | `lojista@squad.com` | `123` | Acesso administrativo: Produtos, Grades de SKUs, Entradas, Saídas, Ajustes Manuais e Histórico. |
| **Vendedor** | `vendedor@squad.com` | `123` | Acesso operacional: Registro de Saídas (baixa de estoque) e futuras consultas rápidas. |

---

## 🗄️ Observações sobre o Banco de Dados

* O banco local utilizado para desenvolvimento é o **SQLite** (`src/SquadEstoque.Web/Estoque.db`).
* Os arquivos de banco de dados (`*.db`, `*.db-wal`, `*.db-shm`) são de uso local e estão configurados no `.gitignore` da raiz para não serem versionados pelo Git.
* A evolução do schema é versionada por meio das Migrations do Entity Framework Core (localizadas em `src/SquadEstoque.Web/Migrations/Estoque/`), em conjunto com a configuração do modelo no código (`src/SquadEstoque.Web/Data/EstoqueContext.cs`).

---

## 📚 Documentação Técnica e de Negócio

A documentação detalhada do projeto está organizada na pasta `docs/`:

* **`docs/01-negocio/`:** Domínio, regras de negócio, personas e glossário do varejo de calçados (`dominio.md`).
* **`docs/02-requisitos/`:** SRS (`srs.md`), Casos de Uso (`casos-de-uso.md`) e Histórias de Usuário (`user-stories.md`).
* **`docs/03-modelagem/`:** Dicionário de dados (`dicionario-de-dados.md`), modelo conceitual, modelo lógico e modelo físico SQLite (`modelo_fisico.md`).
* **`docs/04-arquitetura/`:** Arquitetura de software, decisões arquiteturais e fluxos de execução (`arquitetura.md`).
* **`docs/05-uml/`:** Diagramas UML de classes, casos de uso, DERs, fluxos operacionais e diagramas de sequência.

---

## ⚠️ Diretrizes de Trabalho da Equipe

O desenvolvimento do projeto segue práticas inspiradas em **Extreme Programming (XP)**. Consulte o arquivo [`CONTRIBUTING.md`](CONTRIBUTING.md) para o guia completo de contribuição.

1. **Princípios de Trabalho:**
   * Mudanças pequenas e atômicas.
   * Feedback rápido e validações frequentes.
   * Revisão contínua por pares via Pull Request.
   * Integração frequente com a branch principal.
   * Simplicidade arquitetural e foco no valor de negócio.
   * Testes manuais e validação de regras antes de solicitar merge.
2. **Fluxo de Branches:** Como regra de colaboração da equipe, não realize alterações diretamente na branch `main`. Crie branches curtas por funcionalidade ou correção (`feat/...`, `fix/...`) e submeta Pull Requests.
3. **Respeito aos Requisitos:** Não implemente funcionalidades sem especificação prévia validada na documentação.
4. **Domínio Legado:** Não remova o domínio legado `Movie` neste momento; ele permanece isolado e preservado como referência estrutural.
5. **Contenção Arquitetural:** Mantenha o padrão ASP.NET Core MVC tradicional sem introduzir camadas redundantes (Clean Architecture, Services, Repositories ou DTOs adicionais) sem decisão técnica explicitamente aprovada.
