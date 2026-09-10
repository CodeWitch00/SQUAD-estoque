# UX — especificações, protótipos, fluxos e evidências

Esta pasta concentra o inventário de interface e os protótipos navegacionais do MVP. Requisitos de negócio permanecem em `02-requisitos`; diagramas UML, em `06-uml`.

## Situação dos fluxos

| Fluxo | Protótipo | Situação no sistema |
| --- | --- | --- |
| Produtos | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Implementado |
| Grade | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Parcialmente implementado: disponível ao lojista; fluxo do vendedor e última atualização pendentes |
| Entrada | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Implementado |
| Saída administrativa | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Implementado; permissão atual requer correção |
| Ajuste | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Implementado |
| Movimentações | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Implementado |
| Início do lojista | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Planejado |
| Saldos zerados | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Planejado para Sprint 3 |
| Rupturas | [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html) | Planejado para Sprint 3 |
| Início e consulta do vendedor | [Protótipo do vendedor](prototipos/vendedor/prototipo-vendedor.html) | Planejado no módulo operacional do vendedor |
| Grade do vendedor | [Protótipo do vendedor](prototipos/vendedor/prototipo-vendedor.html) | Planejado no módulo operacional do vendedor |
| Vendeu / Não tinha / Desistiu | [Protótipo do vendedor](prototipos/vendedor/prototipo-vendedor.html) | Planejado no módulo operacional do vendedor |

## Artefatos

- [Inventário de telas e navegação](inventario-telas-e-mapa-navegacao.md): origem, destino, permissão, status e rastreabilidade RF/UC/US.
- [Mapa visual consolidado](mapa-navegacao-mvp.svg): fluxo-alvo por perfil.
- [Especificação dos componentes mobile](S1-UX-010-especificacao-componentes-mobile.md): fonte oficial dos componentes da busca e da grade do vendedor.
- [Protótipos](prototipos/README.md): interfaces navegáveis separadas por perfil.
- [Protótipo do lojista](prototipos/lojista/prototipo-lojista.html): referência visual desktop/tablet.
- [Protótipo do vendedor](prototipos/vendedor/prototipo-vendedor.html): referência visual mobile.
- [Decisões de interface](decisoes/decisoes-de-interface.md): limites e decisões usados nos protótipos.
- [Evidências](evidencias/README.md): protocolo e estado das capturas exigidas no cartão.

## Convenções

- Verde: tela existente na baseline.
- Laranja tracejado: tela ou estado pendente.
- Cinza: legado fora do fluxo do MVP.
- Os HTMLs representam a interface proposta para implementação, mas não substituem os requisitos e as especificações.
- Nenhuma funcionalidade além de RF-01 a RF-23, UC-01 a UC-12 e US-01 a US-13 deve ser adicionada aos protótipos.
