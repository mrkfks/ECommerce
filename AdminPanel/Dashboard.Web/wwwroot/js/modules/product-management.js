export const ProductManagement = (function () {

    function init() {
        initBulkSelect();
        initBulkUpdate();
    }

    function initBulkSelect() {
        const selectAll = document.getElementById('selectAll');
        const ChecksSelector = '.product-check';

        if (!selectAll) return;

        selectAll.addEventListener('change', function () {
            const checks = document.querySelectorAll(ChecksSelector);
            checks.forEach(c => c.checked = this.checked);
            updateBulkBtn();
        });

        document.querySelectorAll(ChecksSelector).forEach(c => {
            c.addEventListener('change', updateBulkBtn);
        });
    }

    function updateBulkBtn() {
        const count = document.querySelectorAll('.product-check:checked').length;
        const btnBulk = document.getElementById('btnBulkUpdate');
        const selectedCountSpan = document.getElementById('selectedCount');

        if (selectedCountSpan) selectedCountSpan.textContent = count;
        if (btnBulk) btnBulk.style.display = count > 0 ? 'block' : 'none';
    }

    function initBulkUpdate() {
        const confirmBtn = document.getElementById('confirmBulkUpdate');
        if (!confirmBtn) return;

        confirmBtn.addEventListener('click', function () {
            const ids = Array.from(document.querySelectorAll('.product-check:checked')).map(c => parseInt(c.value));
            const percentage = parseFloat(document.getElementById('pricePercentage').value);

            if (ids.length === 0) return;

            // Use UiHelpers Loading State
            UiHelpers.showLoadingBtn(confirmBtn);

            fetch('/Product/BulkUpdate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ productIds: ids, priceIncreasePercentage: percentage })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        location.reload();
                    } else {
                        UiHelpers.confirm('Hata', 'İşlem başarısız: ' + data.message, null);
                        UiHelpers.resetLoadingBtn(confirmBtn);
                    }
                })
                .catch(err => {
                    console.error(err);
                    UiHelpers.confirm('Hata', 'Bir hata oluştu.', null);
                    UiHelpers.resetLoadingBtn(confirmBtn);
                });
        });
    }

    return {
        init: init
    };
})();

// Auto-init for Product Pages
if (window.location.pathname.toLowerCase().includes('/product')) {
    document.addEventListener('DOMContentLoaded', () => {
        ProductManagement.init();
    });
}
