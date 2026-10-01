// Browser integration for S2-QA-031. Requires a disposable local Development server.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const { execFileSync } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');

const base = process.env.TEST_BASE_URL || 'http://localhost:5281';
const database = process.env.TEST_DATABASE;
const python = process.env.TEST_PYTHON || 'python';
const skuId = '03100000-0000-0000-0000-000000000038';
if (!database) throw new Error('TEST_DATABASE deve apontar para um banco SQLite descartável.');
const snapshot = () => JSON.parse(execFileSync(python, [
    'tests/browser/assert-desistiu.py', database, skuId
], { encoding: 'utf8' }));

(async () => {
    const output = process.env.TEST_OUTPUT || 'tmp/s2-qa-031-evidencias';
    fs.mkdirSync(output, { recursive: true });
    const before = snapshot();
    assert.equal(before.balance, 2);
    assert.equal(before.movements, 0);
    assert.equal(before.ruptures, 0);

    const browser = await chromium.launch({ channel: process.env.TEST_BROWSER || 'chrome', headless: true });
    const page = await browser.newPage({ viewport: { width: 390, height: 844 } });
    try {
        await page.goto(`${base}/Account/Login`);
        await page.locator('#Email').fill('vendedor@squad.com');
        await page.locator('#Senha').fill('123');
        await Promise.all([
            page.waitForURL('**/Estoque/Consulta'),
            page.getByRole('button', { name: 'Entrar', exact: true }).click()
        ]);

        let postCount = 0;
        page.on('request', request => { if (request.method() === 'POST') postCount++; });
        const url = `${base}/Estoque/Consulta?termo=S2-QA-031&produtoId=03100000-0000-0000-0000-000000000001`;
        await page.goto(url);
        const field = page.locator('[data-numeracao="38"]');
        assert.equal(await field.getAttribute('data-saldo'), '2');
        const desistiu = page.getByRole('button', { name: 'Desistiu', exact: true });
        assert.equal(await desistiu.isEnabled(), true);
        await page.screenshot({ path: path.join(output, 'antes-desistiu.png'), fullPage: true });

        await desistiu.click();
        await page.waitForURL(`${base}/Estoque/Consulta`);
        assert.equal(await page.locator('#Termo').inputValue(), '');
        assert.equal(postCount, 0, 'Desistiu deve encerrar via GET e não enviar requisições POST.');

        await page.goto(url);
        const pageAfter = await page.content();
        assert.match(pageAfter, /data-numeracao="38"[^>]*data-saldo="2"/);
        assert.equal(postCount, 0);
        await page.screenshot({ path: path.join(output, 'depois-desistiu.png'), fullPage: true });

        const after = snapshot();
        assert.deepEqual(after, before, 'Saldo, movimentações e rupturas devem permanecer inalterados.');
        const result = {
            outcome: 'PASS',
            before,
            after,
            postCount,
            authenticatedProfile: 'VENDEDOR',
            newConsultationAvailable: true,
            screenshots: ['antes-desistiu.png', 'depois-desistiu.png']
        };
        fs.writeFileSync(path.join(output, 'resultado.json'), JSON.stringify(result, null, 2));
        console.log(JSON.stringify(result, null, 2));
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
