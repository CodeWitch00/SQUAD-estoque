# UX — especificações, protótipos, fluxos e evidências

Esta pasta concentra o inventário de interface e os protótipos navegacionais do MVP. Requisitos de negócio permanecem em `02-requisitos`; diagramas UML, em `06-uml`.

## Situação dos fluxos

| Fluxo | Protótipo | Situação no sistema |
| --- | --- | --- |
| Produtos e grade do lojista | [Protótipo do lojista](prototipos/prototipo-lojista.html) | Implementados em Produtos; cadastro e manutenção disponíveis ao LOJISTA. |
| Consulta e grade do vendedor | [Protótipo do vendedor](prototipos/prototipo-vendedor.html) | Implementadas em `/Estoque/Consulta`, incluindo busca, numerações, saldos e estados de disponibilidade. |
| Entrada, saída administrativa e ajuste | [Protótipo do lojista](prototipos/prototipo-lojista.html) | Implementados para LOJISTA; a saída administrativa não é uma ação do VENDEDOR. |
| Venda, ruptura e desistência | [Protótipo do vendedor](prototipos/prototipo-vendedor.html) | Implementados pelos resultados Vendeu, Não tinha e Desistiu no fluxo de atendimento. |
| Saldos zerados e histórico de rupturas | [Protótipo do lojista](prototipos/prototipo-lojista.html) | Implementados em `/SaldosZerados` e `/Rupturas`. |
| Início dos perfis | Protótipos de cada perfil | Após o login, VENDEDOR segue para `/Estoque/Consulta` e LOJISTA para `/Produtos`; não há dashboard separado. |

## Artefatos

- [Inventário de telas e navegação](inventario-telas-e-mapa-navegacao.md): origem, destino, permissão, status e rastreabilidade RF/UC/US.
- [Mapa visual consolidado](mapa-navegacao-mvp.svg): fluxo-alvo por perfil.
- [Especificação dos componentes mobile](especificacao-componentes-mobile.md): fonte oficial dos componentes da busca e da grade do vendedor.
- [Fluxo do atendimento do vendedor - S2-UX-001](s2-ux-001-fluxo-atendimento-vendedor.md): estados e transições entre a grade e a nova consulta.
- [Protótipos](prototipos/README.md): interfaces navegáveis separadas por perfil.
- [Protótipo do lojista](prototipos/prototipo-lojista.html): referência visual desktop/tablet.
- [Protótipo do vendedor](prototipos/prototipo-vendedor.html): referência visual mobile.
- [Decisões de interface](decisoes/decisoes-de-interface.md): limites e decisões usados nos protótipos.
- [Evidências](evidencias/README.md): protocolo e estado das capturas exigidas no cartão.

## Convenções

- Verde: tela existente na baseline.
- Laranja tracejado: tela ou estado pendente.
- Cinza: legado fora do fluxo do MVP.
- Os HTMLs representam a interface proposta para implementação, mas não substituem os requisitos e as especificações.
- Nenhuma funcionalidade além de RF-01 a RF-25, UC-01 a UC-12 e US-01 a US-13 deve ser adicionada aos protótipos.
