# Aplicação web

Esta pasta contém a aplicação ASP.NET Core MVC do SQUAD Estoque.

O fluxo oficial de preparação, branches, implementação, testes e Pull Requests
está no [CONTRIBUTING.md](../../CONTRIBUTING.md). Não mantenha instruções de
contribuição diferentes neste arquivo.

## Estrutura

```text
Controllers/   actions MVC e coordenação das requisições
Data/          contextos do Entity Framework Core
Migrations/    histórico versionado de schema
Models/        entidades e ViewModels usados pela aplicação
Views/         páginas Razor
wwwroot/       CSS, JavaScript, imagens e bibliotecas da interface
Program.cs     composição e pipeline HTTP
```

## Comandos a partir da raiz

```bash
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

O arquivo [mise.toml](mise.toml) declara .NET 10 para quem usa o `mise`. O uso
do gerenciador é opcional quando o SDK compatível já está instalado.

## Dados locais

As connection strings padrão usam SQLite local. Bancos e arquivos auxiliares
`*.db`, `*.db-wal` e `*.db-shm` não devem ser versionados.

Para criar os usuários demonstrativos em um banco local vazio, siga a seção
[Executar localmente](../../CONTRIBUTING.md#24-executar-localmente). Nunca use
essas credenciais no ambiente publicado.
