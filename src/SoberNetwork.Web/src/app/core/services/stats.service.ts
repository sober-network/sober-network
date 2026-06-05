import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PlatformStats {
  memberCount: number;
  groupCount:  number;
  meetingCount: number;
}

@Injectable({ providedIn: 'root' })
export class StatsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/stats`;

  getStats(): Observable<PlatformStats> {
    return this.http.get<PlatformStats>(this.base);
  }
}
