"""Prepara o SKU isolado do teste S2-QA-031 num banco SQLite descartável migrado."""
import sqlite3
import sys
from pathlib import Path

database = Path(sys.argv[1]).resolve(strict=True)
product = '03100000-0000-0000-0000-000000000001'
sku = '03100000-0000-0000-0000-000000000038'
with sqlite3.connect(database) as connection:
    connection.execute(
        'INSERT INTO Produto (Id,Nome,Marca,Categoria,Cor,Ativo) VALUES (?,?,?,?,?,1)',
        (product, 'S2-QA-031 Runner de teste', 'SQUAD', 'Tênis', 'Azul'))
    connection.execute(
        'INSERT INTO Sku (Id,ProdutoId,Numeracao,SaldoAtual,Ativo) VALUES (?,?,?,?,1)',
        (sku, product, '38', 2))
    connection.commit()
