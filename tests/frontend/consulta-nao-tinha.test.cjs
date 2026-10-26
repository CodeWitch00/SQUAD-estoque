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
        <button class="consulta-acao" data-resultado="nao-tinha" data-nao-tinha-url="/Estoque/RegistrarNaoTinha" data-produto-id="produto-consultado" data-atendimento-id="atendimento-1" disabled>Não tinha</button>
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

test('busca atualiza resultados automaticamente durante a digitacao', async (t) => {
    const page = await browser.newPage();
    t.after(() => page.close());
    await page.setContent(`
        <div class="consulta-painel">
            <form action="http://consulta.test/Estoque/Consulta">
                <div><input class="consulta-campo" name="Termo" minlength="2"></div>
                <button type="submit" class="consulta-botao" aria-label="Buscar">
                    <svg aria-hidden="true"></svg><span class="visually-hidden">Buscar</span>
                </button>
            </form>
        </div>
        <div id="consulta-resultados-area">
            <p class="consulta-resultados-contagem">0 produtos encontrados</p>
        </div>`);
    await page.evaluate(() => {
        window.consultas = [];
        window.fetch = async url => {
            window.consultas.push(String(url));
            return {
                ok: true,
                text: async () => `<div id="consulta-resultados-area"><p class="consulta-resultados-contagem">1 produto encontrado</p><a class="consulta-resultado">Tênis Court Vision Low</a></div>`
            };
        };
    });
    await page.addScriptTag({ path: path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/js/consulta.js') });

    const campo = page.locator('input[name="Termo"]');
    await campo.pressSequentially('Tênis', { delay: 30 });
    await page.waitForFunction(() => document.querySelector('.consulta-resultado')?.textContent.includes('Tênis Court Vision Low'));

    assert.equal(await campo.inputValue(), 'Tênis');
    assert.equal(await campo.evaluate(element => element === document.activeElement), true);
    assert.equal(await page.locator('.consulta-resultados-contagem').innerText(), '1 produto encontrado');
    assert.equal(await page.getByRole('button', { name: 'Buscar' }).count(), 1);
    assert.match(await page.evaluate(() => window.consultas.at(-1)), /[?&]termo=T%C3%AAnis(?:&|$)/);
    assert.equal(await page.evaluate(() => window.consultas.length), 1);
});

test('selecao da numeracao e persistida antes de liberar as acoes', async (t) => {
    const page = await browser.newPage();
    t.after(() => page.close());
    await page.setContent(`
        <div id="consulta-antiforgery"><input name="__RequestVerificationToken" value="token-teste"></div>
        <section class="consulta-grade" data-atendimento-id="" data-selecionar-sku-url="/Estoque/SelecionarSku">
            <div class="consulta-grade-item"><input type="radio" name="skuSelecionado" value="sku-39" data-numeracao="39" data-saldo="3"></div>
        </section>
        <section class="consulta-atendimento-acoes">
            <p id="consulta-atendimento-resumo"><span id="consulta-atendimento-selecao"></span><span id="consulta-atendimento-saldo"></span></p>
            <button class="consulta-acao" data-resultado="vendeu" disabled>Vendeu</button>
            <button class="consulta-acao consulta-acao--desistiu" data-resultado="desistiu" data-desistiu-url="/Estoque/RegistrarDesistiu" hidden disabled>Desistiu</button>
            <p id="consulta-venda-retorno" hidden></p><p id="consulta-acao-feedback" hidden></p>
        </section>`);
    await page.evaluate(() => {
        window.pedidosSelecao = [];
        window.fetch = (url, options) => {
            window.pedidosSelecao.push({ url, fields: [...options.body.entries()] });
            return new Promise(resolve => window.confirmarSelecao = () => resolve({
                ok: true,
                headers: new Headers({ 'content-type': 'application/json' }),
                json: async () => ({ skuId: 'sku-39', saldoConsultado: 3, persistido: true, atendimentoId: 'atendimento-1' })
            }));
        };
    });
    await page.addScriptTag({ path: path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/js/consulta.js') });

    await page.locator('[value="sku-39"]').check();
    assert.equal(await page.getByRole('button', { name: 'Vendeu' }).isDisabled(), true);
    assert.deepEqual(await page.evaluate(() => window.pedidosSelecao[0]), {
        url: '/Estoque/SelecionarSku',
        fields: [['skuId', 'sku-39'], ['__RequestVerificationToken', 'token-teste']]
    });
    await page.evaluate(() => window.confirmarSelecao());
    await page.waitForFunction(() => !document.querySelector('[data-resultado="vendeu"]').disabled);
    assert.equal(await page.locator('[value="sku-39"]').isChecked(), true);
    assert.equal(await page.locator('.consulta-grade').getAttribute('data-atendimento-id'), 'atendimento-1');
    assert.equal(await page.getByRole('button', { name: 'Desistiu' }).isVisible(), true);
});

test('venda confirma o registro e retorna automaticamente para a busca', async (t) => {
    const page = await browser.newPage();
    t.after(() => page.close());
    let exibiuGrade = false;
    await page.route('http://consulta.test/**', route => {
        const body = !exibiuGrade
            ? `<div class="consulta-vendedor">
                <div id="consulta-antiforgery"><input name="__RequestVerificationToken" value="token"></div>
                <section class="consulta-grade" data-atendimento-id="atendimento-1">
                    <div class="consulta-grade-item"><input type="radio" name="skuSelecionado" value="sku-39" data-numeracao="39" data-saldo="2" checked></div>
                </section>
                <section class="consulta-atendimento-acoes">
                    <p id="consulta-atendimento-resumo"><span id="consulta-atendimento-selecao"></span><span id="consulta-atendimento-saldo"></span></p>
                    <button class="consulta-acao" data-resultado="vendeu" data-vender-url="/Estoque/Vender" data-produto-id="produto-1" data-atendimento-id="atendimento-1">Vendeu</button>
                    <p id="consulta-venda-retorno" tabindex="-1" hidden></p><p id="consulta-acao-feedback" hidden></p>
                </section>
              </div>`
            : '<div class="consulta-vendedor"><input class="consulta-campo" name="Termo" value=""></div>';
        exibiuGrade = true;
        return route.fulfill({ contentType: 'text/html', body });
    });
    await page.goto('http://consulta.test/Estoque/Consulta?atendimentoId=atendimento-1');
    await page.evaluate(() => {
        window.fetch = async () => ({
            ok: true,
            headers: new Headers({ 'content-type': 'application/json' }),
            json: async () => ({ skuId: 'sku-39', saldoAtual: 1 })
        });
    });
    await page.addScriptTag({ path: path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/js/consulta.js') });

    await page.getByRole('button', { name: 'Vendeu' }).click();
    await page.waitForFunction(() => !document.querySelector('#consulta-venda-retorno').hidden);
    assert.match(await page.locator('#consulta-venda-retorno').innerText(), /Voltando para a busca/);
    await page.waitForURL('http://consulta.test/Estoque/Consulta');
    assert.equal(await page.locator('[name="Termo"]').count(), 1);
});

for (const numero of ['37', '39']) {
    test(`Não tinha aceita nº ${numero}, envia SKU/produto/token e preserva saldo`, async (t) => {
        const page = await abrir(t);
        const botao = page.getByRole('button', { name: 'Não tinha', exact: true });
        assert.equal(await botao.isDisabled(), true);
        await page.locator(`input[data-numeracao="${numero}"]`).check();
        assert.equal(await botao.isEnabled(), true);
        await botao.click();
        assert.deepEqual(await page.evaluate(() => window.pedidos), [{
            url: '/Estoque/RegistrarNaoTinha', method: 'POST',
            fields: [['skuId', `sku-${numero}`], ['produtoId', 'produto-consultado'], ['atendimentoId', 'atendimento-1'], ['__RequestVerificationToken', 'token-teste']]
        }]);
        await page.evaluate(n => window.responder(true, { skuId: `sku-${n}` }), numero);
        await page.waitForFunction(() => !document.querySelector('#consulta-acao-feedback').hidden);
        assert.match(await page.locator('#consulta-acao-feedback').innerText(), new RegExp(`nº ${numero}`, 'i'));
        assert.equal(await page.locator('#consulta-acao-feedback img').count(), 0);
        assert.equal(await page.locator(`input[data-numeracao="${numero}"]`).isChecked(), true);
        assert.deepEqual(await page.locator('[data-saldo]').evaluateAll(inputs => inputs.map(i => i.dataset.saldo)), ['0', '3']);
    });
}

test('Não tinha confirma o registro e retorna automaticamente para a busca', async (t) => {
    const page = await browser.newPage();
    t.after(() => page.close());
    await page.route('http://consulta.test/**', route => route.fulfill({
        contentType: 'text/html',
        body: '<div class="consulta-vendedor"><input class="consulta-campo" name="Termo" value=""></div>'
    }));
    await page.goto('http://consulta.test/preparacao');
    await page.setContent(`<div class="consulta-vendedor">
        <div id="consulta-antiforgery"><input name="__RequestVerificationToken" value="token"></div>
        <section class="consulta-grade">
            <div class="consulta-grade-item"><input type="radio" name="skuSelecionado" value="sku-37" data-numeracao="37" data-saldo="0" checked></div>
        </section>
        <section class="consulta-atendimento-acoes">
            <p id="consulta-atendimento-resumo"></p>
            <button class="consulta-acao" data-resultado="nao-tinha" data-nao-tinha-url="/Estoque/RegistrarNaoTinha" data-produto-id="produto-1" data-atendimento-id="atendimento-1">Não tinha</button>
            <p id="consulta-venda-retorno" hidden></p><p id="consulta-acao-feedback" tabindex="-1" hidden></p>
        </section>
    </div>`);
    await page.evaluate(() => {
        window.fetch = async () => ({
            ok: true,
            headers: new Headers({ 'content-type': 'application/json' }),
            json: async () => ({ skuId: 'sku-37' })
        });
    });
    await page.addScriptTag({ path: path.resolve(__dirname, '../../src/SquadEstoque.Web/wwwroot/js/consulta.js') });

    const naoTinha = page.getByRole('button', { name: 'Não tinha', exact: true });
    assert.equal(await naoTinha.isEnabled(), true);
    await naoTinha.dispatchEvent('click');
    await page.waitForFunction(() => !document.querySelector('#consulta-acao-feedback').hidden);
    assert.match(await page.locator('#consulta-acao-feedback').innerText(), /Voltando para a busca/);
    assert.equal(await page.getByRole('link', { name: 'Nova consulta' }).count(), 0);
    await page.waitForURL('http://consulta.test/Estoque/Consulta');
    assert.equal(await page.locator('[name="Termo"]').count(), 1);
});

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
