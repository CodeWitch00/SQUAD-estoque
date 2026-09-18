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
})();
