import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  MemberProfileResponse, UpdateProfileRequest, ChangePasswordRequest,
  ChangeEmailRequest, SetSobrietyDateRequest, SobrietyVisibilityRequest,
  SobrietyResponse, SetPhoneRequest, AdminMemberResponse, DeleteAccountRequest
} from '@app/core/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/members`;

  // ── Profile ─────────────────────────────────────────────────────────────────

  getMyProfile(): Observable<MemberProfileResponse> {
    return this.http.get<MemberProfileResponse>(`${this.base}/me`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<MemberProfileResponse> {
    return this.http.put<MemberProfileResponse>(`${this.base}/me`, request);
  }

  // ── Credentials ─────────────────────────────────────────────────────────────

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/me/password`, request);
  }

  changeEmail(request: ChangeEmailRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/me/email`, request);
  }

  // ── Sobriety ────────────────────────────────────────────────────────────────

  setSobrietyDate(request: SetSobrietyDateRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/me/sobriety-date`, request);
  }

  removeSobrietyDate(): Observable<void> {
    return this.http.delete<void>(`${this.base}/me/sobriety-date`);
  }

  setSobrietyVisibility(request: SobrietyVisibilityRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/me/sobriety-date/visibility`, request);
  }

  getMySobriety(): Observable<SobrietyResponse> {
    return this.http.get<SobrietyResponse>(`${this.base}/me/sobriety`);
  }

  // ── Phone ────────────────────────────────────────────────────────────────────

  setPhone(request: SetPhoneRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/me/phone`, request);
  }

  // ── Account ──────────────────────────────────────────────────────────────────

  deleteAccount(request: DeleteAccountRequest): Observable<void> {
    return this.http.delete<void>(`${this.base}/me`, { body: request });
  }

  // ── SuperAdmin ───────────────────────────────────────────────────────────────

  getAllMembers(): Observable<AdminMemberResponse[]> {
    return this.http.get<AdminMemberResponse[]>(this.base);
  }

  adminDeleteMember(userId: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${userId}/deactivate`, {});
  }
}
