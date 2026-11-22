const { test } = require('node:test');
const assert = require('node:assert/strict');
const { chromium } = require('playwright');

const origin = 'https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net';
const prefix = 'DEMO-251126';
const email = process.env.SQUAD_AZURE_LOJISTA_EMAIL || 'lojista@squad.com';
const password = process.env.SQUAD_AZURE_LOJISTA_PASSWORD;

test('Azure: LOJISTA consulta o catálogo e identifica modelos de demonstração', { timeout: 90000 }, async () => {
    assert.ok(password, 'Defina SQUAD_AZURE_LOJISTA_PASSWORD fora do repositório antes de executar.');

    const browser = await chromium.launch({ headless: true });
    try {
        const context = await browser.newContext({
            acceptDownloads: false,
            serviceWorkers: 'block'
        });
        const page = await context.newPage();

        const login = await page.goto(`${origin}/Account/Login`, { waitUntil: 'domcontentloaded' });
        assert.equal(login?.status(), 200, 'A página de login deve estar disponível.');
        assert.equal(new URL(page.url()).origin, origin, 'O formulário deve pertencer à aplicação esperada.');
        await page.getByLabel('E-mail').fill(email);
        await page.getByLabel('Senha', { exact: true }).fill(password);
        await Promise.all([
            page.waitForURL(url => url.origin === origin && url.pathname === '/Produtos'),
            page.getByRole('button', { name: 'Entrar' }).click()
        ]);

        assert.equal(new URL(page.url()).origin, origin);
        assert.equal(await page.getByRole('heading', { name: 'Catálogo de Produtos' }).count(), 1,
            'O login deve abrir o catálogo do LOJISTA.');

        await page.getByLabel('Buscar produto').fill(prefix);
        await Promise.all([
            page.waitForURL(url => url.origin === origin && url.pathname === '/Produtos' && url.searchParams.get('busca') === prefix),
            page.getByRole('button', { name: 'Aplicar filtros' }).click()
        ]);

        const encontrados = [];
        const paginas = new Set();
        while (true) {
            const url = page.url();
            assert.ok(!paginas.has(url), 'A paginação retornou a uma página já lida.');
            paginas.add(url);

            const linhas = await page.locator('.product-table tbody tr').evaluateAll(rows => rows.map(row => ({
                nome: row.querySelector('.product-model span')?.textContent?.trim() || '',
                marca: row.querySelector('[data-label="Marca"]')?.textContent?.trim() || '',
                cor: row.querySelector('[data-label="Cor"]')?.textContent?.trim() || '',
                status: row.querySelector('[data-label="Status"]')?.textContent?.trim() || '',
                detalhes: row.querySelector('a[href*="/Produtos/Details/"]')?.getAttribute('href') || ''
            })));
            encontrados.push(...linhas.filter(item => item.nome.toUpperCase().startsWith(prefix)));

            const proxima = page.getByRole('link', { name: 'Próxima página' });
            if (await proxima.count() === 0 || await proxima.getAttribute('aria-disabled') === 'true' ||
                (await proxima.getAttribute('class') || '').split(/\s+/).includes('disabled')) break;
            await proxima.click();
            await page.waitForLoadState('domcontentloaded');
        }

        console.log(`Páginas consultadas: ${paginas.size}; modelos com prefixo ${prefix}: ${encontrados.length}`);
        for (const item of encontrados) {
            console.log(JSON.stringify(item));
        }
        await context.close();
    } finally {
        await browser.close();
    }
});
