export const UiHelpers = (function () {

    function showLoadingBtn(btnElement, text = 'Yükleniyor...') {
        const originalText = btnElement.innerHTML;
        btnElement.dataset.originalText = originalText;
        btnElement.disabled = true;
        btnElement.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${text}`;
    }

    function resetLoadingBtn(btnElement) {
        if (btnElement.dataset.originalText) {
            btnElement.innerHTML = btnElement.dataset.originalText;
            btnElement.disabled = false;
        }
    }

    function initValidation() {
        // Ensure validation is unobtrusive
        if ($.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse("form");
        }
    }

    function initTables() {
        // Check for empty tables and add zebra stripping / empty state
        document.querySelectorAll('table tbody').forEach(tbody => {
            if (tbody.children.length === 0 || (tbody.children.length === 1 && tbody.children[0].innerText.trim() === '')) {
                const colCount = tbody.closest('table').querySelector('thead tr').children.length;
                tbody.innerHTML = `
                    <tr>
                        <td colspan="${colCount}" class="text-center py-4 text-muted">
                            <i class="fa-solid fa-folder-open fa-2x mb-2"></i>
                            <p class="mb-0">Görüntülenecek kayıt bulunamadı.</p>
                        </td>
                    </tr>
                `;
            }
        });
    }

    function createConfirmModal(title, message, callback) {
        // Remove existing modal if any
        const existingModal = document.getElementById('uiConfirmModal');
        if (existingModal) existingModal.remove();

        const modalHtml = `
            <div class="modal fade" id="uiConfirmModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p>${message}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                            <button type="button" class="btn btn-danger" id="uiConfirmBtn">Onayla</button>
                        </div>
                    </div>
                </div>
            </div>`;

        document.body.insertAdjacentHTML('beforeend', modalHtml);
        const modalEl = document.getElementById('uiConfirmModal');
        const modal = new bootstrap.Modal(modalEl);

        document.getElementById('uiConfirmBtn').addEventListener('click', function () {
            modal.hide();
            if (callback) callback();
        });

        modal.show();
    }

    function initFormSubmitHandlers() {
        document.addEventListener('submit', function (e) {
            const form = e.target;
            // Ignore if validation failed (managed by jquery.validate.unobtrusive)
            if ($(form).valid && !$(form).valid()) return;

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                showLoadingBtn(submitBtn, 'Bekleyiniz...');
            }
        });
    }

    return {
        showLoadingBtn,
        resetLoadingBtn,
        initValidation,
        initTables,
        confirm: createConfirmModal,
        initFormHandlers: initFormSubmitHandlers
    };
})();

document.addEventListener('DOMContentLoaded', () => {
    UiHelpers.initValidation();
    UiHelpers.initTables();
    UiHelpers.initFormHandlers();
    window.UiHelpers = UiHelpers;
});
