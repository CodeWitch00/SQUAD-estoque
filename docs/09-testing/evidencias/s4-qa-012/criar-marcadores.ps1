$ErrorActionPreference = 'Stop'
$base = 'http://localhost:8080'
$marker = 'QA-PERSIST-20261119-S4QA012'
$out = [ordered]@{ marker = $marker; steps = @() }

function Get-Token([string]$html) {
  $m = [regex]::Match($html, 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"')
  if (-not $m.Success) { $m = [regex]::Match($html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"') }
  if (-not $m.Success) { throw 'AntiForgery token not found' }
  return $m.Groups[1].Value
}

function Login([string]$email, [string]$senha) {
  $s = $null
  $r = Invoke-WebRequest -Uri "$base/Account/Login" -UseBasicParsing -SessionVariable s
  $t = Get-Token $r.Content
  $r2 = Invoke-WebRequest -Uri "$base/Account/Login" -Method Post -Body @{ Email=$email; Senha=$senha; LembrarMe='false'; __RequestVerificationToken=$t } -WebSession $s -UseBasicParsing -MaximumRedirection 5
  return $s
}

# Informe credenciais demonstrativas locais por variaveis de ambiente; nao as grave neste arquivo.
$lojEmail = $env:S4QA_LOJISTA_EMAIL
$lojSenha = $env:S4QA_LOJISTA_SENHA
$venEmail = $env:S4QA_VENDEDOR_EMAIL
$venSenha = $env:S4QA_VENDEDOR_SENHA
if (-not $lojEmail -or -not $lojSenha -or -not $venEmail -or -not $venSenha) {
  throw 'Defina S4QA_LOJISTA_EMAIL, S4QA_LOJISTA_SENHA, S4QA_VENDEDOR_EMAIL e S4QA_VENDEDOR_SENHA no ambiente.'
}

# 1) Login LOJISTA e criar produto
$ls = Login $lojEmail $lojSenha
$createPage = Invoke-WebRequest -Uri "$base/Produtos/Create" -WebSession $ls -UseBasicParsing
$ct = Get-Token $createPage.Content
$createResp = Invoke-WebRequest -Uri "$base/Produtos/Create" -Method Post -WebSession $ls -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue -Body @{
  Nome=$marker; Marca='QA-S4QA012'; Categoria='Persistencia'; Cor='TesteAzul'; __RequestVerificationToken=$ct
}
if ($null -eq $createResp) { $createResp = $Error[0].Exception.Response }
$loc = $null
try { $loc = $createResp.Headers.Location } catch {}
if (-not $loc -and $createResp.Headers['Location']) { $loc = $createResp.Headers['Location'] }
$produtoId = $null
if ($loc -match '/Produtos/AdicionarNumeracoes/([0-9a-fA-F-]{36})') { $produtoId = $Matches[1] }
if (-not $produtoId) { throw "Produto nao criado/redirecionado. Location=$loc Status=$($createResp.StatusCode)" }
$out.produtoId = $produtoId
$out.steps += 'produto_criado'

# 2) Adicionar numeracao 40 com saldo inicial 5 (gera SKU + movimentacao ENTRADA)
$addUrl = "$base/Produtos/AdicionarNumeracoes/$produtoId"
$addPage = Invoke-WebRequest -Uri $addUrl -WebSession $ls -UseBasicParsing
$at = Get-Token $addPage.Content
$body = @{
  ProdutoId=$produtoId; CadastroInicial='true'; NumeracoesExtras=''; __RequestVerificationToken=$at
}
for ($i=0; $i -lt 11; $i++) {
  $num = (34+$i).ToString()
  $body["Opcoes[$i].Numeracao"] = $num
  $sel = if ($num -eq '40') { 'true' } else { 'false' }
  $qtd = if ($num -eq '40') { '5' } else { '0' }
  $body["Opcoes[$i].Selecionada"] = $sel
  $body["Opcoes[$i].Quantidade"] = $qtd
}
$addResp = Invoke-WebRequest -Uri $addUrl -Method Post -WebSession $ls -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue -Body $body
if ($null -eq $addResp) { $addResp = $Error[0].Exception.Response }
$addStatus = $addResp.StatusCode
$out.addGradeStatus = [int]$addStatus
$out.steps += 'grade_adicionada'

# 3) Descobrir SKU e registrar entrada extra (+3) para historico adicional
Start-Sleep -Milliseconds 300
$sqlSku = "SELECT Id, Numeracao, SaldoAtual FROM Sku WHERE ProdutoId='$produtoId' AND Numeracao='40';"
$skuInfo = docker run --rm -v squad-estoque-data:/data:ro alpine:3.22 sh -c "apk add --no-cache sqlite >/dev/null 2>&1; sqlite3 -separator '|' -noheader /data/Estoque.db \"$sqlSku\"" 2>&1 | Where-Object { $_ -match '^[0-9a-fA-F-]{36}\|' } | Select-Object -Last 1
$skuParts = ($skuInfo -as [string]).Trim().Split('|')
$skuId = $skuParts[0]
if (-not $skuId) { throw "SKU nao encontrado para produto $produtoId. raw=$skuInfo" }
$out.skuId = $skuId
$out.saldoAposGrade = $skuParts[2]
$entPage = Invoke-WebRequest -Uri "$base/Movimentacoes/Entrada?skuId=$skuId" -WebSession $ls -UseBasicParsing
$et = Get-Token $entPage.Content
$entResp = Invoke-WebRequest -Uri "$base/Movimentacoes/Entrada" -Method Post -WebSession $ls -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue -Body @{
  Tipo='ENTRADA'; SkuId=$skuId; Quantidade='3'; Motivo='QA-PERSIST entrada extra 2026-11-19'; __RequestVerificationToken=$et
}
if ($null -eq $entResp) { $entResp = $Error[0].Exception.Response }
$out.entradaExtraStatus = [int]$entResp.StatusCode
$out.steps += 'entrada_extra'

# 4) Registrar ruptura como VENDEDOR no mesmo SKU (fluxo Estoque/RegistrarNaoTinha)
$vs = Login $venEmail $venSenha
$consultaPage = Invoke-WebRequest -Uri "$base/Estoque/Consulta" -WebSession $vs -UseBasicParsing
$vt = Get-Token $consultaPage.Content
$rupResp = Invoke-WebRequest -Uri "$base/Estoque/RegistrarNaoTinha" -Method Post -WebSession $vs -UseBasicParsing -Body @{
  skuId=$skuId; produtoId=$produtoId; __RequestVerificationToken=$vt
}
$out.rupturaStatus = [int]$rupResp.StatusCode
$out.rupturaBody = ($rupResp.Content | Out-String).Trim()
$out.steps += 'ruptura_registrada'

$out | ConvertTo-Json -Depth 6
