import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { inject } from '@angular/core';
import { CompanyContextService } from '../services/company-context.service';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const companyContext = inject(CompanyContextService);
  // Sadece API isteklerini işle
  if (!req.url.startsWith('http')) {
    req = req.clone({
      url: `${environment.apiUrl}${req.url}`
    });
  }

  // Content-Type header'ı ekle (FormData hariç)
  if (!(req.body instanceof FormData)) {
    req = req.clone({ setHeaders: { 'Content-Type': 'application/json' } });
  }

  // Şirket tenant başlığını her durumda ekle
  const cid = companyContext.getCompanyId();
  if (cid !== null && cid !== undefined) {
    req = req.clone({ setHeaders: { 'X-Company-Id': String(cid) } });
  }

  return next(req);
};
