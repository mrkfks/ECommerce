import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import { join } from 'node:path';

const browserDistFolder = join(import.meta.dirname, '../browser');

const app = express();
const angularApp = new AngularNodeAppEngine();

/**
 * Reverse proxy: /api ve /uploads isteklerini backend API'ye yönlendir.
 * Render.com'da API_URL env olarak ayarlanır (ör. https://ecommerce-api.onrender.com)
 */
const API_URL = process.env['API_URL'] || 'http://localhost:5010';

app.use('/api', async (req, res) => {
  const targetUrl = `${API_URL}/api${req.url}`;
  try {
    const headers: Record<string, string> = {};
    for (const [key, value] of Object.entries(req.headers)) {
      if (value && key !== 'host' && key !== 'connection') {
        headers[key] = Array.isArray(value) ? value.join(', ') : value;
      }
    }

    let body: string | undefined;
    if (req.method !== 'GET' && req.method !== 'HEAD') {
      body = await new Promise<string>((resolve) => {
        let data = '';
        req.on('data', (chunk: Buffer) => (data += chunk.toString()));
        req.on('end', () => resolve(data));
      });
    }

    const fetchRes = await fetch(targetUrl, {
      method: req.method,
      headers,
      body: body || undefined,
    });

    res.status(fetchRes.status);
    fetchRes.headers.forEach((value, key) => {
      if (key !== 'transfer-encoding') res.setHeader(key, value);
    });
    const responseBody = await fetchRes.arrayBuffer();
    res.send(Buffer.from(responseBody));
  } catch (err) {
    console.error('[proxy] Error forwarding to API:', err);
    res.status(502).json({ error: 'API proxy error' });
  }
});

app.use('/uploads', async (req, res) => {
  const targetUrl = `${API_URL}/uploads${req.url}`;
  try {
    const fetchRes = await fetch(targetUrl);
    res.status(fetchRes.status);
    fetchRes.headers.forEach((value, key) => {
      if (key !== 'transfer-encoding') res.setHeader(key, value);
    });
    const data = await fetchRes.arrayBuffer();
    res.send(Buffer.from(data));
  } catch {
    res.status(502).json({ error: 'Upload proxy error' });
  }
});

/**
 * Serve static files from /browser
 */
app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

/**
 * Handle all other requests by rendering the Angular application.
 */
app.use((req, res, next) => {
  angularApp
    .handle(req)
    .then((response) =>
      response ? writeResponseToNodeResponse(response, res) : next(),
    )
    .catch(next);
});

/**
 * Start the server if this module is the main entry point, or it is ran via PM2.
 * The server listens on the port defined by the `PORT` environment variable, or defaults to 4000.
 */
if (isMainModule(import.meta.url) || process.env['pm_id']) {
  const port = process.env['PORT'] || 4000;
  app.listen(port, (error) => {
    if (error) {
      throw error;
    }
  });
}

/**
 * Request handler used by the Angular CLI (for dev-server and during build) or Firebase Cloud Functions.
 */
export const reqHandler = createNodeRequestHandler(app);
