# S4-OPS-015 — Configurações externas e segredos

## Inventário (somente nomes)

| Nome | Natureza e armazenamento |
| --- | --- |
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | Identificadores de identidade, não senhas; armazenados como GitHub Actions secrets e consumidos pelo login OIDC. |
| `ConnectionStrings__EstoqueContext` | Configuração do SQLite no App Service; o projeto não exige senha de servidor de banco. Não incluir credenciais nessa string. |
| `DataProtection__KeysPath` | Caminho externo persistente. Os arquivos de chaves são sensíveis; restringir acesso ao diretório e aos backups. Não incluí-los no pacote ou imagem. |
| `ASPNETCORE_ENVIRONMENT`, `AllowedHosts` | Configurações não secretas do App Service. |
| Senha do primeiro LOJISTA | Entrada padrão do bootstrap, nunca argumento, arquivo versionado ou variável de ambiente. Banco armazena BCrypt; seguir `primeiro-acesso-lojista.md`. |

Não há segredo JWT, client secret Azure ou publish profile exigido pelo fluxo
atual. OIDC usa credenciais temporárias. Não criar segredos fictícios apenas
para preencher o inventário. `.env` não é carregado automaticamente pelo ASP.NET;
no Compose, serve à interpolação das variáveis explicitamente declaradas.

## Configuração e conferência da hospedagem

1. No App Service correto, configurar os nomes de ambiente listados acima conforme
   `azure.md`, sem imprimir valores em logs, comandos gravados ou capturas.
2. Conferir a identidade federada e seu escopo no App Service. No GitHub, conferir
   somente nomes dos secrets com `gh secret list`, nunca seus valores.
3. Publicar somente a versão integrada aprovada. Confirmar inicialização em
   Production e `/health`, login e autorização dos dois perfis. Registrar commit,
   horário e resultados; omitir valores sensíveis e cookies.
4. Verificar permissões do diretório de chaves e backups. Chaves persistidas em
   disco não equivalem a chaves criptografadas em repouso; conferir a proteção
   oferecida pela hospedagem e registrar limitações.

## Verificação reproduzível

Na raiz do checkout, com Python 3.10+, Git e SDK .NET 10:

```text
python scripts/test-secret-boundaries.py
python scripts/test-secret-boundaries.py --docker
```

O segundo comando exige Docker. Ambos usam valores sintéticos e criação
exclusiva: um arquivo local existente interrompe a verificação sem sobrescrevê-lo.
O teste confere exclusão no Git e no `dotnet publish`; `--docker` também confere
o contexto copiado por Docker. O workflow `secret-boundaries.yml` prevê Windows
e Ubuntu. Resultado local não comprova execução do CI nem estado do Azure.

Para verificar exposição além dos nomes convencionais:

1. Atualizar refs e obter todo o histórico autorizado, inclusive branches/tags.
2. Executar um scanner de segredos aprovado sobre esse histórico com saída
   redigida; restringir acesso ao relatório. Nunca copiar correspondências para PR.
3. Inspecionar pacote final e todas as camadas da imagem candidata, inclusive
   metadados e histórico de build. O teste de contexto não examina imagens antigas
   nem segredos injetados por argumentos de build.
4. Inspecionar logs de inicialização, login inválido, bootstrap e deploy, além
   dos relatórios e artefatos publicados. Buscar marcadores fictícios em ambiente
   isolado; valores reais devem ser verificados apenas em ferramenta restrita.
5. Registrar escopo, commit, ferramenta, resultado e limitações. Ausência de
   correspondências não prova ausência absoluta de segredos.

`.gitignore` não remove arquivos já rastreados nem limpa histórico. As exclusões
do projeto e do Docker cobrem nomes convencionais; não detectam uma senha
colocada em um arquivo de código ou JSON de nome diferente. Não usar `git add -f`
para contornar as exclusões.

## Substituição e resposta a comprometimento

- **Credencial de automação:** revogar a credencial ou confiança comprometida
  no provedor, corrigir permissões e atualizar o secret correspondente em Settings
  → Secrets and variables → Actions. O fluxo atual usa OIDC, sem client secret
  duradouro. Se a identidade mudar, atualizar os identificadores e a federação;
  validar a próxima execução autorizada sem reconstruir código.
- **Configuração externa:** substituir pelo mecanismo do App Service e reiniciar
  a aplicação quando necessário. Validar `/health` e os fluxos; não gravar o valor
  em `appsettings.json`, Dockerfile ou YAML.
- **Senha de usuário:** o bootstrap não redefine contas existentes. Encaminhar
  à responsável pelo ambiente um procedimento autorizado de recuperação; não
  apagar o banco nem executar bootstrap novamente para tentar trocar a senha.
- **Chaves de cookies:** coordenar revogação/substituição com a responsável e
  invalidar sessões afetadas. Simplesmente gerar uma chave nova não revoga as
  antigas. Não apagar o diretório indiscriminadamente; avaliar dados protegidos
  e recuperação antes de intervir.
- **Vazamento no Git/artefato:** revogar primeiro, identificar distribuição e
  remover artefatos expostos. Reescrita do histórico exige decisão específica da
  mantenedora e coordenação dos clones; exclusão em um commit não basta.

## Evidências desta retomada — 19/11/2026

- GitHub: os três nomes `AZURE_*` acima foram confirmados por listagem de secrets,
  sem leitura de valores. Nenhum PR aberto foi encontrado para
  `ops/s4-ops-015-felipe` nessa consulta.
- Windows: sondas sintéticas de Git e publicação aprovadas. A consulta NuGet de
  vulnerabilidades emitiu NU1900 por indisponibilidade de rede.
- Docker indisponível nesta sessão; Ubuntu/CI, imagem final, histórico completo,
  logs reais e configuração/inicialização no App Service ainda não comprovados.
- Não houve alteração no Azure ou no Trello. O cartão não está integralmente
  concluído: exige essas verificações, PR e revisão da mantenedora.
- Trabalho retomado em `dev/felipe`, conforme decisão de Felipe. Preservadas as
  exclusões anteriores do S4-OPS-008 durante a sincronização com `main`.
