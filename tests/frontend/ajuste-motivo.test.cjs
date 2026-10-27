const { test, before, after } = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const fs = require('node:fs');
const { chromium } = require('playwright');

const web = path.resolve(__dirname, '../../src/SquadEstoque.Web');
let browser;
before(async () => { browser = await chromium.launch({ headless: true }); });
after(async () => { await browser?.close(); });

async function abrir(t) {
    const page = await browser.newPage();
    t.after(() => page.close());
    // O marcador e o script devem continuar conectados à view real.
    const view = fs.readFileSync(path.join(web, 'Views/Movimentacoes/Ajuste.cshtml'), 'utf8');
    assert.match(view, /<textarea asp-for="Motivo" data-normalizar-motivo/);
    assert.match(view, /<script src="~\/js\/ajuste.js"/);
    await page.setContent(`<form action="http://ajuste.test/Movimentacoes/Ajuste" method="post">
        <textarea name="Motivo" data-normalizar-motivo maxlength="500" data-val="true"
            data-val-required="O motivo do ajuste é obrigatório."
            data-val-minlength="O motivo deve conter pelo menos 5 caracteres." data-val-minlength-min="5"
            data-val-length="O motivo não pode exceder 500 caracteres." data-val-length-max="500"></textarea>
        <span data-valmsg-for="Motivo" data-valmsg-replace="true"></span>
        <button type="submit">Salvar ajuste</button></form>`);
    for (const script of ['lib/jquery/dist/jquery.min.js',
        'lib/jquery-validation/dist/jquery.validate.min.js',
        'lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js', 'js/ajuste.js']) {
        await page.addScriptTag({ path: path.join(web, 'wwwroot', script) });
    }
    await page.waitForFunction(() => !!window.jQuery('[name=Motivo]').rules().normalizer);
    // Intercepta somente o transporte; a validação real do formulário continua ativa.
    await page.evaluate(() => {
        window.envios = [];
        document.querySelector('form').addEventListener('submit', event => {
            if (event.defaultPrevented) return;
            window.envios.push(new FormData(event.target).get('Motivo'));
            event.preventDefault();
        });
    });
    return page;
}

for (const [nome, motivo, valido] of [
    ['500 caracteres com espaços nas extremidades', '  ' + 'a'.repeat(500) + '  ', true],
    ['500 caracteres normalizados com espaços internos', ' \t' + 'a'.repeat(498) + '  \r\n b\u0085', true],
    ['mínimo com espaços', '  abcde  ', true],
    ['501 caracteres após normalização', ' ' + 'a'.repeat(501) + ' ', false],
    ['menos de cinco após normalização', ' a   b ', false],
    ['somente espaços e Unicode', ' \t\r\n\u0085\u00a0\u3000 ', false],
    ['vazio', '', false]
]) {
    test(`formulário de ajuste: ${nome}`, async t => {
        const page = await abrir(t);
        const field = page.locator('[name=Motivo]');
        assert.equal(await field.getAttribute('maxlength'), null);
        await field.fill(motivo);
        assert.equal(await field.inputValue(), motivo.replace(/\r\n/g, '\n'));
        await page.getByRole('button', { name: 'Salvar ajuste' }).click();
        const envios = await page.evaluate(() => window.envios);
        assert.equal(envios.length, valido ? 1 : 0);
        const erro = await page.locator('[data-valmsg-for=Motivo]').innerText();
        if (valido) assert.equal(erro, '');
        else assert.match(erro, /obrigatório|pelo menos 5|500 caracteres/);
    });
}
