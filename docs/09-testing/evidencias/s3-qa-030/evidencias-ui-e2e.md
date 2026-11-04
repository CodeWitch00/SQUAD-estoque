# Evidências de UI e HTTP — S3-QA-030 (v2)

**Execução:** 03/11/2026, entre 23:10 e 23:16 (America/Sao_Paulo)
**Baseline:** `main` `9111713` + correção `ccd27fb`
**Navegador:** OpenCode 2.2.5 / Chrome 152.0.7977.167 / Electron 44.9.1, viewport 1000×700
**Observação:** capturas de tela não puderam ser geradas (`browser.screenshot` retorna “Screenshot needs a visible tab”). Os registros abaixo são snapshots de texto/DOM, URLs, códigos HTTP e consultas ao SQLite. Nenhuma senha, hash ou e-mail é reproduzido.

## 1. Login, logout e autorização

### Login do LOJISTA

- URL inicial: `http://localhost:5186/Account/Login?ReturnUrl=%2F`
- Após autenticar: `http://localhost:5186/Produtos`
- Trecho da tela: `Catálogo de Produtos` / `Consulte os modelos, as numerações e o estado de cada produto.`

### Logout do LOJISTA

- Ação: POST no formulário `/Account/Logout`
- URL final: `http://localhost:5186/Account/Login`
- Trecho da tela: `Bem-vindo` / `E-mail` / `Senha` / `Entrar`

### Rotas anônimas (HTTP 302)

| Rota | Status | Location |
|---|---|---|
| `/Produtos` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FProdutos` |
| `/Estoque/Consulta` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FEstoque%2FConsulta` |
| `/Movimentacoes/Entrada` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FMovimentacoes%2FEntrada` |
| `/Movimentacoes/Ajuste` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FMovimentacoes%2FAjuste` |
| `/SaldosZerados` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FSaldosZerados` |
| `/Rupturas` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FRupturas` |
| `/Atendimentos` | 302 | `http://localhost:5186/Account/Login?ReturnUrl=%2FAtendimentos` |

### VENDEDOR em rotas administrativas

Requisições autenticadas com o perfil VENDEDOR, seguindo redirecionamento:

| Rota solicitada | URL final | Conteúdo |
|---|---|---|
| `/Produtos` | `/Account/AccessDenied?ReturnUrl=%2FProdutos` | `Acesso Não Autorizado` |
| `/Movimentacoes` | `/Account/AccessDenied?ReturnUrl=%2FMovimentacoes` | `Acesso Não Autorizado` |
| `/Movimentacoes/Entrada` | `/Account/AccessDenied?ReturnUrl=%2FMovimentacoes%2FEntrada` | `Acesso Não Autorizado` |
| `/Movimentacoes/Ajuste` | `/Account/AccessDenied?ReturnUrl=%2FMovimentacoes%2FAjuste` | `Acesso Não Autorizado` |
| `/SaldosZerados` | `/Account/AccessDenied?ReturnUrl=%2FSaldosZerados` | `Acesso Não Autorizado` |
| `/Rupturas` | `/Account/AccessDenied?ReturnUrl=%2FRupturas` | `Acesso Não Autorizado` |
| `/Atendimentos` | `/Account/AccessDenied?ReturnUrl=%2FAtendimentos` | `Acesso Não Autorizado` |

### LOJISTA na consulta do vendedor

- Requisição: `GET /Estoque/Consulta`
- URL final: `http://localhost:5186/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta`
- Conteúdo: `Acesso Não Autorizado`

## 2. E2E-02 — grade oficial 37–40 sem workaround

### Produto criado

- Nome: `Tênis E2E QA V2`
- ID: `63A0854A-5CD0-4FFE-B6D8-CC89031B8608`
- Marca: `Marca QA`
- Categoria: `Tênis`
- Cor: `Azul`
- URL após salvar os dados: `/Produtos/AdicionarNumeracoes/63a0854a-5cd0-4ffe-b6d8-cc89031b8608?cadastroInicial=True`

### Estado do campo “Outras numerações” antes do envio

Inspeção do DOM com o campo vazio:

```json
{
  "required": false,
  "validationMessage": "",
  "formValid": true,
  "extrasValue": ""
}
```

### Envio da grade

- Tamanhos marcados: 37 (1), 38 (2), 39 (1), 40 (0)
- “Outras numerações”: vazio
- Requisição observada:

```text
POST http://localhost:5186/Produtos/AdicionarNumeracoes/63a0854a-5cd0-4ffe-b6d8-cc89031b8608
Status: 302
```

- URL final: `/Produtos/Details/63a0854a-5cd0-4ffe-b6d8-cc89031b8608`
- Trecho da tela:

```text
Tênis E2E QA V2
Marca QA · Tênis · Azul
Grade de numerações
37  1 par(es)  03/11/2026 23:11  Último par
38  2 par(es)  03/11/2026 23:11  Disponível
39  1 par(es)  03/11/2026 23:11  Último par
40  0 par(es)  Atualização não registrada  Indisponível
```

Nenhuma numeração 45 foi criada.

## 3. Entradas

### Entrada individual no 38

- URL: `/Movimentacoes/Entrada?skuId=1232624f-3e5c-4ef2-a7ab-6f09c70d08c2`
- Saldo antes: `2 par(es)`
- Quantidade: 1
- Motivo: `Entrada individual E2E v2`
- Após confirmar: `/Produtos/Details/63a0854a-...`
- Saldo do 38 na tela: `3 par(es)` às `23:12`

### Entrada em lote no 40

- URL: `/Movimentacoes/EntradaLote?produtoId=63a0854a-...`
- Saldo antes do 40: `0 par(es)`
- Quantidade recebida: 2
- Observação: `Entrada em lote E2E v2`
- Após confirmar: `/Produtos/Details/63a0854a-...`
- Saldo do 40 na tela: `2 par(es)` às `23:12`

### Quantidade zero na entrada

- Campo `#Quantidade` com valor `0`
- `form.checkValidity()`: `false`
- `validationMessage`: `O valor deve ser maior ou igual a 1.`
- Nenhum POST foi enviado.

## 4. Ajustes

### 39: 1 → 3

- URL: `/Movimentacoes/Ajuste?skuId=18b71c60-6f5c-4937-be4d-7baf4a3e9cd8`
- Saldo antes: `1 par(es)`
- Novo saldo: `3`
- Motivo: `Ajuste E2E v2 de inventario`
- Resultado: `/Produtos/Details/63a0854a-...` com `39  3 par(es)`

### 40: 2 → 0

- URL: `/Movimentacoes/Ajuste?skuId=cf3c93a8-11d5-4f64-bbcd-1a4998d37eaa`
- Saldo antes: `2 par(es)`
- Novo saldo: `0`
- Motivo: `Ajuste E2E v2 para zerar tamanho 40`
- Resultado: `/Produtos/Details/63a0854a-...` com `40  0 par(es)` e estado `Indisponível`

### Ajuste sem alteração

- SKU 39, saldo atual 3, novo saldo 3
- Motivo: `Teste sem alteracao v2`
- Resultado: permaneceu em `/Movimentacoes/Ajuste`
- Mensagem visível: `Não há alteração de saldo a ser registrada.`
- Nenhuma movimentação nova.

## 5. Consulta e venda

### Busca

| Termo | Resultado |
|---|---|
| `a` | `Digite pelo menos 2 caracteres para buscar.` |
| `zzz` | `Produto não encontrado.` |
| `Tênis` | `1 produto encontrado` / `Tênis E2E QA V2` |

### Grade do vendedor

```text
Disponíveis 3
Nº 37  Último par   STATUS: ÚLTIMO PAR
Nº 38  3 pares      STATUS: DISPONÍVEL
Nº 39  3 pares      STATUS: DISPONÍVEL
Indisponíveis 1
Nº 40  Sem estoque  STATUS: INDISPONÍVEL
```

### Venda 1 no 38

- POST `http://localhost:5186/Estoque/Vender` → 200
- Saldo 3 → 2
- Mensagem: `Venda registrada para o nº 38.` / `Saldo atualizado: 2 pares. Voltando para a busca…`
- Atendimento: `08041A65-DC67-4A91-BE13-096AB3AFBC7B`

### Venda 2 no 38

- POST `http://localhost:5186/Estoque/Vender` → 200
- Saldo 2 → 1
- Mensagem: `Venda registrada para o nº 38.` / `Saldo atualizado: 1 pares. Voltando para a busca…`
- Atendimento: `4A274547-7D6D-499D-8930-1E4E522EEFF1`
- O card do 38 passou a `Último par` e recebeu a classe `consulta-grade-item--ultimo-par`.

## 6. Ruptura

Três ações no tamanho 40, com saldo 0:

```text
POST http://localhost:5186/Estoque/RegistrarNaoTinha → 200
Mensagem: Falta registrada para o nº 40.
          O saldo do sistema não foi alterado. Voltando para a busca…
```

- Saldo do 40 permaneceu 0 nas três vezes.
- Atendimentos: `2FDE8C14-C7BF-4D44-85CF-A9BC36155572`, `72FE7D40-3B81-493E-9663-A3BD12432609`, `968FCC7A-AB9D-4EAE-AF2C-56B40D0B399B`, todos `NAO_TINHA` com `SaldoConsultado=0`.
- Nenhuma movimentação de saída foi criada.

## 7. Desistiu

- SKU 39, saldo 3
- Seleção persistida: POST `/Estoque/SelecionarSku` → 200
- Atendimento: `888E1B29-A2F0-494B-AACD-11BBEBB305C4`
- Ação `Desistiu`: POST `/Estoque/RegistrarDesistiu` → 200
- Tela final: `/Estoque/Consulta`

```text
Atendimento encerrado.
Nenhuma movimentação foi registrada.
Qual modelo o cliente procura?
```

- Saldo do 39 permaneceu 3.
- Nenhuma movimentação e nenhuma ruptura foram criadas.
- Nova busca abriu o produto e a grade novamente.

## 8. Saldos zerados, rupturas e atendimentos

### `/SaldosZerados`

```text
Saldos zerados para reposição
Tênis E2E QA V2
Marca QA · Azul
Numerações sem saldo:
40
Atualizado em 03/11/2026 23:12
```

Somente o 40. Os tamanhos 37 (1), 38 (1) e 39 (3) não aparecem.

### `/Rupturas`

```text
Histórico de rupturas por modelo e numeração. Datas e horários em UTC.
Modelo            Numeração  Ocorrências  Última ruptura (UTC)
Tênis E2E QA V2   40         3            04/11/2026 02:15
```

### `/Atendimentos`

```text
Pendentes 0
Concluídos 6
Alertas para revisar 4

23:15  Vendedor Teste  39  Desistiu   3 par(es)  —
23:15  Vendedor Teste  40  Não tinha  0 par(es)  Encerrado em menos de 5 segundos
23:14  Vendedor Teste  40  Não tinha  0 par(es)  Encerrado em menos de 5 segundos
23:14  Vendedor Teste  40  Não tinha  0 par(es)  Encerrado em menos de 5 segundos
23:14  Vendedor Teste  38  Vendeu     2 par(es)  Encerrado em menos de 5 segundos
23:13  Vendedor Teste  38  Vendeu     3 par(es)  —
```

Os quatro alertas de “menos de 5 segundos” são efeito da execução assistida, que clicou mais rápido que uma pessoa.

### `/Movimentacoes`

```text
Mostrando 1–9 de 9 movimentações
23:14:21  38  SAÍDA   1  Vendedor Teste  —
23:14:11  38  SAÍDA   1  Vendedor Teste  —
23:12:32  40  AJUSTE  2  Lojista Teste   Ajuste E2E v2 para zerar tamanho 40
23:12:28  39  AJUSTE  2  Lojista Teste   Ajuste E2E v2 de inventario
23:12:16  40  ENTRADA 2  Lojista Teste   Entrada em lote E2E v2
23:12:06  38  ENTRADA 1  Lojista Teste   Entrada individual E2E v2
23:11:45  39  ENTRADA 1  Lojista Teste   Saldo inicial no cadastro da grade
23:11:45  38  ENTRADA 2  Lojista Teste   Saldo inicial no cadastro da grade
23:11:45  37  ENTRADA 1  Lojista Teste   Saldo inicial no cadastro da grade
```

## 9. Estado final conferido no SQLite

```text
Produtos: 1
SKUs: 4
Movimentações: 9
Atendimentos: 6 (6 concluídos)
Rupturas: 3
Usuários: 2

Saldos: 37=1, 38=1, 39=3, 40=0
```

Exportação completa e sanitizada: [`estado-final-banco.json`](estado-final-banco.json).

## 10. Checklist de conferência da evidência

- [ ] O campo “Outras numerações” estava vazio e o POST da grade retornou 302.
- [ ] Os quatro SKUs 37–40 existem, sem 45.
- [ ] As 9 movimentações e os 6 atendimentos conferem com as tabelas acima.
- [ ] As 3 rupturas são do tamanho 40 e não geraram saída.
- [ ] O `Desistiu` do 39 não alterou saldo nem criou ruptura.
- [ ] Nenhuma senha, hash ou e-mail aparece neste arquivo ou no JSON.
