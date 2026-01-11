import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'products/:categoryId',
    renderMode: RenderMode.Server
  },
  {
    path: 'product/:productId',
    renderMode: RenderMode.Server
  },
  {
    path: 'order/:orderId',
    renderMode: RenderMode.Server
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
