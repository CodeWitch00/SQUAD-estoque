SQUAD ESTOQUE

Guia Visual, Identidade e Código de Referência de Interface — Cartão S1-UX-009

- Área: UX/Frontend
- Reunião-alvo: 26/08/2026
- Prazo: 09/09/2026 às 23:59 (BRT)
- Branch sugerida: "docs/s1-ux-009-integrante"
- Tecnologias: HTML5, CSS3, JavaScript e ASP.NET Core MVC
- Escopo: Guia visual + código de referência
- Documentação: "docs/05-ux/"

---

1. Objetivo

Definir uma referência visual curta e consistente para os protótipos e futuras Views do sistema SQUAD Estoque.

O guia estabelece referências para:

- identidade visual;
- cores e estados da interface;
- tipografia;
- botões e componentes;
- formulários e tabelas;
- ativos de marca;
- responsividade;
- acessibilidade;
- separação visual conforme o perfil de acesso.

Os exemplos devem representar o domínio de estoque de calçados e seguir o padrão ASP.NET Core MVC utilizado pela aplicação.

«Este documento não define regras de negócio nem implementa funcionalidades de Venda Rápida.»

---

2. Sistema Visual e Paleta de Cores

A linguagem visual utiliza cabeçalhos em azul-petróleo, superfícies claras, blocos suavemente arredondados e tipografia sem serifa.

As cores abaixo são tratadas como referência visual atual. Elas não devem ser classificadas como identidade visual oficial sem que sua origem seja confirmada nos protótipos, Views existentes ou documentação aprovada pela equipe responsável.

Paleta de referência

Aplicação| Cor| Uso
Institucional / Cabeçalho| "#17657D"| Cabeçalho, títulos e elementos primários
Ação / Destaque| "#55B9D9"| Ações primárias e destaques
Superfície| "#FFFFFF"| Cards, formulários e tabelas
Fundo geral| "#F3F7F9"| Fundo das páginas
Texto principal| "#20282D"| Textos de leitura
Texto técnico| "#121C25"| Código e informações técnicas

Estados

Estado| Cor| Uso
Sucesso| "#38D39F"| Operação concluída
Atenção| "#F1B84B"| Situações que exigem atenção
Erro| "#E66A67"| Erros e falhas de validação

Os estados não devem depender somente da cor para transmitir informação. Utilizar também texto ou ícone quando necessário.

Origem da paleta

A origem das cores deve ser registrada a partir de uma das seguintes fontes:

- protótipos aprovados;
- Views atualmente utilizadas pela aplicação;
- documentação de identidade visual;
- decisão formal da equipe de UX/Produto.

Enquanto essa origem não estiver confirmada, utilizar a classificação “referência visual atual”.

---

3. Verificação de Contraste

As principais combinações da interface devem atender aos critérios de contraste da WCAG.

Combinação| Aplicação| Contraste aproximado| Resultado
"#FFFFFF" sobre "#17657D"| Cabeçalho e títulos| 6,7:1| Adequado
"#20282D" sobre "#FFFFFF"| Texto principal| 14:1| Adequado
"#17657D" sobre "#FFFFFF"| Títulos e links| 6,7:1| Adequado
"#10232D" sobre "#55B9D9"| Texto do botão primário| 7,2:1| Adequado
"#17657D" sobre "#F3F7F9"| Texto sobre fundo geral| 5,9:1| Adequado

Os valores devem ser confirmados durante a validação final dos protótipos e da implementação.

---

4. Identidade Visual e Tipografia

Logo e ativos de marca

Não criar, redesenhar ou inventar versões alternativas da marca.

Os ativos existentes no repositório devem ser identificados e diferenciados entre:

- uso atual na aplicação;
- ativo aprovado;
- ativo aguardando confirmação.

A existência de um arquivo no repositório não significa automaticamente que ele seja oficialmente aprovado.

Inventário dos ativos

«Preencher esta tabela após a conferência do repositório.»

Arquivo| Localização| Uso atual| Status
"[arquivo encontrado]"| "[caminho]"| "[uso]"| Aprovado / A confirmar
"[arquivo encontrado]"| "[caminho]"| "[uso]"| Aprovado / A confirmar

Caso exista no projeto:

img/logo-squad-estoque.png

o arquivo deve ser validado antes de ser tratado como ativo oficial.

Tipografia

A referência atual utiliza:

font-family: Arial, Helvetica, sans-serif;

Títulos devem possuir maior peso visual e escala responsiva quando necessário.

h1 {
  font-size: clamp(1.8rem, 4vw, 2.5rem);
}

h2 {
  font-size: clamp(1.5rem, 3vw, 2rem);
}

Caso os protótipos aprovados definam outra família tipográfica, a decisão documentada deve prevalecer.

---

5. Componentes, Responsividade e Acessibilidade

Componente| Diretriz
Botões| Azul-petróleo ou azul de ação, contraste adequado e raio de 5px
Estados| Normal, Hover, Foco, Ativo, Desabilitado, Sucesso e Erro
Foco| Indicador visível, preferencialmente com "outline" de pelo menos 3px
Responsividade| Utilizar Grid/Flexbox conforme a necessidade
Mobile| Em telas até 600px, navegação pode ser empilhada e ações ocupar 100% da largura
Formulários| Todo campo deve possuir "<label>" associado
Tabelas| Utilizar estrutura semântica e "<th scope="col">"
Imagens| Utilizar "alt" descritivo para imagens informativas
Feedback| Não utilizar somente cor para comunicar estados

Separação por perfil

A interface deve respeitar os perfis existentes no sistema.

Lojista

- funcionalidades administrativas disponibilizadas para o perfil;
- gestão e consulta de estoque conforme as permissões da aplicação.

Vendedor

- funcionalidades disponibilizadas para o perfil;
- venda e consultas permitidas pela aplicação.

«A ocultação de elementos na interface não substitui a autorização no servidor.»

---

6. Código HTML5 de Referência

O exemplo representa o domínio de estoque de calçados.

No ASP.NET Core MVC, a estrutura deve ser adaptada para as respectivas Views ".cshtml".

<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>SQUAD Estoque</title>
  <link rel="stylesheet" href="style.css">
</head>

<body>

  <header class="cabecalho">
    <div class="container cabecalho-conteudo">

      <a
        class="marca"
        href="/"
        aria-label="Página inicial do SQUAD Estoque"
      >
        <img
          src="img/logo-squad-estoque.png"
          alt="Logo do SQUAD Estoque"
        >
      </a>

      <div>
        <h1>SQUAD Estoque</h1>
        <p>Sistema de gestão de estoque de calçados</p>
      </div>

    </div>
  </header>

  <nav class="navegacao" aria-label="Navegação principal">
    <div class="container">
      <ul>
        <li><a href="#inicio">Início</a></li>
        <li><a href="#produtos">Produtos</a></li>
        <li><a href="#movimentacoes">Movimentações</a></li>
        <li><a href="#consultas">Consultas</a></li>
      </ul>
    </div>
  </nav>

  <main>

    <section id="inicio" class="secao">
      <div class="container">

        <h2>Painel de Estoque</h2>

        <p class="descricao">
          Consulte produtos e acompanhe as movimentações
          do estoque de forma simples e organizada.
        </p>

        <div class="cards">

          <article class="card">
            <h3>Produtos</h3>
            <p>
              Cadastre e consulte os calçados disponíveis.
            </p>
            <a href="#produtos">Acessar</a>
          </article>

          <article class="card">
            <h3>Movimentações</h3>
            <p>
              Consulte entradas, saídas e ajustes de estoque.
            </p>
            <a href="#movimentacoes">Acessar</a>
          </article>

          <article class="card">
            <h3>Consultas</h3>
            <p>
              Consulte rapidamente os dados do estoque.
            </p>
            <a href="#consultas">Acessar</a>
          </article>

        </div>

      </div>
    </section>

    <section id="produtos" class="secao secao-clara">
      <div class="container">

        <h2>Produtos</h2>

        <form class="formulario">

          <div class="campo">
            <label for="produto">Nome do calçado</label>

            <input
              type="text"
              id="produto"
              name="produto"
              placeholder="Digite o nome do calçado"
            >
          </div>

          <div class="campo">
            <label for="categoria">Categoria</label>

            <select id="categoria" name="categoria">
              <option value="">Selecione</option>
              <option value="tenis">Tênis</option>
              <option value="sandalia">Sandália</option>
              <option value="bota">Bota</option>
              <option value="sapato">Sapato</option>
              <option value="chinelo">Chinelo</option>
              <option value="sapatilha">Sapatilha</option>
            </select>
          </div>

          <div class="acoes">
            <button type="submit" class="botao">
              Cadastrar
            </button>

            <button
              type="reset"
              class="botao botao-secundario"
            >
              Limpar
            </button>
          </div>

        </form>

      </div>
    </section>

    <section id="movimentacoes" class="secao">
      <div class="container">

        <h2>Movimentações</h2>

        <div class="tabela-container">

          <table>

            <caption>
              Últimas movimentações do estoque
            </caption>

            <thead>
              <tr>
                <th scope="col">Produto</th>
                <th scope="col">Tipo</th>
                <th scope="col">Quantidade</th>
                <th scope="col">Data</th>
              </tr>
            </thead>

            <tbody>
              <tr>
                <td>Tênis esportivo</td>
                <td>Entrada</td>
                <td>20</td>
                <td>26/08/2026</td>
              </tr>

              <tr>
                <td>Sandália feminina</td>
                <td>Saída</td>
                <td>3</td>
                <td>26/08/2026</td>
              </tr>
            </tbody>

          </table>

        </div>

      </div>
    </section>

  </main>

  <footer class="rodape">
    <div class="container">
      <p>
        &copy; 2026 SQUAD Estoque.
        Todos os direitos reservados.
      </p>
    </div>
  </footer>

</body>
</html>

---

7. Código CSS3 de Referência

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: Arial, Helvetica, sans-serif;
  color: #20282D;
  background: #F3F7F9;
  line-height: 1.5;
}

.container {
  width: min(92%, 1100px);
  margin: 0 auto;
}

/* Cabeçalho */

.cabecalho {
  background: #17657D;
  color: #FFFFFF;
  padding: 1.5rem 0;
}

.cabecalho-conteudo {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.marca {
  display: inline-flex;
  border-radius: 8px;
}

.marca img {
  width: 70px;
  height: 70px;
  object-fit: contain;
  background: #FFFFFF;
  border-radius: 8px;
}

.cabecalho h1 {
  font-size: clamp(1.8rem, 4vw, 2.5rem);
}

/* Navegação */

.navegacao {
  background: #FFFFFF;
  border-bottom: 1px solid #DBE1E5;
}

.navegacao ul {
  list-style: none;
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
}

.navegacao a {
  display: block;
  padding: 0.8rem 1rem;
  color: #17657D;
  text-decoration: none;
  font-weight: bold;
}

.navegacao a:hover {
  background: #EAF3F6;
}

.navegacao a:focus-visible {
  outline: 3px solid #55B9D9;
  outline-offset: -3px;
}

/* Seções */

.secao {
  padding: 4rem 0;
}

.secao-clara {
  background: #FFFFFF;
}

.secao h2 {
  color: #17657D;
  font-size: clamp(1.5rem, 3vw, 2rem);
  margin-bottom: 1rem;
}

/* Cards */

.cards {
  display: grid;
  grid-template-columns: repeat(
    auto-fit,
    minmax(220px, 1fr)
  );
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.card {
  background: #FFFFFF;
  border: 1px solid #DBE1E5;
  border-radius: 10px;
  padding: 1.5rem;
}

.card h3 {
  color: #17657D;
  margin-bottom: 0.5rem;
}

/* Formulários */

.formulario {
  max-width: 700px;
  display: grid;
  gap: 1rem;
}

.campo {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.campo label {
  font-weight: bold;
}

.campo input,
.campo select {
  width: 100%;
  padding: 0.8rem;
  border: 1px solid #999999;
  border-radius: 5px;
  font-size: 1rem;
  background: #FFFFFF;
  color: #20282D;
}

.campo input:focus-visible,
.campo select:focus-visible {
  outline: 3px solid #55B9D9;
  outline-offset: 2px;
}

/* Botões */

.acoes {
  display: flex;
  gap: 1rem;
}

.botao {
  padding: 0.7rem 1rem;
  border: 2px solid #17657D;
  border-radius: 5px;
  background: #55B9D9;
  color: #10232D;
  font-weight: bold;
  cursor: pointer;
}

.botao:hover {
  background: #3CA8CB;
}

.botao:focus-visible {
  outline: 3px solid #17657D;
  outline-offset: 3px;
}

.botao-secundario {
  background: #FFFFFF;
  color: #17657D;
}

/* Tabelas */

.tabela-container {
  width: 100%;
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: #FFFFFF;
}

caption {
  text-align: left;
  font-weight: bold;
  margin-bottom: 0.75rem;
}

th,
td {
  padding: 0.8rem;
  border: 1px solid #DBE1E5;
  text-align: left;
}

th {
  background: #17657D;
  color: #FFFFFF;
}

/* Rodapé */

.rodape {
  padding: 2rem 0;
  background: #17657D;
  color: #FFFFFF;
  text-align: center;
}

/* Responsividade */

@media (max-width: 600px) {

  .cabecalho-conteudo {
    flex-direction: column;
    text-align: center;
  }

  .navegacao ul {
    flex-direction: column;
  }

  .navegacao a {
    text-align: center;
    border-bottom: 1px solid #DBE1E5;
  }

  .acoes {
    flex-direction: column;
  }

  .acoes .botao {
    width: 100%;
  }

  .secao {
    padding: 2.5rem 0;
  }
}

---

8. Matriz de Atendimento

Requisito| Status| Resultado
Descrição do PR| Corrigido| Escopo limitado ao guia de identidade visual e código de referência
Ativos de marca| Em validação| Inventário deve ser preenchido com os arquivos reais do repositório
Aprovação da marca| Em validação| Uso atual não é tratado automaticamente como aprovação oficial
Cores| Em validação| Tratadas como referência visual até confirmação da origem
Contraste| Atendido| Principais combinações verificadas
Tipografia| Atendido| Arial, Helvetica, sans-serif
Responsividade| Atendido| Grid/Flexbox e comportamento mobile
Acessibilidade| Atendido| Labels, foco visível, "alt" e tabelas semânticas
Domínio| Corrigido| Exemplos representam estoque de calçados
Perfis| Atendido| Separação entre Lojista e Vendedor
Arquitetura| Atendido| Referência alinhada ao ASP.NET Core MVC
Guia| Atendido| Conteúdo reduzido e focado em orientar protótipos e Views
Venda Rápida| Fora do escopo| Não faz parte desta branch
Documentação| Atendido| Destinada a "docs/05-ux/"
