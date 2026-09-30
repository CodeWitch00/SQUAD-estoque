// Executar com Node.js e Playwright disponíveis: node --test tests/frontend/consulta-nao-tinha.test.cjs
const { test, before, after } = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const { chromium } = require('playwright');

let browser;
before(async () => {
    browser = await chromium.launch({
        headless: true,
        executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH || undefined
    });
});
after(async () => { await browser?.close(); });

async function abrir(t) {
    const page = await browser.newPage();
    t.after(() => page.close());
    await page.setContent(`
        <div class="consulta-painel"><form><input name="Termo"></form></div>
        <div id="consulta-antiforgery"><input name="__RequestVerificationToken" value="token-teste"></div>
        <section class="consulta-grade">
            <h2 id="titulo-grade">Tênis &lt;img src=x onerror=alert(1)&gt;</h2>
            <p id="consulta-venda-retorno" hidden></p>
            <div class="consulta-grade-item"><input type="radio" name="skuSelecionado" value="sku-37" data-numeracao="37" data-saldo="0"></div>
            <div class="consulta-grade-item"><input type="radio" name="skuSelecionado" value="sku-39" data-numeracao="39" data-saldo="3"></div>
        </section>
        <p id="consulta-atendimento-resumo"></p>
        <button class="consulta-acao" data-resultado="vendeu" disabled>Vendeu</button>
        <button class="consulta-acao" data-resultado="nao-tinha" data-nao-tinha-url="/Estoque/RegistrarNaoTinha" disabled>Não tinha</button>
        <p id="consulta-acao-feedback" tabindex="-1" hidden></p>
    `);
    await page.evaluate(() => {
        window.pedidos = [];
        window.fetch = (url, options) => {
            window.pedidos.push({ url, method: options.method, fields: [...options.body.entries()] });
            return new Promise((resolve, reject) => {
                window.responder = (ok, data) => resolve({ ok, headers: new Headers({ 'content-type': 'application/json' }), json: async () => data });
                window.falhar = () => reject(new Error('Conexão interrompida'));
            });
        };
    });
    await page.addScriptTag({ path: path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/js/consulta.js') });
    return page;
}

test('busca permite digitar o nome completo sem envio automatico', async (t) => {
    const page = await browser.newPage();
    t.after(() => page.close());
    await page.setContent('<div class="consulta-painel"><form><input name="Termo"></form></div>');
    await page.evaluate(() => {
        window.enviosAutomaticos = 0;
        HTMLFormElement.prototype.requestSubmit = () => window.enviosAutomaticos++;
    });
    await page.addScriptTag({ path: path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/js/consulta.js') });

    const campo = page.locator('input[name="Termo"]');
    await campo.pressSequentially('Tênis Court Vision Low', { delay: 100 });
    await page.waitForTimeout(1000);

    assert.equal(await campo.inputValue(), 'Tênis Court Vision Low');
    assert.equal(await campo.evaluate(element => element === document.activeElement), true);
    assert.equal(await page.evaluate(() => window.enviosAutomaticos), 0);
});

for (const numero of ['37', '39']) {
    test(`Não tinha aceita nº ${numero}, envia só SKU/token e preserva saldo`, async (t) => {
        const page = await abrir(t);
        const botao = page.getByRole('button', { name: 'Não tinha', exact: true });
        assert.equal(await botao.isDisabled(), true);
        await page.locator(`input[data-numeracao="${numero}"]`).check();
        assert.equal(await botao.isEnabled(), true);
        await botao.click();
        assert.deepEqual(await page.evaluate(() => window.pedidos), [{
            url: '/Estoque/RegistrarNaoTinha', method: 'POST',
            fields: [['skuId', `sku-${numero}`], ['__RequestVerificationToken', 'token-teste']]
        }]);
        await page.evaluate(n => window.responder(true, { skuId: `sku-${n}` }), numero);
        await page.waitForFunction(() => !document.querySelector('#consulta-acao-feedback').hidden);
        assert.match(await page.locator('#consulta-acao-feedback').innerText(), new RegExp(`Nº ${numero}`));
        assert.equal(await page.locator('#consulta-acao-feedback img').count(), 0);
        assert.equal(await page.locator(`input[data-numeracao="${numero}"]`).isChecked(), true);
        assert.deepEqual(await page.locator('[data-saldo]').evaluateAll(inputs => inputs.map(i => i.dataset.saldo)), ['0', '3']);
    });
}

test('envio pendente bloqueia seleção e reentrada mesmo após evento change', async (t) => {
    const page = await abrir(t);
    await page.locator('[data-numeracao="39"]').check();
    await page.getByRole('button', { name: 'Não tinha', exact: true }).click();
    await page.locator('[data-numeracao="37"]').dispatchEvent('change');
    await page.locator('[data-resultado="nao-tinha"]').dispatchEvent('click');
    assert.equal(await page.evaluate(() => window.pedidos.length), 1);
    assert.equal(await page.locator('input[name="skuSelecionado"]:disabled').count(), 2);
    assert.equal(await page.locator('.consulta-acao:disabled').count(), 2);
    assert.equal(await page.locator('.consulta-grade').getAttribute('aria-busy'), 'true');
    assert.equal(await page.getByRole('button', { name: 'Registrando…' }).count(), 1);
});

for (const erro of ['rejeicao', 'rede', 'sku-divergente']) {
    test(`erro ${erro} libera interface sem anunciar sucesso ou reenviar`, async (t) => {
        const page = await abrir(t);
        await page.locator('[data-numeracao="39"]').check();
        await page.getByRole('button', { name: 'Não tinha', exact: true }).click();
        await page.evaluate(tipo => {
            if (tipo === 'rede') window.falhar();
            else window.responder(tipo === 'sku-divergente', { skuId: 'outro-sku' });
        }, erro);
        await page.waitForFunction(() => !document.querySelector('#consulta-acao-feedback').hidden);
        assert.match(await page.locator('#consulta-acao-feedback').innerText(), /Não foi possível confirmar/);
        assert.equal(await page.getByRole('button', { name: 'Não tinha', exact: true }).isEnabled(), true);
        assert.equal(await page.locator('.consulta-grade').getAttribute('aria-busy'), null);
        assert.equal(await page.locator('input:disabled').count(), 0);
        assert.equal(await page.evaluate(() => window.pedidos.length), 1);
    });
}
