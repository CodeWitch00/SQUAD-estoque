using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Controllers;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

// As constraints de unicidade e saldo são protegidas pelo EstoqueContext.
// A saída compartilhada e os controllers são verificados contra SQLite real.
public sealed class EstoqueDomainPersistenceTests
{
    [Fact]
    public void Produto_requires_identification_fields()
    {
        var produto = new Produto();
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            produto,
            new ValidationContext(produto),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Nome)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Marca)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Categoria)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Cor)));
    }

    [Fact]
    public async Task Sku_with_same_product_and_numeracao_cannot_be_persisted_twice()
    {
        await using var database = await TestDatabase.CreateAsync();
        var produto = CreateProduto();
        database.Context.Add(produto);
        database.Context.Sku.AddRange(
            CreateSku(produto.Id, "38", 1),
            CreateSku(produto.Id, "38", 2));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task Sku_with_negative_balance_cannot_be_persisted()
    {
        await using var database = await TestDatabase.CreateAsync();
        var produto = CreateProduto();
        database.Context.Add(produto);
        database.Context.Sku.Add(CreateSku(produto.Id, "39", -1));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task Entrada_registers_movement_and_increases_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 3);
        var controller = CreateController(database.Context, data.Usuario);

        var before = DateTime.UtcNow;
        var result = await controller.Entrada(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 4,
            Motivo = "Reposição"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(7, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.NotEqual(Guid.Empty, movimentacao.Id);
        Assert.Equal(data.Sku.Id, movimentacao.SkuId);
        Assert.Equal(TipoMovimentacao.ENTRADA, movimentacao.Tipo);
        Assert.Equal(4, movimentacao.Quantidade);
        Assert.Equal(data.Usuario.Id, movimentacao.UsuarioId);
        Assert.InRange(movimentacao.CriadoEm, before, DateTime.UtcNow);
        Assert.Equal("Reposição", movimentacao.Motivo);
    }

    [Fact]
    public async Task Movimentacoes_index_filters_by_product_type_period_and_responsible()
    {
        await using var database = await TestDatabase.CreateAsync();
        var air = CreateProduto();
        air.Nome = "Tênis Air Zoom Pegasus";
        air.Marca = "Nike";
        var derby = CreateProduto();
        derby.Nome = "Sapato Derby";
        derby.Marca = "Ferracini";
        var airSku = CreateSku(air.Id, "40", 5);
        var derbySku = CreateSku(derby.Id, "41", 5);
        var responsavel = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Responsável Um",
            Email = $"{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.LOJISTA
        };
        var outroResponsavel = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Responsável Dois",
            Email = $"{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.LOJISTA
        };

        database.Context.AddRange(air, derby, airSku, derbySku, responsavel, outroResponsavel);
        database.Context.Movimentacao.AddRange(
            CreateMovement(airSku, responsavel, TipoMovimentacao.SAIDA, new DateTime(2026, 9, 30, 18, 0, 0)),
            CreateMovement(airSku, outroResponsavel, TipoMovimentacao.ENTRADA, new DateTime(2026, 9, 15)),
            CreateMovement(derbySku, responsavel, TipoMovimentacao.SAIDA, new DateTime(2026, 9, 20)));
        await database.Context.SaveChangesAsync();

        var controller = CreateController(database.Context, responsavel);
        var result = await controller.Index(
            "Air", "SAIDA", new DateTime(2026, 9, 1), new DateTime(2026, 9, 30), responsavel.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MovimentacoesIndexViewModel>(view.Model);
        var movimento = Assert.Single(model.Movimentacoes);
        Assert.Equal(airSku.Id, movimento.SkuId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimento.Tipo);
        Assert.Equal(responsavel.Id, movimento.UsuarioId);
        Assert.Equal(1, model.TotalItens);
        Assert.Equal(new DateTime(2026, 9, 1), model.DataInicio);
        Assert.Equal(new DateTime(2026, 9, 30), model.DataFim);
    }

    [Fact]
    public async Task Movimentacoes_index_paginates_most_recent_first_and_clamps_page()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 1);
        var baseDate = new DateTime(2026, 9, 1);
        for (var index = 0; index < 21; index++)
        {
            database.Context.Movimentacao.Add(CreateMovement(
                data.Sku, data.Usuario, TipoMovimentacao.ENTRADA, baseDate.AddDays(index)));
        }

        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context, data.Usuario);

        var secondPageResult = await controller.Index(null, null, null, null, null, 2);
        var secondPage = Assert.IsType<MovimentacoesIndexViewModel>(Assert.IsType<ViewResult>(secondPageResult).Model);
        Assert.Equal(2, secondPage.PaginaAtual);
        Assert.Equal(2, secondPage.TotalPaginas);
        Assert.Single(secondPage.Movimentacoes);
        Assert.Equal(baseDate, secondPage.Movimentacoes[0].CriadoEm);

        var invalidPageResult = await controller.Index(null, null, null, null, null, 0);
        var invalidPage = Assert.IsType<MovimentacoesIndexViewModel>(Assert.IsType<ViewResult>(invalidPageResult).Model);
        Assert.Equal(1, invalidPage.PaginaAtual);
        Assert.Equal(20, invalidPage.Movimentacoes.Count);
    }

    [Fact]
    public async Task Movimentacoes_index_rejects_invalid_period_without_querying_results()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 1);
        database.Context.Movimentacao.Add(CreateMovement(
            data.Sku, data.Usuario, TipoMovimentacao.ENTRADA, new DateTime(2026, 9, 10)));
        await database.Context.SaveChangesAsync();

        var controller = CreateController(database.Context, data.Usuario);
        var result = await controller.Index(
            null, null, new DateTime(2026, 10, 1), new DateTime(2026, 9, 1), null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MovimentacoesIndexViewModel>(view.Model);
        Assert.Empty(model.Movimentacoes);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage == "A data inicial não pode ser posterior à data final.");
    }

    [Fact]
    public async Task Saida_registers_movement_and_reduces_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 5);
        var controller = CreateController(database.Context, data.Usuario);

        var before = DateTime.UtcNow;
        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 2
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(3, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.NotEqual(Guid.Empty, movimentacao.Id);
        Assert.Equal(data.Sku.Id, movimentacao.SkuId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimentacao.Tipo);
        Assert.Equal(2, movimentacao.Quantidade);
        Assert.Equal(data.Usuario.Id, movimentacao.UsuarioId);
        Assert.InRange(movimentacao.CriadoEm, before, DateTime.UtcNow);
        Assert.Null(movimentacao.Motivo);
    }

    [Fact]
    public async Task Saida_with_insufficient_balance_is_rejected_by_controller()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 1);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 2
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Ajuste_registers_movement_and_updates_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 2);
        var controller = CreateController(database.Context, data.Usuario);

        var before = DateTime.UtcNow;
        var result = await controller.Ajuste(new AjusteEstoqueViewModel
        {
            SkuId = data.Sku.Id,
            NovoSaldoApurado = 6,
            Motivo = "Contagem física"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(6, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.NotEqual(Guid.Empty, movimentacao.Id);
        Assert.Equal(data.Sku.Id, movimentacao.SkuId);
        Assert.Equal(TipoMovimentacao.AJUSTE, movimentacao.Tipo);
        Assert.Equal(4, movimentacao.Quantidade);
        Assert.Equal(data.Usuario.Id, movimentacao.UsuarioId);
        Assert.InRange(movimentacao.CriadoEm, before, DateTime.UtcNow);
        Assert.Equal("Contagem física", movimentacao.Motivo);
    }

    [Fact]
    public async Task Ajuste_without_reason_is_rejected_by_controller()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 2);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Ajuste(new AjusteEstoqueViewModel
        {
            SkuId = data.Sku.Id,
            NovoSaldoApurado = 6,
            Motivo = string.Empty
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(2, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Ruptura_can_be_persisted_without_changing_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 3);
        database.Context.Ruptura.Add(new Ruptura
        {
            Id = Guid.NewGuid(),
            SkuId = data.Sku.Id,
            UsuarioId = data.Usuario.Id,
            CriadoEm = DateTime.UtcNow
        });

        await database.Context.SaveChangesAsync();

        Assert.Single(await database.Context.Ruptura.ToListAsync());
        Assert.Equal(3, data.Sku.SaldoAtual);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Venda_rapida_removes_exactly_one_pair_from_selected_sku(int saldo)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, saldo);
        data.Usuario.Perfil = PerfilUsuario.VENDEDOR;
        var outroSku = CreateSku(data.Sku.ProdutoId, "40", 7);
        database.Context.Add(outroSku);
        await database.Context.SaveChangesAsync();
        var antes = DateTime.UtcNow;

        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);

        Assert.Null(resultado.Erro);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(saldo - 1, (await database.Context.Sku.FindAsync(data.Sku.Id))!.SaldoAtual);
        Assert.Equal(7, (await database.Context.Sku.FindAsync(outroSku.Id))!.SaldoAtual);
        var movimento = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(data.Sku.Id, movimento.SkuId);
        Assert.Equal(data.Usuario.Id, movimento.UsuarioId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimento.Tipo);
        Assert.Equal(1, movimento.Quantidade);
        Assert.InRange(movimento.CriadoEm, antes, DateTime.UtcNow);
        Assert.Empty(await database.Context.Ruptura.ToListAsync());
    }

    [Fact]
    public async Task Venda_rapida_rejects_zero_balance_without_movement()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 0);
        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);
        Assert.Contains("Saldo insuficiente", resultado.Erro);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(0, (await database.Context.Sku.FindAsync(data.Sku.Id))!.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Venda_rapida_rejects_missing_sku()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        var resultado = await database.Context.RegistrarVendaRapidaAsync(Guid.NewGuid(), data.Usuario.Id);
        Assert.Contains("não foi encontrado", resultado.Erro);
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Shared_saida_rejects_nonpositive_quantity(int quantidade)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        var resultado = await database.Context.RegistrarSaidaAsync(data.Sku.Id, quantidade, data.Usuario.Id);
        Assert.Equal("Quantidade", resultado.Campo);
        Assert.NotNull(resultado.Erro);
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Failed_movement_rolls_back_balance_and_allows_next_operation()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, Guid.NewGuid()));
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Equal(3, await database.Context.Sku.AsNoTracking()
            .Where(s => s.Id == data.Sku.Id).Select(s => s.SaldoAtual).SingleAsync());
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());

        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);
        Assert.Null(resultado.Erro);
        Assert.Equal(2, data.Sku.SaldoAtual);
        Assert.Single(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Shared_saida_refreshes_previously_tracked_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        await database.Context.Sku.Where(s => s.Id == data.Sku.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.SaldoAtual, 0));
        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);
        Assert.NotNull(resultado.Erro);
        Assert.Equal(0, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Administrative_saida_preserves_quantity_reason_user_and_redirect()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 5);
        var controller = CreateController(database.Context, data.Usuario);
        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id, Quantidade = 3, Motivo = "  Venda administrativa  "
        });
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Produtos", redirect.ControllerName);
        Assert.Equal(data.Sku.ProdutoId, redirect.RouteValues!["id"]);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(2, (await database.Context.Sku.FindAsync(data.Sku.Id))!.SaldoAtual);
        var movimento = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(3, movimento.Quantidade);
        Assert.Equal(data.Usuario.Id, movimento.UsuarioId);
        Assert.Equal("Venda administrativa", movimento.Motivo);
    }

    [Fact]
    public async Task Saida_without_authenticated_identifier_does_not_change_stock()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        var controller = CreateController(database.Context, data.Usuario);
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id, Quantidade = 1
        });
        Assert.IsType<ChallengeResult>(result);
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Adicionar_numeracao_creates_new_sku_with_zero_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 4), ("39", 2), ("40", 1));
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = await controller.AdicionarNumeracoes(data.Produto.Id, new AdicionarNumeracoesViewModel
        {
            ProdutoId = data.Produto.Id,
            NumeracoesGrade = "41"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        database.Context.ChangeTracker.Clear();
        var skus = await database.Context.Sku.Where(sku => sku.ProdutoId == data.Produto.Id)
            .OrderBy(sku => sku.Numeracao).ToListAsync();
        Assert.Equal(new[] { "38", "39", "40", "41" }, skus.Select(sku => sku.Numeracao));
        Assert.Equal(0, skus.Single(sku => sku.Numeracao == "41").SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Adicionar_numeracoes_cria_varios_skus_atomically()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0), ("39", 1));
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = await controller.AdicionarNumeracoes(data.Produto.Id, new AdicionarNumeracoesViewModel
        {
            ProdutoId = data.Produto.Id,
            NumeracoesGrade = "40, 41, 42"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(5, await database.Context.Sku.CountAsync(sku => sku.ProdutoId == data.Produto.Id));
        Assert.All(await database.Context.Sku.Where(sku => sku.ProdutoId == data.Produto.Id &&
            new[] { "40", "41", "42" }.Contains(sku.Numeracao)).ToListAsync(),
            sku => Assert.Equal(0, sku.SaldoAtual));
    }

    [Theory]
    [InlineData("40")]
    [InlineData("41, 40, 42")]
    public async Task Adicionar_numeracoes_rejeita_duplicidade_existente_sem_criacao_parcial(string numeracoes)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0), ("39", 0), ("40", 3));
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = await controller.AdicionarNumeracoes(data.Produto.Id, new AdicionarNumeracoesViewModel
        {
            ProdutoId = data.Produto.Id,
            NumeracoesGrade = numeracoes
        });

        Assert.IsType<ViewResult>(result);
        Assert.Contains(controller.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage!.Contains("já existe"));
        Assert.Equal(3, await database.Context.Sku.CountAsync(sku => sku.ProdutoId == data.Produto.Id));
        Assert.Equal(3, data.Skus["40"].SaldoAtual);
    }

    [Fact]
    public async Task Adicionar_numeracoes_rejeita_duplicidade_no_formulario()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = await controller.AdicionarNumeracoes(data.Produto.Id, new AdicionarNumeracoesViewModel
        {
            ProdutoId = data.Produto.Id,
            NumeracoesGrade = "41, 41"
        });

        Assert.IsType<ViewResult>(result);
        Assert.Contains(controller.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage!.Contains("mais de uma vez"));
        Assert.Single(await database.Context.Sku.Where(sku => sku.ProdutoId == data.Produto.Id).ToListAsync());
    }

    [Fact]
    public async Task Adicionar_numeracoes_rejeita_campo_vazio()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = await controller.AdicionarNumeracoes(data.Produto.Id, new AdicionarNumeracoesViewModel
        {
            ProdutoId = data.Produto.Id,
            NumeracoesGrade = "   "
        });

        Assert.IsType<ViewResult>(result);
        Assert.Contains(controller.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage!.Contains("ao menos uma numeração"));
        Assert.Single(await database.Context.Sku.Where(sku => sku.ProdutoId == data.Produto.Id).ToListAsync());
    }

    [Fact]
    public async Task Adicionar_numeracoes_rejeita_produto_inexistente()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = await controller.AdicionarNumeracoes(Guid.NewGuid(), new AdicionarNumeracoesViewModel
        {
            ProdutoId = Guid.NewGuid(),
            NumeracoesGrade = "41"
        });

        Assert.IsType<NotFoundResult>(result);
        Assert.Single(await database.Context.Sku.Where(sku => sku.ProdutoId == data.Produto.Id).ToListAsync());
    }

    [Fact]
    public async Task Nova_numeracao_aparece_na_entrada_em_lote_sem_movimentacao()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0), ("39", 2));
        var produtosController = CreateProdutosController(database.Context, data.Usuario);

        await produtosController.AdicionarNumeracoes(data.Produto.Id, new AdicionarNumeracoesViewModel
        {
            ProdutoId = data.Produto.Id,
            NumeracoesGrade = "41"
        });

        database.Context.ChangeTracker.Clear();
        var movimentacoesController = CreateController(database.Context, data.Usuario);
        var result = await movimentacoesController.EntradaLote(data.Produto.Id);
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EntradaLoteViewModel>(view.Model);
        Assert.Contains(model.Itens, item => item.Numeracao == "41" && item.SaldoAtual == 0);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
        Assert.Equal(2, (await database.Context.Sku.SingleAsync(sku => sku.Numeracao == "39")).SaldoAtual);
    }

    [Fact]
    public async Task Produtos_index_searches_by_name_and_brand()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        data.Produto.Nome = "Tênis Air Zoom Pegasus";
        data.Produto.Marca = "Nike";
        var outroProduto = CreateProduto();
        outroProduto.Nome = "Tênis 574 Core";
        outroProduto.Marca = "New Balance";
        var terceiroProduto = CreateProduto();
        terceiroProduto.Nome = "Sapato Derby";
        terceiroProduto.Marca = "Ferracini";
        database.Context.AddRange(outroProduto, terceiroProduto);
        await database.Context.SaveChangesAsync();
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var porNome = Assert.IsType<ViewResult>(await controller.Index("Air", null, null, null, 1));
        var modeloPorNome = Assert.IsType<ProdutosIndexViewModel>(porNome.Model);
        Assert.Single(modeloPorNome.Produtos);
        Assert.Equal("Tênis Air Zoom Pegasus", modeloPorNome.Produtos[0].Nome);

        var porMarca = Assert.IsType<ViewResult>(await controller.Index("Nike", null, null, null, 1));
        var modeloPorMarca = Assert.IsType<ProdutosIndexViewModel>(porMarca.Model);
        Assert.Single(modeloPorMarca.Produtos);
        Assert.Equal("Nike", modeloPorMarca.Produtos[0].Marca);

        var inexistente = Assert.IsType<ViewResult>(await controller.Index("Inexistente", null, null, null, 1));
        Assert.Empty(Assert.IsType<ProdutosIndexViewModel>(inexistente.Model).Produtos);
    }

    [Fact]
    public async Task Produtos_index_applies_status_brand_category_filters_together()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        data.Produto.Nome = "Produto alvo";
        data.Produto.Marca = "Nike";
        data.Produto.Categoria = "Running";
        var inativo = CreateProduto();
        inativo.Nome = "Produto inativo";
        inativo.Marca = "Nike";
        inativo.Categoria = "Running";
        inativo.Ativo = false;
        var outraCategoria = CreateProduto();
        outraCategoria.Nome = "Produto casual";
        outraCategoria.Marca = "Nike";
        outraCategoria.Categoria = "Casual";
        database.Context.AddRange(inativo, outraCategoria);
        await database.Context.SaveChangesAsync();
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = Assert.IsType<ViewResult>(await controller.Index(
            null, "ativo", "Nike", "Running", 1));
        var model = Assert.IsType<ProdutosIndexViewModel>(result.Model);
        Assert.Single(model.Produtos);
        Assert.Equal("Produto alvo", model.Produtos[0].Nome);
        Assert.Equal("ativo", model.Status);
        Assert.Equal("Nike", model.Marca);
        Assert.Equal("Running", model.Categoria);
    }

    [Fact]
    public async Task Produtos_index_paginates_twenty_items_and_preserves_query_state()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        data.Produto.Nome = "Produto 00";
        data.Produto.Marca = "Marca filtrada";
        data.Produto.Categoria = "Categoria filtrada";
        for (var index = 1; index < 25; index++)
        {
            var produto = CreateProduto();
            produto.Nome = $"Produto {index:00}";
            produto.Marca = "Marca filtrada";
            produto.Categoria = "Categoria filtrada";
            database.Context.Add(produto);
        }
        await database.Context.SaveChangesAsync();
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var pageOneResult = Assert.IsType<ViewResult>(await controller.Index(
            "Produto", "ativo", "Marca filtrada", "Categoria filtrada", 1));
        var pageOne = Assert.IsType<ProdutosIndexViewModel>(pageOneResult.Model);
        Assert.Equal(1, pageOne.PaginaAtual);
        Assert.Equal(25, pageOne.TotalItens);
        Assert.Equal(2, pageOne.TotalPaginas);
        Assert.Equal(20, pageOne.Produtos.Count);
        Assert.Equal("Produto", pageOne.Busca);
        Assert.Equal("ativo", pageOne.Status);
        Assert.Equal("Marca filtrada", pageOne.Marca);
        Assert.Equal("Categoria filtrada", pageOne.Categoria);

        var pageTwoResult = Assert.IsType<ViewResult>(await controller.Index(
            "Produto", "ativo", "Marca filtrada", "Categoria filtrada", 2));
        var pageTwo = Assert.IsType<ProdutosIndexViewModel>(pageTwoResult.Model);
        Assert.Equal(2, pageTwo.PaginaAtual);
        Assert.Equal(5, pageTwo.Produtos.Count);
        Assert.Empty(pageOne.Produtos.Select(p => p.Id).Intersect(pageTwo.Produtos.Select(p => p.Id)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(99, 2)]
    public async Task Produtos_index_normalizes_invalid_page(int paginaInformada, int paginaEsperada)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedGradeAsync(database.Context, ("38", 0));
        for (var index = 1; index < 25; index++)
        {
            var produto = CreateProduto();
            produto.Nome = $"Produto {index:00}";
            database.Context.Add(produto);
        }
        await database.Context.SaveChangesAsync();
        var controller = CreateProdutosController(database.Context, data.Usuario);

        var result = Assert.IsType<ViewResult>(await controller.Index(null, null, null, null, paginaInformada));
        Assert.Equal(paginaEsperada, Assert.IsType<ProdutosIndexViewModel>(result.Model).PaginaAtual);
    }

    [Fact]
    public async Task Entrada_lote_updates_multiple_skus_in_one_submission()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Observacao = "  Recebimento fornecedor  ",
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = 2 },
                new() { SkuId = data.Sku38.Id, QuantidadeRecebida = 4 },
                new() { SkuId = data.Sku40.Id, QuantidadeRecebida = 3 }
            }
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(data.Produto.Id, redirect.RouteValues!["id"]);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(3, (await database.Context.Sku.FindAsync(data.Sku36.Id))!.SaldoAtual);
        Assert.Equal(6, (await database.Context.Sku.FindAsync(data.Sku38.Id))!.SaldoAtual);
        Assert.Equal(3, (await database.Context.Sku.FindAsync(data.Sku40.Id))!.SaldoAtual);

        var movimentos = await database.Context.Movimentacao.OrderBy(m => m.SkuId).ToListAsync();
        Assert.Equal(3, movimentos.Count);
        Assert.All(movimentos, movimento =>
        {
            Assert.Equal(TipoMovimentacao.ENTRADA, movimento.Tipo);
            Assert.Equal(data.Usuario.Id, movimento.UsuarioId);
            Assert.Equal("Recebimento fornecedor", movimento.Motivo);
        });
        var quantidadesPorSku = movimentos.ToDictionary(m => m.SkuId, m => m.Quantidade);
        Assert.Equal(2, quantidadesPorSku[data.Sku36.Id]);
        Assert.Equal(4, quantidadesPorSku[data.Sku38.Id]);
        Assert.Equal(3, quantidadesPorSku[data.Sku40.Id]);
    }

    [Fact]
    public async Task Entrada_lote_ignores_empty_and_zero_quantities()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = 2 },
                new() { SkuId = data.Sku38.Id, QuantidadeRecebida = null },
                new() { SkuId = data.Sku40.Id, QuantidadeRecebida = 0 }
            }
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(3, data.Sku36.SaldoAtual);
        Assert.Equal(2, data.Sku38.SaldoAtual);
        Assert.Equal(0, data.Sku40.SaldoAtual);
        Assert.Single(await database.Context.Movimentacao.ToListAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task Entrada_lote_rejects_when_no_positive_quantity_is_informed(int? quantidade)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = quantidade },
                new() { SkuId = data.Sku38.Id, QuantidadeRecebida = 0 }
            }
        });

        Assert.IsType<ViewResult>(result);
        Assert.Contains(controller.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage!.Contains("pelo menos uma numeração"));
        Assert.Equal(1, data.Sku36.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Entrada_lote_rejects_negative_quantity_without_changes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = -1 },
                new() { SkuId = data.Sku38.Id, QuantidadeRecebida = 2 }
            }
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, data.Sku36.SaldoAtual);
        Assert.Equal(2, data.Sku38.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Entrada_lote_rejects_sku_from_another_product_atomically()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var outroProduto = CreateProduto();
        var skuDeOutroProduto = CreateSku(outroProduto.Id, "42", 7);
        database.Context.AddRange(outroProduto, skuDeOutroProduto);
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = 2 },
                new() { SkuId = skuDeOutroProduto.Id, QuantidadeRecebida = 5 }
            }
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, data.Sku36.SaldoAtual);
        Assert.Equal(7, skuDeOutroProduto.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Entrada_lote_rejects_unknown_sku_without_changes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = 2 },
                new() { SkuId = Guid.NewGuid(), QuantidadeRecebida = 3 }
            }
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, data.Sku36.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Entrada_lote_rejects_duplicate_sku_ids_without_changes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedBatchAsync(database.Context);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.EntradaLote(new EntradaLoteViewModel
        {
            ProdutoId = data.Produto.Id,
            Itens = new List<EntradaLoteItemViewModel>
            {
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = 2 },
                new() { SkuId = data.Sku36.Id, QuantidadeRecebida = 3 }
            }
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, data.Sku36.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    private static MovimentacoesController CreateController(EstoqueContext context, Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        return new MovimentacoesController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static ProdutosController CreateProdutosController(EstoqueContext context, Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        return new ProdutosController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static async Task<TestData> SeedAsync(EstoqueContext context, int initialBalance)
    {
        var produto = CreateProduto();
        var sku = CreateSku(produto.Id, "38", initialBalance);
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário de Teste",
            Email = $"{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash-isolado-de-teste",
            Perfil = PerfilUsuario.LOJISTA
        };

        context.AddRange(produto, sku, usuario);
        await context.SaveChangesAsync();
        return new TestData(sku, usuario);
    }

    private static async Task<GradeTestData> SeedGradeAsync(EstoqueContext context,
        params (string Numeracao, int Saldo)[] grade)
    {
        var produto = CreateProduto();
        var skus = grade.ToDictionary(item => item.Numeracao,
            item => CreateSku(produto.Id, item.Numeracao, item.Saldo));
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário de Teste",
            Email = $"{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash-isolado-de-teste",
            Perfil = PerfilUsuario.LOJISTA
        };

        context.Add(produto);
        context.AddRange(skus.Values);
        context.Add(usuario);
        await context.SaveChangesAsync();
        return new GradeTestData(produto, skus, usuario);
    }

    private static async Task<BatchTestData> SeedBatchAsync(EstoqueContext context)
    {
        var produto = CreateProduto();
        var sku36 = CreateSku(produto.Id, "36", 1);
        var sku38 = CreateSku(produto.Id, "38", 2);
        var sku40 = CreateSku(produto.Id, "40", 0);
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário de Teste",
            Email = $"{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash-isolado-de-teste",
            Perfil = PerfilUsuario.LOJISTA
        };

        context.AddRange(produto, sku36, sku38, sku40, usuario);
        await context.SaveChangesAsync();
        return new BatchTestData(produto, sku36, sku38, sku40, usuario);
    }

    private static Produto CreateProduto()
    {
        return new Produto
        {
            Id = Guid.NewGuid(),
            Nome = "Tênis de Teste",
            Marca = "Marca Teste",
            Categoria = "Esportivo",
            Cor = "Preto"
        };
    }

    private static Sku CreateSku(Guid produtoId, string numeracao, int saldoAtual)
    {
        return new Sku
        {
            Id = Guid.NewGuid(),
            ProdutoId = produtoId,
            Numeracao = numeracao,
            SaldoAtual = saldoAtual
        };
    }

    private static Movimentacao CreateMovement(
        Sku sku,
        Usuario usuario,
        TipoMovimentacao tipo,
        DateTime criadoEm)
    {
        return new Movimentacao
        {
            Id = Guid.NewGuid(),
            SkuId = sku.Id,
            Sku = sku,
            Tipo = tipo,
            Quantidade = 1,
            UsuarioId = usuario.Id,
            Usuario = usuario,
            CriadoEm = criadoEm
        };
    }

    private sealed record TestData(Sku Sku, Usuario Usuario);

    private sealed record BatchTestData(Produto Produto, Sku Sku36, Sku Sku38, Sku Sku40, Usuario Usuario);

    private sealed record GradeTestData(Produto Produto, Dictionary<string, Sku> Skus, Usuario Usuario);

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, EstoqueContext context)
        {
            _connection = connection;
            Context = context;
        }

        public EstoqueContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<EstoqueContext>()
                .UseSqlite(connection)
                .Options;
            var context = new EstoqueContext(options);
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
