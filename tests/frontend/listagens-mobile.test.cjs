const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const web = path.resolve(__dirname, '../../src/SquadEstoque.Web');
const css = fs.readFileSync(path.join(web, 'wwwroot/css/site.css'), 'utf8');
const ruptureCss = fs.readFileSync(path.join(web, 'Views/Rupturas/Index.cshtml.css'), 'utf8');
const bootstrap = fs.readFileSync(path.join(web, 'wwwroot/lib/bootstrap/dist/css/bootstrap.min.css'), 'utf8');

test('listagens e filtros cabem em smartphones sem perder conteúdo', async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        for (const width of [320, 360, 390, 430, 768, 1024, 1280, 1440]) {
            const page = await browser.newPage({ viewport: { width, height: 800 } });
            await page.setContent(`<style>${bootstrap}\n${css}\n${ruptureCss}</style><main class="admin-content">
              <form class="product-filter-surface"><div class="product-filter-search"><input class="form-control" value="Modelo longo"></div><details class="product-filter-more" open data-has-filters="false"><summary class="product-filter-more-toggle">Filtros</summary><div class="product-filter-more-fields"><div class="product-filter-field"><select id="status"><option selected>Ativos</option></select></div><div class="product-filter-field"><select><option>Marca</option></select></div><div class="product-filter-field"><select><option>Categoria</option></select></div></div></details><div class="product-filter-actions"><button class="btn btn-primary">Aplicar filtros</button></div></form>
              <div class="admin-mobile-cards"><table class="table product-table admin-mobile-cards-table"><tbody><tr><td class="product-model" data-label="Modelo"><div>Modelo de tênis com nome longo que quebra linha</div></td><td data-label="Marca">SQUAD</td><td data-label="Categoria">Calçado</td><td data-label="Cor">Azul marinho</td><td data-label="Grade"><span class="badge product-size-chip">37</span><span class="badge product-size-chip">38</span><span class="badge product-size-chip">39</span></td><td data-label="Status"><span class="badge product-status">Ativo</span></td><td data-label="Ações"><div class="admin-row-actions product-row-actions"><a class="btn btn-sm" href="#detalhes">Detalhes</a><a class="btn btn-sm" href="#editar">Editar</a><button class="btn btn-sm">Desativar</button></div></td></tr></tbody></table></div>
              <div class="admin-mobile-cards"><table class="table movement-table admin-mobile-cards-table"><tbody><tr><td class="movement-date" data-label="Data e hora">20/11/2026 10:30</td><td class="movement-product" data-label="Produto">Modelo de tênis com nome longo que quebra linha</td><td data-label="Numeração">38</td><td data-label="Tipo"><span class="badge movement-type">ENTRADA</span></td><td class="movement-quantity" data-label="Quantidade">2</td><td class="movement-responsible" data-label="Responsável">Pessoa responsável com nome longo</td><td class="movement-reason" data-label="Motivo / Observação">Observação longa de conferência de estoque</td></tr></tbody></table></div>
              <div class="rupture-table-shell"><table class="table rupture-table"><tbody><tr><th scope="row" data-label="Modelo"><span class="d-block">Modelo de tênis com nome longo que quebra linha</span><span class="d-block">SQUAD · Azul</span></th><td data-label="Numeração">38</td><td data-label="Ocorrências"><strong>4</strong></td><td data-label="Última ruptura (UTC)">20/11/2026 10:30</td></tr></tbody></table></div>
            </main>`);
            if (width < 768) {
                await page.locator('.product-filter-more').evaluate(el => { el.open = false; });
                assert.equal(await page.locator('#status').isVisible(), false);
                await page.locator('.product-filter-more-toggle').click();
                assert.equal(await page.locator('#status').isVisible(), true);
            } else {
                assert.equal(await page.locator('#status').isVisible(), true);
                const columns = await page.locator('.product-filter-field').evaluateAll(fields => fields.map(field => field.getBoundingClientRect().top));
                assert.ok(Math.max(...columns) - Math.min(...columns) < 2, `filtros alinhados em ${width}px`);
            }
            if (width < 768) {
                assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true, `sem rolagem horizontal em ${width}px`);
            }
            for (const text of ['Modelo de tênis com nome longo', 'Detalhes', 'Editar', 'Desativar', 'Observação longa']) {
                assert.equal(await page.getByText(text, { exact: false }).count() > 0, true, `${text} presente em ${width}px`);
            }
            assert.equal(await page.locator('.rupture-table td[data-label="Ocorrências"]').innerText(), '4');
            await page.close();
        }
    } finally {
        await browser.close();
    }
});
