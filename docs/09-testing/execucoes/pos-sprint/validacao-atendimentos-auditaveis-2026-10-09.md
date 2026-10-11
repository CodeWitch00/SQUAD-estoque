# Validação de atendimentos pendentes e auditáveis

**Data:** 09/10/2026  
**Branch:** `dev/rayana`

## Escopo validado

- seleção de modelo cria atendimento pendente vinculado ao vendedor;
- modelo totalmente sem saldo não cria pendência apenas pela consulta, mas permite registrar **Não tinha** para uma numeração solicitada;
- a grade é exibida sem manter a busca na tela;
- a grade compacta separa numerações disponíveis das indisponíveis e mantém saldo, estado e seleção legíveis sem depender apenas de cor;
- outra consulta pode ser iniciada sem apagar a pendência anterior;
- pendências reaparecem após nova requisição e podem ser retomadas;
- a última numeração selecionada e seu saldo são persistidos antes do resultado e, ao retomar, somente esse calçado é exibido;
- o toque isolado em uma numeração sem saldo não a associa à pendência; o SKU só é registrado ao confirmar **Não tinha**;
- Vendeu e Não tinha exigem SKU pertencente ao produto consultado;
- Vendeu conclui o atendimento na mesma transação da saída;
- após confirmar Vendeu, a mensagem é exibida e o sistema retorna automaticamente para a busca;
- Não tinha conclui o atendimento junto do registro de ruptura;
- Desistiu conclui o atendimento sem movimentação ou ruptura;
- um atendimento concluído não pode ser concluído novamente;
- outro vendedor não pode concluir uma pendência que não lhe pertence;
- movimentações e rupturas ficam vinculadas ao atendimento de origem;
- o lojista possui visão exclusiva de pendências, resultados e alertas de revisão;
- o vendedor não possui endpoint para editar ou excluir o histórico.

## Cenários anti-burla

O registro não desaparece quando o vendedor informa um resultado: muda de Pendente para Concluído e mantém vendedor, produto, SKU quando aplicável, saldo consultado, resultado e horários. Encerramentos muito rápidos, pendências antigas e Não tinha informado com saldo positivo são sinalizados na visão do lojista.

## Verificações

```text
npm run test:frontend
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore
git diff --check
```

Resultado esperado desta versão:

- 20 testes frontend aprovados;
- 175 testes .NET aprovados;
- build sem erros ou avisos;
- migration `AddAtendimentoAuditavel` criada e versionada, sem execução de `database update` sobre o banco local.
