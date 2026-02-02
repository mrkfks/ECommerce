import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageUrlService {
  private readonly defaultImage = 'assets/images/no-image.svg';

  /**
   * API'den gelen resim URL'lerini normalize eder
   * Mutlak URL'leri göreceli URL'lere dönüştürür
   */
  normalize(url: string | null | undefined): string {
    if (!url) return this.defaultImage;
    
    // Zaten göreceli URL ise veya assets ile başlıyorsa aynen döndür
    if (url.startsWith('/') || url.startsWith('assets/')) {
      return url;
    }
    
    // Mutlak URL ise (http:// veya https://) göreceli hale getir
    if (url.startsWith('http://') || url.startsWith('https://')) {
      try {
        const urlObj = new URL(url);
        // /uploads ile başlayan yolu döndür
        if (urlObj.pathname.startsWith('/uploads')) {
          return urlObj.pathname;
        }
        return url; // Harici URL ise aynen döndür (örn: unsplash)
      } catch {
        return url;
      }
    }
    
    return url;
  }

  /**
   * Resim listesindeki tüm URL'leri normalize eder
   */
  normalizeImages(images: any[]): any[] {
    if (!images || !Array.isArray(images)) return [];
    
    return images.map(img => ({
      ...img,
      imageUrl: this.normalize(img.imageUrl)
    }));
  }
}
