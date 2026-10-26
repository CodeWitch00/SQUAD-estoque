document.querySelectorAll('[data-preview-input]').forEach((input) => {
    const preview = document.getElementById(input.dataset.previewInput);
    if (!preview) return;

    let urlAnterior;
    input.addEventListener('change', () => {
        if (urlAnterior) URL.revokeObjectURL(urlAnterior);
        const arquivo = input.files?.[0];
        if (!arquivo) {
            preview.hidden = true;
            preview.removeAttribute('src');
            return;
        }
        urlAnterior = URL.createObjectURL(arquivo);
        preview.src = urlAnterior;
        preview.hidden = false;
    });
});
