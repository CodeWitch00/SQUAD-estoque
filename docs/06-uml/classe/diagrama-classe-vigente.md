# Diagrama de classes vigente

O diagrama de classes do SQUAD Estoque é apresentado na monografia em duas
visões complementares para manter legíveis os atributos C#, as associações e
suas multiplicidades em página A4. As duas figuras representam as nove classes
de domínio persistidas e as quinze associações do modelo vigente.

**Commit da implementação conferida:** `26f042289596a3785a4624e4ed9a82d2888c0175`.

| Visão | Classes | Associações | Fonte editável | SVG | PNG |
| --- | ---: | ---: | --- | --- | --- |
| Catálogo e atendimento | 6 | 10 | [Mermaid](classe-catalogo-atendimento.mmd) | [Figura](classe-catalogo-atendimento.svg) | [Imagem](classe-catalogo-atendimento.png) |
| Entrada fiscal | 6 | 5 | [Mermaid](classe-entrada-fiscal.mmd) | [Figura](classe-entrada-fiscal.svg) | [Imagem](classe-entrada-fiscal.png) |

`Usuario`, `Produto` e `Sku` aparecem nas duas visões para tornar cada figura
compreensível de forma independente. Os atributos são uma seleção do modelo
orientado a objetos: a visão de catálogo prioriza estado, resultado e coleções
de navegação; a visão fiscal prioriza os vínculos e dados da importação. Os
tipos exibidos são tipos C#, e as associações não usam setas de navegabilidade.

As chaves estrangeiras escalares omitidas da figura de catálogo permanecem
nas [entidades C#](../../../src/SquadEstoque.Web/Models/Entities/). O esquema
SQLite, suas colunas e restrições físicas estão documentados no
[DER vigente](../der/der-vigente.md). Este diagrama de classes não deve
ser interpretado como uma reprodução integral das tabelas do banco.

Para a página A4 retrato, a visão de catálogo pode ser inserida com 16 × 22,1
cm. A visão fiscal, mais alta, deve ser inserida com aproximadamente 13,2 ×
23,2 cm, reservando espaço adicional para a legenda no Word.
