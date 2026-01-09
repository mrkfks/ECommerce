import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  // Sadece API isteklerini işle
  if (!req.url.startsWith('http')) {
    req = req.clone({
      url: `${environment.apiUrl}${req.url}`
    });
  }

  // Content-Type header'ı ekle (FormData hariç)
  if (!(req.body instanceof FormData)) {
    req = req.clone({
      setHeaders: {
        'Content-Type': 'application/json'
      }
    });
  }

  return next(req);
};
