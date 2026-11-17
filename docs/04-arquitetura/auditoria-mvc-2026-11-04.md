# Auditoria MVC — separação de responsabilidades

## Escopo e referência

Auditoria de autenticação, estoque, venda, ruptura e relatórios, com inspeção
de Controllers, entidades, ViewModels, Razor, JavaScript próprio e testes.
Este documento registra recomendações; nenhuma correção ou refatoração foi
implementada neste cartão.

Base auditada: [`f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0`](https://github.com/CodeWitch00/ESTOQUE/tree/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0).
As referências por linha abaixo apontam para essa revisão imutável.

Fontes: [guia de contribuição](../../CONTRIBUTING.md),
[arquitetura](arquitetura.md), [domínio](../01-negocio/dominio.md),
[requisitos](../02-requisitos/srs.md),
[casos de uso](../02-requisitos/casos-de-uso.md) e código da revisão indicada.

## Parecer

As regras críticas examinadas são executadas no servidor. Não foi identificada,
na inspeção realizada, autorização ou validação de saldo cuja única proteção
seja Razor ou JavaScript.

A separação MVC é parcial: a maioria das telas usa ViewModels, mas há telas
que recebem entidades e ViewModels que ainda transportam entidades completas.
Há também cálculo de resumo no Razor e classificação visual repetida no
JavaScript. Essas ocorrências não demonstram, por si, alteração indevida de
saldo ou autorização contornável. O risco funcional mais concreto está na
divergência entre o resumo reapresentado da NF-e e os dados usados na persistência.

## Evidências por fluxo

| Fluxo | Evidência no servidor | Papel da interface e conclusão |
| --- | --- | --- |
| Autenticação | [AccountController:35–94](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/AccountController.cs#L35-L94): ModelState, BCrypt, claims geradas a partir do usuário persistido, destino local e antiforgery no login/logout. [Program:24–33](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Program.cs#L24-L33): autenticação por cookies. | Login usa `LoginViewModel`. Ocultação de menus por perfil em `_Layout` é apresentação; Controllers também restringem acesso. |
| Consulta de estoque | [EstoqueController:12–180](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/EstoqueController.cs#L12-L180): perfil VENDEDOR, identidade, tamanho do termo, produtos/SKUs ativos e projeção para ViewModels. | Razor agrupa disponibilidade; o navegador seleciona SKU e envia identificadores. Esses dados não substituem a consulta ao banco. Ver MVC-03. |
| Entrada e ajuste | [MovimentacoesController:221–448](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs#L221-L448) e [630–712](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/MovimentacoesController.cs#L630-L712): LOJISTA, antiforgery, quantidade, motivo, saldo apurado, identidade e transações. | `min` e validação de formulário são auxiliares. O ajuste calcula diferença absoluta no servidor, conforme RN-02; não é defeito a ausência de sinal na quantidade histórica. |
| Venda | [EstoqueController:283–343](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/EstoqueController.cs#L283-L343) e [SaidaEstoque:10–116](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Data/SaidaEstoque.cs#L10-L116): SKU/produto, vendedor, atendimento pendente, quantidade fixa de um par na venda rápida, UPDATE condicionado ao saldo, movimento e commit na mesma transação. | `consulta.js` bloqueia botões por conveniência e atualiza a tela com `data.saldoAtual`; não calcula a baixa persistida. Desabilitar JavaScript não remove a proteção de saldo. |
| Ruptura e desistência | [EstoqueController:251–278](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/EstoqueController.cs#L251-L278) e [346–400](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/EstoqueController.cs#L346-L400): antiforgery, proprietário/status do atendimento e SKU ativo pertencente ao produto. | “Não tinha” cria ruptura explícita sem baixar saldo, inclusive com saldo positivo. “Desistiu” encerra atendimento sem criar venda/ruptura. Isso respeita a distinção entre saldo físico e demanda não atendida. |
| Relatórios | [RupturasController:9–34](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/RupturasController.cs#L9-L34), [SaldosZeradosController:9](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/SaldosZeradosController.cs#L9) e [AtendimentosController:9–77](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/AtendimentosController.cs#L9-L77): LOJISTA, consultas, agrupamentos, contagens e alertas no servidor. | As telas recebem ViewModels e exibem resultados. Ressalva de recorte de dados em MVC-04; divergência de versão de saldos zerados descrita adiante. |

### MVC-01 — entidades entregues à apresentação — P2

**Arquivos e evidências:**
[EntradasFiscais/Index.cshtml:1](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Views/EntradasFiscais/Index.cshtml#L1),
[Produtos/Details.cshtml:1](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Views/Produtos/Details.cshtml#L1),
[ProdutosIndexViewModel:8](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Models/ViewModels/ProdutosIndexViewModel.cs#L8) e
[MovimentacoesIndexViewModel:9](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Models/ViewModels/MovimentacoesIndexViewModel.cs#L9).

**Constatação:** a listagem fiscal recebe `IReadOnlyList<EntradaFiscal>`;
detalhes recebe `Produto`. As listagens de produtos e movimentações possuem
ViewModel externo, mas suas coleções contêm entidades persistidas.
Portanto, não é possível confirmar uso integral de ViewModels próprios nas telas.

**Risco:** acoplamento a navegações e ao schema; inclusão futura de campos da
entidade pode ampliar o contrato da tela sem decisão explícita. Não há evidência
aqui de vazamento de senha ou de overposting causado por essas telas de leitura.

**Correção recomendada:** projetar somente os campos exibidos para ViewModels
de detalhe/linha no Controller, mantendo acesso direto ao contexto e MVC simples.
Não introduzir repositórios ou camadas adicionais.

**Confirmação futura:** renderizar listagens e detalhes com ViewModels sem
navegações de entidades, preservando campos, paginação e autorização atuais.

### MVC-02 — resumo fiscal reapresentado a partir de campos do POST — P2

**Arquivos e evidências:**
[Conferir.cshtml:45–53 e 92](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Views/EntradasFiscais/Conferir.cshtml#L45-L92),
[EntradasFiscaisController:189–239](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs#L189-L239) e
[307–316](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/EntradasFiscaisController.cs#L307-L316).

**Constatação:** quantidade, descrição e valor voltam em campos ocultos. Em erro,
`RetornarConferenciaAsync` recompõe o cabeçalho, mas reapresenta `model.Itens`.
O Razor calcula `Model.Itens.Sum(i => i.Quantidade)` para prometer o total da entrada.
A confirmação efetiva usa `entrada.Itens.Single(...)` e `item.Quantidade` do banco.

**Risco:** valores adulterados no POST podem reaparecer na conferência após erro,
divergindo da quantidade que realmente será adicionada. A soma é um resumo
operacional calculado no Razor sobre dados não reidratados; não é evidência de
que o navegador consiga escolher a quantidade persistida da NF-e.

**Correção recomendada:** reconstruir no servidor os campos somente de leitura
e um total explícito a partir da entrada persistida, preservando apenas os campos
editáveis permitidos. Tratar também entradas de ModelState dos campos recompostos
para que TagHelpers não reapresentem valores enviados anteriormente.

**Confirmação futura:** enviar quantidade oculta alterada com um campo obrigatório
inválido; a tela deve retornar o valor/total persistido e a confirmação deve
continuar usando exclusivamente a quantidade original. Cenário inferido por
inspeção estática, sem reprodução HTTP específica nesta auditoria.

### MVC-03 — classificação de disponibilidade repetida — P3

**Arquivos e evidências:**
[Consulta.cshtml:85–89 e 162–166](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Views/Estoque/Consulta.cshtml#L85-L166),
[consulta.js:168–180](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/wwwroot/js/consulta.js#L168-L180),
[ConsultaEstoqueViewModel:50](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Models/ViewModels/ConsultaEstoqueViewModel.cs#L50) e
[Produtos/Details.cshtml:70–81](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Views/Produtos/Details.cshtml#L70-L81).

**Constatação:** zero/um/mais pares são classificados no ViewModel, Razor e JS.
**Risco:** rótulos, grupos e cores podem divergir após mudança da convenção
“último par”. É duplicação de apresentação baseada em conceito do domínio,
não regra crítica indevidamente transferida ao cliente.

**Correção recomendada:** centralizar os valores de apresentação no ViewModel
e retorná-los na resposta da venda quando necessário. Manter os bloqueios
visuais auxiliares e a validação independente de saldo no servidor.

**Confirmação futura:** conferir saldos 0, 1 e 2 no carregamento e após venda;
texto, classe e acessibilidade devem permanecer equivalentes.

### MVC-04 — totais sem indicação do recorte de 200 atendimentos — P2

**Arquivos e evidências:**
[AtendimentosController:28–50](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Controllers/AtendimentosController.cs#L28-L50) e
[Atendimentos/Index.cshtml:10–22](https://github.com/CodeWitch00/ESTOQUE/blob/f87c14c9f42fe2e780ffd7b71810093cb0fbe7d0/src/SquadEstoque.Web/Views/Atendimentos/Index.cshtml#L10-L22).

**Constatação:** o servidor seleciona os 200 registros mais recentes antes de
contar pendentes, concluídos e alertas; a tela não informa esse recorte.
**Risco:** interpretar os indicadores como totais gerais e deixar pendências
antigas fora da análise. As regras de contagem/alerta estão corretamente no
Controller; a deficiência é o contrato de apresentação do alcance dos números.

**Correção recomendada:** explicitar o recorte no ViewModel e na tela ou calcular
indicadores globais no servidor, conforme decisão de produto.
**Confirmação futura:** com 201 registros e uma pendência antiga, verificar que
a interface comunica corretamente o conjunto representado pelos indicadores.

## Conferência dos contratos das telas

| Tela | Contrato observado | Resultado |
| --- | --- | --- |
| Account/Login | `LoginViewModel` | Adequado. |
| Estoque/Consulta | `ConsultaEstoqueViewModel` e projeções de produto/SKU/atendimento | Adequado, com duplicação visual MVC-03. |
| Produtos/Create, Edit, AdicionarNumeracoes | `ProdutoCreateViewModel`, `ProdutoEditViewModel`, `AdicionarNumeracoesViewModel` | ViewModels específicos de entrada; Controllers validam e persistem. |
| Produtos/Index e Details | `ProdutosIndexViewModel` com entidades; `Produto` diretamente | Parcial; MVC-01. |
| Movimentacoes/Entrada, Saida, Ajuste, EntradaLote | `MovimentacaoCreateViewModel`, `AjusteEstoqueViewModel`, `EntradaLoteViewModel` | ViewModels específicos de entrada. |
| Movimentacoes/Index | `MovimentacoesIndexViewModel` com `Movimentacao` | Parcial; MVC-01. |
| EntradasFiscais/Importar e Conferir | `ImportarEntradaFiscalViewModel`, `ConferirEntradaFiscalViewModel` | Contratos específicos; ressalva MVC-02 na reapresentação. |
| EntradasFiscais/Index | `IReadOnlyList<EntradaFiscal>` | Sem ViewModel próprio; MVC-01. |
| Rupturas/Index, SaldosZerados/Index, Atendimentos/Index | `RupturasIndexViewModel`, `SaldosZeradosViewModel`, `AtendimentosIndexViewModel` | Contratos próprios, agregações no servidor; ressalva MVC-04. |

## Diferenças relevantes já presentes em origin/main

Na base `f87c14c`, `SaldosZeradosController` filtra saldo zero sem restringir
produto/SKU ativo. A referência remota `9111713` já contém os dois filtros,
introduzidos em `d7912e4`; não abrir correção duplicada desse comportamento.
Também contém tratamento de indisponibilidade dos relatórios no Controller
e ajustes responsivos de Rupturas/SaldosZerados. Esses trechos foram inspecionados
por `git show origin/main:<arquivo>`, sem trocar ou alterar a branch main.

## Validação e revisão técnica

Inspeção cruzada de cada operação entre tela, Controller e persistência; busca
por `@model`, `@inject`, LINQ, saldo, quantidade, `disabled`, autorização e
antiforgery nas Views e no JavaScript próprio. Bibliotecas de terceiros não
foram auditadas internamente. Não foi encontrado acesso ao contexto ou gravação
de dados diretamente nas Views examinadas. Formatação de datas, paginação,
pluralização, rótulos de enum e exibição de menus foram tratados como apresentação.

Compilação local com .NET SDK 10.0.400: aprovada, sem avisos ou erros, usando
`dotnet build tests/SquadEstoque.Web.Tests/SquadEstoque.Web.Tests.csproj --no-restore`.
Suíte local `dotnet test --no-build`: 297 aprovados, 0 falhas e 1 ignorado (P95 opt-in).
A tentativa de executar `npm run test:frontend` não avançou porque `npm` não está
no PATH desta sessão; a verificação de frontend fica a cargo da CI do PR.
Resultado da CI deve ser consultado no PR; não foi criada suíte nova
para este cartão documental. Os testes existentes de autenticação, ruptura,
venda transacional, ajuste e relatórios dão suporte à verificação do servidor,
mas não provam todos os cenários de melhoria sugeridos neste documento.

O pedido de revisão no PR não representa revisão concluída nem aprovação.
Nenhum merge ou movimentação no Trello integra esta entrega.
