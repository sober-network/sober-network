import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

/** Structured metadata for client log entries. Values must be strings — no PII (T12). */
export type ClientLogData = Record<string, string>;

@Injectable({ providedIn: 'root' })
export class ClientLogService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/api/client-log`;

  info(message: string, data?: ClientLogData): void {
    this.send('info', message, data);
  }

  warn(message: string, data?: ClientLogData): void {
    this.send('warn', message, data);
  }

  error(message: string, data?: ClientLogData): void {
    this.send('error', message, data);
  }

  private send(level: string, message: string, data?: ClientLogData): void {
    this.http
      .post(this.url, { level, message, data })
      .subscribe({ error: () => {} }); // fire-and-forget
  }
}
