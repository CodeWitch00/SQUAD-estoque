# Verificação de persistência do banco no Azure

## Ambiente e objetivo

- App Service: `asp-squad-estoque-free`.
- Banco configurado no runbook da `main`: `/home/data/Estoque.db`.
- Objetivo: confirmar que um produto fictício permanece após reiniciar o App Service, antes de configurar a publicação automática.

## Registro anterior ao reinício

Em 24/09/2026, por volta de 20:36 UTC, a conta fictícia de `LOJISTA` cadastrou o produto abaixo pelo formulário da aplicação publicada. O cadastro foi consultado novamente em uma sessão independente.

| Campo | Valor |
| --- | --- |
| Nome | `TESTE PERSISTENCIA CD` |
| ID | `26330c97-4324-45a2-a058-705a296f965b` |
| Marca | `SQUAD` |
| Categoria | `Teste` |
| Cor | `Azul` |
| Numeração | `40` |

**Resultado antes do reinício:** o produto apareceu no catálogo e a página de detalhes confirmou nome, ID e demais campos.

## Reinício e conferência

| Etapa | Resultado |
| --- | --- |
| Reiniciar `asp-squad-estoque-free` no Portal Azure | Executado pela responsável, confirmado por mensagem |
| Aguardar o aplicativo voltar e consultar `/health` | `Healthy`, HTTP 200 |
| Abrir `/Produtos/Details/26330c97-4324-45a2-a058-705a296f965b` em nova sessão | HTTP 200 após novo login de `LOJISTA` |
| Conferir nome, marca, categoria, cor e numeração | Todos os campos e a numeração 40 permaneceram presentes |

**Conclusão:** a verificação após o reinício foi aprovada em 24/09/2026, por volta de 20:54 UTC. Ela comprova a persistência deste registro através de um reinício do App Service. A preservação após uma nova publicação ainda precisa ser conferida separadamente.

## Preparação do backup

No SSH do App Service, o banco `/home/data/Estoque.db` estava presente com 104 KiB. Como o contêiner não tinha `sqlite3` nem `python3`, foi baixada uma ferramenta temporária oficial do SQLite em `/tmp` e usada a operação `.backup` para criar a cópia consistente.

| Verificação | Resultado |
| --- | --- |
| Cópia criada em `/home/data/backups` | `Estoque-20260924-223611.db`, 106.496 bytes |
| `PRAGMA integrity_check` | `ok` |
| Cópia baixada para armazenamento local | Confirmado pela responsável; mantida fora dos commits |
| Restauração da cópia | Ainda não testada |

**Conclusão:** existe uma cópia íntegra do banco anterior ao deploy automático, guardada fora do App Service. Isso não comprova ainda que o procedimento de restauração funciona.

Nenhuma migração, exclusão de dados ou publicação foi executada nesta etapa.
