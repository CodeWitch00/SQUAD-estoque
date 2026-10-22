%% DER físico do schema SQLite efetivamente implementado no commit 327efc5.
%% Tipos físicos: Guid -> TEXT; string/DateTime -> TEXT; int/bool/enum -> INTEGER.
%% Não existe atributo de data de última atualização na tabela Sku.

erDiagram
    Produto ||--o{ Sku : "ProdutoId -> Id; ON DELETE RESTRICT"
    Sku ||--o{ Movimentacao : "SkuId -> Id; ON DELETE RESTRICT"
    Sku ||--o{ Ruptura : "SkuId -> Id; ON DELETE RESTRICT"
    Usuario ||--o{ Movimentacao : "UsuarioId -> Id; ON DELETE RESTRICT"
    Usuario ||--o{ Ruptura : "UsuarioId -> Id; ON DELETE RESTRICT"

    Usuario {
        TEXT Id PK "NOT NULL"
        TEXT Nome "NOT NULL; max 150 no EF"
        TEXT Email "NOT NULL; max 255 no EF; UNIQUE"
        TEXT SenhaHash "NOT NULL; max 255 no EF"
        INTEGER Perfil "NOT NULL; CHECK Perfil IN (0, 1)"
    }

    Produto {
        TEXT Id PK "NOT NULL"
        TEXT Nome "NOT NULL; max 200 no EF"
        TEXT Marca "NOT NULL; max 100 no EF"
        TEXT Categoria "NOT NULL; max 100 no EF"
        TEXT Cor "NOT NULL; max 80 no EF"
        INTEGER Ativo "NOT NULL; bool; sem DEFAULT físico"
    }

    Sku {
        TEXT Id PK "NOT NULL"
        TEXT ProdutoId FK "NOT NULL"
        TEXT Numeracao "NOT NULL; max 10 no EF"
        INTEGER SaldoAtual "NOT NULL; CHECK SaldoAtual >= 0"
        INTEGER Ativo "NOT NULL; bool; sem DEFAULT físico"
    }

    Movimentacao {
        TEXT Id PK "NOT NULL"
        TEXT SkuId FK "NOT NULL"
        INTEGER Tipo "NOT NULL; CHECK Tipo IN (0, 1, 2)"
        INTEGER Quantidade "NOT NULL; CHECK Quantidade > 0"
        TEXT UsuarioId FK "NOT NULL"
        TEXT CriadoEm "NOT NULL; DateTime"
        TEXT Motivo "NULL"
    }

    Ruptura {
        TEXT Id PK "NOT NULL"
        TEXT SkuId FK "NOT NULL"
        TEXT UsuarioId FK "NOT NULL"
        TEXT CriadoEm "NOT NULL; DateTime"
    }

%% Restrições e índices físicos implementados:
%% PK_Usuario(Id), PK_Produto(Id), PK_Sku(Id), PK_Movimentacao(Id), PK_Ruptura(Id)
%% IX_Usuario_Email UNIQUE (Email)
%% IX_Sku_ProdutoId_Numeracao UNIQUE (ProdutoId, Numeracao)
%% IX_Movimentacao_SkuId (SkuId)
%% IX_Movimentacao_UsuarioId (UsuarioId)
%% IX_Ruptura_SkuId (SkuId)
%% IX_Ruptura_UsuarioId (UsuarioId)
%% CHECK: Usuario Perfil IN (0, 1)
%% CHECK: Sku SaldoAtual >= 0
%% CHECK: Movimentacao Quantidade > 0
%% CHECK: Movimentacao Tipo IN (0, 1, 2)
