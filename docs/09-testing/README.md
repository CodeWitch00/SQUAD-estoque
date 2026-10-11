# Testes e evidências

Esta pasta reúne documentação de qualidade: estratégia, especificações, roteiros, relatórios de execuções e evidências visuais. Os testes executáveis estão em [`tests/`](../../tests/); aqui ficam os critérios e os registros que explicam o que foi validado, em qual ambiente e com qual resultado.

## Mapa

| Conteúdo | Local |
|---|---|
| Plano geral, catálogo de casos e critérios de liberação | [plano-de-testes.md](plano-de-testes.md) |
| Especificações administrativa, autenticação, não funcional e vendedor | [especificacoes/](especificacoes/) |
| Relatórios e resultados de execução | [execucoes/](execucoes/) |
| Capturas e artefatos visuais | [evidencias/](evidencias/) |
| Código automatizado .NET e frontend | [`tests/`](../../tests/) |

## Relatórios por período

- [Sprint 1](execucoes/sprint-1/): baseline e validações iniciais, preservadas como histórico.
- [Sprint 2](execucoes/sprint-2/): venda rápida, grade, ruptura, `Não tinha`, `Desistiu` e validações relacionadas.
- [Sprint 3](execucoes/sprint-3/): decremento atômico e venda transacional.
- [Pós-sprint](execucoes/pos-sprint/): validações realizadas após o prazo da Sprint 2; a data do relatório não altera o escopo do cartão a que ele se refere.
- [Homologação](execucoes/homologacao/): registros de homologação, inclusive bloqueios históricos que precisam ser reavaliados antes de serem considerados atuais.

As capturas são agrupadas em [evidencias/sprint-2](evidencias/sprint-2/) ou em pastas nomeadas pelo ambiente. Um relatório deve indicar explicitamente quando a validação foi emulada, local, publicada ou executada em aparelho físico.

## Como interpretar os resultados

- Uma especificação define casos e critérios; não comprova que foram executados.
- Um relatório registra uma execução datada e ligada a um estado específico do código. Contagens de execuções diferentes não devem ser somadas.
- O resultado local não substitui CI; uma CI aprovada não substitui validação manual quando o critério depende do dispositivo, da percepção visual ou do ambiente publicado.
- Relatórios históricos não são descartados só por estarem antigos: devem ser marcados como históricos ou supersedidos quando houver evidência posterior.
