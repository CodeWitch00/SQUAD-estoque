# Operação do SQUAD Estoque

Esta pasta reúne procedimentos operacionais e evidências. O fluxo diário de
preparação, implementação, testes, Git e Pull Request está somente no
[CONTRIBUTING.md](../../CONTRIBUTING.md).

## Documentos atuais

| Necessidade | Documento |
| --- | --- |
| Conferir a baseline técnica | [checklist-baseline.md](checklist-baseline.md) |
| Executar e preservar dados com Docker | [docker.md](docker.md) |
| Consultar evidências do cartão S2-BE-007 | [s2-be-007-evidencias.md](s2-be-007-evidencias.md) |
| Consultar evidências do cartão S2-BE-008 | [s2-be-008-evidencias.md](s2-be-008-evidencias.md) |

## Limites

- procedimentos de deploy, backup e recuperação devem refletir o ambiente real;
- segredos, senhas e bancos reais não pertencem à documentação;
- uma validação planejada não deve ser descrita como executada;
- mudanças de infraestrutura precisam de decisão e responsável explícitos;
- o runbook público de Azure será consolidado depois que o primeiro deploy
  manual, a persistência e a restauração de backup forem comprovados.

Planejamento privado, decisões de gestão e acompanhamento pessoal permanecem
fora da documentação pública do projeto.
