# S3-BE-007 - ViewModel de saldos zerados

## Escopo e rastreabilidade

Contrato de exibição para RF-21, UC-11 e US-12. Esta entrega prepara os dados
do lojista; a consulta ao banco, o controller, a autorização e a tela ficam
para as respectivas entregas. Não implementa integralmente RF-21.

Arquivos:

- `src/SquadEstoque.Web/Models/ViewModels/SaldosZeradosViewModel.cs`;
- `tests/SquadEstoque.Web.Tests/SaldosZeradosViewModelTests.cs`.

## Contrato

`SaldosZeradosViewModel.Produtos` contém `ProdutoSaldoZeradoViewModel`, com
`Id`, `Nome`, `Marca`, `Cor` e `NumeracoesZeradas`. As coleções são inicializadas
vazias. Nenhuma propriedade expõe entidades EF ou navegações de persistência.

`Criar` recebe uma coleção já materializada do ViewModel existente
`ProdutoConsultaResultadoViewModel`, reaproveitando os dados de produto e SKU.
O chamador deve fornecer metadados coerentes para cada Id de produto; se um
produto aparecer mais de uma vez, seus SKUs serão combinados e os metadados da
primeira ocorrência serão usados. A definição dos registros consultados e das
regras de atividade pertence à futura consulta, não a este contrato.

Regras de composição:

- agrupar por Id, preservando produtos homônimos como grupos distintos;
- selecionar apenas `SaldoAtual == 0`, sem incluir saldo positivo ou negativo;
- remover numerações repetidas do mesmo produto;
- omitir produtos que não possuem numeração zerada;
- ordenar produtos por Nome, Marca, Cor e, para desempate, Id;
- ordenar textos com `StringComparer.Ordinal`, independentemente da cultura;
- preservar a entrada e materializar novas coleções de saída.

A numeração é texto no domínio. A ordenação é explicitamente textual, não
numérica: `10`, `35/36`, `40`, `9`. Isso preserva faixas e outros formatos sem
inventar normalização de numeração neste cartão.

## Exemplo reproduzível

```csharp
var produtos = new[]
{
    new ProdutoConsultaResultadoViewModel
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Nome = "Tênis Urban",
        Marca = "Marca A",
        Cor = "Preto",
        Skus = new()
        {
            new() { Numeracao = "40", SaldoAtual = 0 },
            new() { Numeracao = "38", SaldoAtual = 0 },
            new() { Numeracao = "39", SaldoAtual = 2 }
        }
    }
};
var modelo = SaldosZeradosViewModel.Criar(produtos);
// Um produto, com NumeracoesZeradas = ["38", "40"].
```

## Validação

Os testes de composição usam apenas objetos em memória. Cobrem filtro exato,
metadados, produtos sem saldo zero, homônimos, repetição de produto e numeração,
ordenação com desempates, culturas pt-BR/en-US/tr-TR, preservação da entrada,
coleção vazia e rejeição de entrada nula.

Execução padrão:

```bash
dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --configuration Release --no-build
```

Na validação local em Windows com SDK 10.0.401, houve travamento do compilador.
A baseline executou 203 testes aprovados após compilação diagnóstica sem
analisadores. Para a implementação final, limitar `DOTNET_PROCESSOR_COUNT=2`
permitiu compilar com os analisadores habilitados, sem mudar os projetos.
O build final usou `--no-restore --disable-build-servers
-p:UseSharedCompilation=false -m:1`. O restore emitiu NU1900 por indisponibilidade
da consulta de vulnerabilidades do NuGet. O CI preserva sua configuração original
e deve ser conferido antes do merge.

Resultado local final: **214 testes aprovados, zero falhas e zero ignorados**,
incluindo 11 casos deste cartão. A execução usou `--collect:"XPlat Code Coverage"`
e gerou relatórios TRX e Cobertura para o PDF de evidências.

Não há mudança visual a homologar neste cartão. A revisão manual consiste em
conferir o contrato, a ausência de acesso ao banco e o diff restrito ao escopo.
A revisão da mantenedora, o merge e a validação integrada continuam necessários.

## Governança

Foi autorizada especificamente a branch `feat/s3-be-007-felipe`, como exceção
ao uso de branch pessoal previsto no CONTRIBUTING atual. O Trello não foi
alterado nesta execução.
