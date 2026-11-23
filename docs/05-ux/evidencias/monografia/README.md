# Capturas da interface para a monografia

Capturas reais da aplicação SQUAD Estoque em **23/11/2026**, com Chromium automatizado por Playwright. Versão do código: commit `dcb6c488b58d88363f6ba2d881b43783f9c14560` da branch `main`.

**Ambiente:** aplicação ASP.NET Core MVC local, em `Development`, com SQLite isolado em `/tmp/squad-monografia-capturas/Estoque.db`. Os produtos, saldos, movimentações, ruptura e XML da NF-e são fictícios e foram criados pelos formulários da aplicação. A NF-e permanece em `RASCUNHO`; sua entrada **não** foi confirmada. O Azure e seu banco publicado não foram utilizados. As contas locais de demonstração não aparecem nas figuras. A automação usou fator de escala 2 e aguardou fontes e imagens antes de capturar.

| Arquivo | Figura sugerida | Perfil e funcionalidade | Rota | Viewport | PNG | Conteúdo |
|---|---|---|---|---|---|---|
| `01-login.png` | Figura 7 — Interface de autenticação | Acesso público; login | `/Account/Login` | 390 × 844 | 780 × 1688 | Fundo fotográfico, logotipo, slogan e formulário sem credenciais preenchidas. |
| `02-vendedor-consulta.png` | Figura 8 — Consulta de produtos pelo vendedor | VENDEDOR; busca automática | `/Estoque/Consulta` | 390 × 844 | 780 × 1688 | Pesquisa por “MONOGRAFIA 2026” e dois modelos retornados. |
| `03-vendedor-grade.png` | Figura 9 — Grade de numerações e disponibilidade por SKU | VENDEDOR; grade | `/Estoque/Consulta` com produto selecionado | 390 × 844 | 780 × 1968 | Modelo Corrida: nº 37 sem estoque, nº 38 último par e nº 39 com quatro pares. Captura de página completa para mostrar os três estados. |
| `04-vendedor-atendimento.png` | Figura 10 — Registro do resultado de atendimento | VENDEDOR; atendimento pendente | `/Estoque/Consulta` com atendimento retomado | 390 × 844 | 780 × 1688 | Nº 39 selecionado e ações Vendeu, Não tinha e Desistiu disponíveis. |
| `05-lojista-produtos.png` | Figura 11 — Interface administrativa de produtos | LOJISTA; catálogo | `/Produtos` | 1440 × 1000 | 2880 × 2000 | Dois modelos fictícios ativos, grades e ações administrativas. |
| `06-lojista-movimentacoes.png` | Figura 12 — Histórico de movimentações de estoque | LOJISTA; movimentações | `/Movimentacoes` | 1440 × 1000 | 2880 × 2000 | Registros demonstrativos de entrada, saída e ajuste, com quantidade e motivo. |
| `07-lojista-rupturas.png` | Figura 13 — Histórico de rupturas de estoque | LOJISTA; ruptura | `/Rupturas` | 1440 × 1000 | 2880 × 2000 | Uma ocorrência de demanda não atendida para o modelo Corrida, nº 37. |
| `08-lojista-entrada-fiscal.png` | Figura 14 — Conferência dos itens da entrada fiscal | LOJISTA; conferência de NF-e | `/EntradasFiscais/Conferir/{id}` | 1440 × 1000 | 2880 × 2000 | XML fictício importado, item e quantidade visíveis, status RASCUNHO e opção de confirmar entrada. |

**Fonte sugerida para todas as figuras:** Elaborado pelos autores (2026).

As quatro imagens mobile foram produzidas com viewport de 390 × 844; `03-vendedor-grade.png` utiliza captura de página completa para manter os três estados na mesma figura. As imagens desktop usam exatamente o viewport de 1440 × 1000. Não havia foto de produto cadastrada no banco local isolado; a identidade visual da aplicação, inclusive o logotipo e o fundo do login, foi preservada.
