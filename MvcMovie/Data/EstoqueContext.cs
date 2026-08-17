using System;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RN-01: SKU único por (ProdutoId, Numeracao)
        modelBuilder.Entity<Sku>()
            .HasIndex(s => new { s.ProdutoId, s.Numeracao })
            .IsUnique();

        // RF-01 / RF-04: E-mail único para usuário
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // CHECK constraints de integridade no SQLite (RN-02 e RF-10)
        modelBuilder.Entity<Sku>()
            .ToTable(t => t.HasCheckConstraint("chk_sku_saldo_nao_negativo", "SaldoAtual >= 0"));

        modelBuilder.Entity<Movimentacao>()
            .ToTable(t => t.HasCheckConstraint("chk_movimentacao_quantidade_positiva", "Quantidade > 0"));

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
    }
}
