# Persistência da movimentação de venda — 24/09/2026

## Escopo e evidência automatizada

Teste: `VendaRapidaHttpTests.Completed_sale_persists_one_traceable_exit_for_authenticated_seller`.

Uma fábrica por teste cria SQLite relacional em memória exclusivo. Produto, SKU e vendedor têm identificadores novos. O vendedor próprio evita que uma atribuição incorreta ao usuário padrão passe despercebida. O login usa o formulário real, cookie e antiforgery; a venda envia somente o SKU e o token, sem fornecer o usuário.

Pré-condição: tabela de movimentações vazia e SKU ativo com saldo 2. Após um único POST de venda concluído com HTTP 200 e mensagem de sucesso, outro contexto consulta toda a tabela, sem rastreamento.

| Requisito | Asserção |
| --- | --- |
| Unicidade | `Assert.Single` sobre toda a tabela, sem filtro por SKU ou usuário |
| Identificação | ID da movimentação diferente de `Guid.Empty` |
| Tipo | `TipoMovimentacao.SAIDA` |
| Quantidade | Exatamente 1 |
| SKU | ID do SKU criado e vendido no teste |
| Usuário | ID do vendedor autenticado pelo login real |
| Data | `CriadoEm` entre os instantes UTC imediatamente antes e depois do POST |

Não há dependência de ordem de testes, relatórios ou exibição do histórico. Nenhum código de produção foi alterado.

## Execução local

- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj`: aprovado, 0 avisos e 0 erros.
- `dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --logger "trx;LogFileName=suite-movimentacao-venda.trx"`: 84 aprovados, 0 falhas, 0 ignorados.
- TRX local: `tests/SquadEstoque.Web.Tests/TestResults/suite-movimentacao-venda.trx`.
- Revisão local: conferidos isolamento, autenticação real, leitura em novo contexto, limite temporal fechado ao concluir o POST e unicidade global.
- `git diff --check`: encontrou whitespace apenas nos dois documentos preexistentes de validação da venda rápida de 22/09, preservados fora do escopo. A verificação restrita aos arquivos deste cartão passou.

## Verificação manual do ambiente publicado

No endereço Azure fornecido, o login de vendedor foi concluído e redirecionou para `/Estoque/Consulta`. A busca por `Tênis` retornou `Tênis run`, marca Olympics, cor Preto, produto `91978fba-8983-4163-9be8-a5464e8449fe`. A grade apresentou apenas tamanho 37, saldo 0, INDISPONÍVEL, sem ação de venda.

A venda manual não foi executada por falta de SKU disponível nesse resultado. A persistência no Azure não foi verificada; a comprovação dos campos e da unicidade é a do teste automatizado em SQLite isolado. Relatórios e histórico não foram acessados.

## Entrega e pendências externas

Branch de trabalho: `dev/emmy`. Nenhuma alteração foi aplicada à `main`; a tentativa inicial de troca foi bloqueada antes de executar e, após orientação do solicitante, não foi repetida.

O solicitante abrirá o PR vinculado ao cartão e solicitará revisão de outro integrante. Revisão externa, GitHub Actions e registro do link do PR no cartão permanecem pendentes dessa etapa. Este documento contém os resultados para anexar ao cartão; nenhum cartão externo foi editado.
