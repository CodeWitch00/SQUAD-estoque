# Evidências da venda transacional

O decremento do saldo e a criação da movimentação de venda são executados na mesma transação SQLite. A movimentação só é criada depois que o decremento condicional altera o SKU. O sucesso é retornado após o commit. Se ocorrer falha, a operação é revertida, a movimentação cancelada sai do rastreamento e o saldo é recarregado do banco.

Conflitos de escrita recebem uma mensagem compreensível: “O estoque está sendo atualizado por outra operação. Tente novamente.” A venda rápida retorna status 409 nesse caso; a saída administrativa apresenta a mensagem no formulário. Falhas de persistência usam a mensagem de operação cancelada, sem expor detalhes do banco.

## Critérios gerais de aprovação

| Validação | Esperado para aprovação | Resultado obtido |
| --- | --- | --- |
| Build Release | Compilar sem erros. | Aprovado, com zero avisos e zero erros. |
| Suite automatizada | Todos os testes aprovados, sem falhas. | 167 aprovados, zero falhas e zero ignorados; duração de 42 segundos. |
| Testes transacionais | Aprovar os cenários de sucesso, rejeição, rollback e conflito nos dois fluxos. | 14 novos casos aprovados. |
| Cobertura de linhas | Atingir o mínimo de 50% definido no CI. | 77,43% na execução local. |
| Verificação do diff | Não apresentar erros de whitespace. | Aprovado. |

## Resultado esperado e resultado obtido

Os cenários usam SQLite em arquivo e verificam o estado persistido em um novo contexto. Cada execução começa com saldo cinco e uma movimentação histórica de entrada. A venda rápida retira um par; a saída administrativa retira três.

| Cenário | Esperado para aprovação | Resultado obtido |
| --- | --- | --- |
| Commit da venda rápida | Confirmar saldo quatro e uma nova movimentação de saída de um par na mesma transação. | Aprovado. Antes do commit, a transação continha saldo quatro e uma saída. Após o commit, ambos estavam persistidos. |
| Commit da saída administrativa | Confirmar saldo dois e uma nova movimentação de saída de três pares na mesma transação. | Aprovado. Antes do commit, a transação continha saldo dois e uma saída. Após o commit, ambos estavam persistidos. |
| Dados da movimentação | Registrar o SKU selecionado, a quantidade correta, o usuário informado e uma data UTC dentro do intervalo da operação. | Aprovado nos dois fluxos. Identificadores, quantidade, usuário e data conferidos; motivo administrativo normalizado. |
| Falha na inserção da movimentação | Reverter o decremento e manter apenas a movimentação histórica. Permitir uma nova venda no mesmo contexto. | Aprovado nos dois fluxos. Falha de chave estrangeira na inserção; saldo restaurado para cinco, nenhuma nova saída e nova venda aprovada. |
| Falha real no commit SQLite | Reverter o saldo e a movimentação mesmo depois de SaveChanges concluir a inserção. | Aprovado nos dois fluxos. A FK foi adiada até o commit. A transação já continha o saldo reduzido e uma saída quando o SQLite rejeitou o commit; depois do rollback, saldo cinco e apenas o histórico permaneceram. |
| Decremento rejeitado por saldo zero | Não criar movimentação nem confirmar a transação. | Aprovado nos dois fluxos. Saldo permaneceu zero, nenhuma saída criada e nenhuma tentativa de commit. |
| SQLite bloqueado por outra escrita | Retornar conflito seguro sem alterar saldo ou histórico. Permitir nova tentativa após liberar o bloqueio. | Aprovado nos dois fluxos. Mensagem prevista retornada, saldo cinco preservado e nova venda aprovada após liberar a transação concorrente. |
| Mensagem de conflito nos controllers | Venda rápida com status 409 e mensagem segura; saída administrativa com erro no formulário. | Aprovado. Ambos os controllers retornaram a mensagem prevista, sem venda parcial. |
| Mensagem após falha de commit | Apresentar falha compreensível sem incluir SQL, chave estrangeira ou detalhes da exceção na resposta. | Aprovado. Venda rápida retornou “Não foi possível registrar a venda. Tente novamente.” A saída administrativa informou operação cancelada. |
| Preservação do histórico | Manter todos os campos da movimentação anterior inalterados após sucesso, rejeição ou rollback. | Aprovado. ID, SKU, tipo, quantidade, usuário, data e motivo do registro histórico foram preservados. |
| Recuperação do contexto após rollback | Remover a saída cancelada do rastreamento, restaurar o saldo rastreado e encerrar a transação. | Aprovado. Nenhuma movimentação cancelada permaneceu rastreada, saldo cinco restaurado e nenhuma transação ativa. |

A verificação de falha no commit usa uma chave estrangeira adiada somente no banco de teste. A aplicação mantém as constraints existentes e não modifica o esquema nem registros históricos.
