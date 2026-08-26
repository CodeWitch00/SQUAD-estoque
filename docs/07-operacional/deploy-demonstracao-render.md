# Publicação para demonstração no Render

Este procedimento publica o Squad Estoque como uma demonstração pública. Ele não
representa a arquitetura recomendada para produção real, pois utiliza bancos SQLite
no sistema de arquivos temporário do serviço.

## Pré-requisitos

- repositório hospedado no GitHub;
- conta no Render conectada à conta do GitHub;
- alterações de deploy presentes na branch `main`.

## Arquivos usados no deploy

- `Dockerfile`: compila e executa a aplicação ASP.NET Core;
- `.dockerignore`: reduz o conteúdo enviado para a construção da imagem;
- `render.yaml`: configura o serviço, as variáveis e a verificação de saúde;
- `Program.cs`: aplica as migrações e, somente quando habilitado, cria os usuários
  de demonstração.

## Como publicar

1. Acesse o painel do Render e selecione **New > Blueprint**.
2. Conecte o repositório do Squad Estoque.
3. Selecione a branch `main` e confirme o uso do `render.yaml` da raiz.
4. Revise o nome `squad-estoque-demo` e o plano gratuito.
5. Crie o Blueprint e acompanhe os logs até o serviço ficar disponível.
6. Abra a URL fornecida pelo Render e valide a tela de login.

O deploy automático está configurado como `checksPass`: novas alterações na branch
configurada só são publicadas depois que as verificações do GitHub terminarem com
sucesso.

## Acessos de demonstração

| Perfil | E-mail | Senha |
|---|---|---|
| VENDEDOR | `vendedor@squad.com` | `123` |
| LOJISTA | `lojista@squad.com` | `123` |

Essas credenciais são públicas e fracas por escolha consciente para uma demonstração.
Elas não devem ser usadas em produção real.

## Verificação após o deploy

1. Acesse `/health` e confirme uma resposta HTTP 200.
2. Entre como VENDEDOR e confira o fluxo de consulta de estoque.
3. Saia da sessão e entre como LOJISTA.
4. Confirme que cada perfil acessa apenas suas áreas autorizadas.
5. Reinicie o serviço, se desejar demonstrar novamente com os dados recriados.

## Limitações conhecidas

- Os bancos ficam em `/tmp` e podem ser apagados em reinicializações ou novos deploys.
- O plano gratuito pode suspender o serviço por inatividade; o primeiro acesso pode
  demorar mais enquanto ele volta a funcionar.
- A chave usada para proteger o cookie de autenticação também não é persistente;
  sessões abertas podem ser encerradas após reinicializações.
- Esta configuração usa uma única instância e não é apropriada para escala horizontal.

Para produção real, use banco persistente gerenciado, segredos fortes, usuários
provisionados com segurança, persistência das chaves de proteção e monitoramento.
