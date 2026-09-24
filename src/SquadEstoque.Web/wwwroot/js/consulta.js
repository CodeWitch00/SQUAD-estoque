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

    let vendaPendente = false;
    let gradeDesatualizada = false;
    const selecionado = () => grade.querySelector('input[name="skuSelecionado"]:checked');
    const atualizarSelecao = () => {
        const radio = selecionado();
        grade.querySelectorAll('.consulta-grade-item').forEach((card) => {
            const input = card.querySelector('input');
            input.disabled = vendaPendente;
            const selecionadoEsteCard = input === radio;
            card.classList.toggle('consulta-grade-item--selecionado', selecionadoEsteCard);
        });
        botoes.forEach((botao) => {
            if (vendaPendente) {
                botao.disabled = true;
            } else if (botao.dataset.resultado === 'desistiu') {
                botao.disabled = false;
            } else if (botao.dataset.resultado === 'vendeu') {
                botao.disabled = gradeDesatualizada || !radio || Number(radio.dataset.saldo) <= 0;
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
        retorno.hidden = !gradeDesatualizada;
    };
    // O POST já devolve o saldo persistido. Não calcular saldo - 1 no navegador
    // nem depender de um segundo GET para refletir uma venda confirmada.
    const atualizarSkuVendido = (radio, saldo) => {
        const card = radio.closest('.consulta-grade-item');
        const estado = saldo > 1 ? 'Disponível' : saldo === 1 ? 'Último par' : 'Indisponível';
        const classe = saldo > 1 ? 'disponivel' : saldo === 1 ? 'ultimo-par' : 'indisponivel';
        const quantidade = `${saldo} ${saldo === 1 ? 'par' : 'pares'}`;
        radio.dataset.saldo = String(saldo);
        card.classList.remove('consulta-grade-item--disponivel', 'consulta-grade-item--ultimo-par', 'consulta-grade-item--indisponivel');
        card.classList.add(`consulta-grade-item--${classe}`);
        card.querySelector('.consulta-grade-saldo').textContent = quantidade;
        const status = card.querySelector('.consulta-grade-estado');
        const marcador = status.querySelector('.consulta-grade-marcador');
        status.replaceChildren(marcador, document.createTextNode(` ${estado}`));
        card.querySelector('.consulta-grade-card-conteudo')
            .setAttribute('aria-label', `Nº ${radio.dataset.numeracao}, ${quantidade}, ${estado}`);
    };
    grade.addEventListener('change', (event) => {
        if (event.target.matches('input[name="skuSelecionado"]')) atualizarSelecao();
    });

    const buscarGradeAtualizada = async (manterSelecao = false) => {
        const skuSelecionadoAntes = manterSelecao ? selecionado()?.value : null;
        const response = await fetch(window.location.href, { credentials: 'same-origin', cache: 'no-store' });
        if (!response.ok) throw new Error('Consulta indisponível');
        const page = new DOMParser().parseFromString(await response.text(), 'text/html');
        const listaAtualizada = page.querySelector('.consulta-grade-lista');
        const lista = grade.querySelector('.consulta-grade-lista');
        if (!listaAtualizada || !lista) throw new Error('Grade indisponível');
        lista.replaceWith(listaAtualizada);
        gradeDesatualizada = false;
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
        if (vendaPendente || botao.disabled) return;
        const tipo = botao.dataset.resultado;
        const radio = selecionado();
        if (tipo === 'vendeu') {
            if (!radio || Number(radio.dataset.saldo) <= 0 || botao.disabled) return;

            const skuId = radio.value;
            const numero = radio.dataset.numeracao;
            vendaPendente = true;
            grade.setAttribute('aria-busy', 'true');
            atualizarSelecao();
            const textoBotao = botao.textContent;
            botao.textContent = 'Registrando…';
            let sucesso = false;
            let mensagem;

            try {
                const { response, data } = await postResultado(botao.dataset.venderUrl, skuId);
                sucesso = response.ok && data?.skuId === skuId &&
                    Number.isInteger(data.saldoAtual) && data.saldoAtual >= 0;
                if (sucesso) atualizarSkuVendido(radio, data.saldoAtual);
                mensagem = sucesso
                    ? `Venda registrada. Nº ${numero} · saldo atualizado: ${data.saldoAtual} ${data.saldoAtual === 1 ? 'par' : 'pares'}.`
                    : response.status === 400 && data?.mensagem?.includes('Saldo insuficiente')
                        ? 'Venda não registrada. O saldo desta numeração foi atualizado. Confira a grade.'
                        : data?.mensagem || 'Não foi possível registrar a venda. Tente novamente.';
            } catch {
                mensagem = 'Não foi possível confirmar a venda. Confira o estoque e o histórico antes de tentar novamente.';
            }

            if (!sucesso) {
                try {
                    await buscarGradeAtualizada(true);
                } catch {
                    gradeDesatualizada = true;
                    mensagem += ' Atualize a consulta para conferir o saldo vigente.';
                }
            }

            vendaPendente = false;
            grade.removeAttribute('aria-busy');
            botao.textContent = textoBotao;
            atualizarSelecao();

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
            atualizarSelecao();
            feedback.hidden = false;
        }
        if (!sucesso) {
            atualizarSelecao();
            feedback.hidden = false;
        }
        feedback.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }));

    atualizarSelecao();
})();
