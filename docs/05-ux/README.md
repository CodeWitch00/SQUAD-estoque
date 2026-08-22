# UX — telas, fluxos e evidências

Esta pasta concentra o inventário de interface e os protótipos navegacionais do MVP. Requisitos de negócio permanecem em `02-requisitos`; diagramas UML, em `06-uml`.

## Situação dos fluxos

| Fluxo | Protótipo | Situação no sistema |
| --- | --- | --- |
| Produtos | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Implementado |
| Grade | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Parcialmente implementado: disponível ao lojista; fluxo do vendedor e última atualização pendentes |
| Entrada | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Implementado |
| Saída administrativa | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Implementado; permissão atual requer correção |
| Ajuste | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Implementado |
| Movimentações | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Implementado |
| Início do lojista | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Planejado |
| Saldos zerados | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Planejado para Sprint 3 |
| Rupturas | [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html) | Planejado para Sprint 3 |
| Início e consulta do vendedor | [Fluxo do vendedor](fluxos/vendedor/fluxo-vendedor.html) | Planejado no módulo operacional do vendedor |
| Grade do vendedor | [Fluxo do vendedor](fluxos/vendedor/fluxo-vendedor.html) | Planejado no módulo operacional do vendedor |
| Vendeu / Não tinha / Desistiu | [Fluxo do vendedor](fluxos/vendedor/fluxo-vendedor.html) | Planejado no módulo operacional do vendedor |

## Artefatos

- [Inventário de telas e navegação](inventario-telas-e-mapa-navegacao.md): origem, destino, permissão, status e rastreabilidade RF/UC/US.
- [Mapa visual consolidado](mapa-navegacao-mvp.svg): fluxo-alvo por perfil.
- [Fluxo do lojista](fluxos/lojista/fluxo-lojista.html): protótipo navegacional desktop/tablet.
- [Fluxo do vendedor](fluxos/vendedor/fluxo-vendedor.html): protótipo navegacional mobile.
- [Decisões de interface](decisoes/decisoes-de-interface.md): limites e decisões usados nos protótipos.
- [Evidências](evidencias/README.md): protocolo e estado das capturas exigidas no cartão.

## Convenções

- Verde: tela existente na baseline.
- Laranja tracejado: tela ou estado pendente.
- Cinza: legado fora do fluxo do MVP.
- Os HTMLs são protótipos de fluxo, não implementação funcional nem fonte de requisitos.
- Nenhuma funcionalidade além de RF-01 a RF-23, UC-01 a UC-12 e US-01 a US-13 deve ser adicionada aos protótipos.
