using System;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Data;

public class EstoqueContext : DbContext
{
    public EstoqueContext(DbContextOptions<EstoqueContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuario { get; set; } = default!;
    public DbSet<Produto> Produto { get; set; } = default!;
    public DbSet<Sku> Sku { get; set; } = default!;
    public DbSet<Movimentacao> Movimentacao { get; set; } = default!;
    public DbSet<Ruptura> Ruptura { get; set; } = default!;
    public DbSet<Atendimento> Atendimento { get; set; } = default!;
    public DbSet<EntradaFiscal> EntradaFiscal { get; set; } = default!;
    public DbSet<ItemEntradaFiscal> ItemEntradaFiscal { get; set; } = default!;
    public DbSet<VinculoProdutoFornecedor> VinculoProdutoFornecedor { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RN-01: SKU único por (ProdutoId, Numeracao)
        modelBuilder.Entity<Sku>()
            .HasIndex(s => new { s.ProdutoId, s.Numeracao })
            .IsUnique();

        modelBuilder.Entity<Produto>().HasIndex(p => p.Marca);
        modelBuilder.Entity<Produto>().HasIndex(p => p.Categoria);

        modelBuilder.Entity<Sku>()
            .HasIndex(s => s.ProdutoId, "IX_Sku_ProdutoId_SaldoZero")
            .HasFilter("SaldoAtual = 0");

        // Substitui o indice convencional da FK SkuId e cobre COUNT/MAX(CriadoEm).
        modelBuilder.Entity<Ruptura>()
            .HasIndex(r => new { r.SkuId, r.CriadoEm });

        modelBuilder.Entity<Sku>()
            .Property(s => s.UltimaAtualizacaoSaldoEm)
            .HasConversion(
                value => value,
                value => value.HasValue
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                    : (DateTime?)null);

        // RF-01 / RF-04: E-mail único para usuário
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // CHECK constraints de integridade no SQLite (RN-02 e RF-10)
        modelBuilder.Entity<Sku>()
            .ToTable(t => t.HasCheckConstraint("chk_sku_saldo_nao_negativo", "SaldoAtual >= 0"));

        modelBuilder.Entity<Movimentacao>()
            .ToTable(t => t.HasCheckConstraint("chk_movimentacao_quantidade_positiva", "Quantidade > 0"));

        // SQLite armazena enums como inteiros. Os CHECKs rejeitam valores fora do domínio.
        modelBuilder.Entity<Usuario>()
            .ToTable(t => t.HasCheckConstraint("chk_usuario_perfil", "Perfil IN (0, 1)"));

        modelBuilder.Entity<Movimentacao>()
            .ToTable(t => t.HasCheckConstraint("chk_movimentacao_tipo", "Tipo IN (0, 1, 2)"));

        modelBuilder.Entity<Atendimento>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("chk_atendimento_status", "Status IN (0, 1)");
                t.HasCheckConstraint("chk_atendimento_resultado", "Resultado IS NULL OR Resultado IN (0, 1, 2)");
                t.HasCheckConstraint("chk_atendimento_saldo", "SaldoConsultado IS NULL OR SaldoConsultado >= 0");
                t.HasCheckConstraint("chk_atendimento_conclusao",
                    "(Status = 0 AND Resultado IS NULL AND ConcluidoEm IS NULL) OR " +
                    "(Status = 1 AND Resultado IS NOT NULL AND ConcluidoEm IS NOT NULL)");
                t.HasCheckConstraint("chk_atendimento_sku_resultado",
                    "Resultado IS NULL OR Resultado = 2 OR SkuId IS NOT NULL");
            });

        modelBuilder.Entity<Atendimento>()
            .HasIndex(a => new { a.UsuarioId, a.Status, a.CriadoEm });

        modelBuilder.Entity<Movimentacao>()
            .HasIndex(m => m.AtendimentoId)
            .IsUnique();

        modelBuilder.Entity<Ruptura>()
            .HasIndex(r => r.AtendimentoId)
            .IsUnique();

        modelBuilder.Entity<EntradaFiscal>()
            .HasIndex(e => e.ChaveAcesso)
            .IsUnique();

        modelBuilder.Entity<VinculoProdutoFornecedor>()
            .HasIndex(v => new { v.CnpjFornecedor, v.CodigoFornecedor })
            .IsUnique();

        // Relacionamentos e chaves estrangeiras com deleção restritiva para preservar histórico (RN-03, RN-06)
        modelBuilder.Entity<Sku>()
            .HasOne(s => s.Produto)
            .WithMany(p => p.Skus)
            .HasForeignKey(s => s.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Movimentacao>()
            .HasOne(m => m.Sku)
            .WithMany(s => s.Movimentacoes)
            .HasForeignKey(m => m.SkuId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Movimentacao>()
            .HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ruptura>()
            .HasOne(r => r.Sku)
            .WithMany(s => s.Rupturas)
            .HasForeignKey(r => r.SkuId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ruptura>()
            .HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Atendimento>()
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Atendimento>()
            .HasOne(a => a.Produto)
            .WithMany()
            .HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Atendimento>()
            .HasOne(a => a.Sku)
            .WithMany()
            .HasForeignKey(a => a.SkuId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Movimentacao>()
            .HasOne(m => m.Atendimento)
            .WithOne()
            .HasForeignKey<Movimentacao>(m => m.AtendimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ruptura>()
            .HasOne(r => r.Atendimento)
            .WithOne()
            .HasForeignKey<Ruptura>(r => r.AtendimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EntradaFiscal>()
            .HasOne(e => e.Usuario)
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemEntradaFiscal>()
            .HasOne(i => i.EntradaFiscal)
            .WithMany(e => e.Itens)
            .HasForeignKey(i => i.EntradaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemEntradaFiscal>()
            .HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemEntradaFiscal>()
            .HasOne(i => i.Sku)
            .WithMany()
            .HasForeignKey(i => i.SkuId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VinculoProdutoFornecedor>()
            .HasOne(v => v.Sku)
            .WithMany()
            .HasForeignKey(v => v.SkuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
