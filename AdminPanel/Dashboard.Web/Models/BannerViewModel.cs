using System.ComponentModel.DataAnnotations;

namespace Dashboard.Web.Models
{
    public class BannerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Resim URL gereklidir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string ImageUrl { get; set; } = string.Empty;

        [Url(ErrorMessage = "Geçerli bir link giriniz")]
        public string? Link { get; set; }

        [Range(0, 1000, ErrorMessage = "Sıra 0-1000 arasında olmalıdır")]
        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public int CompanyId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
