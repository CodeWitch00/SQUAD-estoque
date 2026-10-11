# Validação do fluxo de atendimento do vendedor — 22/09/2026

## Escopo

Cartão S2-FE-010: selecionar uma numeração na grade e registrar o resultado do atendimento na área comum de ações.

## Fluxo revisado

- Cada card representa uma numeração, saldo e estado. O card inteiro seleciona um único SKU.
- Vendeu e Não tinha iniciam desabilitados e habilitam após seleção. Desistiu pode ser escolhido sem numeração.
- Nova consulta sem registrar resultado permanece disponível.
- A grade usa duas colunas em larguras mobile usuais; as ações ficam fixas por `position: sticky` e respeitam a área segura inferior do smartphone.
- A busca é automática após pelo menos dois caracteres, com debounce de 800 ms.

## Regras exercitadas

- Venda protegida por antiforgery decrementa uma unidade e registra movimentação; saldo zero é recusado no servidor.
- Não tinha cria Ruptura associada ao SKU e usuário, sem alterar saldo.
- Saldo zero isoladamente não cria ruptura. Desistiu e nova consulta não enviam POST nem alteram dados.
- A View não contém comandos Vendeu repetidos nos cards; o JavaScript preserva seleção exclusiva e habilita as ações necessárias.

## Verificação automatizada

- Build do projeto Web: aprovado.
- Suíte `SquadEstoque.Web.Tests`: 84 testes aprovados.
- A validação manual em aparelho físico ainda deve ser feita antes de considerar o aceite mobile final; a renderização responsiva e o fluxo foram verificados na estrutura Razor/Bootstrap e pelos testes HTTP.

## Divergência registrada

O cartão anterior de S2-FE-010 descrevia Vendeu direto em cada SKU. A alteração atual segue o fluxo aprovado em `prototipo-vendedor.html` e nos UC-04/05/06: selecionar numeração e, em seguida, selecionar o resultado.
