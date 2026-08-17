# Tabela de Entidades e Atributos - Sistema SQUAD

## Entidades principais

PRODUTO → modelo do calçado
SKU → variação (tamanho)
ESTOQUE → saldo por SKU
MOVIMENTAÇÃO → histórico (entrada/saída/ajuste)
RUPTURA → demanda não atendida
USUARIO → ator do sistema


| Entidade | Atributos |
|---|---|
| **USUARIO** | `id`, `nome`, `email`, `senha_hash`, `perfil` |
| **PRODUTO** | `id`, `nome`, `marca`, `categoria`, `cor`, `ativo` |
| **SKU** | `id`, `produto_id`, `numeracao`, `saldo_atual`, `ativo` |
| **MOVIMENTACAO** | `id`, `sku_id`, `tipo`, `quantidade`, `usuario_id`, `criado_em`, `motivo` |
| **RUPTURA** | `id`, `sku_id`, `usuario_id`, `criado_em` |