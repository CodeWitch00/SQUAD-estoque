# Relatório  do primeiro LOJISTA

- **Escopo:** cadastro inicial de LOJISTA em um SQLite vazio, sem senha demonstrativa.

## Resultado comprovado

| Verificação automatizada                             | Resultado                                                                                   |
| ------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| Comando `--bootstrap-lojista` fora de `Production` | Recusado; não cria banco em `Development`.                                               |
| Senha curta em `Production`                          | Recusada.                                                                                   |
| Primeiro cadastro em banco vazio                       | Cria exatamente um usuário com perfil `LOJISTA`; a senha é armazenada como hash BCrypt. |
| Saída do comando                                      | Não contém a senha nem o hash; confirma `Primeiro LOJISTA criado.`                      |
| Segunda execução no mesmo banco                      | Recusada porque já existe usuário.                                                        |

Os testes usam SQLite temporário e não alteram o banco do Azure. A senha de teste
é fictícia. O código exige senha entre 12 caracteres e 72 bytes UTF-8, recebida
pela entrada padrão, sem argumento ou variável de ambiente.

## Execução reproduzível

Na raiz do repositório, com .NET 10 instalado:

```bash
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj \
  --filter 'FullyQualifiedName~PrimeiroLojistaBootstrapTests' \
  --logger 'console;verbosity=quiet'
```

**Resultado :** 2 aprovados, 0 falhos, 0 ignorados. A suíte de
inicialização por ambiente também passou: 8 testes específicos aprovados no
total. O [workflow da `main`](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/42896094609)
concluiu build, testes e deploy no Azure com sucesso.

## Procedimento para o ambiente alvo

O passo a passo seguro para executar o cadastro no App Service está em
[primeiro-acesso-lojista.md](../07-operacional/primeiro-acesso-lojista.md).
Antes da execução, confirme `ASPNETCORE_ENVIRONMENT=Production`, o banco SQLite
persistente correto e a ausência de usuários. A senha deve ser digitada sem
eco no terminal; não a inclua em capturas, logs ou no Trello. Após o cadastro,
confirme o login do novo LOJISTA e o acesso às páginas permitidas.

**Limite da evidência:** o cadastro real no banco do Azure não foi executado nem
comprovado neste relatório. Se o banco já possuir usuários, o comando recusa o
cadastro. A mudança não remove contas demonstrativas antigas: os logins
`lojista@squad.com` e `vendedor@squad.com` ainda podem funcionar caso essas
contas já existam no banco.
