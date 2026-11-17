# Fluxos vigentes

Os fluxos resumem as decisões observáveis do código. Cada gravação é validada
novamente no servidor; as telas não definem a autorização ou o saldo persistido.

## Consulta e atendimento do VENDEDOR

```mermaid
flowchart TD
    A(["Iniciar consulta"]) --> B["Buscar produto ou retomar atendimento pendente"]
    B --> C["Exibir produto, grade e saldo por SKU"]
    C --> D{Selecionou numeracao?}
    D -->|nao| FIM(["Consulta sem alteracao"])
    D -->|sim| E{Saldo maior que zero?}
    E -->|sim| P["Criar ou atualizar Atendimento pendente com SKU e saldo consultado"]
    E -->|nao| Z["Toque isolado nao cria pendencia"]
    Z --> ZD{Pedido especifico nao atendido?}
    ZD -->|nao| FIM
    ZD -->|sim| NR["Registrar Nao tinha: criar e concluir Atendimento e Ruptura"]
    P --> R{Resultado do atendimento}
    R -->|Vendeu| V["Baixar um par, registrar Movimentacao e concluir Atendimento"]
    R -->|Nao tinha| N["Registrar Ruptura e concluir Atendimento sem alterar saldo"]
    R -->|Desistiu| X["Concluir Atendimento sem Movimentacao ou Ruptura"]
    V --> FIM2(["Nova consulta permitida"])
    N --> FIM2
    X --> FIM2
    NR --> FIM2
```

O vendedor pode ter vários atendimentos pendentes. A retomada carrega somente
o produto e o SKU daquele atendimento. As operações `Vendeu`, `Não tinha` e
`Desistiu` são implementadas em
[`EstoqueController`](../../../src/SquadEstoque.Web/Controllers/EstoqueController.cs).

## Ajuste manual do LOJISTA

```mermaid
flowchart TD
    A(["Abrir Ajuste como LOJISTA"]) --> B["Informar SKU, novo saldo apurado e motivo"]
    B --> C{Campos validos?}
    C -->|nao| E(["Exibir erros; sem gravacao"])
    C -->|sim| D["Iniciar transacao e carregar SKU"]
    D --> F{SKU existe e novo saldo difere do atual?}
    F -->|nao| E
    F -->|sim| G["Definir SaldoAtual igual ao novo saldo e atualizar data"]
    G --> H["Criar Movimentacao AJUSTE com quantidade igual a diferenca absoluta"]
    H --> I{SaveChanges e commit funcionaram?}
    I -->|sim| J(["Saldo e historico confirmados juntos"])
    I -->|nao| K(["Rollback; saldo e historico preservados"])
```

O novo saldo deve ser não negativo. O motivo é obrigatório. O formulário e a
ação exigem `LOJISTA`; a quantidade da movimentação não tem sinal. Veja
[`MovimentacoesController.Ajuste`](../../../src/SquadEstoque.Web/Controllers/MovimentacoesController.cs).

## Entrada por XML de NF-e

```mermaid
flowchart TD
    A(["LOJISTA envia XML da NF-e"]) --> B{XML valido e chave nova?}
    B -->|nao| E(["Rejeitar importacao"])
    B -->|sim| C["Salvar EntradaFiscal e itens como RASCUNHO"]
    C --> D["Conferir produto, numeracao e foto opcional por item"]
    D --> F{Dados de confirmacao validos?}
    F -->|nao| D
    F -->|sim| G["Iniciar transacao"]
    G --> H["Criar ou associar Produto e Sku; incrementar saldo"]
    H --> I["Registrar Movimentacao de entrada e vinculo do fornecedor"]
    I --> J["Marcar EntradaFiscal como CONFIRMADA"]
    J --> K{Salvar e confirmar transacao?}
    K -->|sim| L(["Estoque atualizado"])
    K -->|nao| M(["Rollback; nenhum saldo confirmado"])
```

O rascunho não altera o estoque; a confirmação usa uma transação. A chave de
acesso única impede importar a mesma NF-e novamente. Veja
[`EntradasFiscaisController`](../../../src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs).

## Primeiro acesso do LOJISTA

```mermaid
flowchart TD
    A(["Operador executa --bootstrap-lojista"]) --> B{Ambiente Production?}
    B -->|nao| X(["Recusar comando"])
    B -->|sim| C["Aplicar migrations no SQLite configurado"]
    C --> D{Banco sem usuarios?}
    D -->|nao| X
    D -->|sim| E{Senha recebida pela entrada padrao e valida?}
    E -->|nao| X
    E -->|sim| F["Criar um LOJISTA com hash BCrypt em transacao"]
    F --> G(["Encerrar comando sem iniciar servidor HTTP"])
```

A senha não é passada em argumento ou variável de ambiente. O comando não
remove usuários antigos e não cria `VENDEDOR`. Consulte o
[procedimento de primeiro acesso](../../07-operacional/primeiro-acesso-lojista.md).
