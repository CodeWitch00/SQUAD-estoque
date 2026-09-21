# SQUAD Estoque

Sistema web acadêmico para controle de estoque de uma loja de calçados. O
produto controla produtos, grades de numeração, saldos por SKU, movimentações e
rupturas, com acessos distintos para `LOJISTA` e `VENDEDOR`.

Tecnologias: ASP.NET Core MVC, .NET 10, Razor Views, Entity Framework Core,
SQLite, Bootstrap, autenticação por cookies, xUnit e GitHub Actions.

## Comece por aqui

Existe um único fluxo obrigatório para preparar o computador, escolher uma
tarefa, implementar, testar e abrir Pull Request:

> [CONTRIBUTING.md — Guia de desenvolvimento e contribuição](CONTRIBUTING.md)

Não trabalhe diretamente na `main`. Não versione bancos, senhas, tokens,
arquivos `bin/`, `obj/` ou configurações privadas.

## Execução rápida

Pré-requisitos:

- Git;
- SDK .NET 10;
- navegador atualizado.

Na raiz do repositório:

```bash
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

A aplicação local usa normalmente `http://localhost:5186`. Se o terminal
informar outra porta, use o endereço exibido por ele.

Para criar os usuários demonstrativos em um banco local vazio, consulte a seção
[Executar localmente](CONTRIBUTING.md#24-executar-localmente). As credenciais
demonstrativas nunca devem ser usadas no Azure.

## Ambientes

| Ambiente | Uso | Banco |
| --- | --- | --- |
| Computador do integrante | Implementação e validação manual | SQLite local e individual |
| Testes locais e GitHub Actions | Testes automatizados | SQLite isolado e recriado por execução |
| Azure compartilhado | Homologação da versão integrada e demonstração | SQLite persistente do Azure |

Os desenvolvedores compartilham os registros de homologação acessando a mesma
aplicação no Azure pelo navegador. Nenhum computador deve abrir diretamente o
arquivo SQLite hospedado.

O App Service gratuito já foi provisionado, mas o pacote do SQUAD Estoque ainda
não foi publicado. O estado e as regras de uso do ambiente estão no
[CONTRIBUTING.md](CONTRIBUTING.md#8-ambiente-compartilhado-no-azure).

## Arquitetura preservada

O projeto usa ASP.NET Core MVC tradicional em uma única aplicação:

```text
Navegador
    |
    v
Controllers MVC ----> Razor Views
    |
    v
EstoqueContext
    |
    v
SQLite
```

Controllers podem acessar diretamente o `EstoqueContext`. Não introduza Clean
Architecture, Repository Pattern, Unit of Work adicional, CQRS, MediatR,
microserviços ou camadas artificiais sem decisão explícita da equipe.

As regras centrais são:

- SKU representa Produto + Numeração;
- o saldo nunca pode ficar negativo;
- alterações de saldo geram movimentações históricas;
- registros históricos persistidos não são reescritos;
- autorização respeita os perfis `LOJISTA` e `VENDEDOR`;
- ruptura segue o comportamento aprovado nos requisitos.

## Onde consultar

| Necessidade | Fonte |
| --- | --- |
| Preparar ambiente e contribuir | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Domínio e regras de negócio | [docs/01-negocio/dominio.md](docs/01-negocio/dominio.md) |
| Requisitos e casos de uso | [docs/02-requisitos](docs/02-requisitos/) |
| Dados e relacionamentos | [docs/03-modelagem](docs/03-modelagem/) |
| Arquitetura | [docs/04-arquitetura/arquitetura.md](docs/04-arquitetura/arquitetura.md) |
| Telas e experiência | [docs/05-ux](docs/05-ux/) |
| Diagramas | [docs/06-uml](docs/06-uml/) |
| Docker, baseline e operação | [docs/07-operacional/README.md](docs/07-operacional/README.md) |
| Estratégia e execução de testes | [docs/09-testing/plano-de-testes.md](docs/09-testing/plano-de-testes.md) |
| Planejamento público | [docs/10-planejamento](docs/10-planejamento/) |
| Pipeline de CI | [.github/workflows/dotnet.yml](.github/workflows/dotnet.yml) |

Código e testes representam o comportamento real da implementação. A
documentação define domínio, requisitos e decisões; o planejamento define o
escopo de cada entrega.
