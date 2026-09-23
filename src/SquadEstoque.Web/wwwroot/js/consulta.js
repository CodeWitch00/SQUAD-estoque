(() => {
    const form = document.querySelector('.consulta-painel form');
    const field = form?.querySelector('input[name="Termo"]');
    if (!form || !field) return;

    let timer;
    let composing = false;

    const scheduleSearch = () => {
        clearTimeout(timer);
        const term = field.value.trim();
        if (composing || term.length < 2 || term.length > 100) return;

        timer = setTimeout(() => form.requestSubmit(), 800);
    };

    field.addEventListener('input', scheduleSearch);
    field.addEventListener('compositionstart', () => {
        composing = true;
        clearTimeout(timer);
    });
    field.addEventListener('compositionend', () => {
        composing = false;
        scheduleSearch();
    });
    form.addEventListener('submit', () => clearTimeout(timer));

    const grade = document.querySelector('.consulta-grade');
    const retorno = document.querySelector('#consulta-venda-retorno');
    const resumo = document.querySelector('#consulta-atendimento-resumo');
    const feedback = document.querySelector('#consulta-acao-feedback');
    const botoes = document.querySelectorAll('.consulta-acao[data-resultado]');
    if (!grade || !retorno || !resumo || !feedback || botoes.length === 0) return;

    const selecionado = () => grade.querySelector('input[name="skuSelecionado"]:checked');
    const atualizarSelecao = () => {
        const radio = selecionado();
        grade.querySelectorAll('.consulta-grade-item').forEach((card) => {
            const selecionadoEsteCard = card.querySelector('input') === radio;
            card.classList.toggle('consulta-grade-item--selecionado', selecionadoEsteCard);
        });
        botoes.forEach((botao) => {
            if (botao.dataset.resultado === 'desistiu') {
                botao.disabled = false;
            } else if (botao.dataset.resultado === 'vendeu') {
                botao.disabled = !radio || Number(radio.dataset.saldo) <= 0;
            } else {
                botao.disabled = !radio;
            }
        });
        if (!radio) {
            resumo.textContent = 'Selecione a numeração solicitada pelo cliente.';
        } else if (Number(radio.dataset.saldo) === 0) {
            resumo.textContent = `Nº ${radio.dataset.numeracao} · Indisponível`;
        } else if (Number(radio.dataset.saldo) === 1) {
            resumo.textContent = `Nº ${radio.dataset.numeracao} · Último par`;
        } else {
            resumo.textContent = `Nº ${radio.dataset.numeracao} · ${radio.dataset.saldo} pares disponíveis`;
        }
        feedback.hidden = true;
        retorno.hidden = true;
    };
    grade.addEventListener('change', (event) => {
        if (event.target.matches('input[name="skuSelecionado"]')) atualizarSelecao();
    });

    const buscarGradeAtualizada = async (manterSelecao = false) => {
        const skuSelecionadoAntes = manterSelecao ? selecionado()?.value : null;
        const response = await fetch(window.location.href, { credentials: 'same-origin' });
        if (!response.ok) throw new Error('Consulta indisponível');
        const page = new DOMParser().parseFromString(await response.text(), 'text/html');
        const listaAtualizada = page.querySelector('.consulta-grade-lista');
        const lista = grade.querySelector('.consulta-grade-lista');
        if (!listaAtualizada || !lista) throw new Error('Grade indisponível');
        lista.replaceWith(listaAtualizada);
        if (skuSelecionadoAntes) {
            const atual = grade.querySelector(`input[name="skuSelecionado"][value="${skuSelecionadoAntes}"]`);
            if (atual) atual.checked = true;
        }
        atualizarSelecao();
    };

    const postResultado = async (url, skuId) => {
        const token = document.querySelector('#consulta-antiforgery input[name="__RequestVerificationToken"]')?.value;
        const body = new FormData();
        body.append('skuId', skuId);
        body.append('__RequestVerificationToken', token || '');
        const response = await fetch(url, {
            method: 'POST', body, credentials: 'same-origin',
            headers: { Accept: 'application/json' }
        });
        const data = response.headers.get('content-type')?.includes('application/json') ? await response.json() : null;
        return { response, data };
    };

    botoes.forEach((botao) => botao.addEventListener('click', async () => {
        const tipo = botao.dataset.resultado;
        const radio = selecionado();
        if (tipo === 'vendeu') {
            if (!radio || Number(radio.dataset.saldo) <= 0 || botao.disabled) return;

            const skuId = radio.value;
            const numero = radio.dataset.numeracao;
            botao.disabled = true;
            retorno.hidden = true;
            feedback.hidden = true;
            let sucesso = false;
            let mensagem;

            try {
                const { response, data } = await postResultado(botao.dataset.venderUrl, skuId);
                sucesso = response.ok && data?.skuId === skuId;
                mensagem = sucesso
                    ? `Venda registrada. Nº ${numero} · saldo atualizado: ${data.saldoAtual} ${data.saldoAtual === 1 ? 'par' : 'pares'}.`
                    : response.status === 400 && data?.mensagem?.includes('Saldo insuficiente')
                        ? 'Venda não registrada. O saldo desta numeração foi atualizado. Confira a grade.'
                        : data?.mensagem || 'Não foi possível registrar a venda. Tente novamente.';
            } catch {
                mensagem = 'Não foi possível registrar a venda. Verifique a conexão e tente novamente.';
            }

            try {
                await buscarGradeAtualizada(true);
                if (!sucesso && Number(selecionado()?.dataset.saldo ?? -1) === 0) {
                    mensagem = 'Venda não registrada. O saldo desta numeração foi atualizado. Confira a grade.';
                }
            } catch {
                mensagem += ' Atualize a consulta para conferir o saldo vigente.';
            }

            retorno.classList.toggle('consulta-venda-retorno--erro', !sucesso);
            retorno.textContent = mensagem;
            retorno.hidden = false;
            retorno.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
            return;
        }

        if (tipo !== 'desistiu' && !radio) return;

        const produto = grade.querySelector('#titulo-grade').textContent.trim();
        const numero = radio?.dataset.numeracao;
        botoes.forEach((item) => item.disabled = true);
        feedback.hidden = true;
        retorno.hidden = true;

        if (tipo === 'desistiu') {
            window.location.assign('/Estoque/Consulta');
            return;
        }

        let sucesso = false;
        let mensagem = '';
        try {
            const action = '/Estoque/RegistrarNaoTinha';
            const { response, data } = await postResultado(action, radio.value);
            sucesso = response.ok && data?.skuId === radio.value;
            if (sucesso) {
                mensagem = `Ruptura registrada\n${produto}\nNº ${numero}\nO saldo não foi alterado.`;
            } else {
                mensagem = data?.mensagem || 'Não foi possível registrar o resultado. Tente novamente.';
            }
        } catch {
            mensagem = 'Não foi possível registrar o resultado. Verifique a conexão e tente novamente.';
        }

        feedback.classList.toggle('consulta-venda-retorno--erro', !sucesso);
        feedback.textContent = mensagem;
        feedback.hidden = false;
        if (sucesso) {
            feedback.classList.remove('consulta-venda-retorno--erro');
            feedback.innerHTML = mensagem.split('\n').map((linha, indice) =>
                indice === 0 ? `<strong>${linha}</strong>` : `<span>${linha}</span>`
            ).join('<br>');
            if (tipo === 'nao-tinha') {
                radio.checked = false;
                atualizarSelecao();
                feedback.hidden = false;
            }
            botoes.forEach((item) => item.disabled = item.dataset.resultado !== 'desistiu' && !selecionado());
        }
        if (!sucesso) botoes.forEach((item) => {
            item.disabled = item.dataset.resultado !== 'desistiu' && !selecionado();
        });
        feedback.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }));

    atualizarSelecao();
})();
