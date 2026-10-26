# Entrada de mercadorias por NF-e

## Escopo implementado

O módulo recebe manualmente o XML autorizado da NF-e. Ele não consulta a SEFAZ, não importa DANFE/PDF e não executa OCR.

1. O lojista envia um XML de até 5 MB.
2. O sistema lê chave, número, fornecedor e itens em modo seguro, sem DTD ou resolução externa.
3. A entrada e seus itens são persistidos como `RASCUNHO`; nenhum saldo é alterado.
4. Um vínculo anterior por CNPJ do fornecedor + código do item sugere automaticamente produto e SKU.
5. Itens não reconhecidos exigem conferência de modelo, marca, categoria, cor e numeração.
6. Uma foto JPG, PNG ou WebP de até 2 MB pode ser capturada pela câmera ou escolhida da galeria.
7. Ao confirmar, produtos, SKUs, saldos, movimentações e vínculos são gravados em uma única transação.
8. A chave de acesso única impede importação duplicada.

## Imagens

O arquivo é salvo em `wwwroot/uploads/produtos/` com nome aleatório. O banco mantém somente `Produto.ImagemUrl`. A imagem pertence ao modelo e é reutilizada por todas as numerações. Produtos sem foto continuam válidos.

## Opções no cadastro do lojista

O catálogo oferece os atalhos **Cadastro manual** e **Importar NF-e**. A tela **Novo produto** também apresenta as duas opções. No cadastro manual, os campos são agrupados em dados do modelo, foto opcional e numerações; marca e categoria sugerem valores já usados. A foto pode ser tirada pela câmera ou escolhida na galeria e tem prévia antes de salvar. Na edição, o lojista pode trocar ou remover a foto.

## Limitações conhecidas

- quantidades comerciais fracionárias são rejeitadas, pois o domínio controla pares inteiros;
- a sugestão inicial de numeração usa o último termo numérico da descrição e sempre depende de conferência;
- catálogos e APIs são específicos de cada fornecedor e permanecem fora deste MVP;
- PDF, OCR e download automático de documentos fiscais permanecem fora do escopo.
