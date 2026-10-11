# Validação da entrada de mercadorias por NF-e

**Data:** 09/10/2026  
**Branch:** `dev/rayana`

## Cenários automatizados

- acesso exclusivo do lojista;
- XML válido cria rascunho sem movimentar estoque;
- conferência cadastra modelo e numeração novos;
- confirmação incrementa saldo e cria movimentação de entrada;
- vínculo fornecedor + código aponta para o SKU confirmado;
- chave de acesso já confirmada não pode ser importada novamente;
- build, testes HTTP e testes frontend permanecem aprovados.

## Resultado

- 20 testes frontend aprovados;
- 175 testes .NET aprovados;
- build sem erros ou avisos;
- modelo do Entity Framework sem alterações pendentes após a migration;
- `git diff --check` aprovado.

## Limites

A migration `AddEntradaFiscalEImagemProduto` foi criada, mas não foi aplicado `database update` manualmente ao banco local. O ambiente normal aplica migrations pendentes na inicialização, conforme o comportamento já existente em `Program.cs`.
