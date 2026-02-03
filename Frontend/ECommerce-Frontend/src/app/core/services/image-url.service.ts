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
    console.log('ImageUrlService.normalize() input:', trimmedUrl);
    
    // Zaten göreceli URL ise veya assets ile başlıyorsa aynen döndür
    if (trimmedUrl.startsWith('/') || trimmedUrl.startsWith('assets/')) {
      // Eğer /uploads ile başlıyorsa API base URL'ine ekle
      if (trimmedUrl.startsWith('/uploads')) {
        const result = `${this.apiBaseUrl}${trimmedUrl}`;
        console.log('ImageUrlService.normalize() output (relative /uploads):', result);
        return result;
      }
      console.log('ImageUrlService.normalize() output (already relative):', trimmedUrl);
      return trimmedUrl;
    }
    
    // Mutlak URL ise kontrol et
    if (trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://')) {
      // Zaten tam URL ise aynen döndür
      console.log('ImageUrlService.normalize() output (absolute URL):', trimmedUrl);
      return trimmedUrl;
    }
    
    // "uploads/" ile başlıyorsa API URL'ine ekle
    if (trimmedUrl.startsWith('uploads/')) {
      const result = `${this.apiBaseUrl}/${trimmedUrl}`;
      console.log('ImageUrlService.normalize() output (relative uploads/):', result);
      return result;
    }
    
    // Hiçbir pattern'e uymuyorsa olduğu gibi döndür
    const result = trimmedUrl;
    console.log('ImageUrlService.normalize() output:', result);
    return result;
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
