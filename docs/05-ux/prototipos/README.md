# Protótipos de interface

Esta pasta reúne as referências visuais e navegáveis das interfaces do MVP, separadas por perfil de usuário.

## Demonstração

- [Abrir fluxo pelo login compartilhado](login.html)
- [Abrir no GitHub Pages](https://codewitch00.github.io/SQUAD-estoque/docs/05-ux/prototipos/login.html)
- [Visualizar sem GitHub Pages](https://html-preview.github.io/?url=https://raw.githubusercontent.com/CodeWitch00/SQUAD-estoque/main/docs/05-ux/prototipos/login.html)

Para navegar pelo protótipo, use `vendedor@squad.com` ou `lojista@squad.com` com qualquer senha preenchida.

## Arquivos

- [Login compartilhado](login.html): autenticação e encaminhamento para o perfil identificado.
- [Protótipo do vendedor](vendedor/prototipo-vendedor.html): experiência mobile de busca, consulta da grade e registro do resultado do atendimento.
- [Protótipo do lojista](lojista/prototipo-lojista.html): experiência desktop/tablet para gestão de produtos, estoque e rupturas.

## Publicação no GitHub Pages

1. Envie estes arquivos para a branch `main` do repositório no GitHub.
2. No repositório, abra **Settings → Pages**.
3. Em **Build and deployment**, selecione **Deploy from a branch**.
4. Selecione a branch **main**, a pasta **/(root)** e clique em **Save**.
5. Aguarde a publicação e abra o link do GitHub Pages indicado acima.

Os caminhos entre as páginas são relativos. Por isso, o login continuará funcionando mesmo que o protótipo do vendedor seja atualizado, desde que seu nome e sua pasta permaneçam `vendedor/prototipo-vendedor.html`.

## Convenções

- Os protótipos devem representar a interface proposta para a implementação.
- Estados normal, vazio, carregando, sucesso e erro devem ser apresentados separadamente.
- Os dois perfis devem compartilhar identidade visual, componentes e padrões de interação.
- Requisitos e regras de negócio permanecem nos documentos oficiais; os protótipos não os substituem.
- Evidências de revisão e capturas ficam em [`../evidencias`](../evidencias/README.md).

## Fontes relacionadas

- [Especificação dos componentes mobile](../S1-UX-010-especificacao-componentes-mobile.md)
- [Inventário de telas e mapa de navegação](../inventario-telas-e-mapa-navegacao.md)
- [Mapa de navegação do MVP](../mapa-navegacao-mvp.svg)
- [Decisões de interface](../decisoes/decisoes-de-interface.md)
