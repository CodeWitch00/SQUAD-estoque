# Evidência: validação de imagem Docker em Production — S4-QA-011

## Execução

- PR: [#115 — Validar build limpo da imagem Docker em Production](https://github.com/CodeWitch00/SQUAD-estoque/pull/115)
- Workflow: [QA da imagem Docker, execução 3](https://github.com/CodeWitch00/SQUAD-estoque/actions/runs/44822131199)
- Commit de merge temporário usado pelo GitHub Actions: `fa9625c62b4faafc27d637a7fd655b03ca1b7a38`
- Head do PR nessa execução: `25b1dddfa518bd1b163cd2d54cd047460797adc3`
- Base `main`: `41027e5a74f538501b4ecac2f06be51c2521322f`
- Runner: Ubuntu 26.04.1, Docker Server 29.4.2, plataforma da imagem `linux/amd64`
- Log completo: artefato `qa-imagem` da execução do workflow

## Resultado

O job terminou com sucesso. O roteiro criou o contexto usando somente arquivos versionados, executou build limpo (`--pull --no-cache`) e validou a imagem e a inicialização em Production.

| Verificação | Resultado |
| --- | --- |
| Imagem criada | `squad-estoque:qa-fa9625c62b4f` |
| ID da imagem | `sha256:ac963403ddfb9d32ce691020a7f4d7877fdc58b93d637828ba947d7c08b9b611` |
| Arquivos SQLite, segredo ou credencial nos appsettings/metadados da imagem | Nenhum encontrado |
| Bootstrap do primeiro LOJISTA, com senha aleatória efêmera | Passou |
| Aplicação em Production ouvindo em 8080 | Passou |
| `GET /health` | `Healthy` |
| SQLite no volume externo | Passou |
| `GET /Produtos` anônimo | HTTP 302 |
| Login de LOJISTA | HTTP 302 |
| `GET /Produtos` autenticado | HTTP 200; catálogo vazio esperado |

O container e o volume temporário foram removidos pelo próprio roteiro ao final.

## Observações

O log registrou avisos de Data Protection e redirecionamento HTTPS no teste HTTP local; não houve erros `fail:` e os cenários acima passaram. A inspeção também registrou que o assembly inclui a senha fixa das contas demonstrativas. Esse trecho só semeia contas quando o ambiente é Development e `Demo:SeedUsers=true`; Production foi testado sem esse seed e com usuário de bootstrap independente. A literal permanece no código publicado e deve ser removida em uma tarefa específica se a política exigir ausência de credenciais demonstrativas também no assembly.

Esta validação foi automatizada. A revisão por uma segunda pessoa deve ser registrada no próprio PR antes de considerar concluído o critério de revisão independente do cartão.
