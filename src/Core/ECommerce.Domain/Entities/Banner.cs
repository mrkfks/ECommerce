namespace ECommerce.Domain.Entities
{
    public class Banner
    {
        private Banner() { }

        public int Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string ImageUrl { get; private set; } = string.Empty;
        public string RedirectUrl { get; private set; } = string.Empty;

        public static Banner Create(string title, string imageUrl, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Banner başlığı boş olamaz.", nameof(title));
            
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Banner resim URL'si boş olamaz.", nameof(imageUrl));
            
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Banner yönlendirme URL'si boş olamaz.", nameof(redirectUrl));

            return new Banner
            {
                Title = title,
                ImageUrl = imageUrl,
                RedirectUrl = redirectUrl
            };
        }

        public void Update(string title, string imageUrl, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Banner başlığı boş olamaz.", nameof(title));
            
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Banner resim URL'si boş olamaz.", nameof(imageUrl));
            
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Banner yönlendirme URL'si boş olamaz.", nameof(redirectUrl));

            Title = title;
            ImageUrl = imageUrl;
            RedirectUrl = redirectUrl;
        }
    }
}