// Genel yardımcı fonksiyonlar
const SiteHelpers = {
    // Alert gösterme
    showAlert(message, type = 'info') {
        const alertDiv = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fa-solid fa-circle-info me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        document.querySelector('.container:first-of-type')?.insertAdjacentHTML('afterbegin', alertDiv);
        
        setTimeout(() => {
            document.querySelector('.alert')?.remove();
        }, 5000);
    },

    // Onay dialogu
    confirmDelete(message = 'Bu kaydı silmek istediğinizden emin misiniz?') {
        return confirm(message);
    },

    // Fiyat formatla
    formatPrice(price) {
        return new Intl.NumberFormat('tr-TR', { 
            style: 'currency', 
            currency: 'TRY' 
        }).format(price);
    },

    // Tarih formatla
    formatDate(dateString) {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('tr-TR').format(date);
    }
};

// Global kullanım
window.SiteHelpers = SiteHelpers;
