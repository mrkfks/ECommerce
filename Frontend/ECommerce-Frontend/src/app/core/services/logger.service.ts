import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LoggerService {
  private enabled = !environment.production;

  log(...args: any[]) {
    if (this.enabled) console.log(...args);
  }

  info(...args: any[]) {
    if (this.enabled) console.info(...args);
  }

  warn(...args: any[]) {
    if (this.enabled) console.warn(...args);
  }

  error(...args: any[]) {
    if (this.enabled) console.error(...args);
  }

  debug(...args: any[]) {
    if (this.enabled && console.debug) console.debug(...args);
  }
}
