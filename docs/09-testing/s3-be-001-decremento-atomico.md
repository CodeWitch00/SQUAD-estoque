# S3-BE-001 — Decremento atômico

Data: 08/10/2026 (America/Sao_Paulo). Branch: `dev/emmy`.
Cartão: [S3-BE-001] Implementar decremento atômico — venda concorrente sem saldo negativo.
Base: `a6c1877`. A branch main e as alterações locais de atividades anteriores foram preservadas.

## Implementação e revisão técnica

`SaidaEstoque.RegistrarSaidaAsync` executa `ExecuteUpdateAsync` com filtro por SKU e `SaldoAtual >= quantidade`, atribuindo `SaldoAtual - quantidade` diretamente no SQLite. Zero linhas alteradas retorna saldo insuficiente, sem movimentação. SKU inexistente mantém a mensagem específica. Venda rápida continua retirando um par; saída administrativa preserva quantidade, usuário e motivo.

O UPDATE e a inserção da movimentação usam a mesma transação. O saldo rastreado é recarregado após o UPDATE e após rollback, evitando que SaveChanges regrave saldo antigo. Não há mutex em memória nem troca de banco ou migration.

Revisão local: conferidos o predicado condicional, a sincronização do ChangeTracker, o rollback e os dois controllers que utilizam a operação compartilhada. Revisão humana será solicitada no PR; aprovação depende do integrante.

## Testes

Novo teste parametrizado usa SQLite em arquivo, duas conexões/contextos independentes e início sincronizado. Ambos carregam o saldo inicial antes da disputa. Verifica uma única saída aprovada, uma rejeição por saldo insuficiente, saldo final zero e exatamente uma movimentação com quantidade e usuário corretos:

- Duas vendas rápidas do último par.
- Duas saídas administrativas do último par.
- Duas saídas administrativas de três pares disputando saldo três.

Testes existentes cobrem sucesso, saldo zero/insuficiente, quantidade inválida, SKU inexistente, saldo rastreado desatualizado, rollback por falha na movimentação e reutilização do contexto, além dos fluxos HTTP e permissões.

Comandos de validação:

```powershell
git diff --check
dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj
dotnet test tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --logger 'trx;LogFileName=s3-be-001.trx' --results-directory output/s3-be-001 --collect:'XPlat Code Coverage'
```

Build final: aprovado, zero avisos e zero erros. Suite final: 153 aprovados, zero falhas e zero ignorados (58 segundos). Cobertura de linhas: 73,66%, acima do mínimo de 50% do CI. `git diff --check` aprovado. TRX e Cobertura ficam em `output/s3-be-001` na cópia isolada, fora dos arquivos versionados.

## Conferência manual sugerida

Com um SKU de saldo um, abrir duas sessões autenticadas e vender o mesmo SKU nas duas. Confirmar uma venda, rejeição na segunda, saldo zero e uma movimentação. Repetir entre venda rápida e saída administrativa. Esta conferência de interface não foi executada nesta atividade; a disputa no banco e os fluxos HTTP foram validados automaticamente.

O Trello fica sob responsabilidade do usuário. Copiar o link do PR e esta evidência ao cartão. A main não deve receber merge até a aprovação humana e CI verde.
