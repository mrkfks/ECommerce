import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CompanyContextService {
  private readonly storageKey = 'companyId';
  private companyId: number | null = null;
  private readonly isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);

    if (this.isBrowser) {
      const stored = localStorage.getItem(this.storageKey);
      if (stored) {
        const parsed = Number(stored);
        this.companyId = Number.isFinite(parsed) ? parsed : null;
      } else if (typeof environment.defaultCompanyId === 'number') {
        this.companyId = environment.defaultCompanyId;
        try {
          localStorage.setItem(this.storageKey, String(environment.defaultCompanyId));
        } catch {
          // ignore storage write errors
        }
      }
    } else {
      // SSR ortamında sadece bellek içinde değer tut
      if (typeof environment.defaultCompanyId === 'number') {
        this.companyId = environment.defaultCompanyId;
      }
    }
  }

  setCompanyId(id: number | null): void {
    this.companyId = id;
    if (this.isBrowser) {
      try {
        if (id === null) {
          localStorage.removeItem(this.storageKey);
        } else {
          localStorage.setItem(this.storageKey, String(id));
        }
      } catch {
        // ignore storage errors
      }
    }
  }

  getCompanyId(): number | null {
    return this.companyId;
  }
}
