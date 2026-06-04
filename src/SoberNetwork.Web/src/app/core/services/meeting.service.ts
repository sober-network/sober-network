import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MeetingSearchParams,
  PublicMeetingSearchResponse,
  MailingAddressResponse,
  UpdateMailingAddressRequest,
  TimeBlock,
  MeetingType,
} from '../models/group.models';

@Injectable({ providedIn: 'root' })
export class MeetingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /**
   * Search public meeting schedules. No auth required.
   * Supports day, time block, format, meeting type, open/closed, and location radius filters.
   */
  searchPublicMeetings(params: MeetingSearchParams): Observable<PublicMeetingSearchResponse[]> {
    let httpParams = new HttpParams();
    if (params.days?.length) {
      params.days.forEach(d => (httpParams = httpParams.append('days', String(d))));
    }
    if (params.timeBlock !== undefined) {
      httpParams = httpParams.set('timeBlock', String(params.timeBlock));
    }
    if (params.formats?.length) {
      params.formats.forEach(f => (httpParams = httpParams.append('formats', f)));
    }
    if (params.meetingType !== undefined) {
      httpParams = httpParams.set('meetingType', String(params.meetingType));
    }
    if (params.isOpen !== undefined) {
      httpParams = httpParams.set('isOpen', String(params.isOpen));
    }
    if (params.lat !== undefined) {
      httpParams = httpParams.set('lat', String(params.lat));
    }
    if (params.lon !== undefined) {
      httpParams = httpParams.set('lon', String(params.lon));
    }
    if (params.radiusMiles !== undefined) {
      httpParams = httpParams.set('radiusMiles', String(params.radiusMiles));
    }
    return this.http.get<PublicMeetingSearchResponse[]>(`${this.baseUrl}/api/meetings`, {
      params: httpParams,
    });
  }

  /** Get the authenticated user's optional mailing address (T3 — opt-in). */
  getMyMailingAddress(): Observable<MailingAddressResponse> {
    return this.http.get<MailingAddressResponse>(`${this.baseUrl}/api/members/me/mailing-address`);
  }

  /** Update the authenticated user's optional mailing address (T3 — opt-in). */
  updateMyMailingAddress(request: UpdateMailingAddressRequest): Observable<MailingAddressResponse> {
    return this.http.put<MailingAddressResponse>(`${this.baseUrl}/api/members/me/mailing-address`, request);
  }
}
