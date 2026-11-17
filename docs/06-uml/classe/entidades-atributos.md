# Tabela de Entidades e Atributos - Sistema SQUAD

> **Registro histórico:** lista de entidades do domínio inicial. Consulte o
> [modelo vigente](../../03-modelagem/README.md) para a persistência atual.

## Entidades principais do modelo inicial

PRODUTO → modelo do calçado
SKU → variação (tamanho)
MOVIMENTAÇÃO → histórico (entrada/saída/ajuste)
RUPTURA → demanda não atendida
USUARIO → ator do sistema

`ESTOQUE` não é uma entidade separada: o saldo pertence ao `SKU`.


| Entidade | Atributos |
|---|---|
| **USUARIO** | `id`, `nome`, `email`, `senha_hash`, `perfil` |
| **PRODUTO** | `id`, `nome`, `marca`, `categoria`, `cor`, `ativo` |
| **SKU** | `id`, `produto_id`, `numeracao`, `saldo_atual`, `ativo` |
| **MOVIMENTACAO** | `id`, `sku_id`, `tipo`, `quantidade`, `usuario_id`, `criado_em`, `motivo` |
| **RUPTURA** | `id`, `sku_id`, `usuario_id`, `criado_em` |
