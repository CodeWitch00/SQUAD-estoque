SQUAD ESTOQUE

Guia Visual, Identidade e Código de Referência de Interface — Cartão [S1-UX-009]

* Área: UX/Frontend
* Reunião-alvo: 26/08/2026
* Prazo: 09/09/2026 às 23:59 (BRT)
* Branch sugerida: "docs/s1-ux-009-integrante"
* Tecnologias: HTML5, CSS3, JavaScript e ASP.NET Core MVC
* Escopo: Guia visual + código limpo e revisado
* Documentação: "docs/05-ux/"

---

1. Objetivo da Interface

Definir uma referência visual curta e consistente para os protótipos e futuras Views do sistema SQUAD Estoque.

O guia estabelece referências para:

- identidade visual;
- cores e estados de interface;
- tipografia;
- botões e componentes;
- formulários e tabelas;
- posicionamento dos ativos de marca;
- responsividade;
- acessibilidade;
- separação visual conforme o perfil de acesso.

Os exemplos devem representar o domínio de estoque de calçados e seguir o padrão arquitetural ASP.NET Core MVC utilizado pela aplicação.

Este documento não define regras de negócio nem implementa funcionalidades de Venda Rápida.

---

2. Sistema Visual e Paleta de Cores

A linguagem visual utiliza cabeçalhos em tom azul-petróleo, superfícies claras, blocos suavemente arredondados e tipografia sem serifa.

As cores abaixo representam a referência visual atualmente documentada para a interface. Elas não devem ser classificadas como identidade visual oficial sem que sua origem seja confirmada nos protótipos, Views existentes ou documentação aprovada pela equipe responsável.

Paleta Principal

Aplicação / Elemento| Código Hex| Uso Visual
Institucional / Cabeçalho| "#17657D"| Cabeçalho, títulos de seção e elementos primários
Ação / Destaque| "#55B9D9"| Ações primárias e destaques de interação
Superfícies| "#FFFFFF"| Cards, formulários, tabelas e áreas de conteúdo
Fundo geral| "#F3F7F9"| Fundo principal das páginas
Texto principal| "#20282D"| Textos de leitura
Texto técnico| "#121C25"| Código e painéis técnicos

Origem das cores

A origem da paleta deve ser registrada durante a revisão do cartão:

- protótipos aprovados;
- Views atualmente utilizadas pela aplicação;
- documentação de identidade visual;
- decisão formal da equipe de UX/Produto.

Enquanto a origem não estiver formalmente confirmada, utilizar a classificação “referência visual atual”, e não “paleta oficial”.

Paleta de Estados de Feedback

Estado| Referência| Uso e Aplicação
Sucesso| "#38D39F"| Confirmação de operação concluída
Atenção / Alerta| "#F1B84B"| Pendências e situações que exigem atenção
Erro| "#E66A67"| Falhas e erros de validação

Os estados não devem depender somente da cor para transmitir informação. Sempre que necessário, utilizar texto ou ícone complementar.

Verificação de Contraste

As principais combinações utilizadas na interface devem atender aos critérios de contraste da WCAG.

Combinação| Aplicação| Contraste aproximado| Resultado
"#FFFFFF" sobre "#17657D"| Cabeçalho e títulos em fundo azul| 6,7:1| Adequado para texto normal
"#20282D" sobre "#FFFFFF"| Texto principal| 14:1| Adequado
"#17657D" sobre "#FFFFFF"| Títulos e links| 6,7:1| Adequado
"#10232D" sobre "#55B9D9"| Texto do botão primário| 7,2:1| Adequado
"#17657D" sobre "#F3F7F9"| Texto sobre fundo geral| 5,9:1| Adequado

Os valores devem ser confirmados durante a validação final do protótipo/implementação.

---

3. Diretrizes de Identidade Visual e Tipografia

Logo e ativos de marca

Não criar, redesenhar ou inventar versões alternativas da marca.

Os arquivos de imagem existentes no repositório devem ser inventariados antes de definir qual deles é considerado oficial.

O caminho abaixo pode ser utilizado como referência somente se o arquivo realmente existir no projeto e estiver validado:

img/logo-squad-estoque.png

A presença de um arquivo no repositório não significa automaticamente que ele esteja aprovado pela equipe de marca.

A documentação deve registrar:

Arquivo| Localização| Uso| Aprovação
"[arquivo encontrado]"| "[caminho]"| "[uso]"| Confirmar
"[arquivo encontrado]"| "[caminho]"| "[uso]"| Confirmar

Tipografia

A referência atual utiliza:

font-family: Arial, Helvetica, sans-serif;

Títulos devem possuir maior peso visual e escala responsiva utilizando "clamp()" quando apropriado.

Exemplo:

h1 {
  font-size: clamp(1.8rem, 4vw, 2.5rem);
}

h2 {
  font-size: clamp(1.5rem, 3vw, 2rem);
}

Caso os protótipos aprovados definam outra família tipográfica, essa decisão deve prevalecer sobre esta referência.

---

4. Componentes, Responsividade e Acessibilidade

Componente / Regra| Diretriz de Design & Comportamento
Botões| Azul-petróleo ou azul de ação, texto com contraste adequado e bordas arredondadas de 5px
Estados| Normal, Hover, Foco, Ativo, Desabilitado, Sucesso e Erro
Foco| Indicador de foco visível, com "outline" de pelo menos 3px
Responsividade| Layout fluido utilizando Grid/Flexbox conforme necessidade
Mobile| Em telas até 600px, navegação pode ser empilhada e ações podem ocupar 100% da largura
Formulários| Todo campo deve possuir "<label>" associado
Tabelas| Utilizar estrutura semântica e "<th scope="col">"
Imagens| Utilizar "alt" descritivo quando a imagem possuir função informativa
Estados| Não utilizar apenas cor para comunicar sucesso, alerta ou erro

Separação por perfil

A interface deve respeitar os perfis existentes no sistema.

Lojista

- acesso às funcionalidades administrativas disponibilizadas para o perfil;
- gestão e consulta de estoque conforme as permissões definidas pela aplicação.

Vendedor

- acesso somente às funcionalidades disponibilizadas para o perfil;
- funcionalidades de venda e consulta permitidas pela aplicação.

A ocultação de elementos na interface não substitui a autorização no servidor.

---

5. Código HTML5 de Referência (Revisado e Corrigido)

O exemplo abaixo representa o domínio de estoque de calçados e serve como referência estrutural para futuras Views.

No ASP.NET Core MVC, o conteúdo equivalente deve ser adaptado para as respectivas Views ".cshtml".

<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>SQUAD Estoque</title>
  <link rel="stylesheet" href="style.css">
</head>

<body>
  <!-- Cabeçalho -->
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

  <!-- Navegação -->
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

  <!-- Conteúdo Principal -->
  <main>

    <!-- Painel de Início -->
    <section id="inicio" class="secao">
      <div class="container">
        <h2>Painel de Estoque</h2>

        <p class="descricao">
          Consulte produtos e acompanhe as movimentações do estoque
          de forma simples e organizada.
        </p>

        <div class="cards">

          <article class="card">
            <h3>Produtos</h3>
            <p>
              Cadastre e consulte os calçados disponíveis no estoque.
            </p>
            <a href="#produtos">Acessar</a>
          </article>

          <article class="card">
            <h3>Movimentações</h3>
            <p>
              Registre e consulte entradas, saídas e ajustes de estoque.
            </p>
            <a href="#movimentacoes">Acessar</a>
          </article>

          <article class="card">
            <h3>Consultas</h3>
            <p>
              Consulte rapidamente os dados disponíveis no estoque.
            </p>
            <a href="#consultas">Acessar</a>
          </article>

        </div>
      </div>
    </section>

    <!-- Cadastro de Produtos -->
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

    <!-- Tabela de Movimentações -->
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

              <tr>
                <td>Bota casual</td>
                <td>Ajuste</td>
                <td>2</td>
                <td>26/08/2026</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>

    <!-- Consultas -->
    <section id="consultas" class="secao secao-clara">
      <div class="container">
        <h2>Controle de Estoque</h2>

        <div class="consulta">

          <div>
            <strong>Produtos cadastrados</strong>
            <span>125</span>
          </div>

          <div>
            <strong>Itens disponíveis</strong>
            <span>480</span>
          </div>

          <div>
            <strong>Itens em baixa</strong>
            <span>12</span>
          </div>

        </div>
      </div>
    </section>

  </main>

  <!-- Rodapé -->
  <footer class="rodape">
    <div class="container">
      <p>
        &copy; 2026 SQUAD Estoque. Todos os direitos reservados.
      </p>
    </div>
  </footer>

</body>
</html>

---

6. Código CSS3 de Estilização e Layout (Revisado)

/* =========================================
   Configurações Globais
   ========================================= */

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: Arial, Helvetica, sans-serif;
  color: #20282D;
  background-color: #F3F7F9;
  line-height: 1.5;
}

.container {
  width: min(92%, 1100px);
  margin: 0 auto;
}

/* =========================================
   Cabeçalho
   ========================================= */

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

.cabecalho p {
  margin-top: 0.25rem;
}

/* =========================================
   Navegação
   ========================================= */

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

/* =========================================
   Seções
   ========================================= */

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

.descricao {
  max-width: 750px;
}

/* =========================================
   Cards
   ========================================= */

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
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.card h3 {
  color: #17657D;
  margin-bottom: 0.5rem;
}

.card a {
  display: inline-block;
  margin-top: 1rem;
  color: #17657D;
  font-weight: bold;
}

/* =========================================
   Formulários
   ========================================= */

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
  border-color: #17657D;
}

/* =========================================
   Botões
   ========================================= */

.acoes {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.botao {
  display: inline-block;
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

.botao:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

/* =========================================
   Tabelas
   ========================================= */

.tabela-container {
  width: 100%;
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: #FFFFFF;
}

/* =========================================
   Consultas
   ========================================= */

.consulta {
  display: grid;
  grid-template-columns: repeat(
    auto-fit,
    minmax(180px, 1fr)
  );

  gap: 1rem;
}

.consulta div {
  display: flex;
  flex-direction: column;
  padding: 1.5rem;
  background: #F3F7F9;
  border: 1px solid #DBE1E5;
  border-radius: 8px;
}

.consulta span {
  color: #17657D;
  font-size: 1.8rem;
  font-weight: bold;
}

/* =========================================
   Rodapé
   ========================================= */

.rodape {
  padding: 2rem 0;
  background: #17657D;
  color: #FFFFFF;
  text-align: center;
}

/* =========================================
   Responsividade
   ========================================= */

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
    text-align: center;
  }

  .secao {
    padding: 2.5rem 0;
  }
}
7. Matriz de Atendimento aos Requisitos
Requisito / Critério
Status
Observação e Resultado
Respeito aos Ativos de Marca
Em validação
Os arquivos existentes no repositório devem ser inventariados e sua aprovação formal confirmada
Cores
Em validação
A paleta foi mantida como referência visual atual; a classificação como oficial depende da origem documentada
Contraste
Atendido
Principais combinações foram verificadas como referência de acessibilidade
Tipografia
Atendido
Arial, Helvetica, sans-serif mantida como referência
Responsividade
Atendido
Grid/Flexbox e comportamento específico para telas de até 600px
Acessibilidade
Atendido
Labels associados, foco visível, tabelas semânticas e textos alternativos
Domínio
Corrigido
Exemplos alterados de produtos eletrônicos para estoque de calçados
Perfis
Atendido
Interface considera separação entre Lojista e Vendedor
Arquitetura
Atendido
Exemplos alinhados ao padrão ASP.NET Core MVC
Código de referência
Atendido
HTML/CSS mantidos curtos e focados na orientação visual
Venda Rápida
Fora do escopo
Nenhum VendaRapidaService, TypeScript ou Jest foi incluído
Documentação
Atendido
Documento destinado a docs/05-ux/
