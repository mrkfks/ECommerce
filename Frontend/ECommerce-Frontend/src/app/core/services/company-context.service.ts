import { Injectable, Inject, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CompanyContextService {
  private readonly storageKey = 'companyId';
  private readonly isBrowser: boolean;

  // Use a signal for reactivity
  companyId = signal<number | null>(null);

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);

    // We start with null and wait for DesignService to set the correct ID
    // based on the current domain. This prevents stale IDs from localStorage
    // causing 400/500 errors on the first API calls.
    this.companyId.set(null);
  }

  setCompanyId(id: number | null): void {
    this.companyId.set(id);
    if (this.isBrowser && id !== null) {
      try {
        localStorage.setItem(this.storageKey, String(id));
      } catch {
        // ignore storage errors
      }
    }
  }

  clearCompanyId(): void {
    this.companyId.set(null);
    if (this.isBrowser) {
      localStorage.removeItem(this.storageKey);
    }
  }

  getCompanyId(): number | null {
    return this.companyId();
  }
}
