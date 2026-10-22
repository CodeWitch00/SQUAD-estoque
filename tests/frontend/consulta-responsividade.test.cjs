const { test, before, after } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

let browser;
const css = fs.readFileSync(
    path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/css/site.css'),
    'utf8');

before(async () => {
    browser = await chromium.launch({
        headless: true,
        executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH || undefined
    });
});

after(async () => { await browser?.close(); });

function markup() {
    const cards = ['37', '38', '39', '40'].map(numero => `
        <li class="consulta-grade-item consulta-grade-item--disponivel">
            <label class="consulta-grade-card">
                <input class="consulta-grade-selecao" type="radio" name="skuSelecionado">
                <span class="consulta-grade-card-conteudo">
                    <span class="consulta-grade-numero"><span class="consulta-grade-prefixo">Nº</span>${numero}</span>
                    <span class="consulta-grade-saldo">3 pares</span>
                    <strong class="consulta-grade-estado"><span class="consulta-grade-marcador"></span>Disponível</strong>
                </span>
            </label>
        </li>`).join('');

    return `
        <div class="seller-layout">
            <nav class="squad-navbar"><div class="squad-navbar-inner">
                <a class="squad-brand" href="#">SQUAD</a>
                <button class="squad-toggler" type="button">Menu</button>
                <div class="squad-session"><span class="squad-role">VENDEDOR</span><button class="squad-logout">Sair</button></div>
            </div></nav>
            <main class="seller-main">
                <div class="consulta-vendedor">
                    <section class="consulta-grade">
                        <header class="consulta-grade-cabecalho">
                            <div><h1>Modelo com nome suficientemente comprido para validar contenção</h1><p class="consulta-produto-meta">Marca · Categoria · Cor</p></div>
                            <span class="consulta-grade-contagem">4 numerações</span>
                        </header>
                        <ul class="consulta-grade-lista">${cards}</ul>
                    </section>
                    <section class="consulta-atendimento-acoes">
                        <p class="consulta-atendimento-resumo">Selecione a numeração solicitada pelo cliente.</p>
                        <div class="consulta-acoes-grade"><button class="consulta-acao">Vendeu</button><button class="consulta-acao">Não tinha</button><button class="consulta-acao consulta-acao--desistiu">Desistiu</button></div>
                        <a class="consulta-nova-consulta" href="#">Nova consulta</a>
                    </section>
                </div>
            </main>
        </div>`;
}

for (const [width, height] of [[360, 800], [390, 844], [430, 932], [1280, 800]]) {
    test(`seller layout remains usable at ${width}x${height}`, async (t) => {
        const page = await browser.newPage({ viewport: { width, height } });
        t.after(() => page.close());
        await page.setContent(`<style>${css}</style>${markup()}`);

        const metrics = await page.evaluate(() => ({
            scrollWidth: document.documentElement.scrollWidth,
            clientWidth: document.documentElement.clientWidth,
            cards: [...document.querySelectorAll('.consulta-grade-card')].map(element => element.getBoundingClientRect().toJSON()),
            actions: [...document.querySelectorAll('.consulta-acao, .consulta-nova-consulta')].map(element => element.getBoundingClientRect().toJSON())
        }));

        await page.locator('.consulta-acao').first().focus();
        const focusOutline = await page.locator('.consulta-acao').first().evaluate(element => getComputedStyle(element).outlineWidth);

        assert.ok(metrics.scrollWidth <= metrics.clientWidth, 'não deve haver rolagem horizontal');
        assert.ok(metrics.cards.every(rect => rect.width >= 44 && rect.height >= 44), 'cards devem ter alvo de toque adequado');
        assert.ok(metrics.actions.every(rect => rect.width >= 44 && rect.height >= 44), 'ações devem ter alvo de toque adequado');
        assert.ok(parseFloat(focusOutline) >= 2, 'ações devem manter foco visível');
    });
}
