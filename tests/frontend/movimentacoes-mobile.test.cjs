const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const web = path.resolve(__dirname, '../../src/SquadEstoque.Web');
const css = fs.readFileSync(path.join(web, 'wwwroot/css/site.css'), 'utf8');
const bootstrap = fs.readFileSync(path.join(web, 'wwwroot/lib/bootstrap/dist/css/bootstrap.min.css'), 'utf8');

test('filtros de movimentações refluem e preservam valores no mobile e desktop', async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        for (const width of [320, 360, 390, 430, 768, 1440]) {
            const page = await browser.newPage({ viewport: { width, height: 800 } });
            await page.setContent(`<style>${bootstrap}\n${css}</style>
                <main class="admin-content">
                  <div class="admin-page-heading movement-page-heading mb-4"><div><h1>Histórico de Movimentações</h1><p class="admin-page-intro">Acompanhe entradas, saídas e ajustes registrados no estoque.</p></div>
                    <div class="admin-page-actions"><a class="btn btn-primary" href="/Movimentacoes/Entrada">Nova entrada</a><a class="btn btn-outline-primary" href="/Movimentacoes/Saida">Nova saída</a><a class="btn btn-outline-primary" href="/Movimentacoes/Ajuste">Ajuste manual</a></div></div>
                  <details class="movement-filters" open data-has-filters="false"><summary class="movement-filters-toggle">Filtros</summary><form class="movement-filter-surface"><div class="movement-filter-search"><label for="busca">Buscar produto</label><input id="busca" value="Tênis"></div><div class="movement-filter-field"><label for="tipo">Tipo</label><select id="tipo"><option>Todos</option><option selected>Saída</option></select></div></form></details>
                  <div class="movement-list-summary">Mostrando 1–20 de 30 movimentações</div>
                </main>`);
            await page.evaluate(() => {
                const filtros = document.querySelector('.movement-filters');
                filtros.open = !matchMedia('(max-width: 767.98px)').matches || filtros.dataset.hasFilters === 'true';
            });
            const filters = page.locator('.movement-filter-surface');
            assert.equal(await filters.isVisible(), width >= 768, `filtros iniciais em ${width}px`);
            if (width < 768) {
                await page.locator('.movement-filters-toggle').click();
                assert.equal(await filters.isVisible(), true);
                assert.equal(await page.locator('#busca').inputValue(), 'Tênis');
                assert.equal(await page.locator('#tipo').inputValue(), 'Saída');
                await page.locator('.movement-filters-toggle').click();
                assert.equal(await filters.isVisible(), false);
            }
            assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true, `sem overflow em ${width}px`);
            await page.close();
        }
    } finally {
        await browser.close();
    }
});
