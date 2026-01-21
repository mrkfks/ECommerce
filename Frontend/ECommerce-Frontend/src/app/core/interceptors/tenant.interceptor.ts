import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { DesignService } from '../services/design.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const designService = inject(DesignService);
  const settings = designService.settings();

  if (settings && settings.id) {
    req = req.clone({
      setHeaders: {
        'X-Company-Id': settings.id.toString()
      }
    });
  }

  return next(req);
};
