import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ClientLogService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/api/client-log`;

  info(message: string, data?: unknown): void {
    this.send('info', message, data);
  }

  warn(message: string, data?: unknown): void {
    this.send('warn', message, data);
  }

  error(message: string, data?: unknown): void {
    this.send('error', message, data);
  }

  private send(level: string, message: string, data?: unknown): void {
    this.http
      .post(this.url, { level, message, data })
      .subscribe({ error: () => {} }); // fire-and-forget
  }
}
