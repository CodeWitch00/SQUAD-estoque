"""Compara saldo, movimentações e rupturas do SKU isolado antes/depois do fluxo."""
import json
import sqlite3
import sys
from pathlib import Path

database = Path(sys.argv[1]).resolve(strict=True)
sku = sys.argv[2]
with sqlite3.connect(database) as connection:
    balance = connection.execute('SELECT SaldoAtual FROM Sku WHERE Id=?', (sku,)).fetchone()
    if balance is None:
        raise AssertionError(f'SKU do teste não encontrado: {sku}')
    state = {
        'balance': balance[0],
        'movements': connection.execute('SELECT count(*) FROM Movimentacao WHERE SkuId=?', (sku,)).fetchone()[0],
        'ruptures': connection.execute('SELECT count(*) FROM Ruptura WHERE SkuId=?', (sku,)).fetchone()[0],
    }
print(json.dumps(state))
