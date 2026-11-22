const { chromium } = require('playwright');
const assert = require('node:assert/strict');

const origin = process.env.SQUAD_AZURE_URL ||
    'https://asp-squad-estoque-free-hgabd8asd5f4a9g9.northcentralus-01.azurewebsites.net';
const email = 'lojista@squad.com';
const password = process.env.SQUAD_AZURE_LOJISTA_PASSWORD;
const execute = process.argv.includes('--execute');
const validateGrade = process.argv.includes('--validate-grade');
const writeApproval = process.env.SQUAD_AZURE_ALLOW_WRITES;

const products = [
    { name: 'DEMO-251126-01 Corrida', brand: 'Nike', color: 'Azul', sizes: { 37: 3, 38: 1, 39: 0, 40: 2 } },
    { name: 'DEMO-251126-02 Casual', brand: 'Nike', color: 'Branco', sizes: { 36: 0, 37: 2, 38: 1, 39: 4 } },
    { name: 'DEMO-251126-03 Urbano', brand: 'Nike', color: 'Preto', sizes: { 38: 2, 39: 0, 40: 1, 41: 3 } },
    { name: 'DEMO-251126-04 Cenários', brand: 'Nike', color: 'Cinza', sizes: { 37: 4, 38: 1, 39: 0, 40: 2 } }
];

function assertSafeTarget() {
    const url = new URL(origin);
    assert.equal(url.protocol, 'https:', 'O acesso ao Azure deve usar HTTPS.');
    assert.ok(url.hostname.endsWith('.azurewebsites.net'), 'SQUAD_AZURE_URL deve apontar ao App Service esperado.');
}

async function loginAsLojista(page) {
    const response = await page.goto(`${origin}/Account/Login`, { waitUntil: 'domcontentloaded' });
    assert.equal(response?.status(), 200, 'Não foi possível abrir o login.');
    assert.equal(new URL(page.url()).origin, new URL(origin).origin, 'Origem de login inesperada.');
    await page.getByLabel('E-mail').fill(email);
    await page.getByLabel('Senha', { exact: true }).fill(password);

    let postResponse;
    try {
        [postResponse] = await Promise.all([
            page.waitForResponse(r => {
                const requestUrl = new URL(r.url());
                return r.request().method() === 'POST' && requestUrl.origin === new URL(origin).origin &&
                    requestUrl.pathname === '/Account/Login';
            }, { timeout: 20000 }),
            page.getByRole('button', { name: 'Entrar' }).click()
        ]);
    } catch {
        const lastUrl = new URL(page.url());
        throw new Error(`Timeout aguardando resposta do POST de login; tela atual: ${lastUrl.pathname}.`);
    }

    const status = postResponse.status();
    const redirect = postResponse.headers().location;
    if (status >= 500) {
        throw new Error(`Erro da aplicação durante login (HTTP ${status}).`);
    }
    if (status >= 300 && status < 400) {
        const destination = redirect ? new URL(redirect, origin) : null;
        if (!destination || destination.origin !== new URL(origin).origin || !destination.pathname.startsWith('/Produtos')) {
            throw new Error(`Login redirecionou para destino inesperado (HTTP ${status}, ${destination?.pathname || 'sem destino'}).`);
        }
        await page.waitForURL(url => url.origin === destination.origin && url.pathname.startsWith('/Produtos'), { timeout: 15000 })
            .catch(() => { throw new Error(`Login respondeu com redirecionamento, mas a página de produtos não carregou (HTTP ${status}).`); });
    } else if (status === 200 && new URL(page.url()).pathname === '/Account/Login') {
        const messages = (await page.locator('.login-alert, [data-valmsg-summary="true"]').allTextContents())
            .map(message => message.trim()).filter(Boolean).join(' ');
        if (/inválid/i.test(messages)) {
            throw new Error('Autenticação recusada: a aplicação exibiu erro de credenciais. Verifique a variável de ambiente e a conta autorizada.');
        }
        throw new Error(`O POST de login retornou à tela de login sem autenticar (HTTP ${status}); validação: ${messages || 'sem mensagem visível'}.`);
    } else if (status < 200 || status >= 300) {
        throw new Error(`Resposta inesperada do login (HTTP ${status}, tela ${new URL(page.url()).pathname}).`);
    }

    assert.equal(await page.getByRole('heading', { name: 'Catálogo de Produtos' }).count(), 1,
        `Login concluído (HTTP ${status}), mas o catálogo do LOJISTA não foi renderizado.`);
}

async function searchProducts(page, searchTerm) {
    await page.goto(`${origin}/Produtos?busca=${encodeURIComponent(searchTerm)}`, { waitUntil: 'domcontentloaded' });
    assert.equal(new URL(page.url()).origin, new URL(origin).origin, 'A consulta saiu da origem autorizada.');
    assert.equal(await page.getByRole('heading', { name: 'Catálogo de Produtos' }).count(), 1,
        'A sessão LOJISTA foi perdida durante a consulta.');

    const matches = [];
    const seenPages = new Set();
    while (true) {
        const currentUrl = page.url();
        assert.ok(!seenPages.has(currentUrl), 'Loop inesperado na paginação do catálogo.');
        seenPages.add(currentUrl);
        const rows = await page.locator('.product-table tbody tr').evaluateAll(elements => elements.map(row => ({
            name: row.querySelector('.product-model span')?.textContent?.trim() || '',
            brand: row.querySelector('[data-label="Marca"]')?.textContent?.trim() || '',
            color: row.querySelector('[data-label="Cor"]')?.textContent?.trim() || '',
            status: row.querySelector('[data-label="Status"]')?.textContent?.trim() || '',
            details: row.querySelector('a[href*="/Produtos/Details/"]')?.getAttribute('href') || ''
        })));
        matches.push(...rows.filter(row => row.name.toLocaleLowerCase('pt-BR').includes(searchTerm.toLocaleLowerCase('pt-BR'))));

        const next = page.getByRole('link', { name: 'Próxima página' });
        if (await next.count() === 0 || (await next.getAttribute('class') || '').split(/\s+/).includes('disabled')) break;
        await Promise.all([page.waitForNavigation(), next.click()]);
    }
    return matches;
}

async function openCreateForm(page, product) {
    await page.goto(`${origin}/Produtos/Create`, { waitUntil: 'domcontentloaded' });
    assert.equal(await page.getByRole('heading', { name: 'Novo produto' }).count(), 1, 'Formulário de produto indisponível.');
    await page.getByLabel('Nome do Modelo').fill(product.name);
    await page.getByLabel('Marca', { exact: true }).fill(product.brand);
    await page.getByLabel('Categoria', { exact: true }).fill('Tênis');
    await page.getByLabel('Cor', { exact: true }).fill(product.color);
}

async function fillGrade(page, product, missingSizes, reportFields = false) {
    assert.equal(await page.getByRole('heading', { name: /^(Cadastrar grade|Adicionar numerações)$/ }).count(), 1,
        'A aplicação não abriu o formulário de grade esperado.');

    const available = await page.locator('input[name$=".Numeracao"]').evaluateAll(inputs =>
        inputs.map(input => ({ size: input.value, name: input.name })));
    const requiredSizes = missingSizes;
    for (const size of requiredSizes) {
        assert.ok(available.some(item => item.size === size),
            `A numeração ${size} não está disponível nas opções; automação interrompida.`);
    }

    for (const option of available) {
        const selected = requiredSizes.includes(option.size);
        assert.match(option.name, /^Opcoes\[\d+\]\.Numeracao$/,
            `Nome de campo inesperado para a numeração ${option.size}: ${option.name}.`);
        const checkboxName = option.name.replace(/\.Numeracao$/, '.Selecionada');
        const checkboxSelector = `input[type="checkbox"][name="${checkboxName}"]`;
        const checkbox = page.locator(checkboxSelector);
        assert.equal(await checkbox.count(), 1,
            `Esperado um checkbox para a numeração ${option.size}; seletor: ${checkboxSelector}.`);
        const quantityName = option.name.replace(/\.Numeracao$/, '.Quantidade');
        const quantitySelector = `input[name="${quantityName}"]`;
        const quantity = page.locator(quantitySelector);
        assert.equal(await quantity.count(), 1,
            `Esperado um campo de quantidade para a numeração ${option.size}; seletor: ${quantitySelector}.`);
        if (selected) await checkbox.check();
        await quantity.fill(String(selected ? product.sizes[option.size] : 0));
        if (reportFields && selected) {
            assert.equal(await checkbox.isChecked(), true, `Checkbox da numeração ${option.size} não foi marcado.`);
            assert.equal(await quantity.inputValue(), String(product.sizes[option.size]),
                `Quantidade da numeração ${option.size} não foi preenchida.`);
            console.log(`Numeração ${option.size}: ${checkboxName} e ${quantityName} encontrados; preenchimento OK (${product.sizes[option.size]}).`);
        }
    }
}

async function inspectProduct(page, product) {
    const matches = await searchProducts(page, product.name);
    const exact = matches.filter(item => item.name === product.name);
    assert.equal(matches.length, exact.length,
        `Encontrados nomes parciais para ${product.name}; estado ambíguo, interrompido antes de qualquer escrita.`);
    assert.ok(exact.length <= 1, `Produto duplicado ${product.name}; interrompido antes de qualquer escrita.`);
    if (exact.length === 0) return { state: 'absent', product, missingSizes: Object.keys(product.sizes) };

    const catalogItem = exact[0];
    assert.equal(catalogItem.brand, product.brand, `Marca divergente em ${product.name}; retomada interrompida.`);
    assert.equal(catalogItem.color, product.color, `Cor divergente em ${product.name}; retomada interrompida.`);
    assert.match(catalogItem.status, /Ativo/i, `Produto inativo ${product.name}; retomada interrompida.`);
    assert.ok(catalogItem.details, `Link de detalhes ausente para ${product.name}.`);
    await page.goto(new URL(catalogItem.details, origin).href, { waitUntil: 'domcontentloaded' });
    assert.equal(await page.getByRole('heading', { name: product.name, exact: true }).count(), 1,
        `Detalhes inesperados para ${product.name}.`);
    const detailMeta = (await page.locator('.product-details-meta').innerText()).split('·').map(part => part.trim());
    assert.equal(detailMeta[1], 'Tênis', `Categoria divergente em ${product.name}; retomada interrompida.`);
    const actual = await page.locator('.product-details-table tbody tr').evaluateAll(rows => rows.map(row => ({
        size: row.querySelector('[data-label="Numeração"]')?.textContent?.trim() || '',
        balance: Number((row.querySelector('[data-label="Saldo atual"]')?.textContent || '').match(/\d+/)?.[0] || 0)
    })));
    const expectedSizes = Object.keys(product.sizes);
    const actualSizes = actual.map(row => row.size);
    assert.ok(actualSizes.every(size => Object.hasOwn(product.sizes, size)),
        `A grade de ${product.name} contém numerações fora do plano; retomada interrompida.`);
    for (const row of actual) {
        assert.equal(row.balance, product.sizes[row.size],
            `Saldo divergente em ${product.name}, numeração ${row.size}; nenhuma escrita será feita.`);
    }
    const missingSizes = expectedSizes.filter(size => !actualSizes.includes(size));
    const addGrade = page.locator('a.product-details-grade-action');
    return {
        state: missingSizes.length === 0 ? 'complete' : 'partial',
        product,
        missingSizes,
        addGradeUrl: await addGrade.getAttribute('href')
    };
}

async function main() {
    assertSafeTarget();
    assert.ok(!(execute && validateGrade), 'Use --validate-grade ou --execute, nunca os dois juntos.');
    assert.ok(password, 'Defina SQUAD_AZURE_LOJISTA_PASSWORD no ambiente, sem incluir a senha no comando ou em arquivos.');
    if (execute) {
        assert.equal(writeApproval, 'BACKUP_VERIFICADO_E_EXECUCAO_AUTORIZADA',
            'Para escrita, defina SQUAD_AZURE_ALLOW_WRITES=BACKUP_VERIFICADO_E_EXECUCAO_AUTORIZADA após autorização explícita.');
    }

    const browser = await chromium.launch({ headless: true });
    const context = await browser.newContext({ acceptDownloads: false, serviceWorkers: 'block' });
    const page = await context.newPage();
    try {
        await loginAsLojista(page);
        if (validateGrade) {
            let blockedWrites = 0;
            await context.route('**/*', async route => {
                if (!['GET', 'HEAD'].includes(route.request().method())) {
                    blockedWrites++;
                    await route.abort('blockedbyclient');
                } else {
                    await route.continue();
                }
            });
            const product = products[0];
            const state = await inspectProduct(page, product);
            assert.equal(state.state, 'partial', `${product.name} precisa existir com grade parcial para esta validação.`);
            assert.ok(state.addGradeUrl, `Formulário de grade indisponível para ${product.name}.`);
            await page.goto(new URL(state.addGradeUrl, origin).href, { waitUntil: 'domcontentloaded' });
            await fillGrade(page, product, Object.keys(product.sizes), true);
            assert.equal(blockedWrites, 0, 'O navegador tentou enviar uma requisição de escrita; operação bloqueada.');
            console.log('Pré-validação concluída: formulário preenchido no navegador, sem envio ou salvamento.');
            return;
        }
        const states = [];
        for (const product of products) {
            states.push(await inspectProduct(page, product));
        }

        for (const state of states) {
            const { product } = state;
            const description = state.state === 'absent' ? 'ausente' :
                state.state === 'complete' ? 'completo' : `parcial; faltam ${state.missingSizes.join(', ')}`;
            console.log(`${execute ? 'PRÉ-CHECAGEM' : 'DRY-RUN'} ${product.name}: ${description}.`);
        }
        if (!execute) return;

        for (const state of states) {
            const { product } = state;
            const current = await inspectProduct(page, product);
            if (current.state === 'complete') continue;
            assert.equal(current.state, state.state,
                `O estado de ${product.name} mudou após a pré-checagem; execução interrompida antes da próxima escrita.`);
            assert.deepEqual(current.missingSizes, state.missingSizes,
                `A grade de ${product.name} mudou após a pré-checagem; execução interrompida.`);

            if (current.state === 'absent') {
                await openCreateForm(page, product);
                await page.getByRole('button', { name: 'Salvar e cadastrar grade' }).click();
                await page.waitForURL(url => url.origin === new URL(origin).origin &&
                    url.pathname.startsWith('/Produtos/AdicionarNumeracoes/'));
                current.addGradeUrl = page.url();
            } else {
                assert.ok(current.addGradeUrl, `Ação para completar grade ausente em ${product.name}; execução interrompida.`);
                await page.goto(new URL(current.addGradeUrl, origin).href, { waitUntil: 'domcontentloaded' });
            }

            await fillGrade(page, product, current.missingSizes);
            await page.getByRole('button', { name: 'Salvar grade' }).click();
            await page.waitForURL(url => url.origin === new URL(origin).origin && url.pathname.startsWith('/Produtos/Details/'));
            const verified = await inspectProduct(page, product);
            assert.equal(verified.state, 'complete', `A conferência pós-cadastro de ${product.name} não ficou completa.`);
            console.log(`${product.name}: grade completada e saldos conferidos.`);
        }
    } finally {
        await context.close();
        await browser.close();
    }
}

main().catch(error => {
    console.error(`Automação interrompida: ${error.message}`);
    process.exitCode = 1;
});
