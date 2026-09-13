using System.ComponentModel.DataAnnotations;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class AtendimentoVendedorViewModelTests
{
    [Theory]
    [InlineData(ResultadoAtendimento.Vendeu)]
    [InlineData(ResultadoAtendimento.NaoTinha)]
    [InlineData(ResultadoAtendimento.Desistiu)]
    public void Resultado_documentado_e_aceito(ResultadoAtendimento resultado)
    {
        var model = ModeloValido();
        model.Resultado = resultado;

        var erros = Validar(model);

        Assert.Empty(erros);
    }

    [Fact]
    public void Resultado_pode_nao_ser_informado()
    {
        var model = ModeloValido();
        model.Resultado = null;

        var erros = Validar(model);

        Assert.Empty(erros);
    }

    [Fact]
    public void Resultado_fora_dos_valores_documentados_e_rejeitado()
    {
        var model = ModeloValido();
        model.Resultado = (ResultadoAtendimento)99;

        var erros = Validar(model);

        Assert.Contains(erros, erro =>
            erro.MemberNames.Contains(nameof(model.Resultado)) &&
            erro.ErrorMessage == "Informe um resultado de atendimento válido.");
    }

    [Theory]
    [InlineData(ResultadoAtendimento.Vendeu)]
    [InlineData(ResultadoAtendimento.NaoTinha)]
    public void Venda_ou_ruptura_exige_sku(ResultadoAtendimento resultado)
    {
        var model = ModeloValido();
        model.SkuId = null;
        model.Resultado = resultado;

        var erros = Validar(model);

        Assert.Contains(erros, erro =>
            erro.MemberNames.Contains(nameof(model.SkuId)) &&
            erro.ErrorMessage == "Selecione uma numeração para informar Vendeu ou Não tinha.");
    }

    [Fact]
    public void Desistencia_nao_exige_sku()
    {
        var model = ModeloValido();
        model.SkuId = null;
        model.Numeracao = null;
        model.SaldoAtual = null;
        model.Resultado = ResultadoAtendimento.Desistiu;

        var erros = Validar(model);

        Assert.Empty(erros);
    }

    [Fact]
    public void Identificadores_vazios_sao_rejeitados_com_mensagens_compreensiveis()
    {
        var model = ModeloValido();
        model.ProdutoId = Guid.Empty;
        model.SkuId = Guid.Empty;

        var erros = Validar(model);

        Assert.Contains(erros, erro => erro.ErrorMessage == "Selecione o produto consultado.");
        Assert.Contains(erros, erro => erro.ErrorMessage == "Selecione um SKU válido.");
    }

    [Fact]
    public void Dados_de_exibicao_invalidos_sao_rejeitados()
    {
        var model = ModeloValido();
        model.ProdutoNome = string.Empty;
        model.Numeracao = new string('1', 31);
        model.SaldoAtual = -1;

        var erros = Validar(model);

        Assert.Contains(erros, erro => erro.MemberNames.Contains(nameof(model.ProdutoNome)));
        Assert.Contains(erros, erro => erro.MemberNames.Contains(nameof(model.Numeracao)));
        Assert.Contains(erros, erro => erro.MemberNames.Contains(nameof(model.SaldoAtual)));
    }

    private static AtendimentoVendedorViewModel ModeloValido() => new()
    {
        ProdutoId = Guid.NewGuid(),
        ProdutoNome = "Tênis Runner Pro",
        SkuId = Guid.NewGuid(),
        Numeracao = "37",
        SaldoAtual = 1
    };

    private static IReadOnlyList<ValidationResult> Validar(AtendimentoVendedorViewModel model)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), resultados, true);
        return resultados;
    }
}
