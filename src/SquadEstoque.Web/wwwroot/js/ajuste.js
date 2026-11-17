(function ($) {
    'use strict';

    $(function () {
        $('[data-normalizar-motivo]').each(function () {
            // O limite se aplica ao texto normalizado, não ao conteúdo bruto do textarea.
            this.removeAttribute('maxlength');
            $(this).rules('add', {
                normalizer: function (value) {
                    // Mesmos separadores de Char.IsWhiteSpace usados pelo String.Split no servidor.
                    return value.replace(/[\u0009-\u000d\u0020\u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+/g, ' ')
                        .replace(/^ | $/g, '');
                }
            });
        });
    });
})(jQuery);
