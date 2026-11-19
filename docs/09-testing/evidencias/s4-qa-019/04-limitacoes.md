# S4-QA-019 — Limitações da execução

| Limitação | Detalhe | Efeito no parecer |
|---|---|---|
| Capturas de tela | `browser.screenshot` falhou com `Screenshot needs a visible tab. Call browser.tabs.focus and keep its desktop window visible.` | Evidência textual/HTTP/DOM substitui a visual; não impede a validação funcional, mas reduz a rastreabilidade visual |
| Banco de dados | Sem acesso a SQLite do App Service, Kudu ou Azure CLI | Persistência foi comprovada pelas telas `/Produtos/Details`, `/Movimentacoes`, `/Rupturas`, `/Atendimentos` e `/SaldosZerados`; não houve consulta SQL direta |
| Ambiente | Azure compartilhado de homologação/demonstração foi usado como “produção” de referência | Não havia produção autônoma autorizada para escrita; toda a escrita foi em produto fictício |
| Conferência de par | Este agente não pode registrar a revisão de outro integrante | Aceite formal permanece pendente (`S4-P2-13`) |
| Dispositivo móvel | Não houve aparelho físico nem registro de viewports nesta rodada | Não afeta os fluxos funcionais executados, mas mantém pendente a validação móvel complementar |
| Desempenho | P95, cold start e carga não foram medidos | Fora do escopo deste cartão |
| Credenciais | Contas demonstrativas compartilhadas continuam ativas | Pendência `S4-BE-003` |
| Migrações | Não foi possível validar migrations no Azure | Pendência `S4-DB-004` |
| Defeitos antigos | `DEF-S3QA030-02` e `DEF-S3QA030-03` não foram reproduzidos de forma dedicada | Permanecem como ressalvas conhecidas de baixa severidade |
| Alteração de repositório | Nenhuma. Todos os artefatos ficaram na raiz do workspace | Atende à regra do cartão |

## Escopo não coberto neste cartão

- emissão de nota fiscal, PDV, financeiro, ERP, integração externa e multi-loja — fora do escopo do produto conforme o plano de testes;
- testes unitários e de integração da CI — não reexecutados aqui, pois este cartão é de validação final em ambiente publicado;
- correção de defeitos — expressamente fora deste cartão.
