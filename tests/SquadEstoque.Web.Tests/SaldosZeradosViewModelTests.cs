using System.Globalization;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public class SaldosZeradosViewModelTests
{
    [Fact]
    public void Criar_inclui_apenas_saldo_exatamente_zero_e_preserva_dados_do_produto()
    {
        var produto = Produto("Tênis", ("38", 0), ("39", 1), ("40", 3), ("41", -1));

        var resultado = SaldosZeradosViewModel.Criar(new[] { produto });

        var item = Assert.Single(resultado.Produtos);
        Assert.Equal(produto.Id, item.Id);
        Assert.Equal("Tênis", item.Nome);
        Assert.Equal("Marca", item.Marca);
        Assert.Equal("Preto", item.Cor);
        Assert.Equal(new[] { "38" }, item.NumeracoesZeradas);
    }

    [Fact]
    public void Criar_omite_produtos_sem_sku_zerado()
    {
        var resultado = SaldosZeradosViewModel.Criar(new[]
        {
            Produto("Sem grade"),
            Produto("Disponível", ("38", 1), ("39", 4))
        });

        Assert.Empty(resultado.Produtos);
    }

    [Fact]
    public void Criar_mantem_produtos_homonimos_em_grupos_distintos()
    {
        var primeiro = Produto("Tênis", ("38", 0));
        var segundo = Produto("Tênis", ("39", 0));

        var resultado = SaldosZeradosViewModel.Criar(new[] { primeiro, segundo });

        Assert.Equal(2, resultado.Produtos.Count);
        Assert.Equal(new[] { "38" }, resultado.Produtos.Single(p => p.Id == primeiro.Id).NumeracoesZeradas);
        Assert.Equal(new[] { "39" }, resultado.Produtos.Single(p => p.Id == segundo.Id).NumeracoesZeradas);
    }

    [Fact]
    public void Criar_agrupa_repeticoes_do_produto_sem_duplicar_numeracoes()
    {
        var primeiro = Produto("Tênis", ("40", 0), ("38", 0));
        var repetido = Produto("Tênis", ("38", 0), ("39", 0));
        repetido.Id = primeiro.Id;

        var resultado = SaldosZeradosViewModel.Criar(new[] { primeiro, repetido });

        Assert.Equal(new[] { "38", "39", "40" }, Assert.Single(resultado.Produtos).NumeracoesZeradas);
    }

    [Fact]
    public void Criar_ordena_produtos_por_nome_marca_cor_e_id()
    {
        var primeiro = Produto("A", ("38", 0));
        var segundo = Produto("B", ("38", 0));
        segundo.Marca = "A";
        var terceiro = Produto("B", ("38", 0));
        terceiro.Marca = "B";
        terceiro.Cor = "Azul";
        var quarto = Produto("B", ("38", 0));
        quarto.Marca = "B";
        quarto.Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var quinto = Produto("B", ("38", 0));
        quinto.Marca = "B";
        quinto.Id = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var resultado = SaldosZeradosViewModel.Criar(new[] { quinto, terceiro, segundo, quarto, primeiro });

        Assert.Equal(new[] { primeiro.Id, segundo.Id, terceiro.Id, quarto.Id, quinto.Id },
            resultado.Produtos.Select(p => p.Id));
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    public void Criar_ordena_textualmente_independente_da_cultura(string cultura)
    {
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
            var resultado = SaldosZeradosViewModel.Criar(new[]
            {
                Produto("Ágata", ("38", 0)),
                Produto("Zebra", ("9", 0), ("40", 0), ("35/36", 0), ("10", 0))
            });

            Assert.Equal(new[] { "Zebra", "Ágata" }, resultado.Produtos.Select(p => p.Nome));
            Assert.Equal(new[] { "10", "35/36", "40", "9" }, resultado.Produtos[0].NumeracoesZeradas);
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }

    [Fact]
    public void Criar_materializa_resultado_sem_modificar_ou_compartilhar_colecoes_de_entrada()
    {
        var produto = Produto("Tênis", ("40", 0), ("38", 0), ("39", 2));
        var resultado = SaldosZeradosViewModel.Criar(new[] { produto });

        Assert.Equal(new[] { "40", "38", "39" }, produto.Skus.Select(s => s.Numeracao));
        Assert.Equal(new[] { 0, 0, 2 }, produto.Skus.Select(s => s.SaldoAtual));
        produto.Nome = "Alterado";
        produto.Skus[0].Numeracao = "45";
        produto.Skus.Clear();

        Assert.Equal("Tênis", resultado.Produtos[0].Nome);
        Assert.Equal(new[] { "38", "40" }, resultado.Produtos[0].NumeracoesZeradas);
    }

    [Fact]
    public void Criar_aceita_colecao_vazia()
    {
        Assert.Empty(SaldosZeradosViewModel.Criar(Array.Empty<ProdutoConsultaResultadoViewModel>()).Produtos);
    }

    [Fact]
    public void Criar_rejeita_entrada_nula()
    {
        Assert.Throws<ArgumentNullException>(() => SaldosZeradosViewModel.Criar(null!));
    }

    private static ProdutoConsultaResultadoViewModel Produto(
        string nome, params (string Numeracao, int Saldo)[] skus) => new()
    {
        Id = Guid.NewGuid(),
        Nome = nome,
        Marca = "Marca",
        Cor = "Preto",
        Skus = skus.Select(s => new SkuConsultaResultadoViewModel
        {
            Id = Guid.NewGuid(), Numeracao = s.Numeracao, SaldoAtual = s.Saldo
        }).ToList()
    };
}
