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
| Atendimentos auditáveis | — | Implementados em `/Atendimentos`, com pendências, conclusões e alertas. |
| Entrada fiscal e fotos de produto | — | Importação XML, conferência e confirmação em `/EntradasFiscais`; fotos opcionais no cadastro, edição e conferência. |
| Início dos perfis | Protótipos de cada perfil | Após o login, VENDEDOR segue para `/Estoque/Consulta` e LOJISTA para `/Produtos`; não há dashboard separado. |

## Artefatos

- [Inventário de telas e navegação](inventario-telas-e-mapa-navegacao.md): mapa vigente de rotas, permissões e rastreabilidade RF/UC/US.
- [Especificação dos componentes mobile](especificacao-componentes-mobile.md): fonte oficial dos componentes da busca e da grade do vendedor.
- [Fluxo do atendimento do vendedor - S2-UX-001](s2-ux-001-fluxo-atendimento-vendedor.md): estados e transições entre a grade e a nova consulta.
- [Protótipos](prototipos/README.md): interfaces navegáveis separadas por perfil.
- [Protótipo do lojista](prototipos/prototipo-lojista.html): referência visual desktop/tablet.
- [Protótipo do vendedor](prototipos/prototipo-vendedor.html): referência visual mobile.
- [Decisões de interface](decisoes/decisoes-de-interface.md): limites e decisões usados nos protótipos.
- [Evidências](evidencias/README.md): protocolo e estado das capturas exigidas no cartão.

## Protótipos e implementação

- Os HTMLs são protótipos navegáveis de referência visual, não espelhos da interface Razor atual. Estados e telas podem divergir da aplicação.
- Para rotas, permissões e status vigentes, use o inventário e o código; para regras, use o SRS e os casos de uso.
- O escopo normativo vigente vai até RF-30, UC-15 e US-15. Não inferir que uma capacidade esteja pendente apenas por sua ausência em um protótipo.
