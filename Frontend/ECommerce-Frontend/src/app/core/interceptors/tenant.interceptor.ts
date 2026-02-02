import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CompanyContextService } from '../services/company-context.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const companyContext = inject(CompanyContextService);
  const companyId = companyContext.getCompanyId();

  if (companyId) {
    req = req.clone({
      setHeaders: {
        'X-Company-Id': companyId.toString()
      }
    });
  }

  return next(req);
};
