import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, PLATFORM_ID, signal, inject } from '@angular/core';
import { CompanyContextService } from './company-context.service';

export interface CompanySettings {
  id?: number;
  companyName: string;
  logoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  faviconUrl?: string;
  isActive?: boolean;
  isApproved?: boolean;
  domain?: string;
}

// Default theme settings
const DEFAULT_SETTINGS: CompanySettings = {
  id: 1, // Süper adminin şirketi
  companyName: 'Süper Admin Şirketi',
  primaryColor: '#3b82f6',
  secondaryColor: '#1e40af'
};

@Injectable({
  providedIn: 'root'
})
export class DesignService {
  private companyContext = inject(CompanyContextService);

  settings = signal<CompanySettings | null>(null);
  isLoading = signal<boolean>(false);
  loadError = signal<string | null>(null);

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  checkApiConnection(): Promise<boolean> {
    return new Promise((resolve) => {
      this.http.get('/health').subscribe({
        next: () => resolve(true),
        error: () => resolve(false)
      });
    });
  }

  loadSettings() {
    if (!isPlatformBrowser(this.platformId)) return;

    this.isLoading.set(true);
    this.loadError.set(null);

    // Check for domain query param for testing, otherwise use hostname
    const urlParams = new URLSearchParams(window.location.search);
    let domain = urlParams.get('domain') || window.location.hostname;

    this.http.get<CompanySettings>(`/company/settings?domain=${domain}`)
      .subscribe({
        next: (settings) => {
          this.isLoading.set(false);

          // Update company context with the loaded company ID
          if (settings.id) {
            this.companyContext.setCompanyId(settings.id);
          }

          // Check if company is active and approved
          if (settings.isActive === false) {
            this.loadError.set('Bu mağaza şu anda aktif değil.');
            // Hata durumunda dummy theme gösterme
            return;
          }

          if (settings.isApproved === false) {
            this.loadError.set('Bu mağaza henüz onaylanmamış.');
            // Hata durumunda dummy theme gösterme
            return;
          }

          this.settings.set(settings);
          this.applyTheme(settings);
        },
        error: (err) => {
          console.error('Failed to load company settings', err);
          this.isLoading.set(false);
          // Set error message - API bağlantısı başarısız
          this.loadError.set('Mağaza ayarları yüklenemedi. Lütfen daha sonra tekrar deneyin.');
          // Hata durumunda dummy theme gösterme
        }
      });
  }

  private applyDefaultTheme() {
    if (!isPlatformBrowser(this.platformId)) return;

    this.settings.set(DEFAULT_SETTINGS);
    this.companyContext.setCompanyId(DEFAULT_SETTINGS.id ?? 1);
    this.applyTheme(DEFAULT_SETTINGS);
  }

  private applyTheme(settings: CompanySettings) {
    if (!isPlatformBrowser(this.platformId)) return;

    const root = document.documentElement;

    // Apply primary color
    if (settings.primaryColor) {
      root.style.setProperty('--bright-blue', settings.primaryColor);
      root.style.setProperty('--primary-color', settings.primaryColor);

      // Generate lighter/darker variants
      root.style.setProperty('--primary-light', this.adjustColor(settings.primaryColor, 20));
      root.style.setProperty('--primary-dark', this.adjustColor(settings.primaryColor, -20));
    }

    // Apply secondary color
    if (settings.secondaryColor) {
      root.style.setProperty('--secondary-color', settings.secondaryColor);
      root.style.setProperty('--secondary-light', this.adjustColor(settings.secondaryColor, 20));
      root.style.setProperty('--secondary-dark', this.adjustColor(settings.secondaryColor, -20));
    }

    // Update favicon if provided
    if (settings.faviconUrl || settings.logoUrl) {
      this.updateFavicon(settings.faviconUrl || settings.logoUrl!);
    }

    // Update title
    if (settings.companyName) {
      document.title = settings.companyName;
    }
  }

  private updateFavicon(url: string) {
    const link: HTMLLinkElement = document.querySelector("link[rel*='icon']") || document.createElement('link');
    link.type = 'image/x-icon';
    link.rel = 'shortcut icon';
    link.href = url;
    document.getElementsByTagName('head')[0].appendChild(link);
  }

  // Helper to lighten/darken a hex color
  private adjustColor(hex: string, percent: number): string {
    const num = parseInt(hex.replace('#', ''), 16);
    const amt = Math.round(2.55 * percent);
    const R = Math.min(255, Math.max(0, (num >> 16) + amt));
    const G = Math.min(255, Math.max(0, ((num >> 8) & 0x00FF) + amt));
    const B = Math.min(255, Math.max(0, (num & 0x0000FF) + amt));
    return '#' + (0x1000000 + R * 0x10000 + G * 0x100 + B).toString(16).slice(1);
  }

  // Get current logo URL (for use in components)
  getLogoUrl(): string | undefined {
    return this.settings()?.logoUrl;
  }

  // Get company name
  getCompanyName(): string {
    return this.settings()?.companyName || DEFAULT_SETTINGS.companyName;
  }
}
