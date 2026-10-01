// Execute against a disposable local database. Requires Playwright in the test environment.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

(async () => {
    const base = process.env.TEST_BASE_URL || 'http://localhost:5277';
    const output = process.env.TEST_OUTPUT || 'tmp/ruptura-feedback';
    fs.mkdirSync(output, { recursive: true });
    const browser = await chromium.launch({ channel: process.env.TEST_BROWSER || 'chrome', headless: true });
    const page = await browser.newPage({ viewport: { width: 1280, height: 1050 } });
    const results = [];
    try {
        await page.goto(`${base}/Account/Login`);
        await page.locator('#Email').fill('vendedor@squad.com');
        await page.locator('#Senha').fill('123');
        await Promise.all([page.waitForURL('**/Estoque/Consulta'), page.getByRole('button', { name: 'Entrar', exact: true }).click()]);
        await page.goto(`${base}/Estoque/Consulta?termo=RegistroFE027&produtoId=02700000-0000-0000-0000-000000000001`);
        const radio = page.locator('[data-numeracao="38"]');
        const naoTinha = page.getByRole('button', { name: 'Não tinha', exact: true });
        const feedback = page.locator('#consulta-acao-feedback');
        let posts = 0;
        page.on('request', request => {
            if (request.method() === 'POST' && request.url().endsWith('/RegistrarNaoTinha')) posts++;
        });
        await radio.focus();
        await page.keyboard.press('Space');
        await naoTinha.focus();
        await page.keyboard.press('Enter');
        await feedback.filter({ hasText: 'Ruptura registrada para a numeração 38.' }).waitFor();
        assert.equal(await radio.isChecked(), true);
        assert.equal(await radio.getAttribute('data-saldo'), '0');
        assert.equal(await page.getByRole('button', { name: 'Vendeu', exact: true }).isDisabled(), true);
        assert.equal(await feedback.evaluate(el => el === document.activeElement), true);
        assert.equal(await feedback.locator('img').count(), 0);
        assert.ok((await feedback.textContent()).includes('<img src=x onerror=alert(1)>'));
        assert.equal(posts, 1);
        const gradeBox = await page.locator('#grade').boundingBox();
        const actionsBox = await page.locator('.consulta-atendimento-acoes').boundingBox();
        assert.ok(actionsBox.y >= gradeBox.y + gradeBox.height);
        await page.screenshot({ path: path.join(output, 'sucesso-desktop.png'), fullPage: true });
        await page.locator('#grade').screenshot({ path: path.join(output, 'grade-desktop.png') });
        await page.locator('.consulta-atendimento-acoes').screenshot({ path: path.join(output, 'acoes-desktop.png') });
        results.push('Sucesso por teclado: confirmação, SKU e saldo preservados, foco no retorno e texto seguro.');

        await page.reload();
        assert.equal(await feedback.isHidden(), true);
        assert.equal(posts, 1);
        results.push('Recarregamento: nenhum novo POST ou reapresentação da mensagem de sucesso.');

        await radio.focus();
        await page.keyboard.press('Space');
        await page.route('**/RegistrarNaoTinha', route => route.fulfill({ status: 400, contentType: 'application/json', body: JSON.stringify({ mensagem: 'A numeração selecionada não está disponível para registro.' }) }));
        await naoTinha.click();
        await feedback.filter({ hasText: 'não está disponível' }).waitFor();
        assert.equal(await radio.isChecked(), true);
        assert.equal(await naoTinha.isEnabled(), true);
        assert.equal(await page.getByRole('button', { name: 'Vendeu', exact: true }).isDisabled(), true);
        await page.screenshot({ path: path.join(output, 'erro-desktop.png'), fullPage: true });
        results.push('HTTP 400 simulado: mensagem visível, produto e numeração preservados, tentativa liberada.');
        await page.unroute('**/RegistrarNaoTinha');

        await page.route('**/RegistrarNaoTinha', route => route.abort('failed'));
        await naoTinha.click();
        await feedback.filter({ hasText: 'Verifique a conexão' }).waitFor();
        assert.equal(await radio.isChecked(), true);
        assert.equal(await naoTinha.isEnabled(), true);
        results.push('Falha de rede simulada: contexto preservado e controles restaurados.');
        await page.unroute('**/RegistrarNaoTinha');

        let release;
        const gate = new Promise(resolve => release = resolve);
        await page.route('**/RegistrarNaoTinha', async route => { await gate; await route.continue(); });
        const before = posts;
        await naoTinha.click();
        assert.equal(await radio.isDisabled(), true);
        assert.equal(await naoTinha.isDisabled(), true);
        await naoTinha.dispatchEvent('click');
        await radio.dispatchEvent('change');
        assert.equal(await naoTinha.isDisabled(), true);
        release();
        await feedback.filter({ hasText: 'Ruptura registrada para a numeração 38.' }).waitFor();
        assert.equal(posts, before + 1);
        await page.unroute('**/RegistrarNaoTinha');
        results.push('Envio pendente: clique repetido e mudança de seleção não provocam POST duplicado.');

        await page.setViewportSize({ width: 390, height: 844 });
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
        const mobileGrade = await page.locator('#grade').boundingBox();
        const mobileActions = await page.locator('.consulta-atendimento-acoes').boundingBox();
        assert.ok(mobileActions.y >= mobileGrade.y + mobileGrade.height);
        await page.screenshot({ path: path.join(output, 'sucesso-mobile.png'), fullPage: true });
        await page.locator('#grade').screenshot({ path: path.join(output, 'grade-mobile.png') });
        await page.locator('.consulta-atendimento-acoes').screenshot({ path: path.join(output, 'acoes-mobile.png') });
        results.push('Mobile 390 px: sem rolagem horizontal, grade e nova consulta disponíveis.');
        await page.locator('[data-numeracao="39"]').focus();
        await page.keyboard.press('Space');
        assert.equal(await feedback.isHidden(), true);
        assert.equal(await page.getByRole('button', { name: 'Vendeu', exact: true }).isEnabled(), true);
        results.push('Continuidade: outra numeração selecionável e venda habilitada conforme saldo.');
        await page.getByRole('link', { name: 'Nova consulta sem registrar resultado' }).click();
        await page.waitForURL(`${base}/Estoque/Consulta`);
        assert.equal(await page.locator('#Termo').inputValue(), '');
        results.push('Nova consulta: retorna à busca vazia sem registrar resultado adicional.');
        fs.writeFileSync(path.join(output, 'resultados.json'), JSON.stringify(results, null, 2));
        console.log(JSON.stringify(results, null, 2));
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
