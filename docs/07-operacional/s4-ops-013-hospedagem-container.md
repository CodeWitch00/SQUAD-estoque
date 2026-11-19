# S4-OPS-013 - Hospedagem pública com container

## Estado da entrega

**Preparação técnica; publicação do container pendente da líder.** A configuração
da hospedagem pertence à líder, conforme confirmação de Felipe nesta execução.
Nenhum recurso foi contratado, reiniciado ou alterado. Este documento não
comprova implantação de container nem conclusão do cartão.

Base analisada: `5d160a3e55804f889e5edf7d5f0e8c91cf3ed401`.

## Evidências verificadas em 18/11/2026

| Verificação | Evidência | Limite |
| --- | --- | --- |
| Disponibilidade pública | GET HTTPS `/health`: HTTP 200, corpo `Healthy`, verificação TLS do curl igual a 0 | Não comprova imagem Docker, persistência ou login |
| Modalidade atual documentada | [azure.md](azure.md): App Service Linux F1, publicação por ZIP | Configuração real do Portal não foi acessada |
| Build/testes da main | [Execução 44178403978](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/44178403978): 352 aprovados, 1 ignorado, 0 falhas, cobertura 92,96% | Resultado da base, não de nova hospedagem |
| Build da imagem | [Execução 44178461906](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/44178461906): sucesso | Publicação em registro e digest consumível não confirmados |
| Acesso à infraestrutura | Felipe informou que a configuração fica com a líder | Bloqueia alterações e validação administrativa |

Endereço existente:
https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net

Comando de consulta, sem alteração de dados:

```text
curl --fail --show-error --max-time 60 https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net/health
```

## Decisão de hospedagem pendente

Manter a homologação atual sem modificações até a líder decidir o destino.
O requisito continua sendo container com SQLite; não foi proposta troca de banco.

Há uma divergência material: o runbook atual usa SQLite em `/home/data`, mas
a [documentação da equipe Azure](https://github.com/Azure/app-service-linux-docs/blob/master/Things_You_Should_Know/things_you_should_know.md#sqlite-and-other-file-based-databases-are-not-supported)
registra SQLite como não suportado no compartilhamento de rede padrão do
App Service Linux por limitações de locking. Saúde HTTP e um teste antigo de
reinício não demonstram segurança desse armazenamento. Isso não é evidência de
corrupção do banco atual; é impedimento para aprovar a mesma arquitetura sem
esclarecimento técnico. Apenas habilitar persistência em `/home` não resolve
essa incompatibilidade documentada.

| Opção | Situação para este cartão |
| --- | --- |
| App Service atual, ZIP | Disponível para homologação; não comprova consumo da imagem Docker; armazenamento SQLite exige decisão da líder |
| App Service com imagem própria | Exige confirmar SKU, custo, registro, configuração e armazenamento com locking compatível; não aprovado por esta análise |
| Render gratuito | Não atende à persistência do SQLite: não permite disco persistente |
| Render pago com disco | Candidato técnico, não selecionado nem contratado; exige orçamento, conta e teste real de permissões/locking |

A documentação do Render prevê disco em serviço pago, somente uma instância
com acesso e interrupção durante deploy. A referência pública consultada indica
compute a partir de US$ 7/mês e disco US$ 0,25/GB/mês; 1 GB resultaria em uma
estimativa inicial de US$ 7,25/mês, **não uma cotação ou teto autorizado**.
Confirmar plano atual, memória suficiente, tráfego, impostos, câmbio e demais
custos no painel antes de qualquer contratação. Não houve autorização de gasto.

Fontes: [discos](https://render.com/docs/disks),
[limites gratuitos](https://render.com/docs/free),
[preços](https://render.com/pricing?trk=public_profile__reactions-text).

## Configuração que a líder precisa confirmar antes de publicar

1. Provedor, conta, região, plano, custo total e responsável operacional.
2. Imagem de commit aprovado da main, disponível no registro e fixada por digest.
   Usar o fluxo de [entrega da imagem](entrega-imagem.md); não usar `latest` como
   identificação de retorno nem considerar um image ID local como digest remoto.
3. Uma única instância com disco persistente exclusivo e locking SQLite
   compatível. Validar criação, transação de escrita e permissões como usuário
   não root. Não executar a aplicação como root para contornar permissões.
4. HTTPS, hostname definitivo, porta interna 8080, logs de stdout/stderr,
   reinício automático e probe HTTP `/health`. Validar Host usado pelo provedor.
5. Valores de produção no painel/gerenciador do provedor, fora do repositório:
   `ASPNETCORE_ENVIRONMENT=Production`, `AllowedHosts` com hosts explícitos,
   `ConnectionStrings__EstoqueContext` e `DataProtection__KeysPath` apontando
   para o disco. Não publicar senhas nem tokens em evidências.
6. Backup consistente fora do serviço, com restauração ensaiada em isolamento;
   uploads também precisam de destino persistente se usados na homologação.
7. Janela coordenada, estado de dados conhecido e procedimento de retorno.

## Validação no destino e evidências de aceite

- Registrar URL real, identificador do serviço, plano, região e digest implantado.
- Confirmar HTTPS válido e `200 Healthy`, login e autorização dos dois perfis.
  A saúde atual só comprova resposta do processo; não substitui prova de dados.
- Registrar montagem, capacidade e UID/GID sem expor conteúdo privado.
- Em massa fictícia aprovada, criar registro identificável; confirmar o mesmo
  registro após reinício e após substituição do container pelo mesmo digest.
- Registrar logs de inicialização, ausência de erros de storage e teste de
  reinício. O sucesso em volume Docker de CI não substitui teste no provedor.
- Testar retorno à imagem anterior compatível e repetir saúde e conferência
  de dados. Publicar evidências sanitizadas e solicitar revisão do PR.

## Retorno à última versão funcional

Antes da atualização, registrar digest anterior e configurações externas,
confirmar backup consistente e compatibilidade do schema. Em falha, interromper
a atualização, preservar logs e restaurar o digest anterior **sem substituir
o volume**. Validar `/health`, login, perfis e registros de referência.

Se houve migration incompatível, retornar só a imagem não basta: manter o
serviço sem novas escritas e seguir [backup e restauração](backup-restauracao-sqlite.md)
com aprovação da líder. Preservar o banco atual e restaurar primeiro em cópia
isolada. Não apagar volume nem sobrescrever o único backup. Esses passos estão
preparados, mas não foram executados no ambiente público nesta entrega.

## Pendências para conclusão

Decisão de plataforma/custo e solução da divergência de armazenamento; acesso
da líder; publicação por imagem; persistência após reinício/redeploy no destino;
ensaio de retorno; revisão aprovada e atualização do cartão pela responsável.
O Trello não foi modificado, conforme a governança vigente.
