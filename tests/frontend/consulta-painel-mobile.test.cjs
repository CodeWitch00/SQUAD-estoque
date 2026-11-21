const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const web = path.resolve(__dirname, '../../src/SquadEstoque.Web');
const css = fs.readFileSync(path.join(web, 'wwwroot/css/site.css'), 'utf8');

test('painel do atendimento abre, fecha e reabre sem encobrir a grade permanentemente', async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        for (const [width, height] of [[320, 568], [360, 640], [360, 800], [390, 844], [430, 932], [1280, 900]]) {
            const page = await browser.newPage({ viewport: { width, height } });
            await page.setContent(`<style>${css}</style><div class="seller-layout"><main class="seller-main"><div class="consulta-vendedor">
              <section class="consulta-grade" data-atendimento-id=""><header class="consulta-grade-cabecalho"><h1>Modelo longo de calçado</h1></header>
                <button class="consulta-reabrir-acoes" hidden>Ver ações da numeração selecionada</button>
                <ul class="consulta-grade-lista"><li class="consulta-grade-item consulta-grade-item--indisponivel"><label class="consulta-grade-card"><input class="consulta-grade-selecao" type="radio" name="skuSelecionado" value="sku-37" data-numeracao="37" data-saldo="0"><span class="consulta-grade-card-conteudo"><span class="consulta-grade-numero">37</span><span class="consulta-grade-saldo">Sem estoque</span><span class="consulta-grade-estado">Indisponível</span></span></label></li>
                <li class="consulta-grade-item consulta-grade-item--indisponivel"><label class="consulta-grade-card"><input class="consulta-grade-selecao" type="radio" name="skuSelecionado" value="sku-38" data-numeracao="38" data-saldo="0"><span class="consulta-grade-card-conteudo"><span class="consulta-grade-numero">38</span><span class="consulta-grade-saldo">Sem estoque</span><span class="consulta-grade-estado">Indisponível</span></span></label></li></ul></section>
              <section class="consulta-atendimento-acoes"><button class="consulta-painel-fechar" type="button">×</button><p id="consulta-atendimento-resumo"><span id="consulta-atendimento-selecao"></span><span id="consulta-atendimento-saldo"></span></p><button class="consulta-acao" data-resultado="vendeu" disabled>Vendeu</button><button class="consulta-acao" data-resultado="nao-tinha" disabled>Não tinha</button><button class="consulta-acao" data-resultado="desistiu" disabled>Desistiu</button><p id="consulta-venda-retorno" hidden></p><p id="consulta-acao-feedback" hidden></p></section>
            </div></main></div>`);
            await page.addScriptTag({ path: path.join(web, 'wwwroot/js/consulta.js') });
            await page.locator('input[value="sku-37"]').check({ force: true });
            const panel = page.locator('.consulta-atendimento-acoes');
            assert.equal(await panel.isVisible(), true);
            if (width <= 575) {
                const rect = await panel.boundingBox();
                assert.ok(rect.height <= height * .62 + 2, `altura do painel em ${width}x${height}`);
                await page.locator('.consulta-painel-fechar').click();
                assert.equal(await panel.isVisible(), false);
                assert.equal(await page.locator('.consulta-reabrir-acoes').isVisible(), true);
                await page.locator('input[value="sku-38"]').check({ force: true });
                assert.equal(await panel.isVisible(), true);
                await page.locator('.consulta-painel-fechar').click();
                await page.locator('.consulta-reabrir-acoes').click();
                assert.equal(await panel.isVisible(), true);
            }
            assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
            await page.close();
        }
    } finally {
        await browser.close();
    }
});
