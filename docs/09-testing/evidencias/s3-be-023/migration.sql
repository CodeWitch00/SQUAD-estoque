BEGIN TRANSACTION;
ALTER TABLE "Sku" ADD "UltimaAtualizacaoSaldoEm" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261026220326_AddSkuUltimaAtualizacaoSaldo', '10.0.11');

COMMIT;
