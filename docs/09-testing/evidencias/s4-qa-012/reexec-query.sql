.headers on
.mode column
PRAGMA integrity_check;
.print '---USUARIOS---'
SELECT Id, Nome, Email, Perfil FROM Usuario ORDER BY Id;
.print '---PRODUTOS---'
SELECT Id, Nome, Marca, Categoria, Cor, Ativo FROM Produto ORDER BY Id;
.print '---SKUS---'
SELECT s.Id, s.ProdutoId, p.Nome AS ProdutoNome, s.Numeracao, s.SaldoAtual, s.UltimaAtualizacaoSaldoEm, s.Ativo
FROM Sku s JOIN Produto p ON p.Id = s.ProdutoId ORDER BY s.Id;
.print '---MOVIMENTACOES---'
SELECT m.Id, m.SkuId, p.Nome AS ProdutoNome, s.Numeracao, m.Tipo, m.Quantidade, m.CriadoEm, m.Motivo, m.UsuarioId
FROM Movimentacao m JOIN Sku s ON s.Id = m.SkuId JOIN Produto p ON p.Id = s.ProdutoId ORDER BY m.Id;
.print '---RUPTURAS---'
SELECT r.Id, r.SkuId, p.Nome AS ProdutoNome, s.Numeracao, r.UsuarioId, r.CriadoEm, r.AtendimentoId
FROM Ruptura r JOIN Sku s ON s.Id = r.SkuId JOIN Produto p ON p.Id = s.ProdutoId ORDER BY r.Id;
.print '---ATENDIMENTOS---'
SELECT a.Id, a.UsuarioId, a.ProdutoId, p.Nome AS ProdutoNome, a.SkuId, a.Status, a.Resultado, a.CriadoEm, a.ConcluidoEm
FROM Atendimento a JOIN Produto p ON p.Id = a.ProdutoId ORDER BY a.Id;
