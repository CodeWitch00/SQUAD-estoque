# S4-QA-019 — Defeitos conhecidos, observações e impedimentos

## Defeitos do baseline

### DEF-S3QA030-01 — Grade bloqueada por “Outras numerações” obrigatória sem mensagem visível

| Campo | Valor |
|---|---|
| Severidade original | Alta / bloqueante para o E2E-02 |
| Área | Cadastro de produto e grade (`LOJISTA`) |
| Situação nesta validação | **Não reproduzido no ambiente publicado** |
| Evidência de correção | `NumeracoesExtras` sem `data-val-required`; grade 37/38/39 salva com extras vazios; SKUs criados sem numeração artificial |
| Vínculo | Correção validada; aceite formal depende de `S4-P2-13` |

O defeito foi originalmente registrado em `../../execucoes/sprint-3/s3-qa-030-regressao-e2e.md` e no registro consolidado de validações manuais do repositório. Nenhuma correção foi feita neste cartão; apenas foi confirmado que a correção já está publicada.

### DEF-S3QA030-02 — Texto de confirmação “1 pares”

| Campo | Valor |
|---|---|
| Severidade original | Baixa / cosmética |
| Área | Consulta do vendedor / confirmação de venda |
| Situação nesta validação | Não houve reprodução dedicada. A confirmação capturada na venda exibiu o estado “Último par”. |
| Impacto | Não afeta saldo, movimentação ou autorização |
| Vínculo | Ressalva cosmética conhecida; não bloqueia a apresentação |

### DEF-S3QA030-03 — Mensagem de validação em inglês na entrada

| Campo | Valor |
|---|---|
| Severidade original | Baixa / localização |
| Área | Entrada de estoque (`LOJISTA`) |
| Situação nesta validação | Cenário de quantidade 0 não executado nesta rodada |
| Impacto | A operação inválida era corretamente bloqueada; o defeito é de idioma |
| Vínculo | Ressalva conhecida; não bloqueia a apresentação |

## Observação registrada nesta execução

### OBS-S4QA019-01 — Termo da busca do vendedor não preservado na URL

| Campo | Valor |
|---|---|
| Área | Consulta do vendedor (`/Estoque/Consulta`) |
| Comportamento observado | Ao enviar o formulário de busca, a URL final não manteve o parâmetro `Termo`. A navegação direta com `?termo=T%C3%AAnis` retornou 9 produtos. |
| Avaliação | A busca retornou resultados e a seleção do produto/grade funcionou. Não foi caracterizada falha funcional nesta rodada. |
| Classificação | Observação de rastreabilidade/usabilidade |
| Vínculo | Não foi localizado cartão existente específico. Sugere-se registrar como melhoria, sem bloqueio para apresentação. |

## Impedimentos e pendências

| ID | Descrição | Impacto | Situação |
|---|---|---|---|
| `S4-BE-003` | Rotação/remoção do seed e das credenciais demonstrativas antes de qualquer uso real | Segurança operacional | Pendente |
| `S4-DB-004` | Execução/validação das migrations no Azure | Rastreabilidade de esquema publicado | Pendente; sem acesso a Kudu/CLI nesta sessão |
| `S4-P2-13` | Aceite formal do S3-QA-030 após revisão independente | Bloqueia aceite formal | Pendente |
| `LIM-01` | Capturas de tela indisponíveis no navegador desta sessão | Reduz evidência visual | Registrado |
| `LIM-02` | Sem acesso direto ao banco do App Service | Persistência inferida pelas telas | Registrado |
| `LIM-03` | Sem aparelho móvel físico e sem medição de P95/cold start | Cobertura não funcional incompleta | Registrado |

## Resultado da busca por novos defeitos funcionais

Nenhum novo defeito funcional foi identificado nesta execução. As únicas novas observações são de rastreabilidade de URL e de limitação de evidência, ambas sem bloqueio para a apresentação acadêmica.
