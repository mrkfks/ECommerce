import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, PLATFORM_ID, signal } from '@angular/core';

export interface CompanySettings {
  id?: number;
  companyName: string;
  logoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  faviconUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DesignService {
  settings = signal<CompanySettings | null>(null);
  
  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  loadSettings() {
    if (!isPlatformBrowser(this.platformId)) return;

    // Development test: override domain or use localhost
    // For real prod: const domain = window.location.hostname;
    // We can simulate different domains via a query param '?testDomain=tenant1' if we want,
    // or just use window.location.hostname. 
    // Since we are running on localhost:4200, the domain is 'localhost'.
    // We need to ensure 'localhost' is registered in backend or we use a fallback/test domain.
    
    // For this implementation, I will check for a query param 'domain' to allow easy testing.
    const urlParams = new URLSearchParams(window.location.search);
    let domain = urlParams.get('domain') || window.location.hostname;

    // Hardcode fallback for localhost if not provided
    if (domain === 'localhost') {
        // You might want to map localhost to a specific real tenant domain for testing
        // or ensure 'localhost' is in the Db.
        // let's try to get it anyway.
    }

    this.http.get<CompanySettings>(`/company/settings?domain=${domain}`)
      .subscribe({
        next: (settings) => {
          this.settings.set(settings);
          this.applyTheme(settings);
        },
        error: (err) => console.error('Failed to load company settings', err)
      });
  }

  private applyTheme(settings: CompanySettings) {
    if (!isPlatformBrowser(this.platformId)) return;

    const root = document.documentElement;
    if (settings.primaryColor) {
      root.style.setProperty('--bright-blue', settings.primaryColor); // Overriding main color var
      // You might need to adjust other vars depending on your CSS logic
       root.style.setProperty('--primary-color', settings.primaryColor);
    }
    if (settings.secondaryColor) {
      root.style.setProperty('--secondary-color', settings.secondaryColor);
    }
    
    if (settings.logoUrl) {
      // Logic to update favicon could go here
    }
    
    // Update title
    document.title = settings.companyName;
  }
}
