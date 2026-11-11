# Validação da navegação por perfil e catálogo (S2-QA-035)

## Escopo validado

* Visão geral e validação das regras de negócio e controle de acesso para os perfis `LOJISTA` e `VENDEDOR`.


* Testes de layout e responsividade cobrindo as resoluções Desktop ($1920 \times 1080\text{ px}$) e Mobile ($375 \times 812\text{ px}$).


* Login e segregação de perfis (`/login`), com direcionamento automático pós-autenticação.


* Módulo de consulta rápida do Vendedor (`/consulta`) com status de estoque visual (Disponível, Último Par, Indisponível) e desfechos rápidos.


* Módulo Catálogo de Produtos (`/produtos`) com filtros simultâneos por texto, status, marca e categoria.


* Cadastro de produtos (`/produtos/novo`) com geração automática de SKUs por separação de vírgula na grade.


* Histórico de movimentações (`/movimentacoes`) com modais de auditoria, registro de entrada/saída e validação de justificativa em ajustes manuais.



## Resultado

| Cenário | Resultado esperado | Situação |
| --- | --- | --- |
| Autenticação | Inserção de credenciais em `/login`, concedendo acesso e redirecionamento correto conforme perfil | Aprovado

 |
| Catálogo | Carregamento da rota inicial `/produtos` exibindo listagem completa e navegação responsiva | Aprovado

 |
| Filtros do Catálogo | Seleção combinada de status, marca e categoria filtrando a tabela dinamicamente | Aprovado

 |
| Cadastro de Produtos | Inclusão via `/produtos/novo` com geração automática de SKUs zerados a partir de numerações por vírgula | Aprovado

 |
| Consulta do Histórico | Acesso à auditoria em `/movimentacoes` com apresentação completa de registros e filtros | Aprovado

 |
| Registrar Nova Entrada | Abertura do modal de entrada com seleção de produto, saldo atual e campo para nota fiscal | Aprovado

 |
| Registrar Nova Saída | Abertura do modal de baixa de estoque permitindo a inserção de quantidade e observação | Aprovado

 |
| Ajuste Manual | Validação da regra de negócio exigindo justificativa válida (mínimo de 5 caracteres) | Aprovado

 |

## Evidência automatizada

Os cenários foram validados manualmente e suportados por suítes de auditoria e testes de autorização, confirmando a responsividade em Desktop ($1920 \times 1080\text{ px}$) e Mobile ($375 \times 812\text{ px}$), a consistência dos dados de estoque e a segregação efetiva entre os perfis Lojista e Vendedor.