# Guia de desenvolvimento e contribuição

Este é o único passo a passo obrigatório para desenvolver no SQUAD Estoque.
Todos os integrantes devem seguir os mesmos comandos, regras e critérios. Os
documentos em `docs/` explicam domínio, requisitos, arquitetura, testes e
operação; eles não criam um segundo fluxo de desenvolvimento.

## 1. Fluxo oficial

```text
Preparar o computador
        ↓
Escolher e compreender o cartão
        ↓
Atualizar a main
        ↓
Criar uma branch curta da tarefa
        ↓
Implementar em pequenos passos
        ↓
Compilar, testar e revisar o diff
        ↓
Enviar a branch e abrir Pull Request
        ↓
CI verde + revisão de outro integrante
        ↓
Merge na main
        ↓
Validar a versão integrada e concluir o cartão
```

Não pule etapas. Uma funcionalidade existente apenas na máquina ou na branch do
autor ainda não faz parte do produto integrado.

## 2. Preparar o ambiente

### 2.1 Ferramentas

Todos precisam de:

- Git;
- SDK .NET 10;
- navegador atualizado;
- editor com suporte a C#;
- acesso ao GitHub e ao Trello da equipe.

Instale o **SDK**, não apenas o Runtime do .NET.

#### Ubuntu 24.04 ou versão compatível

```bash
sudo apt-get update
sudo apt-get install -y git dotnet-sdk-10.0
```

Se o pacote não estiver disponível, não adicione um repositório aleatório. Use
as instruções da Microsoft correspondentes à sua versão do Ubuntu:
[Instalar .NET no Ubuntu](https://learn.microsoft.com/dotnet/core/install/linux-ubuntu-install).

#### Windows

Abra o PowerShell e use o WinGet:

```powershell
winget install --id Git.Git --exact
winget install Microsoft.DotNet.SDK.10
```

Feche e abra o terminal depois da instalação. A alternativa é baixar o SDK pelo
[instalador oficial do .NET](https://learn.microsoft.com/dotnet/core/install/windows).

#### Git e GitHub

Configure sua identidade com o nome e o e-mail da sua própria conta:

```bash
git config --global user.name "Seu Nome"
git config --global user.email "seu-email@example.com"
```

Para enviar branches e abrir Pull Requests, instale o
[GitHub CLI](https://cli.github.com/) e autentique sua própria conta:

```bash
gh auth login
gh auth status
```

Escolha `GitHub.com`, protocolo `HTTPS` e permita que o GitHub CLI configure a
autenticação do Git. Não compartilhe token ou sessão entre integrantes.

#### mise opcional

O `mise` é uma alternativa para instalar a versão declarada em
[mise.toml](src/SquadEstoque.Web/mise.toml). Ele não é necessário quando o SDK
.NET 10 já funciona no terminal. Quem optar por ele deve seguir a
[instalação oficial do mise](https://mise.jdx.dev/getting-started), entrar em
`src/SquadEstoque.Web` e executar `mise install`.

Confirme a instalação:

```bash
git --version
dotnet --version
```

O `dotnet --version` deve começar com `10.`. Se usar `mise`:

```bash
cd src/SquadEstoque.Web
mise install
mise exec -- dotnet --version
cd ../..
```

### 2.2 Clonar o projeto

```bash
git clone https://github.com/CodeWitch00/SQUAD-estoque.git
cd SQUAD-estoque
git status --short --branch
```

O primeiro clone deve estar limpo e na `main`.

### 2.3 Restaurar, compilar e testar

Execute sempre a partir da raiz do repositório:

```bash
dotnet restore tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build
```

Esses comandos devem passar antes de iniciar a primeira tarefa. Se falharem em
um clone limpo, registre a versão do .NET, sistema operacional, comando e erro
completo antes de alterar código.

### 2.4 Executar localmente

Ubuntu, macOS e shells compatíveis:

```bash
Demo__SeedUsers=true dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

Windows PowerShell:

```powershell
$env:Demo__SeedUsers="true"
dotnet run --project src/SquadEstoque.Web/SquadEstoque.Web.csproj
```

Acesse normalmente `http://localhost:5186`. Se o terminal informar outra porta,
use a porta exibida.

Em um banco local vazio, a opção `Demo__SeedUsers=true` cria somente os usuários
de desenvolvimento:

| Perfil | E-mail | Senha |
| --- | --- | --- |
| LOJISTA | `lojista@squad.com` | `123` |
| VENDEDOR | `vendedor@squad.com` | `123` |

Essas credenciais são públicas e nunca podem ser usadas no Azure. No PowerShell,
remova a variável ao encerrar a validação:

```powershell
Remove-Item Env:Demo__SeedUsers
```

### 2.5 Evitar “na minha máquina funcionou”

Antes de atribuir um problema ao computador de outra pessoa, compare:

1. commit e branch usados;
2. saída de `dotnet --version`;
3. resultado de `git status --short --branch`;
4. variáveis de ambiente aplicadas;
5. comandos executados a partir da raiz;
6. resultado do build e da suíte completa;
7. existência de migration ou dado local necessário ao cenário.

Não use caminhos absolutos da própria máquina no código ou na documentação. Ao
alterar configuração ou scripts, valide em Ubuntu e Windows quando a mudança
puder depender do sistema operacional.

## 3. Ambientes e bancos

| Ambiente | Finalidade | Banco |
| --- | --- | --- |
| Desenvolvimento local | Implementar e explorar uma tarefa | SQLite local do integrante |
| Testes local/CI | Validar regras, MVC e persistência | SQLite isolado, recriado por execução |
| Azure compartilhado | Homologar a `main` e preparar a demonstração | SQLite persistente do Azure |

Cada integrante possui seu próprio banco local. Arquivos `*.db`, `*.db-wal`,
`*.db-shm`, `*.sqlite` e `*.sqlite3` não são compartilhados nem versionados.

Os testes automatizados não usam o banco local nem o banco do Azure. Eles criam
seus próprios dados e devem ser independentes da ordem de execução.

Os mesmos registros de homologação são acessados pela equipe através da
aplicação publicada no Azure. Nenhum computador deve conectar diretamente ao
arquivo SQLite hospedado.

## 4. Executar uma tarefa

### 4.1 Escolher o cartão

Antes de codificar, confirme no Trello:

- objetivo e resultado esperado;
- escopo e o que fica de fora;
- critérios de aceite;
- Sprint, dependências e requisito relacionado;
- responsável e Daily de entrega.

Mova o cartão para `A Fazer`, atribua-o a você e esclareça dúvidas antes da
implementação. Ao começar, mova-o para `Em execução`.

### 4.2 Atualizar a main e criar a branch

Com a árvore de trabalho limpa:

```bash
git switch main
git pull --ff-only
git switch -c tipo/cartao-descricao-integrante
```

Use uma branch curta por tarefa:

```text
feat/s3-be-001-consulta-rayana
fix/s3-qa-004-saldo-felipe
test/s3-qa-010-autorizacao-emmy
docs/s4-doc-006-runbook-nicolas
```

Tipos permitidos: `feat`, `fix`, `test`, `docs`, `refactor` e `chore`. As
branches históricas `dev/<integrante>` podem continuar no Git, mas novas tarefas
devem usar branches curtas e identificáveis.

### 4.3 Implementar

Trabalhe em pequenos passos e apenas no escopo do cartão:

1. localize o Controller, Model, ViewModel, View e testes relacionados;
2. implemente a menor mudança que entrega um comportamento verificável;
3. compile cedo;
4. acrescente teste proporcional ao risco;
5. valide manualmente quando houver interface;
6. revise o próprio diff.

Evite reformatação, limpeza de legado e refatorações que não pertencem à tarefa.

### 4.4 Validar antes do commit

```bash
git status --short
git diff
git diff --check
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-build
```

Registre no cartão ou Pull Request a rota, o perfil, o cenário e o resultado da
validação manual. Mudanças visuais devem ser conferidas em mobile e desktop.

### 4.5 Criar commits e enviar a branch

Revise os arquivos individualmente antes de adicioná-los:

```bash
git add caminho/do/arquivo
git commit -m "tipo: descrição objetiva"
git push -u origin nome-da-branch
```

Exemplos:

```text
feat: implementar consulta por marca
fix: impedir saída com saldo insuficiente
test: cobrir autorização do vendedor
docs: documentar ambiente compartilhado
```

Não use mensagens como `ajustes`, `final` ou `funcionando`. Não use `git add .`
sem conferir o status.

### 4.6 Abrir o Pull Request

O Pull Request aponta para a `main` e informa:

- cartão e objetivo;
- comportamento alterado;
- como validar;
- build e testes executados;
- validações manuais;
- impacto em banco, migration, autenticação ou autorização;
- capturas quando houver mudança visual;
- riscos e limitações conhecidos.

O autor não aprova o próprio trabalho. Outro integrante confere escopo, regras
de negócio, arquitetura, segurança, testes e regressões.

### 4.7 Integrar e concluir

Somente depois de CI verde e aprovação:

1. faça o merge pelo GitHub;
2. valide a mudança integrada na `main`;
3. vincule Pull Request e evidências ao cartão;
4. mova o cartão para `Feito`;
5. atualize a `main` local antes da próxima tarefa.

Não use force push. Não faça merge direto na `main`.

## 5. Regras de implementação

### 5.1 Arquitetura

O sistema usa MVC tradicional:

- Controllers podem acessar diretamente o `EstoqueContext`;
- Models representam entidades persistidas;
- ViewModels atendem necessidades concretas das telas;
- Razor Views cuidam da apresentação;
- Entity Framework Core persiste em SQLite.

Não introduza sem decisão explícita: Clean Architecture, Repository Pattern,
Unit of Work adicional, CQRS, MediatR, microserviços, camadas artificiais,
interfaces ou DTOs sem necessidade concreta.

### 5.2 Domínio

Toda mudança deve preservar:

- SKU representa Produto + Numeração;
- `ProdutoId + Numeracao` é único;
- saldo não pode ficar negativo;
- entradas, saídas e ajustes geram movimentação coerente;
- movimentações persistidas formam histórico e não são reescritas;
- operações de saldo e histórico devem ser consistentes;
- rupturas seguem a documentação aprovada;
- `LOJISTA` e `VENDEDOR` possuem autorizações distintas.

Consulte [domínio](docs/01-negocio/dominio.md),
[requisitos](docs/02-requisitos/srs.md) e
[casos de uso](docs/02-requisitos/casos-de-uso.md) antes de alterar comportamento.

### 5.3 Segurança

- obtenha identidade e perfil pela autenticação;
- valide autorização e entrada no servidor;
- preserve antiforgery nas operações POST;
- mantenha senhas com BCrypt;
- não registre ou versione segredos;
- não confie em campos sensíveis enviados pelo navegador.

## 6. Testes, banco e migrations

Escreva teste automatizado quando a mudança envolver regra, persistência,
autenticação, autorização, Controller ou correção reproduzível de bug. Use
validação manual documentada para responsividade, navegação e experiência.

O plano completo está em
[docs/09-testing/plano-de-testes.md](docs/09-testing/plano-de-testes.md).

Migrations exigem cartão e alteração de schema aprovados. Não crie, aplique,
remova ou edite migration compartilhada como parte de outra tarefa. Registre no
Pull Request os arquivos gerados, o teste com banco novo e o teste de atualização
de banco existente.

## 7. GitHub Actions

O workflow [.github/workflows/dotnet.yml](.github/workflows/dotnet.yml) executa
restore, build, testes e a verificação de cobertura. Ele não faz merge nem
publica a aplicação.

Se o CI falhar:

1. abra a primeira etapa vermelha;
2. leia o erro completo;
3. reproduza localmente;
4. corrija a causa;
5. envie novo commit e aguarde outra execução.

Não faça merge com CI vermelho.

## 8. Ambiente compartilhado no Azure

O ambiente de homologação e demonstração usa:

```text
App Service: asp-squad-estoque-free
Plano: plan-squad-estoque-f1
Região: North Central US
URL: https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net
```

O recurso gratuito já existe, mas o pacote da aplicação ainda não foi
publicado. Até a primeira publicação validada, a URL exibe a página padrão do
Azure.

Depois da publicação, cada integrante acessará a URL pelo navegador com sua
credencial individual. Todos verão o mesmo banco de homologação. Esse ambiente
serve para validar a versão integrada da `main`, ensaiar os fluxos e preparar a
demonstração.

Somente uma pessoa autorizada publica por vez. Antes de cada publicação:

1. confirme commit da `main` e CI verde;
2. interrompa alterações na massa compartilhada;
3. faça backup consistente do SQLite;
4. publique sem incluir banco ou segredo;
5. valide `/health`, login, perfis e fluxos essenciais;
6. confirme a persistência após reinício e republicação;
7. registre versão, horário, responsável e resultado.

Não teste branches individuais no Azure compartilhado. Não use as credenciais
locais `123`. O CD será configurado somente depois do deploy manual e da
persistência estarem comprovados.

## 9. Critérios comuns

Uma tarefa está pronta para começar quando possui objetivo, escopo, critérios de
aceite, dependências, requisito relacionado e responsável definidos.

Uma entrega está concluída quando:

- critérios de aceite atendidos;
- arquitetura e regras preservadas;
- build e testes aprovados;
- validação manual registrada quando aplicável;
- diff sem banco, segredo ou arquivo fora do escopo;
- documentação atualizada quando necessário;
- Pull Request aprovado e CI verde;
- merge e validação na `main` concluídos;
- cartão atualizado com evidências.

## 10. Onde consultar

| Dúvida | Fonte |
| --- | --- |
| O que o produto faz | [README.md](README.md) |
| Como desenvolver e contribuir | Este `CONTRIBUTING.md` |
| Regra de negócio | [docs/01-negocio/dominio.md](docs/01-negocio/dominio.md) |
| Requisito e caso de uso | [docs/02-requisitos](docs/02-requisitos/) |
| Estrutura de dados | [docs/03-modelagem](docs/03-modelagem/) |
| Arquitetura permitida | [docs/04-arquitetura/arquitetura.md](docs/04-arquitetura/arquitetura.md) |
| Tela e navegação | [docs/05-ux](docs/05-ux/) |
| Docker, baseline e operação | [docs/07-operacional/README.md](docs/07-operacional/README.md) |
| Estratégia de testes | [docs/09-testing/plano-de-testes.md](docs/09-testing/plano-de-testes.md) |
| Pipeline | [.github/workflows/dotnet.yml](.github/workflows/dotnet.yml) |

## 11. Quando parar e pedir alinhamento

Pare antes de implementar quando:

- requisito, cartão e código divergirem;
- o escopo crescer;
- houver mudança de arquitetura ou tecnologia;
- uma migration não estiver prevista;
- existir risco de perda de dados;
- autenticação ou autorização não estiverem claras;
- os testes já falharem antes da alteração;
- for necessário alterar histórico Git, banco compartilhado ou infraestrutura.

Registre: contexto, evidência, impacto e decisão necessária.
