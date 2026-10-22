(() => {
    const campoBusca = document.querySelector('.consulta-campo');
    const formularioBusca = campoBusca?.form;
    let resultadosArea = document.querySelector('#consulta-resultados-area');

    const configurarBuscaAutomatica = () => {
        if (!campoBusca || !formularioBusca || !resultadosArea) return;

        let temporizador;
        let sequenciaBusca = 0;
        let controlador;

        const mostrarMensagemBusca = (mensagem) => {
            resultadosArea.innerHTML = `<p class="consulta-sem-resultados" role="status">${mensagem}</p>`;
        };

        const buscarResultados = async () => {
            const termo = campoBusca.value.trim();
            if (termo.length < 2) return;

            const sequenciaAtual = ++sequenciaBusca;
            controlador?.abort();
            controlador = new AbortController();
            const url = new URL(formularioBusca.action || window.location.href, window.location.href);
            url.searchParams.delete('produtoId');
            url.searchParams.set('termo', termo);
            resultadosArea.setAttribute('aria-busy', 'true');

            try {
                const response = await fetch(url, {
                    credentials: 'same-origin',
                    cache: 'no-store',
                    signal: controlador.signal
                });
                if (!response.ok) throw new Error('Consulta indisponível');

                const pagina = new DOMParser().parseFromString(await response.text(), 'text/html');
                const novaArea = pagina.querySelector('#consulta-resultados-area');
                if (!novaArea || sequenciaAtual !== sequenciaBusca) return;

                resultadosArea.replaceWith(novaArea);
                resultadosArea = novaArea;
                if (window.location.origin !== 'null') {
                    history.replaceState(null, '', `${url.pathname}${url.search}`);
                }
            } catch (erro) {
                if (erro.name === 'AbortError' || sequenciaAtual !== sequenciaBusca) return;
                mostrarMensagemBusca('Não foi possível atualizar os resultados. Tente novamente.');
            } finally {
                if (sequenciaAtual === sequenciaBusca) resultadosArea.removeAttribute('aria-busy');
            }
        };

        campoBusca.addEventListener('input', () => {
            window.clearTimeout(temporizador);
            controlador?.abort();
            sequenciaBusca++;
            const termo = campoBusca.value.trim();

            if (termo.length === 0) {
                resultadosArea.replaceChildren();
                return;
            }
            if (termo.length < 2) {
                mostrarMensagemBusca('Digite pelo menos 2 caracteres para buscar.');
                return;
            }

            temporizador = window.setTimeout(buscarResultados, 250);
        });

        formularioBusca.addEventListener('submit', (event) => {
            if (campoBusca.value.trim().length < 2) return;
            event.preventDefault();
            window.clearTimeout(temporizador);
            buscarResultados();
        });
    };

    configurarBuscaAutomatica();

    const grade = document.querySelector('.consulta-grade');
    const retorno = document.querySelector('#consulta-venda-retorno');
    const resumo = document.querySelector('#consulta-atendimento-resumo');
    const selecaoResumo = document.querySelector('#consulta-atendimento-selecao') || resumo;
    const saldoResumo = document.querySelector('#consulta-atendimento-saldo');
    const feedback = document.querySelector('#consulta-acao-feedback');
    const painelAtendimento = document.querySelector('.consulta-atendimento-acoes');
    const fecharPainel = document.querySelector('.consulta-painel-fechar');
    const botoes = document.querySelectorAll('.consulta-acao[data-resultado]');
    let atendimentoAtualId = document.querySelector('[data-atendimento-id]')?.dataset.atendimentoId || '';
    if (grade && atendimentoAtualId && window.location.origin !== 'null' &&
        !new URL(window.location.href).searchParams.has('atendimentoId')) {
        const urlAtendimento = new URL('/Estoque/Consulta', window.location.origin);
        urlAtendimento.searchParams.set('atendimentoId', atendimentoAtualId);
        history.replaceState(null, '', `${urlAtendimento.pathname}${urlAtendimento.search}#grade`);
    }
    if (!grade || !retorno || !resumo || !feedback || botoes.length === 0) return;

    let registroPendente = false;
    let gradeDesatualizada = false;
    let painelDispensado = false;
    const atualizarEspacoPainel = () => {
        if (painelAtendimento) {
            document.body.style.setProperty('--consulta-painel-mobile-altura', `${painelAtendimento.offsetHeight}px`);
        }
    };
    if (painelAtendimento && 'ResizeObserver' in window) {
        new ResizeObserver(atualizarEspacoPainel).observe(painelAtendimento);
    }
    const selecionado = () => grade.querySelector('input[name="skuSelecionado"]:checked');
    const garantirSkuVisivel = (radio) => {
        if (!radio || !painelAtendimento || !window.matchMedia('(max-width: 575.98px)').matches) return;
        window.setTimeout(() => {
            const card = radio.closest('.consulta-grade-item');
            if (!card) return;
            const limitePainel = window.innerHeight - painelAtendimento.offsetHeight - 12;
            const cardRect = card.getBoundingClientRect();
            if (cardRect.bottom <= limitePainel) return;
            const movimentoReduzido = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            window.scrollBy({
                top: cardRect.bottom - limitePainel,
                behavior: movimentoReduzido ? 'auto' : 'smooth'
            });
        }, 210);
    };
    const atualizarSelecao = () => {
        const radio = selecionado();
        const temAtendimento = Boolean(grade.dataset.atendimentoId);
        grade.querySelectorAll('.consulta-grade-item').forEach((card) => {
            const input = card.querySelector('input');
            input.disabled = registroPendente;
            const selecionadoEsteCard = input === radio;
            card.classList.toggle('consulta-grade-item--selecionado', selecionadoEsteCard);
        });
        botoes.forEach((botao) => {
            if (botao.dataset.resultado === 'desistiu') {
                botao.hidden = !temAtendimento;
                botao.disabled = registroPendente || !temAtendimento;
                return;
            }
            if (registroPendente) {
                botao.disabled = true;
            } else if (botao.dataset.resultado === 'vendeu') {
                botao.disabled = gradeDesatualizada || !radio || Number(radio.dataset.saldo) <= 0;
            } else {
                botao.disabled = !radio;
            }
        });
        if (!radio) {
            selecaoResumo.textContent = 'Selecione a numeração solicitada pelo cliente.';
            if (saldoResumo) saldoResumo.textContent = '';
        } else {
            selecaoResumo.textContent = `Nº ${radio.dataset.numeracao} selecionado`;
            const saldo = Number(radio.dataset.saldo);
            if (saldoResumo) saldoResumo.textContent = `${saldo} ${saldo === 1 ? 'par disponível' : 'pares disponíveis'}`;
        }
        const painelVisivel = Boolean(radio) && !painelDispensado;
        painelAtendimento?.classList.toggle('consulta-atendimento-acoes--visivel', painelVisivel);
        document.body.classList.toggle('consulta-painel-mobile-aberto', painelVisivel);
        if (painelVisivel) window.requestAnimationFrame(atualizarEspacoPainel);
        feedback.hidden = true;
        retorno.hidden = !gradeDesatualizada;
    };
    // O POST já devolve o saldo persistido. Não calcular saldo - 1 no navegador
    // nem depender de um segundo GET para refletir uma venda confirmada.
    const atualizarSkuVendido = (radio, saldo) => {
        const card = radio.closest('.consulta-grade-item');
        const estado = saldo > 1 ? 'Disponível' : saldo === 1 ? 'Último par' : 'Indisponível';
        const classe = saldo > 1 ? 'disponivel' : saldo === 1 ? 'ultimo-par' : 'indisponivel';
        const quantidade = saldo <= 0 ? 'Sem estoque' : saldo === 1 ? 'Último par' : `${saldo} pares`;
        radio.dataset.saldo = String(saldo);
        card.classList.remove('consulta-grade-item--disponivel', 'consulta-grade-item--ultimo-par', 'consulta-grade-item--indisponivel');
        card.classList.add(`consulta-grade-item--${classe}`);
        card.querySelector('.consulta-grade-saldo').textContent = quantidade;
        const status = card.querySelector('.consulta-grade-estado');
        status.textContent = `Status: ${estado}`;
        card.querySelector('.consulta-grade-card-conteudo')
            .setAttribute('aria-label', `Nº ${radio.dataset.numeracao}, ${saldo} ${saldo === 1 ? 'par' : 'pares'}, ${estado}`);
    };
    let skuPersistido = selecionado()?.value || null;
    const persistirSkuSelecionado = async (radio) => {
        // Uma numeração zerada só é registrada quando o vendedor confirma "Não tinha".
        if (Number(radio.dataset.saldo) <= 0) return;
        const url = grade.dataset.selecionarSkuUrl;
        if (!url) return;

        let mensagemErro = null;
        let selecaoSalva = false;
        const atendimentoJaExistia = Boolean(atendimentoAtualId);
        registroPendente = true;
        atualizarSelecao();
        try {
            const token = document.querySelector('#consulta-antiforgery input[name="__RequestVerificationToken"]')?.value;
            const body = new FormData();
            if (atendimentoAtualId) body.append('atendimentoId', atendimentoAtualId);
            body.append('skuId', radio.value);
            body.append('__RequestVerificationToken', token || '');
            const response = await fetch(url, {
                method: 'POST', body, credentials: 'same-origin',
                headers: { Accept: 'application/json' }
            });
            const data = response.headers.get('content-type')?.includes('application/json')
                ? await response.json() : null;
            if (!response.ok || data?.skuId !== radio.value || data?.persistido !== true || !data?.atendimentoId) {
                throw new Error(data?.mensagem || 'Não foi possível salvar a numeração.');
            }
            atendimentoAtualId = data.atendimentoId;
            grade.dataset.atendimentoId = atendimentoAtualId;
            botoes.forEach((botao) => { botao.dataset.atendimentoId = atendimentoAtualId; });
            skuPersistido = radio.value;
            radio.dataset.saldo = String(data.saldoConsultado);
            selecaoSalva = true;
            if (window.location.origin !== 'null') {
                const urlAtendimento = new URL('/Estoque/Consulta', window.location.origin);
                urlAtendimento.searchParams.set('atendimentoId', atendimentoAtualId);
                history.replaceState(null, '', `${urlAtendimento.pathname}${urlAtendimento.search}#grade`);
            }
        } catch (erro) {
            radio.checked = false;
            if (skuPersistido) {
                const anterior = grade.querySelector(`input[name="skuSelecionado"][value="${skuPersistido}"]`);
                if (anterior) anterior.checked = true;
            }
            mensagemErro = erro.message || 'Não foi possível salvar a numeração. Tente novamente.';
        } finally {
            registroPendente = false;
            atualizarSelecao();
            feedback.classList.toggle('consulta-venda-retorno--erro', Boolean(mensagemErro));
            if (mensagemErro) {
                feedback.textContent = mensagemErro;
                feedback.hidden = false;
            } else if (selecaoSalva) {
                feedback.textContent = atendimentoJaExistia
                    ? `Atendimento atualizado para o nº ${radio.dataset.numeracao}.`
                    : `Atendimento salvo para o nº ${radio.dataset.numeracao}. Pode buscar o par.`;
                feedback.hidden = false;
            }
        }
    };
    grade.addEventListener('change', async (event) => {
        if (event.target.matches('input[name="skuSelecionado"]')) {
            painelDispensado = false;
            atualizarSelecao();
            garantirSkuVisivel(event.target);
            await persistirSkuSelecionado(event.target);
        }
    });

    fecharPainel?.addEventListener('click', () => {
        painelDispensado = true;
        painelAtendimento.classList.remove('consulta-atendimento-acoes--visivel');
        document.body.classList.remove('consulta-painel-mobile-aberto');
        document.body.style.removeProperty('--consulta-painel-mobile-altura');
        grade.querySelector('input[name="skuSelecionado"]:checked')?.focus({ preventScroll: true });
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

    const postResultado = async (url, skuId, produtoId, atendimentoId) => {
        const token = document.querySelector('#consulta-antiforgery input[name="__RequestVerificationToken"]')?.value;
        const body = new FormData();
        if (skuId !== undefined) body.append('skuId', skuId);
        if (produtoId !== undefined) body.append('produtoId', produtoId);
        if (atendimentoId !== undefined) body.append('atendimentoId', atendimentoId);
        body.append('__RequestVerificationToken', token || '');
        const response = await fetch(url, {
            method: 'POST', body, credentials: 'same-origin',
            headers: { Accept: 'application/json' }
        });
        const data = response.headers.get('content-type')?.includes('application/json') ? await response.json() : null;
        return { response, data };
    };

    const exibirConfirmacao = (elemento, titulo, complemento) => {
        const tituloElemento = document.createElement('strong');
        const complementoElemento = document.createElement('span');
        tituloElemento.textContent = titulo;
        complementoElemento.textContent = complemento;
        elemento.replaceChildren(tituloElemento, complementoElemento);
        elemento.hidden = false;
    };

    const concluirAtendimento = (feedbackAtivo) => {
        painelAtendimento?.classList.add('consulta-atendimento-acoes--concluido');
        retorno.hidden = feedbackAtivo !== retorno;
        feedback.hidden = feedbackAtivo !== feedback;
        botoes.forEach((item) => item.disabled = true);
        grade.querySelectorAll('input[name="skuSelecionado"]').forEach((item) => item.disabled = true);
        window.requestAnimationFrame(atualizarEspacoPainel);
    };

    const agendarRetornoParaBusca = () => {
        window.setTimeout(() => {
            window.location.assign('/Estoque/Consulta');
        }, 1500);
    };

    botoes.forEach((botao) => botao.addEventListener('click', async () => {
        if (registroPendente || botao.disabled) return;
        const tipo = botao.dataset.resultado;
        const radio = selecionado();
        if (tipo === 'vendeu') {
            if (!radio || Number(radio.dataset.saldo) <= 0 || botao.disabled) return;

            const skuId = radio.value;
            const numero = radio.dataset.numeracao;
            registroPendente = true;
            grade.setAttribute('aria-busy', 'true');
            atualizarSelecao();
            const textoBotao = botao.textContent;
            botao.textContent = 'Registrando…';
            let sucesso = false;
            let mensagem;
            let saldoAtual;

            try {
                const { response, data } = await postResultado(
                    botao.dataset.venderUrl, skuId, botao.dataset.produtoId, botao.dataset.atendimentoId);
                sucesso = response.ok && data?.skuId === skuId &&
                    Number.isInteger(data.saldoAtual) && data.saldoAtual >= 0;
                if (sucesso) {
                    saldoAtual = data.saldoAtual;
                    atualizarSkuVendido(radio, saldoAtual);
                }
                mensagem = response.status === 400 && data?.mensagem?.includes('Saldo insuficiente')
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

            registroPendente = false;
            grade.removeAttribute('aria-busy');
            botao.textContent = textoBotao;
            atualizarSelecao();

            retorno.classList.toggle('consulta-venda-retorno--erro', !sucesso);
            if (sucesso) {
                exibirConfirmacao(
                    retorno,
                    `Venda registrada para o nº ${numero}.`,
                    `Saldo atualizado: ${saldoAtual} pares. Voltando para a busca…`);
                concluirAtendimento(retorno);
                agendarRetornoParaBusca();
            } else {
                retorno.textContent = mensagem;
                retorno.hidden = false;
            }
            retorno.focus({ preventScroll: true });
            if (!window.matchMedia('(max-width: 575.98px)').matches) {
                retorno.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
            }
            return;
        }

        if (tipo !== 'desistiu' && !radio) return;

        const skuId = radio?.value;
        const numero = radio?.dataset.numeracao;
        botoes.forEach((item) => item.disabled = true);
        feedback.hidden = true;
        retorno.hidden = true;

        if (tipo === 'desistiu') {
            registroPendente = true;
            grade.setAttribute('aria-busy', 'true');
            atualizarSelecao();
            let data;
            let sucesso = false;
            try {
                const resultado = await postResultado(
                    botao.dataset.desistiuUrl, undefined, undefined, botao.dataset.atendimentoId);
                data = resultado.data;
                sucesso = resultado.response.ok && data?.resultado === 'desistiu' &&
                    data.atendimentoEncerrado === true && data.novaConsultaUrl === '/Estoque/Consulta';
            } catch {
                sucesso = false;
            } finally {
                registroPendente = false;
                grade.removeAttribute('aria-busy');
                atualizarSelecao();
            }
            if (!sucesso) {
                feedback.classList.add('consulta-venda-retorno--erro');
                feedback.textContent = 'Não foi possível encerrar o atendimento. Tente novamente.';
                feedback.hidden = false;
                feedback.focus({ preventScroll: true });
                return;
            }
            feedback.classList.remove('consulta-venda-retorno--erro');
            exibirConfirmacao(feedback, 'Atendimento encerrado.',
                'Nenhuma movimentação foi registrada.');
            concluirAtendimento(feedback);
            try {
                const response = await fetch(data.novaConsultaUrl, { credentials: 'same-origin', cache: 'no-store' });
                if (!response.ok || response.redirected) throw new Error('Consulta indisponível');
                const page = new DOMParser().parseFromString(await response.text(), 'text/html');
                const consulta = page.querySelector('.consulta-vendedor');
                const campo = consulta?.querySelector('input[name="Termo"]');
                if (!campo || campo.value || consulta.querySelector('.consulta-grade')) throw new Error('Consulta inválida');
                const aviso = feedback.cloneNode(true);
                aviso.id = 'consulta-desistiu-retorno';
                aviso.classList.add('mb-4');
                consulta.prepend(aviso);
                campo.setAttribute('aria-describedby', `${campo.getAttribute('aria-describedby') || ''} ${aviso.id}`.trim());
                document.querySelector('.consulta-vendedor').replaceWith(consulta);
                document.body.classList.remove('consulta-painel-mobile-aberto');
                document.body.style.removeProperty('--consulta-painel-mobile-altura');
                history.replaceState(null, '', data.novaConsultaUrl);
                window.scrollTo(0, 0);
                campo.focus({ preventScroll: true });
            } catch {
                // Encerramento confirmado: falha ao carregar a busca não deve repetir o POST.
                feedback.focus({ preventScroll: true });
            }
            return;
        }

        registroPendente = true;
        grade.setAttribute('aria-busy', 'true');
        atualizarSelecao();
        const textoBotao = botao.textContent;
        botao.textContent = 'Registrando…';
        let sucesso = false;
        let mensagem = '';
        try {
            const { response, data } = await postResultado(
                botao.dataset.naoTinhaUrl, skuId, botao.dataset.produtoId || '', botao.dataset.atendimentoId);
            sucesso = response.ok && data?.skuId === skuId;
            if (!sucesso) {
                mensagem = data?.mensagem || 'Não foi possível confirmar o registro. Confira as rupturas antes de tentar novamente.';
            }
        } catch {
            mensagem = 'Não foi possível confirmar o registro. Confira as rupturas antes de tentar novamente.';
        } finally {
            registroPendente = false;
            grade.removeAttribute('aria-busy');
            botao.textContent = textoBotao;
            atualizarSelecao();
        }

        feedback.classList.toggle('consulta-venda-retorno--erro', !sucesso);
        if (sucesso) {
            exibirConfirmacao(
                feedback,
                `Falta registrada para o nº ${numero}.`,
                'O saldo do sistema não foi alterado. Voltando para a busca…');
            concluirAtendimento(feedback);
            agendarRetornoParaBusca();
        } else {
            feedback.textContent = mensagem;
            feedback.hidden = false;
        }
        feedback.focus({ preventScroll: true });
        if (!window.matchMedia('(max-width: 575.98px)').matches) {
            feedback.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
    }));

    atualizarSelecao();
})();
