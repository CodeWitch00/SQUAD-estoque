-- Massa sintetica isolada: reproduz apenas colunas e indices usados pelos planos.
PRAGMA automatic_index = OFF;
CREATE TABLE Produto (Id INTEGER PRIMARY KEY, Nome TEXT NOT NULL, Marca TEXT NOT NULL, Categoria TEXT NOT NULL);
CREATE TABLE Sku (Id INTEGER PRIMARY KEY, ProdutoId INTEGER NOT NULL, Numeracao TEXT NOT NULL, SaldoAtual INTEGER NOT NULL);
CREATE TABLE Ruptura (Id INTEGER PRIMARY KEY, SkuId INTEGER NOT NULL, CriadoEm TEXT NOT NULL);
CREATE UNIQUE INDEX IX_Sku_ProdutoId_Numeracao ON Sku(ProdutoId, Numeracao);
CREATE INDEX IX_Ruptura_SkuId ON Ruptura(SkuId);

WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x + 1 FROM n WHERE x < 10000)
INSERT INTO Produto SELECT x, printf('Modelo %05d', x), printf('Marca %03d', x % 100), printf('Categoria %02d', x % 20) FROM n;
WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x + 1 FROM n WHERE x < 30000)
INSERT INTO Sku SELECT x, (x - 1) / 3 + 1, CAST(37 + ((x - 1) % 3) AS TEXT), CASE WHEN x % 10 = 0 THEN 0 ELSE 2 END FROM n;
WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x + 1 FROM n WHERE x < 50000)
INSERT INTO Ruptura SELECT x, x % 30000 + 1, printf('2026-10-%02dT12:00:00', x % 28 + 1) FROM n;
ANALYZE;

.print ANTES: produtos por marca
EXPLAIN QUERY PLAN SELECT Id FROM Produto WHERE Marca = 'Marca 007' ORDER BY Nome, Id LIMIT 20;
.print ANTES: marcas distintas
EXPLAIN QUERY PLAN SELECT DISTINCT Marca FROM Produto WHERE Marca <> '' ORDER BY Marca;
.print ANTES: categorias distintas
EXPLAIN QUERY PLAN SELECT DISTINCT Categoria FROM Produto WHERE Categoria <> '' ORDER BY Categoria;
.print ANTES: produtos com saldo zero
EXPLAIN QUERY PLAN SELECT p.Id FROM Produto p WHERE EXISTS (SELECT 1 FROM Sku s WHERE s.ProdutoId = p.Id AND s.SaldoAtual = 0);
.print ANTES: rupturas agregadas
EXPLAIN QUERY PLAN SELECT SkuId, COUNT(*), MAX(CriadoEm) FROM Ruptura GROUP BY SkuId;

CREATE INDEX IX_Produto_Marca ON Produto(Marca);
CREATE INDEX IX_Produto_Categoria ON Produto(Categoria);
CREATE INDEX IX_Sku_ProdutoId_SaldoZero ON Sku(ProdutoId) WHERE SaldoAtual = 0;
DROP INDEX IX_Ruptura_SkuId;
CREATE INDEX IX_Ruptura_SkuId_CriadoEm ON Ruptura(SkuId, CriadoEm);
ANALYZE;

.print DEPOIS: produtos por marca
EXPLAIN QUERY PLAN SELECT Id FROM Produto WHERE Marca = 'Marca 007' ORDER BY Nome, Id LIMIT 20;
.print DEPOIS: marcas distintas
EXPLAIN QUERY PLAN SELECT DISTINCT Marca FROM Produto WHERE Marca <> '' ORDER BY Marca;
.print DEPOIS: categorias distintas
EXPLAIN QUERY PLAN SELECT DISTINCT Categoria FROM Produto WHERE Categoria <> '' ORDER BY Categoria;
.print DEPOIS: produtos com saldo zero
EXPLAIN QUERY PLAN SELECT p.Id FROM Produto p WHERE EXISTS (SELECT 1 FROM Sku s WHERE s.ProdutoId = p.Id AND s.SaldoAtual = 0);
.print DEPOIS: rupturas agregadas
EXPLAIN QUERY PLAN SELECT SkuId, COUNT(*), MAX(CriadoEm) FROM Ruptura GROUP BY SkuId;
