import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageUrlService {
  private readonly defaultImage = 'assets/images/no-image.svg';
  private readonly apiBaseUrl = 'http://localhost:5010'; // API base URL

  /**
   * API'den gelen resim URL'lerini normalize eder
   * Mutlak URL'leri göreceli URL'lere dönüştürür
   */
  normalize(url: string | null | undefined): string {
    if (!url || url.trim() === '') return this.defaultImage;
    
    const trimmedUrl = url.trim();
    
    // Zaten göreceli URL ise veya assets ile başlıyorsa aynen döndür
    if (trimmedUrl.startsWith('/') || trimmedUrl.startsWith('assets/')) {
      // Eğer /uploads ile başlıyorsa API base URL'ine ekle
      if (trimmedUrl.startsWith('/uploads')) {
        return `${this.apiBaseUrl}${trimmedUrl}`;
      }
      return trimmedUrl;
    }
    
    // Mutlak URL ise kontrol et
    if (trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://')) {
      return trimmedUrl;
    }
    
    // "uploads/" ile başlıyorsa API URL'ine ekle
    if (trimmedUrl.startsWith('uploads/')) {
      return `${this.apiBaseUrl}/${trimmedUrl}`;
    }
    
    // Hiçbir pattern'e uymuyorsa olduğu gibi döndür
    return trimmedUrl;
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
