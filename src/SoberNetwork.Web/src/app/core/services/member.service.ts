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
    return this.http.post<void>(`${this.base}/me/change-password`, request);
  }

  changeEmail(request: ChangeEmailRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/me/change-email`, request);
  }

  // ── Sobriety ────────────────────────────────────────────────────────────────

  setSobrietyDate(request: SetSobrietyDateRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/me/sobriety-date`, request);
  }

  setSobrietyVisibility(request: SobrietyVisibilityRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/me/sobriety-visibility`, request);
  }

  getMySobriety(): Observable<SobrietyResponse> {
    return this.http.get<SobrietyResponse>(`${this.base}/me/sobriety`);
  }

  // ── Phone ────────────────────────────────────────────────────────────────────

  setPhone(request: SetPhoneRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/me/phone`, request);
  }

  // ── Account ──────────────────────────────────────────────────────────────────

  deleteAccount(request: DeleteAccountRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/me/delete`, request);
  }

  // ── SuperAdmin ───────────────────────────────────────────────────────────────

  getAllMembers(): Observable<AdminMemberResponse[]> {
    return this.http.get<AdminMemberResponse[]>(`${this.base}/admin/all`);
  }

  adminDeleteMember(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/admin/${userId}`);
  }
}
