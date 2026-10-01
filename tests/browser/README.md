# S2-FE-027 - confirmação de ruptura

Teste de navegador opcional, separado da suíte xUnit. Requer Node.js, Playwright
disponível no ambiente e Chrome instalado (ou `TEST_BROWSER` apontando para um
canal instalado). Não adiciona dependências à aplicação. Executado localmente
no Windows em 01/10/2026; não integra o workflow atual do GitHub Actions.

1. Inicie a aplicação em Development com um **banco SQLite descartável**,
   configurando `ConnectionStrings__EstoqueContext` e
   `ConnectionStrings__LegacyMovieContext` para arquivos de teste.
2. Habilite `Demo__SeedUsers=true` somente nesse ambiente local. O teste usa o
   vendedor de demonstração documentado no projeto.
3. Após a criação do banco pela aplicação, execute:
   `python tests/browser/seed-ruptura.py CAMINHO_DO_BANCO_DESCARTAVEL`.
4. Defina `TEST_BASE_URL` para o endereço local (padrão `http://localhost:5277`)
   e `TEST_OUTPUT` para uma pasta de evidências fora dos arquivos versionados.
5. Execute `node tests/browser/ruptura-feedback.cjs`.

O cenário usa um nome de produto contendo uma tag HTML deliberadamente. A tag
deve aparecer como texto, sem criar elementos ou executar código na mensagem.
Cada execução bem-sucedida registra exatamente duas rupturas reais no SKU 38.
Os cenários HTTP 400 e falha de rede são simulados com interceptação no navegador.
Não execute contra banco de produção ou de uso pessoal.

## Cobertura e resultado registrado

- 88 testes xUnit aprovados em Release, incluindo a correção da comparação de
  espaços/quebras de linha no HTML, compatível com Windows e Linux.
- Oito cenários de navegador aprovados: sucesso por teclado; recarga sem novo
  envio; HTTP 400; rede indisponível; envio duplicado bloqueado enquanto pendente;
  layout mobile 390 x 844; troca de numeração; nova consulta.
- Grade e painel de ações não se sobrepõem em desktop e mobile.
- A consulta direta ao banco descartável confirmou saldos 38=0 e 39=3 e nenhuma
  movimentação de estoque criada pelas rupturas.
- Build sem erros. Aviso NU1900 por indisponibilidade da consulta de
  vulnerabilidades do NuGet no ambiente; não representa teste funcional falhando.

## Limites

A proteção de envio é da interface e não representa idempotência no servidor
entre abas ou retransmissões externas. Recarregar não envia um novo POST nem
reapresenta a confirmação. A seleção é preservada após sucesso/erro na página
atual; após recarga, o vendedor seleciona novamente a numeração. A validação de
teclado foi automatizada em Chrome; não houve auditoria com leitor de tela nem
teste em aparelho físico. Revisão por outro integrante permanece necessária.
