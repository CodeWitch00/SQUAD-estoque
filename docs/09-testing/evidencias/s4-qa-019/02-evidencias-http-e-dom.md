# S4-QA-019 — Evidências HTTP, URL e DOM

Registros textuais coletados em 19/11/2026 no ambiente publicado. Não há capturas de tela nesta sessão; veja `04-limitacoes.md`.

## 1. Health check, HTTPS e HSTS — QA-01

Requisição HTTPS:

```text
GET https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net/health
Status: 200
Corpo: Healthy
server: Kestrel
strict-transport-security: max-age=2592000
cache-control: no-store, no-cache
date: Thu, 19 Nov 2026 16:55:47 GMT
```

Requisição HTTP:

```text
GET http://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net/health
Status: 307
location: https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net/health
non-authoritative-reason: HSTS
```

A requisição HTTPS subsequente retornou 200 com `Healthy`.

## 2. Acesso anônimo, login inválido e logout — QA-02, QA-03, QA-09

```text
Anônimo: GET /Produtos
→ /Account/Login?ReturnUrl=%2FProdutos

Login inválido:
POST /Account/Login com conta inexistente
→ permaneceu em /Account/Login
→ alerta: "E-mail ou senha inválidos."

Logout:
POST /Account/Logout
→ /Account/Login

Após logout: GET /Produtos
→ /Account/Login?ReturnUrl=%2FProdutos
```

Não foram registrados senha, token ou cookie.

## 3. Perfis e autorização — QA-04 a QA-08

```text
LOJISTA: login → /Produtos
VENDEDOR: login → /Estoque/Consulta

LOJISTA:
/Produtos                         → 200
/Movimentacoes                    → 200
/Movimentacoes/Entrada            → 200
/Movimentacoes/Ajuste             → 200
/SaldosZerados                    → 200
/Rupturas                         → 200
/Atendimentos                     → 200
/Estoque/Consulta                 → /Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta

VENDEDOR:
/Produtos                         → /Account/AccessDenied?ReturnUrl=...
/Movimentacoes                    → /Account/AccessDenied?ReturnUrl=...
/Movimentacoes/Entrada            → /Account/AccessDenied?ReturnUrl=...
/Movimentacoes/Ajuste             → /Account/AccessDenied?ReturnUrl=...
/SaldosZerados                    → /Account/AccessDenied?ReturnUrl=...
/Rupturas                         → /Account/AccessDenied?ReturnUrl=...
/Atendimentos                     → /Account/AccessDenied?ReturnUrl=...
```

## 4. Versão publicada da interface do vendedor — QA-10

Marcadores presentes no DOM publicado em `/Estoque/Consulta`:

```text
Vendeu
Não tinha
Desistiu
skuSelecionado
Nova consulta
Estados exibidos: "Último par", "Disponível", "Indisponível"
```

Isso afasta a versão antiga encontrada em homologações anteriores, que não continha seleção de SKU nem os resultados de atendimento.

## 5. Cadastro da grade sem workaround — QA-11

No formulário de grade, o campo de numerações extras foi inspecionado:

```text
NumeracoesExtras: sem atributo data-val-required
```

A grade foi salva com “Outras numerações” vazia. O produto controlado passou a exibir:

```text
QA S4 FINAL 2026-11-19
Marca QA Final · Tênis · Azul

NUMERAÇÃO  SALDO ATUAL       ÚLTIMA ATUALIZAÇÃO   ESTADO
37         1 par(es)         19/11/2026 16:59      Último par
38         2 par(es)         19/11/2026 16:59      Disponível
39         0 par(es)         Atualização não registrada  Indisponível
```

Não foi informada uma numeração artificial 45, ao contrário da execução original do `DEF-S3QA030-01`.

## 6. Confirmações do vendedor — QA-12 e QA-14

Venda:

```text
SKU 38 — 2 pares → confirmação exibida com estado "Último par"
Atendimento: 0ed9a7f4-70d6-430b-bf52-53973b8c06a3
Data/hora: 19/11/2026 17:00
```

Desistência:

```text
Atendimento encerrado.
Nenhuma movimentação foi registrada.
Atendimento: 82b407ce-2da4-499d-9c0d-3f11ae845e9e
URL final: /Estoque/Consulta?atendimentoId=82b407ce-2da4-499d-9c0d-3f11ae845e9e#grade
```

Após a desistência, a grade do SKU 37 continuou com 1 par.

## 7. Movimentação de saída e ruptura — QA-12 e QA-13

Trecho de `/Movimentacoes`:

```text
19/11/2026 17:00:42  QA S4 FINAL 2026-11-19  38  SAÍDA    1  Vendedor Teste  —
19/11/2026 16:59:28  QA S4 FINAL 2026-11-19  38  ENTRADA  2  Lojista Teste   Saldo inicial no cadastro da grade
19/11/2026 16:59:28  QA S4 FINAL 2026-11-19  37  ENTRADA  1  Lojista Teste   Saldo inicial no cadastro da grade
```

Não há linha de saída para a numeração 39.

## 8. Rupturas e saldos zerados — QA-13, QA-16 e QA-17

`/Rupturas`:

```text
Modelo                         Numeração  Ocorrências  Última ruptura (UTC)
QA S4 FINAL 2026-11-19         39         1            19/11/2026 17:01
Marca QA Final · Azul
```

`/SaldosZerados`:

```text
QA S4 FINAL 2026-11-19
Marca QA Final · Azul

Numerações sem saldo:
39
Atualização não registrada
```

## 9. Atendimentos rastreáveis — QA-18

Trecho de `/Atendimentos`:

```text
Início            Vendedor         Modelo                    Numeração  Resultado    Saldo consultado
19/11/2026 17:01  Vendedor Teste   QA S4 FINAL 2026-11-19    37         Desistiu     1 par(es)
19/11/2026 17:01  Vendedor Teste   QA S4 FINAL 2026-11-19    39         Não tinha    0 par(es)
19/11/2026 17:00  Vendedor Teste   QA S4 FINAL 2026-11-19    38         Vendeu       2 par(es)
```

Resumo da página: `Pendentes 1`, `Concluídos 19`, `Alertas para revisar 14`. O alerta `Encerrado em menos de 5 segundos` apareceu para o atendimento de `Não tinha`, comportamento de auditoria já conhecido do baseline.

## 10. Ajuste manual e histórico — QA-15 e QA-19

Formulário antes do envio:

```text
Item selecionado: QA S4 FINAL 2026-11-19 (Marca QA Final - Azul)
Numeração: 37
Saldo atual: 1 par(es)
Novo Saldo Real em Loja (Recontagem Físico): 1
Justificativa do Ajuste (Obrigatório, mín. 5 caracteres): vazio
```

Valores enviados:

```text
Novo saldo: 3
Motivo: QA S4 FINAL - recontagem controlada 2026-11-19
```

Depois do envio, a aplicação retornou a `/Produtos/Details/026b3292-...` e exibiu:

```text
37  3 par(es)  19/11/2026 17:03  Disponível
38  1 par(es)  19/11/2026 17:00  Último par
39  0 par(es)  Atualização não registrada  Indisponível
```

Histórico:

```text
19/11/2026 17:03:58  QA S4 FINAL 2026-11-19  37  AJUSTE  2  Lojista Teste
                     Motivo: QA S4 FINAL - recontagem controlada 2026-11-19
```

Documento `/Movimentacoes` respondeu HTTP 200; os recursos estáticos retornaram 200/304.

## 11. Não exposição de credenciais — QA-20

- As senhas das contas demonstrativas não foram gravadas neste pacote.
- O cabeçalho `Set-Cookie` foi redacionado pela ferramenta de inspeção e não foi copiado.
- Tokens antiforgery não foram copiados.
- A revisão textual dos artefatos deste pacote confirmou a ausência de senhas, hashes e cookies completos.

## 12. Limitações evidenciadas — QA-21 a QA-25

Falha de captura:

```text
Screenshot needs a visible tab. Call browser.tabs.focus and keep its desktop window visible.
```

Acesso a banco: não disponível por SQLite local, Kudu ou Azure CLI nesta sessão.

Conferência de par: não executada por este agente. O checklist está em `05-checklist-conferencia-par.md`.
