const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const web = path.resolve(__dirname, '../../src/SquadEstoque.Web');
const bootstrap = fs.readFileSync(path.join(web, 'wwwroot/lib/bootstrap/dist/css/bootstrap.min.css'), 'utf8');
const site = fs.readFileSync(path.join(web, 'wwwroot/css/site.css'), 'utf8');
const logo = fs.readFileSync(path.join(web, 'wwwroot/images/logo-squad-header.png')).toString('base64');

test('alertas de atendimentos e cabeçalho administrativo refluem sem cortar conteúdo', async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        for (const width of [195, 320, 360, 390, 430, 768, 1024, 1440]) {
            const page = await browser.newPage({ viewport: { width, height: 800 } });
            await page.setContent(`<style>${bootstrap}\n${site}</style>
                <div class="admin-layout"><div class="admin-shell"><div class="admin-column">
                    <header class="admin-mobile-header">
                        <button class="admin-menu-toggle"><span>☰</span><span>Menu</span></button>
                        <a class="admin-mobile-brand"><img src="data:image/png;base64,${logo}" alt="SQUAD"></a>
                    </header>
                    <main class="admin-main admin-content">
                        <div class="d-flex flex-wrap align-items-start justify-content-between gap-3 mb-4"><div>
                            <h1 class="mb-1">Atendimentos</h1>
                            <p class="text-secondary mb-0">Acompanhe pendências e resultados informados pelos vendedores.</p>
                        </div></div>
                        <div class="table-responsive admin-mobile-cards" tabindex="0" role="region" aria-label="Histórico auditável de atendimentos">
                            <table class="table align-middle admin-mobile-cards-table atendimentos-table">
                                <thead><tr><th>Início</th><th>Vendedor</th><th>Modelo</th><th>Numeração</th><th>Resultado</th><th>Saldo consultado</th><th>Alerta</th></tr></thead>
                                <tbody><tr><td data-label="Início">20/11/2026 12:30</td><td data-label="Vendedor">Vendedor Teste</td>
                                    <td data-label="Modelo">Modelo Auditoria E2E</td><td data-label="Numeração">38</td>
                                    <td data-label="Resultado">Vendeu</td><td data-label="Saldo consultado">2 par(es)</td>
                                    <td data-label="Alerta"><span class="badge text-bg-warning">Encerrado em menos de 5 segundos</span></td>
                                </tr></tbody>
                            </table>
                        </div>
                    </main>
                </div></div></div>`);

            const result = await page.evaluate(() => {
                const badge = document.querySelector('.atendimentos-table .badge');
                const cell = badge.closest('td');
                const badgeBox = badge.getBoundingClientRect();
                const cellBox = cell.getBoundingClientRect();
                return {
                    horizontalOverflow: document.documentElement.scrollWidth > innerWidth,
                    badgeClipped: badgeBox.left < cellBox.left - 1 || badgeBox.right > cellBox.right + 1,
                    badgeTextIntact: badge.textContent === 'Encerrado em menos de 5 segundos',
                    badgeWrap: getComputedStyle(badge).whiteSpace,
                    logoInside: document.querySelector('.admin-mobile-brand img').getBoundingClientRect().right <= innerWidth + 1
                };
            });

            assert.equal(result.horizontalOverflow, false, `rolagem horizontal em ${width}px`);
            assert.equal(result.badgeClipped, false, `alerta fora da célula em ${width}px`);
            assert.equal(result.badgeTextIntact, true);
            if (width < 768) assert.equal(result.badgeWrap, 'normal');
            else assert.equal(result.badgeWrap, 'nowrap');
            if (width <= 220) assert.equal(result.logoInside, true);
            await page.close();
        }
    } finally {
        await browser.close();
    }
});
