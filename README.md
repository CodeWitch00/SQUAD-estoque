# SQUAD Estoque

Sistema web de controle de estoque para loja de calçados, desenvolvido em **ASP.NET Core MVC** com **Entity Framework Core** e **SQLite**.

---

## 📌 Estado Atual do Projeto

| Etapa | Descrição | Status |
| :--- | :--- | :--- |
| **Etapa 1** | Models e EstoqueContext | ✅ Concluída no código |
| **Etapa 2** | Banco SQLite e migration inicial | ✅ Concluída no código |
| **Etapa 3** | Autenticação e perfis (Lojista / Vendedor) | ✅ Concluída no código |
| **Etapa 4** | Catálogo de produtos e SKUs | ✅ Concluída no código |
| **Etapa 5** | Movimentações de estoque (Entrada, Saída, Ajuste) | ✅ Concluída no código |
| **Etapa 6** | Módulo do vendedor (Consulta rápida e Rupturas) | ⏳ Escopo definido; especificação de implementação e desenvolvimento pendentes |

> **Nota sobre validação:**
> - As Etapas 1 a 5 estão implementadas na base de código conforme documentado.
> - A compilação (`dotnet build`) e a inicialização da aplicação foram verificadas localmente com sucesso.
> - O teste manual completo de todos os fluxos de ponta a ponta e a validação de depuração no VS Code permanecem como etapas de homologação a serem realizadas pela equipe.

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
├── MvcMovie/   # Aplicação ASP.NET Core MVC (código-fonte, views, controllers, migrations e assets)
└── squad/      # Documentação de negócio, requisitos, modelagem, arquitetura e diagramas UML
```

---

## 🚀 Como Executar Localmente

### 1. Pré-requisitos
* SDK do **.NET 10** instalado (versão `10` fixada em `MvcMovie/mise.toml`).

### 2. Comandos para Execução

1. Acesse a pasta do repositório:
   ```bash
   cd estoque
   ```

2. Restaure os pacotes de dependências:
   ```bash
   dotnet restore MvcMovie/MvcMovie.csproj
   ```

3. Compile a aplicação:
   ```bash
   dotnet build MvcMovie/MvcMovie.csproj
   ```

4. Execute o projeto:
   ```bash
   dotnet run --project MvcMovie/MvcMovie.csproj
   ```

---

## 🌐 Porta e Acesso Local

* **Endereço esperado:** `http://localhost:5186` (conforme configurado em `MvcMovie/Properties/launchSettings.json` e observado em execução local anterior).

---

## 👤 Usuários de Desenvolvimento (Seed Automático)

Na inicialização em ambiente de desenvolvimento, caso a tabela `Usuario` esteja vazia, a aplicação insere automaticamente os seguintes usuários para testes:

| Perfil | E-mail | Senha | Acesso / Permissões |
| :--- | :--- | :--- | :--- |
| **Lojista** | `lojista@squad.com` | `123` | Acesso administrativo: Produtos, Grades de SKUs, Entradas, Saídas, Ajustes Manuais e Histórico. |
| **Vendedor** | `vendedor@squad.com` | `123` | Acesso operacional: Registro de Saídas (baixa de estoque) e futuras consultas rápidas. |

---

## 🗄️ Observações sobre o Banco de Dados

* O banco local utilizado para desenvolvimento é o **SQLite** (`MvcMovie/Estoque.db`).
* Os arquivos de banco de dados (`*.db`, `*.db-wal`, `*.db-shm`) são de uso local e estão configurados no `.gitignore` da raiz para não serem versionados pelo Git.
* A evolução do schema é versionada por meio das Migrations do Entity Framework Core (localizadas em `MvcMovie/Migrations/Estoque/`), em conjunto com a configuração do modelo no código (`MvcMovie/Data/EstoqueContext.cs`).

---

## 📚 Documentação Técnica e de Negócio

A documentação detalhada do projeto está organizada na pasta `squad/`:

* **`squad/01-negocio/`:** Domínio, regras de negócio, personas e glossário do varejo de calçados (`dominio.md`).
* **`squad/02-requisitos/`:** SRS (`srs.md`), Casos de Uso (`casos-de-uso.md`) e Histórias de Usuário (`user-stories.md`).
* **`squad/03-modelagem/`:** Dicionário de dados (`dicionario-de-dados.md`), modelo conceitual, modelo lógico e modelo físico SQLite (`modelo_fisico.md`).
* **`squad/04-arquitetura/`:** Arquitetura de software, decisões arquiteturais e fluxos de execução (`arquitetura.md`).
* **`squad/05-uml/`:** Diagramas UML de classes, casos de uso, DERs, fluxos operacionais e diagramas de sequência.

---

## ⚠️ Diretrizes de Trabalho da Equipe

1. **Fluxo de Branches:** Recomenda-se como regra de trabalho não realizar alterações diretas na branch `main`. O fluxo de desenvolvimento da equipe deve ocorrer por meio de branches de funcionalidade com revisão via Pull Request (a configuração de regras de proteção no GitHub deve ser feita pela administração do repositório).
2. **Repositório Remoto e Colaboração:** O compartilhamento do repositório definitivo e o controle de acesso para os colaboradores dependem de configuração e convites na plataforma remota (GitHub).
3. **Respeito aos Requisitos:** Não implementar funcionalidades sem especificação prévia validada na documentação.
4. **Domínio Legado:** Não remover o domínio legado `Movie` neste momento; ele permanece isolado e preservado como referência de estrutura.
5. **Contenção Arquitetural:** Manter o padrão MVC tradicional sem introduzir camadas redundantes (Services, Repositories, DTOs adicionais) sem decisão arquitetural aprovada (consulte `squad/04-arquitetura/arquitetura.md`).
6. **Papel dos Desenvolvedores:** O agente não implementa funcionalidades de negócio; a implementação e evolução do sistema serão realizadas pela equipe de desenvolvimento.
