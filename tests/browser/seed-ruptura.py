"""Cria somente os dados sintéticos do teste em um banco local descartável já migrado."""
import sqlite3
import sys
from pathlib import Path

database = Path(sys.argv[1]).resolve(strict=True)
with sqlite3.connect(database) as connection:
    product = '02700000-0000-0000-0000-000000000001'
    connection.execute(
        'INSERT OR IGNORE INTO Produto (Id,Nome,Marca,Categoria,Cor,Ativo) VALUES (?,?,?,?,?,?)',
        (product, 'RegistroFE027 Runner <img src=x onerror=alert(1)>', 'SQUAD', 'Tenis', 'Preto', 1))
    connection.executemany(
        'INSERT OR IGNORE INTO Sku (Id,ProdutoId,Numeracao,SaldoAtual,Ativo) VALUES (?,?,?,?,?)',
        [('02700000-0000-0000-0000-000000000038', product, '38', 0, 1),
         ('02700000-0000-0000-0000-000000000039', product, '39', 3, 1)])
