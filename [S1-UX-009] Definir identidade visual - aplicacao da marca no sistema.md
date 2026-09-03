Aqui está o conteúdo do documento **Definição da Identidade Visual (SQUAD Estoque)** formatado e organizado em Markdown, incluindo todas as seções, tabelas e códigos corrigidos:

---

# SQUAD ESTOQUE

**Guia Visual, Identidade e Código de Referência de Interface Cartão [S1-UX-009]**

* **Área:** UX/Frontend


* **Reunião-alvo:** 26/08/2026


* **Prazo:** 09/09/2026 às 23:59 (BRT)


* **Branch Sugerida:** `docs/s1-ux-009-integrante`

* **Tecnologias:** HTML5, CSS3, JS Puro


* **Escopo:** Guia visual + Código limpo e revisado



---

## 1. Objetivo da Interface

Definir com precisão a linguagem visual, paleta de cores, tipografia, botões, estados de interação, posicionamento de marca, regras de responsividade e acessibilidade. O objetivo deste guia é orientar a construção dos protótipos e futuras Views do sistema SQUAD Estoque, garantindo padronização e excelência de código.

---

## 2. Sistema Visual e Paleta de Cores

A linguagem visual adota cabeçalhos em tom azul-petróleo, superfícies claras, blocos suavemente arredondados e tipografia limpa, priorizando contraste e legibilidade.

### Paleta Principal

| Aplicação / Elemento | Código Hex | Uso Visual |
| --- | --- | --- |
| **Institucional / Cabeçalho** | `#17657D` | Cabeçalho principal, títulos de seção e elementos primários de marca.

 |
| **Ação / Destaque** | `#55B9D9` | Botões de ação primária, focos de seleção e destaques de interação.

 |
| **Superfícies & Fundo** | `#FFFFFF` / `#F3F7F9` | Cards em fundo branco sobre superfície geral cinza-azulada clara.

 |
| **Texto Principal & Técnico** | `#20282D` / `#121C25` | `#20282D` para textos de leitura e `#121C25` para blocos de código/painéis.

 |

### Paleta de Estados de Feedback

| Estado | Referência | Uso e Aplicação |
| --- | --- | --- |
| **Sucesso** | `#38D39F` | Confirmação de operação concluída com êxito.

 |
| **Atenção / Alerta** | `#F1B84B` | Alertas e situações de pendência que exigem atenção do usuário.

 |
| **Erro** | `#E66A67` | Falha na operação e necessidade de nova tentativa ou correção.

 |

---

## 3. Diretrizes de Identidade Visual e Tipografia

* **Logo Oficial:** Não criar, redesenhar ou inventar versões alternativas da logo. Reservar o caminho `img/logo-squad-estoque.png` até que o arquivo oficial aprovado seja fornecido pela equipe de marca.


* **Tipografia:** Família padrão Arial, Helvetica, sans-serif. Títulos recebem maior peso e escala responsiva via `clamp()`; textos auxiliares mantêm legibilidade sem sobreposições em dispositivos móveis.



---

## 4. Componentes, Responsividade e Acessibilidade

| Componente / Regra | Diretriz de Design & Comportamento |
| --- | --- |
| **Botões** | Fundo azul-petróleo ou azul de ação, texto claro e bordas arredondadas (5px). Padrão de estados: Normal, Hover, Foco (outline 3px), Ativo, Sucesso, Vazio e Erro.

 |
| **Responsividade** | Layout fluido com Grid adaptável. Em telas até 600px, cabeçalhos se alinham verticalmente, a navegação empilha e os botões ocupam 100% da largura.

 |
| **Acessibilidade** | Labels obrigatoriamente associados aos campos, foco de teclado visível, contraste WCAG adequado, alt text na logo e tabelas semânticas com `<th scope="col">`.

 |

---

## 5. Código HTML5 de Referência (Revisado e Corrigido)

Estrutura semântica corrigida (sem falhas de fechamento de aspas ou inconsistências de datas):

```html
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
      <a class="marca" href="index.html" aria-label="Página inicial do SQUAD Estoque">
        <img src="img/logo-squad-estoque.png" alt="Logo oficial do SQUAD Estoque">
      </a>
      <div>
        <h1>SQUAD Estoque</h1>
        <p>Sistema de gestão de estoques</p>
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
        <p class="descricao">Consulte informações e acompanhe as movimentações do estoque de forma simples e organizada.</p>
        <div class="cards">
          <article class="card">
            <span class="icone" aria-hidden="true"></span>
            <h3>Produtos</h3>
            <p>Cadastre e consulte os produtos disponíveis.</p>
            <a href="#produtos">Acessar</a>
          </article>
          <article class="card">
            <span class="icone" aria-hidden="true"></span>
            <h3>Movimentações</h3>
            <p>Registre entradas, saídas e ajustes de estoque.</p>
            <a href="#movimentacoes">Acessar</a>
          </article>
          <article class="card">
            <span class="icone" aria-hidden="true"></span>
            <h3>Consultas</h3>
            <p>Consulte rapidamente as informações disponíveis.</p>
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
            <label for="produto">Nome do produto</label>
            <input type="text" id="produto" name="produto" placeholder="Digite o nome do produto">
          </div>
          <div class="campo">
            <label for="categoria">Categoria</label>
            <select id="categoria" name="categoria">
              <option value="">Selecione</option>
            </select>
          </div>
          <div class="acoes">
            <button type="submit" class="botao">Cadastrar</button>
            <button type="reset" class="botao botao-secundario">Limpar</button>
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
            <caption>Últimas movimentações do estoque</caption>
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
                <td>Notebook</td>
                <td>Entrada</td>
                <td>10</td>
                <td>26/08/2026</td>
              </tr>
              <tr>
                <td>Mouse</td>
                <td>Saída</td>
                <td>3</td>
                <td>26/08/2026</td>
              </tr>
              <tr>
                <td>Teclado</td>
                <td>Ajuste</td>
                <td>2</td>
                <td>26/08/2026</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>

    <!-- Consolidação/Consultas -->
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
      <p>&copy; 2026 SQUAD Estoque. Todos os direitos reservados.</p>
    </div>
  </footer>
</body>
</html>

```

---

## 6. Código CSS3 de Estilização e Layout (Revisado)

```css
/* Configurações Globais */
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

.navegacao a:focus {
  outline: 3px solid #55B9D9;
  outline-offset: -3px;
}

/* Seções e Geral */
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
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
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

/* Formulários e Botões */
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
}

.campo input:focus,
.campo select:focus {
  outline: 3px solid #55B9D9;
  border-color: #17657D;
}

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

.botao:focus {
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

th, td {
  padding: 0.8rem;
  border: 1px solid #DBE1E5;
  text-align: left;
}

th {
  background: #17657D;
  color: #FFFFFF;
}

/* Consultas */
.consulta {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
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
    text-align: center;
  }

  .secao {
    padding: 2.5rem 0;
  }
}

```

---

## 7. Matriz de Atendimento aos Requisitos

| Requisito / Critério | Status | Observação e Resultado |
| --- | --- | --- |
| **Respeito à Logo Oficial** | Concluído | Espaço reservado sem criar marcas fictícias ou não autorizadas.

 |
| **Cores & Contraste** | Concluído | Paleta oficial (`#17657D`, `#55B9D9`, `#F3F7F9`) aplicada integralmente.

 |
| **Erros de Código Sanitizados** | Concluído | Tags HTML, aspas de meta tags e datas da tabela corrigidas.

 |
| **Navegação & Semântica** | Concluído | Tags semânticas (`<header>`, `<nav>`, `<main>`, `<footer>`) e ARIA labels.

 |
