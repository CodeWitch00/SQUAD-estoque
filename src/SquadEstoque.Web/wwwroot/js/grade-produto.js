document.addEventListener('click', (event) => {
    const button = event.target.closest('.grade-quantidade-ajuste');
    if (!button) return;

    const input = button.closest('.input-group')?.querySelector('input[type="number"]');
    if (!input) return;

    const atual = Number.parseInt(input.value, 10) || 0;
    input.value = String(Math.max(0, atual + Number(button.dataset.delta)));
    input.dispatchEvent(new Event('change', { bubbles: true }));
});
