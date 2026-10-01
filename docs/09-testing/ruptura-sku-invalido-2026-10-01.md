# QA — Rejeição segura de ruptura com SKU inválido ou divergente

Validação em 01/10/2026, na branch `dev/emmy`, sobre a base `fb5fef7`.
Requisitos: RF-18, RN-06, UC-05, UC-S3 e cenário VEN-RUP-02 / IT-27.
Dependências: endpoint de ruptura, autenticação de vendedor, antiforgery,
factory HTTP e banco SQLite relacional isolado por cenário.

## Defeito reproduzido e correção

Na execução inicial, cinco casos de SKU inválido passaram. O caso com SKU
ativo de outro produto falhou: o endpoint retornou sucesso e persistiu uma
ruptura para o SKU substituído. O contrato recebia somente `skuId`.

A correção exige `produtoId` válido e verifica que o SKU ativo pertence ao
produto informado, também ativo, antes de qualquer gravação. O botão de
ruptura envia o produto da grade selecionada. O registro de venda mantém
seu contrato. Os testes positivos existentes foram adaptados ao novo campo.
Requisições antigas de ruptura que omitem produto agora são rejeitadas.

## Cenários e asserções

`RupturaInvalidSkuHttpTests` cobre dez casos:

| Entrada | HTTP | Mensagem pública esperada |
| --- | --- | --- |
| SKU ausente, vazio, malformado ou GUID zerado | 400 | Selecione uma numeração para registrar a ruptura. |
| GUID de SKU inexistente | 400 | A numeração selecionada não está disponível para registro. |
| SKU ativo de outro produto ativo | 400 | A numeração selecionada não está disponível para registro. |
| Produto ausente, malformado, GUID zerado ou inexistente, com SKU válido | 400 | A numeração selecionada não está disponível para registro. |

Cada caso autentica um vendedor com cookies reais, consulta um produto e
obtém o token antiforgery da identidade atual. O HTML deve conter o produto
enviado pelo botão e não deve oferecer o SKU do outro produto. Os saldos
incluem zero, último par e saldo positivo.

Antes e depois do POST, as tabelas de ruptura e movimentação devem estar
vazias. Todos os saldos são comparados por ID em novo escopo, com
`AsNoTracking`. A resposta deve ser JSON com exatamente uma propriedade,
`mensagem`, e o texto público previsto. Essa comparação integral exclui
identificadores, saldos, dados de outros produtos e detalhes internos.

## Validação e evidências

- Build da aplicação: aprovado, zero avisos e zero erros.
- Suite .NET completa: 108 aprovados, zero falhas e ignorados. TRX em
  `tests/SquadEstoque.Web.Tests/TestResults/ruptura-sku-invalido-final.trx`.
- Suite de frontend: sete testes aprovados, zero falhas. Inclui envio exato
  de SKU/produto/token, saldo preservado, bloqueio de reentrada e recuperação
  após rejeição, falha de rede ou resposta com SKU divergente.
- Revisão manual do diff: conferidos vínculo produto/SKU, rejeição anterior
  à persistência, mensagens públicas e adaptação dos testes positivos.
- O cartão fornecido não contém roteiro manual específico. A validação
  funcional foi executada por HTTP real com banco relacional e por Chromium
  headless nos testes de frontend; não foi executada homologação manual em
  ambiente publicado.
- `git diff --check` do escopo aprovado. A verificação global aponta somente
  espaços preexistentes nos três relatórios locais de venda, fora da entrega.

A `main` foi somente atualizada a partir do remoto, conforme o roteiro.
Os commits existentes foram trazidos para `dev/emmy`. Nenhum commit desta
atividade será enviado ou integrado à `main` durante esta execução.
Os relatórios e demais arquivos locais anteriores foram preservados e
excluídos do commit. O relatório fornece as evidências para registro no
cartão pelo responsável. O vínculo ao cartão depende do link/identificador
não informado. Revisão por outro integrante será solicitada no PR.
