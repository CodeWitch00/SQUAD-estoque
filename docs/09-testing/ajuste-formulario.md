# Ajuste: validação do motivo no formulário

Data: 27/10/2026. Branch: `dev/emmy`. PR: [#71](https://github.com/CodeWitch00/SQUAD-estoque/pull/71).

## Correção da revisão

O navegador validava o comprimento bruto, embora o servidor validasse o motivo após normalizar os espaços. Um texto de 500 caracteres com dois espaços em cada extremidade era rejeitado no cliente, mesmo sendo válido no servidor.

A view agora carrega `ajuste.js`. O script adiciona um normalizador ao campo Motivo do jQuery Validation antes da aplicação das regras obrigatória, mínima e máxima, mantendo os limites e mensagens emitidos pelas DataAnnotations. Os separadores Unicode correspondem aos de `Char.IsWhiteSpace`, usados pelo `String.Split` no servidor. O atributo nativo `maxlength` é removido para permitir digitar o texto bruto completo. A normalização não altera o texto durante a digitação; o servidor continua responsável pela normalização persistida e pela validação quando JavaScript não está disponível.

## Testes

`tests/frontend/ajuste-motivo.test.cjs` executa o formulário em Chromium com as bibliotecas reais de jQuery Validation e Unobtrusive do projeto. Intercepta apenas o envio, após os validadores, e verifica se o formulário permite ou bloqueia a submissão e apresenta a mensagem esperada. O teste HTTP de GET confirma que a view renderizada contém o marcador, o script versionado e os limites esperados.

| Entrada | Comportamento esperado e confirmado |
| --- | --- |
| 500 caracteres com espaços nas extremidades (504 digitados) | Permite enviar |
| 500 após normalização de espaços internos, tabulações e Unicode | Permite enviar |
| 5 com espaços nas extremidades | Permite enviar |
| 501 após normalização | Bloqueia com erro de máximo |
| Menos de 5 após normalização | Bloqueia com erro de mínimo |
| Somente espaços, tabulações, quebras de linha e Unicode | Bloqueia como obrigatório |
| Vazio | Bloqueia como obrigatório |

Suíte frontend completa: 27 testes aprovados, zero falhas. Suíte .NET completa: 269 testes aprovados, zero falhas e zero ignorados. Build .NET: aprovado, zero avisos e zero erros. `git diff --check`: aprovado.

## Validação na aplicação real

Aplicação local em `http://127.0.0.1:5287`, banco isolado. Login como LOJISTA, SKU com saldo inicial 12.

1. Digitados 500 caracteres `a`, com dois espaços em cada extremidade; novo saldo 15. O formulário permitiu enviar e a aplicação redirecionou para os detalhes com saldo 15.
2. Banco confirmou uma movimentação AJUSTE com quantidade 3 e motivo persistido de exatamente 500 caracteres.
3. Digitados 501 caracteres, também com espaços nas extremidades; novo saldo 18. O navegador bloqueou o envio e exibiu “O motivo não pode exceder 500 caracteres.”. O banco manteve saldo 15.

- [Captura: ajuste aceito](evidencias/ajuste-formulario/limite-500-aceito.png).
- [Captura: excesso rejeitado](evidencias/ajuste-formulario/limite-501-rejeitado.png).
- [Conferência da persistência](evidencias/ajuste-formulario/persistencia.json).
