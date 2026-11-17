# Entrega controlada da imagem Docker

O workflow `Versioned Docker image` é acionado manualmente no GitHub Actions.
Informe o SHA completo do commit aprovado que esteja na ponta de `main`.
Com `publish=false`, ele executa restore, build, testes e build da imagem sem
enviá-la ao registro. Com `publish=true`, envia a imagem validada para GHCR.

A imagem recebe a tag `ghcr.io/<owner>/<repo>:<sha-completo>`. A publicação usa
o `GITHUB_TOKEN` fornecido pelo GitHub Actions, com permissão `packages: write`;
nenhuma credencial é armazenada no repositório. Qualquer falha interrompe a
sequência antes do push. O resumo da execução registra a tag, o ID local e,
quando aplicável, a publicação. Os logs de restore, build, testes e Docker
ficam na execução do workflow.

Para aprovar uma entrega, registre no PR ou no controle da equipe o SHA e o
link da execução simulada (`publish=false`). Em seguida, execute novamente o
workflow com o mesmo SHA e `publish=true`. Confirme no resumo a tag publicada
e registre o digest retornado pelo registro. Use o digest para fixar a imagem
no ambiente de destino; o App Service atual recebe pacote de código e não
consome esta imagem.

Para retornar à versão anterior, selecione o digest da última imagem aprovada
no ambiente consumidor e execute a verificação de saúde e persistência desse
ambiente. Não substitua o volume SQLite durante a troca de imagem. O retorno
do aplicativo não desfaz migrations; faça backup e siga o procedimento de
recuperação do banco antes de qualquer atualização de schema.
