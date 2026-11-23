# Diagrama de classes vigente

O diagrama de classes do SQUAD Estoque é apresentado na monografia em duas
visões complementares para manter legíveis os atributos C#, as associações e
suas multiplicidades em página A4. As duas figuras representam as nove classes
de domínio persistidas e as quinze associações do modelo vigente.

| Visão | Classes | Associações | Fonte editável | Figura vetorial |
| --- | ---: | ---: | --- | --- |
| Catálogo e atendimento | 6 | 10 | [Mermaid](classe-catalogo-atendimento.mmd) | [SVG](classe-catalogo-atendimento.svg) |
| Entrada fiscal | 6 | 5 | [Mermaid](classe-entrada-fiscal.mmd) | [SVG](classe-entrada-fiscal.svg) |

`Usuario`, `Produto` e `Sku` aparecem nas duas visões para tornar cada figura
compreensível de forma independente. Os atributos são uma seleção do modelo
orientado a objetos: a visão de catálogo prioriza estado, resultado e coleções
de navegação; a visão fiscal prioriza os vínculos e dados da importação. Os
tipos exibidos são tipos C#, e as associações não usam setas de navegabilidade.

As chaves estrangeiras escalares omitidas da figura de catálogo permanecem
nas [entidades C#](../../../src/SquadEstoque.Web/Models/Entities/). O esquema
SQLite, suas colunas e restrições físicas estão documentados no
[DER vigente](../der-fisico/der-vigente.md). Este diagrama de classes não deve
ser interpretado como uma reprodução integral das tabelas do banco.
