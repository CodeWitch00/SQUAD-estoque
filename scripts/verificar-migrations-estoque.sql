-- S4-DB-004 — Consultas de verificação das migrations do EstoqueContext.
--
-- Uso:
--   sqlite3 /caminho/Estoque.db < scripts/verificar-migrations-estoque.sql
--
-- Este script é SOMENTE LEITURA. Ele não altera schema nem dados.
-- Em um banco que ainda está em versão anterior, comente as tabelas que ainda
-- não existem antes de executar. O script abaixo assume o schema mais recente.

.headers on
.mode column
.width 45 20 20 20

SELECT '=== 1. Histórico de migrations ===' AS secao;
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;

SELECT '=== 2. Integridade do arquivo ===' AS secao;
PRAGMA integrity_check;

SELECT '=== 3. Violações de foreign key ===' AS secao;
PRAGMA foreign_key_check;

SELECT '=== 4. Tabelas existentes ===' AS secao;
SELECT name
FROM sqlite_master
WHERE type = 'table' AND name NOT LIKE 'sqlite_%'
ORDER BY name;

SELECT '=== 5. Contagens por tabela ===' AS secao;
SELECT 'Usuario' AS tabela, COUNT(*) AS total FROM Usuario
UNION ALL SELECT 'Produto', COUNT(*) FROM Produto
UNION ALL SELECT 'Sku', COUNT(*) FROM Sku
UNION ALL SELECT 'Movimentacao', COUNT(*) FROM Movimentacao
UNION ALL SELECT 'Ruptura', COUNT(*) FROM Ruptura
UNION ALL SELECT 'Atendimento', COUNT(*) FROM Atendimento
UNION ALL SELECT 'EntradaFiscal', COUNT(*) FROM EntradaFiscal
UNION ALL SELECT 'ItemEntradaFiscal', COUNT(*) FROM ItemEntradaFiscal
UNION ALL SELECT 'VinculoProdutoFornecedor', COUNT(*) FROM VinculoProdutoFornecedor
ORDER BY tabela;

SELECT '=== 6. Saldos ===' AS secao;
SELECT
    COUNT(*) AS skus,
    COALESCE(SUM(SaldoAtual), 0) AS soma_saldo,
    MIN(SaldoAtual) AS minimo,
    MAX(SaldoAtual) AS maximo
FROM Sku;

SELECT '=== 7. Índices esperados ===' AS secao;
SELECT name
FROM sqlite_master
WHERE type = 'index'
  AND name IN (
      'IX_Produto_Marca',
      'IX_Produto_Categoria',
      'IX_Sku_ProdutoId_SaldoZero',
      'IX_Ruptura_SkuId_CriadoEm'
  )
ORDER BY name;

SELECT '=== 8. Tabelas temporárias de migration ===' AS secao;
SELECT name
FROM sqlite_master
WHERE name LIKE 'ef_temp_%'
ORDER BY name;

SELECT '=== 9. Lock de migration ===' AS secao;
-- Se a tabela não existir, a consulta abaixo falhará; nesse caso, ignore esta seção.
SELECT *
FROM __EFMigrationsLock;

SELECT '=== 10. Amostra de dados ===' AS secao;
SELECT Id, Nome, Marca, Categoria, Cor, Ativo
FROM Produto
ORDER BY Id
LIMIT 5;

SELECT Id, ProdutoId, Numeracao, SaldoAtual, Ativo
FROM Sku
ORDER BY Id
LIMIT 5;

SELECT Id, SkuId, Tipo, Quantidade, UsuarioId, CriadoEm, Motivo
FROM Movimentacao
ORDER BY Id
LIMIT 5;

SELECT Id, SkuId, UsuarioId, CriadoEm
FROM Ruptura
ORDER BY Id
LIMIT 5;
