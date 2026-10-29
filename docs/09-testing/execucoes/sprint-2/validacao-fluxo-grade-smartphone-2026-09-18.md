# Validação do fluxo até a grade — primeira consulta em smartphone

**Cartão:** S2-QA-006
**Data:** 18/09/2026
**Branch:** `dev/rayana`
**Viewport:** 390 × 844 px, orientação vertical
**Navegador:** Brave com emulação de viewport móvel
**Perfil:** VENDEDOR

## Objetivo e percurso

Foi validado o percurso da primeira consulta após o login:

1. tocar no campo Produto e digitar um termo;
2. aguardar a busca automática;
3. tocar no produto encontrado;
4. conferir a grade, numerações e saldos.

Após o ajuste da busca automática, o percurso chegou à grade com **dois toques** após o login. A digitação não foi contada como toque adicional.

## Resultado funcional

- Login como VENDEDOR: aprovado.
- Busca automática após a digitação: aprovada.
- Seleção do produto: aprovada.
- Grade exibida: aprovada.
- Numerações verificadas: 37, 38 e 39.
- Saldos verificados: 0 pares, 1 par e 2 pares.
- Estados exibidos: INDISPONÍVEL, ÚLTIMO PAR e DISPONÍVEL.

Também foram testados os termos `Tenis`, `tesnis` e `Squad`. A busca passou a ignorar acentos, diferenças de maiúsculas e minúsculas e um erro simples de digitação. A pesquisa continua contemplando nome, marca, categoria e cor.

## Resultado responsivo

- `scrollWidth` igual à largura do viewport: aprovado.
- Rolagem horizontal: não observada.
- Sobreposição de elementos: não observada.
- Campo de busca, resultado e grade: legíveis e acionáveis.
- A seleção utiliza a âncora `#grade` para posicionar a grade na área visível.
- A rolagem vertical permanece disponível para conteúdo abaixo da dobra.

## Falha encontrada e correção

Na primeira execução, a busca exigia o botão **Buscar**, resultando em três toques. Além disso, a comparação literal não encontrava `Tenis` quando o cadastro usava `Tênis`.

Foram aplicados:

- debounce no campo de consulta;
- normalização de acentos;
- tolerância a uma alteração de caractere em palavras com pelo menos quatro letras;
- âncora de posicionamento da grade;
- testes automatizados para busca sem acento, erro simples e marca.

## Verificações técnicas

- `dotnet build src/SquadEstoque.Web/SquadEstoque.Web.csproj --no-restore`: aprovado, 0 erros e 0 avisos.
- Testes `ConsultaOperacionalHttpTests`: 6 aprovados.
- `git diff --check`: aprovado.

## Limitações e pendências

Esta evidência é uma validação técnica em viewport emulado. Ainda é necessário executar:

- o mesmo fluxo nos viewports `360×800` e `430×932`;
- a rodada com cinco vendedores sem treinamento prevista em NFT-MOB-05;
- a conferência das evidências por outro integrante.

As capturas completas do percurso foram registradas durante a execução local no Brave.
