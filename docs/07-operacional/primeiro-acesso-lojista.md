# S4-BE-003 — Primeiro acesso do LOJISTA

Em `Production`, a aplicação aplica migrations na inicialização, mas nunca
cria contas demonstrativas, mesmo se a configuração legada
`Demo__SeedUsers=true` estiver presente. O comando `--bootstrap-lojista` cria
**um único LOJISTA** somente quando a tabela `Usuario` está vazia. Ele não
inicia o servidor HTTP. Em `Development`, `Demo__SeedUsers=true` continua
disponível para a massa local; em `Testing`, os usuários são criados apenas
pela fábrica de testes em SQLite isolado.

## Preparação

1. Confirme que o comando aponta para o banco persistente pretendido e que a
   aplicação publicada já contém esta versão. O operador precisa ter acesso de
   escrita ao banco e permissão para executar o aplicativo.
2. Escolha nome e e-mail do primeiro LOJISTA. Guarde uma senha única com pelo
   menos 12 caracteres e no máximo 72 bytes UTF-8 em um gerenciador de senhas.
   O comando não aceita senha em argumento ou variável de ambiente; lê uma
   linha da entrada padrão.
3. Use um shell interativo sem rastreamento de comandos (`set +x`). A leitura
   silenciosa abaixo evita mostrar a senha no terminal. Não grave a senha em
   script, histórico, logs, capturas de tela ou cartão de tarefa.

## Azure App Service

No Portal Azure, abra o App Service → **SSH** com a aplicação em execução.
O contêiner deve receber `ASPNETCORE_ENVIRONMENT=Production` e a connection
string do SQLite persistido pelas configurações do App Service. Execute no
shell Bash do contêiner:

```bash
set +x
read -r -p 'Nome do primeiro LOJISTA: ' primeiro_nome
read -r -p 'E-mail do primeiro LOJISTA: ' primeiro_email
read -r -s -p 'Senha (min. 12 caracteres, max. 72 bytes): ' primeiro_senha
printf '\n'
printf '%s\n' "$primeiro_senha" | dotnet /home/site/wwwroot/SquadEstoque.Web.dll \
  --bootstrap-lojista "$primeiro_nome" "$primeiro_email"
unset primeiro_senha
```

A saída de sucesso é `Primeiro LOJISTA criado.`. A senha só atravessa o pipe
para a entrada padrão do processo e o banco recebe o hash BCrypt. O nome e
o e-mail aparecem nos argumentos do processo; não os trate como segredos.

## Compose local com volume persistente

Após construir a imagem, pare o serviço para evitar duas inicializações ao
mesmo tempo. O contêiner temporário usa o mesmo volume e as configurações de
`compose.yaml`:

```bash
docker compose stop squad-estoque
set +x
read -r -p 'Nome do primeiro LOJISTA: ' primeiro_nome
read -r -p 'E-mail do primeiro LOJISTA: ' primeiro_email
read -r -s -p 'Senha (min. 12 caracteres, max. 72 bytes): ' primeiro_senha
printf '\n'
printf '%s\n' "$primeiro_senha" | docker compose run --rm -T --no-deps \
  --entrypoint dotnet squad-estoque SquadEstoque.Web.dll \
  --bootstrap-lojista "$primeiro_nome" "$primeiro_email"
unset primeiro_senha
docker compose start squad-estoque
```

## Conferência e limites

Confirme `/health`, faça login com o LOJISTA e acesse Produtos. Depois,
verifique que as rotas exclusivas de VENDEDOR continuam restritas. O comando
recusa senha curta, ambiente diferente de `Production` e qualquer banco que já
possua usuários. Repeti-lo não cria outra conta. Se o banco já contém contas
demonstrativas antigas, este procedimento **não** as remove nem altera senhas;
a equipe deve avaliar e corrigir esses registros em uma tarefa própria antes
de usar o ambiente com dados reais.
