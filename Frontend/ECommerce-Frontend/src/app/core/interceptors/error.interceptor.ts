import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Beklenmeyen bir hata oluştu';

      // Client-side error check (only in browser)
      if (isBrowser && error.error instanceof ErrorEvent) {
        errorMessage = `Hata: ${error.error.message}`;
      } else {
        // Server-side error
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.status === 0) {
          errorMessage = 'Sunucuya bağlanılamıyor. İnternet bağlantınızı kontrol edin.';
        } else if (error.status === 401) {
          errorMessage = 'Oturum süreniz doldu. Lütfen tekrar giriş yapın.';
          // Clear tokens (only in browser)
          if (isBrowser && typeof localStorage !== 'undefined') {
            localStorage.removeItem('auth_token');
            localStorage.removeItem('refresh_token');
            localStorage.removeItem('user');
          }
          if (isBrowser) {
            router.navigate(['/login']);
          }
        } else if (error.status === 403) {
          errorMessage = 'Bu işlem için yetkiniz yok.';
        } else if (error.status === 404) {
          errorMessage = 'İstenen kaynak bulunamadı.';
        } else if (error.status === 500) {
          errorMessage = 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.';
          if (isBrowser) {
            router.navigate(['/error'], { state: { error: error.message } });
          }
        } else if (error.status >= 400 && error.status < 500) {
          errorMessage = error.error?.message || `İstek hatası (${error.status})`;
        }
      }

      console.error('HTTP Error:', {
        status: error.status,
        message: errorMessage,
        url: error.url,
        error: error.error
      });

      return throwError(() => new Error(errorMessage));
    })
  );
};
