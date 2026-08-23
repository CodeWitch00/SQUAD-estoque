# Manual de Onboarding e Setup do Ambiente de Desenvolvimento : SQUAD Estoque

**Publico-alvo:** Equipe de Desenvolvimento (Rayana, Felipe, Emmy e Nicolas)  
**Objetivo:** Padronizar o ambiente de desenvolvimento local para que toda a equipe execute exatamente a mesma versao do .NET, instale o SQLite e DB Browser, saiba utilizar o terminal, compile sem erros, execute a suite de testes automatizados e suba a aplicacao com seguranca.

---

## 1. Stack Oficial, Versoes e Links Oficiais de Download

Para garantir que todos rodem exatamente o mesmo ambiente e sem inconsistencias, utilize os links oficiais abaixo para cada ferramenta:

| Componente / Tecnologia | Versao Oficial | Link Oficial de Acesso / Download | Finalidade no Projeto |
| :--- | :---: | :---: | :--- |
| **.NET SDK** | **.NET 10.0** (`net10.0`) | [Download .NET 10 SDK (Microsoft)](https://dotnet.microsoft.com/download) | Compilacao e execucao do C# e ASP.NET Core MVC |
| **Banco de Dados (SQLite)** | **SQLite 3** | [Download SQLite Tools](https://www.sqlite.org/download.html) | Motor do banco de dados local (`Estoque.db`) |
| **Interface Visual de Banco** | **DB Browser for SQLite** | [Download DB Browser for SQLite](https://sqlitebrowser.org/dl/) | Visualizacao, inspecao de tabelas e execucao de consultas SQL locais |
| **IDE / Editor Oficial** | **Visual Studio Code** | [Download VS Code](https://code.visualstudio.com/) | Editor de codigo principal |
| **Extensao C# Dev Kit** | Mais recente | [C# Dev Kit no VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) | Suporte completo a solucoes C#, IntelliSense e testes |
| **Extensao SQLite Viewer** | Mais recente | [SQLite Viewer no VS Code](https://marketplace.visualstudio.com/items?itemName=qwtel.sqlite-viewer) | Abrir e inspecionar o arquivo `.db` direto no VS Code |
| **Controle de Versao** | **Git 2.40+** | [Download Git for Windows / Linux](https://git-scm.com/downloads) | Versionamento de codigo e fluxo de branches |
| **Framework Web** | ASP.NET Core MVC 10 | [Documentacao ASP.NET Core](https://learn.microsoft.com/aspnet/core/) | Estrutura MVC (Model-View-Controller) |
| **ORM** | Entity Framework Core 10 | [Documentacao EF Core](https://learn.microsoft.com/ef/core/) | Mapeamento de tabelas e Migrations |
| **Frontend / UI** | Bootstrap 5.3.3 | [Documentacao Bootstrap 5](https://getbootstrap.com/) | Layout responsivo (mobile e desktop) |
| **Testes Automatizados** | xUnit | [Documentacao xUnit](https://xunit.net/) | Suite de testes automatizados |

---

## 2. Como Utilizar o Terminal (Guia Pratico para Iniciantes)

O terminal (ou linha de comando) e a ferramenta onde executamos comandos do Git, compilamos o codigo e rodamos o servidor do sistema.

### 2.1. Como abrir o terminal integrado no VS Code (Metodo Recomendado)
A forma mais simples e abrir o terminal direto dentro do Visual Studio Code, pois ele ja inicia dentro da pasta do projeto:
1. Abra o VS Code na pasta do projeto.
2. Pressione o atalho: `Ctrl + '` (ou `Ctrl + J` / menu superior em **Terminal > Novo Terminal**).
3. Uma janela inferior se abrira com o terminal pronto para uso.

### 2.2. Como abrir o terminal fora do VS Code
* **No Windows (Felipe, Emmy, Nicolas):** Pressione a tecla `Windows`, digite `PowerShell` (ou `cmd`) e pressione `Enter`.
* **No Linux Ubuntu (Rayana):** Pressione o atalho `Ctrl + Alt + T`.

### 2.3. Comandos Basicos de Navegacao no Terminal

| O que voce quer fazer? | Comando no Windows (PowerShell / CMD) | Comando no Linux (Ubuntu) | Exemplo pratico |
| :--- | :--- | :--- | :--- |
| **Ver em qual pasta estou** | `pwd` | `pwd` | Mostra o caminho completo da pasta atual |
| **Listar arquivos da pasta** | `dir` ou `ls` | `ls` (ou `ls -la`) | Mostra tudo o que existe dentro da pasta |
| **Entrar em uma pasta** | `cd nome_da_pasta` | `cd nome_da_pasta` | `cd ESTOQUE` |
| **Voltar para a pasta anterior** | `cd ..` | `cd ..` | Volta um nivel para tras na arvore de pastas |
| **Limpar a tela** | `cls` ou `clear` | `clear` | Limpa as linhas antigas da tela |
| **Interromper um programa em execucao** | `Ctrl + C` | `Ctrl + C` | Encerra a aplicacao web quando ela estiver rodando |

### 2.4. Dicas de Ouro para Nao Errar no Terminal
1. **Autocompletar com TAB:** Nunca digite caminhos compridos letra por letra. Digite o inicio (ex: `cd src/Sq`) e aperte a tecla `Tab`. O terminal completa o nome da pasta automaticamente, evitando erros de digitacao.
2. **Historico de Comandos:** Use as setas para cima (`↑`) e para baixo (`↓`) no teclado para navegar pelos comandos que voce ja digitou antes, sem precisar redigita-los.
3. **Copiar e Colar no Terminal:**
   * No Windows PowerShell: Selecione o texto e clique com o botao direito do mouse para colar (ou use `Ctrl + V`).
   * No Linux Terminal: Use `Ctrl + Shift + V` para colar dentro do terminal.
4. **Espacos em nomes de pastas:** Se o nome da pasta contiver espacos, coloque o caminho entre aspas (ex: `cd "Minha Pasta de Projetos"`).

---

## 3. Instalacao Passo a Passo por Sistema Operacional

---

### Guia para Windows 10 / 11 (Felipe, Emmy e Nicolas)

#### Passo 1: Instalar o Git para Windows
1. Acesse: [Git for Windows (Download)](https://git-scm.com/download/win)
2. Baixe o instalador de 64 bits (`.exe`) e execute.
3. Durante a instalacao:
   * **Editor padrao:** Selecione *Visual Studio Code*.
   * **Ajuste do PATH:** Selecione *Git from the command line and also from 3rd-party software*.
   * **Quebras de linha:** Selecione *Checkout Windows-style, commit Unix-style line endings* (`core.autocrlf = true`).
4. Conclua a instalacao.

#### Passo 2: Instalar o .NET 10 SDK
1. Acesse: [Download .NET SDK (Microsoft)](https://dotnet.microsoft.com/download)
2. Baixe o instalador do **.NET 10.0 SDK (x64)** para Windows.
3. Execute o instalador e avance ate o final.
4. Para validar se a instalacao funcionou, abra o terminal e digite:
   ```powershell
   dotnet --version
   ```
   *(Deve retornar `10.0.xxx` indicando sucesso).*

#### Passo 3: Instalar o DB Browser for SQLite e Utilitarios SQLite
1. Acesse a pagina de downloads: [DB Browser for SQLite (Download)](https://sqlitebrowser.org/dl/)
2. Baixe a versao **Standard installer for 64-bit Windows** (`.msi` ou `.exe`).
3. Instale seguindo o assistente padrao.
4. *(Opcional)* Se desejar os utilitarios de linha de comando do SQLite: acesse [SQLite Download Page](https://www.sqlite.org/download.html), baixe o pacote `sqlite-tools-win-x64-*.zip` e extraia em uma pasta de sua preferencia.

#### Passo 4: Configurar o Visual Studio Code (IDE)
1. Baixe e instale o [Visual Studio Code](https://code.visualstudio.com/).
2. Abra o VS Code e abra o menu de extensoes (`Ctrl + Shift + X`).
3. Instale as seguintes extensoes oficiais e recomendadas:
   * **C# Dev Kit** (Microsoft) — Suporte a solucoes C#, build e testes.
   * **C#** (Microsoft) — Suporte a linguagem C#.
   * **SQLite Viewer** (Florian Klampfer) — Permite visualizar as tabelas do banco `Estoque.db` diretamente no VS Code clicando sobre o arquivo.

*(Quem preferir usar o **Visual Studio 2022 Community**, certifique-se de instalar a carga de trabalho "Desenvolvimento Web e ASP.NET" com suporte ao .NET 10).*

---

### Guia para Linux Ubuntu (Rayana)

#### Passo 1: Instalar o .NET 10 SDK, Git, SQLite3 e DB Browser
Abra o terminal e execute:
```bash
# 1. Atualizar repositorios e instalar .NET 10 SDK, Git e SQLite
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0 git sqlite3 sqlitebrowser
```

#### Passo 2: Validar versoes instaladas
```bash
dotnet --version
git --version
sqlite3 --version
```

---

## 4. Clonando e Executando o Projeto pela Primeira Vez

Com o terminal aberto na pasta onde voce guarda seus projetos (ex: `C:\Projetos` no Windows ou `~/Documentos/projetos` no Linux), execute os comandos a seguir:

```bash
# 1. Clonar o repositorio oficial do SQUAD Estoque
git clone https://github.com/CodeWitch00/ESTOQUE.git

# 2. Entrar na pasta do projeto
cd ESTOQUE

# 3. Restaurar todos os pacotes NuGet da aplicacao
dotnet restore src/SquadEstoque.Web/SquadEstoque.Web.csproj

# 4. Compilar o projeto (deve compilar com 0 erros)
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj

# 5. Executar a suite de testes automatizados
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
```

> **Resultado Esperado dos Testes:** Todos os **22 testes** devem passar com status verde (`Passed! - Failed: 0, Passed: 22`).

---

## 5. Subindo o Servidor Web Local

Para iniciar o sistema no seu navegador:

```bash
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

* O terminal exibira uma mensagem parecida com:
  `Now listening on: http://localhost:5186`
* Abra seu navegador (Chrome, Edge, Firefox) e acesse:
  `http://localhost:5186`
* Para parar o servidor quando terminar de testar, volte ao terminal e pressione `Ctrl + C`.

---

## 6. Como Inspecionar o Banco SQLite com o DB Browser

Quando voce roda a aplicacao pela primeira vez, o arquivo local `Estoque.db` e criado automaticamente em `src/SquadEstoque.Web/Estoque.db`.

Para inspecionar o banco visualmente:
1. Abra o programa **DB Browser for SQLite**.
2. Clique no menu superior em **Abrir Banco de Dados** (`Open Database`).
3. Navegue ate a pasta do projeto: `.../src/SquadEstoque.Web/` e selecione o arquivo **`Estoque.db`**.
4. Na aba **Navegar Dados** (`Browse Data`), voce pode selecionar tabelas como:
   * `Usuario` (para conferir os logins e senhas com hash BCrypt)
   * `Produto` (para ver os modelos de calcados cadastrados)
   * `Sku` (para ver as numeracoes e o saldo de cada calcado)
   * `Movimentacao` (para ver o historico de entradas, saidas e ajustes)
   * `Ruptura` (para ver os registros de demanda nao atendida)

> **Importante:** Sempre que terminar de inspecionar ou executar consultas no DB Browser, certifique-se de nao deixar transacoes travadas para que a aplicacao web possa escrever no banco sem bloqueios.

---

## 7. Usuarios Padrao para Testes Locais (Seed Automatico)

O sistema popula automaticamente o banco local SQLite com dois usuarios de teste para facilitar o desenvolvimento:

| Perfil | E-mail de Acesso | Senha | O que esse usuario pode fazer? |
| :--- | :--- | :---: | :--- |
| **Lojista** *(Administrador)* | `lojista@squad.com` | `123` | Acesso total: cadastrar produtos, gerar grade de tamanhos, dar entrada no estoque, realizar ajustes manuais e visualizar historico. |
| **Vendedor** *(Operacional)* | `vendedor@squad.com` | `123` | Acesso de balcao: realizar saida de calcados, consulta de produtos e registro de rupturas. |

---

## 8. Mapa Rapido da Estrutura de Codigo

Para voce saber exatamente onde mexer:

```text
ESTOQUE/
├── src/SquadEstoque.Web/           # PROJETO PRINCIPAL (ASP.NET CORE MVC)
│   ├── Controllers/               # Logica de controle das rotas e acoes
│   │   ├── AccountController.cs   # Login, Logout e Autenticacao por Cookies
│   │   ├── ProdutosController.cs  # CRUD de Produtos e Grades de SKUs
│   │   ├── MovimentacoesController.cs # Entradas, Saidas e Ajustes de Estoque
│   │   └── HomeController.cs      # Pagina inicial
│   ├── Models/                    # Classes de dados e ViewModels
│   │   ├── Produto.cs, Sku.cs, Movimentacao.cs, Ruptura.cs, Usuario.cs
│   │   └── ...ViewModels.cs       # Modelos de validacao de formularios
│   ├── Views/                     # Telas Razor (.cshtml) e layout HTML/Bootstrap
│   │   ├── Account/ (Login)
│   │   ├── Produtos/ (Listagem, Cadastro, Edicao)
│   │   ├── Movimentacoes/ (Entrada, Saida, Ajuste)
│   │   └── Shared/_Layout.cshtml  # Menu superior, rodape e logo
│   ├── Data/
│   │   └── EstoqueContext.cs      # Conexao e mapeamento com o banco SQLite
│   └── Estoque.db                 # Banco local SQLite (gerado automaticamente)
│
├── tests/SquadEstoque.Web.Tests/  # TESTES AUTOMATIZADOS (xUnit)
│   ├── AuthenticationAuthorizationTests.cs
│   ├── EstoqueDomainPersistenceTests.cs
│   └── BasicRoutesTests.cs
│
└── docs/                          # DOCUMENTACAO OFICIAL DO PROJETO
    ├── 01-negocio/                # Regras de negocio e dominio do calcado
    ├── 02-requisitos/             # SRS e Historias de Usuario
    ├── 03-modelagem/              # Modelos e dicionario de dados
    ├── 04-arquitetura/            # Arquitetura do sistema
    ├── 05-ux/                     # Inventario de telas, fluxos e evidencias
    ├── 06-uml/                    # Diagramas UML
    ├── 07-operacional/            # Manuais e checklists operacionais
    └── 08-monografia/             # Textos e normas da monografia
```

---

## 9. Perguntas Frequentes e Resolucao de Problemas (FAQ)

### 1. "A porta `5186` esta ocupada ou deu erro de bind"
* Feche outros terminais que possam estar rodando o projeto.
* Se necessario, encerre o processo do .NET no Gerenciador de Tarefas do Windows ou reinicie o terminal.

### 2. "Deu erro de permissao ao criar o banco SQLite"
* Certifique-se de que a pasta do projeto nao esta em um diretorio protegido do sistema (ex: dentro de `System32` ou `Program Files`). Prefira salvar em `C:\Projetos` ou na pasta do seu usuario (`C:\Users\SeuNome`).

### 3. "Posso enviar o arquivo `Estoque.db` para o GitHub?"
* **NAO.** O banco local `.db` e seus arquivos temporarios (`.db-wal`, `.db-shm`) ja estao configurados no `.gitignore`. Cada desenvolvedor possui seu proprio banco local gerado automaticamente.

### 4. "Como garanto que nao quebrei nada antes de pedir revisao?"
* Execute sempre no terminal da raiz antes de fazer commit:
  ```bash
  dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
  ```
  Se todos os testes passarem, seu codigo esta seguro para abrir Pull Request!
