const { test, before, after } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const web = path.resolve(__dirname, '../../src/SquadEstoque.Web');
const bootstrapCss = fs.readFileSync(path.join(web, 'wwwroot/lib/bootstrap/dist/css/bootstrap.min.css'), 'utf8');
const squadCss = fs.readFileSync(path.join(web, 'wwwroot/css/site.css'), 'utf8');
let browser;

before(async () => {
    browser = await chromium.launch({
        headless: true,
        executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH || undefined
    });
});

after(async () => { await browser?.close(); });

function layoutMarkup() {
    return `
        <div class="admin-layout">
            <div class="admin-shell">
                <aside class="offcanvas offcanvas-lg offcanvas-start squad-sidebar" tabindex="-1" id="squad-admin-navigation"
                       aria-label="Navegação do lojista" data-bs-backdrop="true">
                    <div class="offcanvas-header squad-sidebar-header">
                        <a class="squad-sidebar-brand" href="#"><img src="/logo.png" alt="SQUAD Estoque"></a>
                        <button type="button" class="btn-close btn-close-white d-lg-none" data-bs-dismiss="offcanvas" aria-label="Fechar navegação"></button>
                    </div>
                    <div class="offcanvas-body squad-sidebar-body">
                        <nav class="squad-admin-nav" aria-label="Navegação administrativa">
                            <ul class="nav flex-column">
                                <li class="nav-item"><a class="nav-link active" href="/Produtos" aria-current="page">Produtos</a></li>
                                <li class="nav-item"><a class="nav-link" href="/Movimentacoes">Movimentações</a></li>
                                <li class="nav-item"><a class="nav-link" href="/SaldosZerados">Saldos zerados</a></li>
                                <li class="nav-item"><a class="nav-link" href="/Rupturas">Histórico de rupturas</a></li>
                                <li class="nav-item"><a class="nav-link" href="/Atendimentos">Atendimentos</a></li>
                            </ul>
                        </nav>
                        <div class="squad-sidebar-session"><span class="squad-role">LOJISTA</span><button class="squad-sidebar-logout">Sair da conta</button></div>
                    </div>
                </aside>
                <div class="admin-column">
                    <header class="admin-mobile-header">
                        <button class="admin-menu-toggle" type="button" data-bs-toggle="offcanvas" data-bs-target="#squad-admin-navigation"
                                aria-controls="squad-admin-navigation" aria-label="Abrir menu administrativo"><span>☰</span><span>Menu</span></button>
                        <a class="admin-mobile-brand" href="#"><img src="/logo.png" alt="SQUAD"></a>
                    </header>
                    <main class="admin-main admin-content">
                        <h1>Catálogo de Produtos</h1>
                        <div class="table-responsive admin-mobile-cards">
                            <table class="table admin-mobile-cards-table"><thead><tr><th>Modelo</th><th>Status</th></tr></thead>
                                <tbody><tr><td data-label="Modelo">Tênis teste</td><td data-label="Status">Ativo</td></tr></tbody>
                            </table>
                        </div>
                    </main>
                </div>
            </div>
        </div>`;
}

async function abrir(t, width, height = 900) {
    const page = await browser.newPage({ viewport: { width, height } });
    t.after(() => page.close());
    await page.setContent(`<base href="http://localhost/"><style>${bootstrapCss}\n${squadCss}</style>${layoutMarkup()}`);
    await page.addScriptTag({ path: path.join(web, 'wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js') });
    return page;
}

for (const width of [320, 360, 390, 430, 768, 1024, 1440]) {
    test(`menu administrativo e conteúdo cabem em ${width}px`, async t => {
        const page = await abrir(t, width);
        const metrics = await page.evaluate(() => ({
            scrollWidth: document.documentElement.scrollWidth,
            clientWidth: document.documentElement.clientWidth,
            sidebarPosition: getComputedStyle(document.querySelector('.squad-sidebar')).position,
            sidebarWidth: document.querySelector('.squad-sidebar').getBoundingClientRect().width,
            tableWidth: document.querySelector('.admin-mobile-cards-table').getBoundingClientRect().width,
            mainWidth: document.querySelector('.admin-main').getBoundingClientRect().width,
            menuVisible: getComputedStyle(document.querySelector('.admin-mobile-header')).display !== 'none'
        }));

        assert.ok(metrics.scrollWidth <= metrics.clientWidth, 'não deve haver rolagem horizontal na página');
        assert.ok(metrics.tableWidth <= metrics.mainWidth, 'a tabela adaptada deve caber na área de conteúdo');
        if (width < 992) {
            assert.equal(metrics.sidebarPosition, 'fixed', 'o menu deve usar Offcanvas abaixo de 992px');
            assert.ok(metrics.menuVisible, 'o botão para abrir o menu deve aparecer');
        } else {
            assert.equal(metrics.sidebarPosition, 'sticky', 'o menu deve permanecer lateral no desktop');
            assert.ok(!metrics.menuVisible, 'o botão mobile não deve aparecer no desktop');
            assert.ok(metrics.sidebarWidth >= 184 && metrics.sidebarWidth <= 220, 'largura lateral dentro da faixa definida');
        }
    });
}

test('Offcanvas abre por teclado e fecha pelo botão', async t => {
    const page = await abrir(t, 768);
    const abrirMenu = page.getByRole('button', { name: 'Abrir menu administrativo' });
    await abrirMenu.focus();
    assert.ok(parseFloat(await abrirMenu.evaluate(element => getComputedStyle(element).outlineWidth)) >= 2);
    await abrirMenu.press('Enter');
    await page.locator('#squad-admin-navigation.show').waitFor();
    await page.getByRole('button', { name: 'Fechar navegação' }).click();
    await page.locator('#squad-admin-navigation').waitFor({ state: 'hidden' });
});

for (const [width, height] of [[320, 568], [360, 640], [360, 800], [390, 844], [640, 360]]) {
    test(`rodapé acessível no menu em ${width}x${height}`, async t => {
        const page = await abrir(t, width, height);
        const menu = page.locator('#squad-admin-navigation');
        await page.getByRole('button', { name: 'Abrir menu administrativo' }).click();
        await page.locator('#squad-admin-navigation.show').waitFor();
        await menu.locator('.squad-sidebar-logout').scrollIntoViewIfNeeded();
        const metrics = await page.evaluate(() => {
            const panel = document.querySelector('#squad-admin-navigation').getBoundingClientRect();
            const button = document.querySelector('.squad-sidebar-logout').getBoundingClientRect();
            return { panelBottom: panel.bottom, buttonBottom: button.bottom, buttonTop: button.top, innerHeight };
        });
        assert.ok(metrics.buttonTop >= 0 && metrics.buttonBottom <= metrics.innerHeight, 'sair deve estar integralmente visível após rolagem');
        assert.ok(metrics.panelBottom <= metrics.innerHeight + 1, 'painel não deve ultrapassar a viewport');
        await page.getByRole('button', { name: 'Fechar navegação' }).click();
        await menu.waitFor({ state: 'hidden' });
        await page.getByRole('button', { name: 'Abrir menu administrativo' }).click();
        await page.locator('.offcanvas-backdrop').click({ position: { x: Math.max(1, width - 5), y: 10 } });
        await menu.waitFor({ state: 'hidden' });
    });
}
