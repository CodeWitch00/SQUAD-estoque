param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../TestResults/rnf02')
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$output = [System.IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path (Join-Path $output 'resultado.json')) {
    throw 'O destino já contém medição. Escolha outra pasta para preservar todas as execuções.'
}
Push-Location $repo
$oldPerf = $env:RNF02_PERF
$oldOutput = $env:RNF02_OUTPUT
$oldCommit = $env:RNF02_COMMIT
try {
    $env:RNF02_PERF = '1'
    $env:RNF02_OUTPUT = $output
    $env:RNF02_COMMIT = git rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Não foi possível identificar o commit.' }
    dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj `
        --configuration Release --filter 'FullyQualifiedName~ConsultaP95Tests' `
        --logger 'console;verbosity=normal'
    if ($LASTEXITCODE -ne 0) { throw "Medição reprovada ou execução incompleta. Consulte $output e o log." }
    Write-Output "Evidências RNF-02: $output"
}
finally {
    $env:RNF02_PERF = $oldPerf
    $env:RNF02_OUTPUT = $oldOutput
    $env:RNF02_COMMIT = $oldCommit
    Pop-Location
}
