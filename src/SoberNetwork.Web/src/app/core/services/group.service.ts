import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  GroupResponse, GroupSummaryResponse, GroupMemberResponse, PagedResponse,
  CreateGroupRequest, UpdateGroupRequest, JoinGroupRequest,
  JoinRequestResponse, UpdateMemberRoleRequest, UpdateMemberStatusRequest,
  ApproveJoinRequest, PhoneVisibilityRequest,
  PhoneListEntryResponse, MemberDetailResponse,
  MeetingResponse, AdminMeetingResponse, CreateMeetingRequest, UpdateMeetingRequest
} from '@app/core/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/groups`;

  // ── Group CRUD ──────────────────────────────────────────────────────────────

  getMyGroups(): Observable<GroupResponse[]> {
    return this.http.get<GroupResponse[]>(this.base);
  }

  getAllGroups(): Observable<GroupSummaryResponse[]> {
    return this.http.get<GroupSummaryResponse[]>(`${this.base}/all`);
  }

  getGroup(slug: string): Observable<GroupResponse> {
    return this.http.get<GroupResponse>(`${this.base}/${slug}`);
  }

  getGroupInfo(slug: string): Observable<GroupSummaryResponse> {
    return this.http.get<GroupSummaryResponse>(`${this.base}/${slug}/info`);
  }

  createGroup(request: CreateGroupRequest): Observable<GroupResponse> {
    return this.http.post<GroupResponse>(this.base, request);
  }

  updateGroup(slug: string, request: UpdateGroupRequest): Observable<GroupResponse> {
    return this.http.put<GroupResponse>(`${this.base}/${slug}`, request);
  }

  deleteGroup(slug: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${slug}`);
  }

  // ── Meetings ────────────────────────────────────────────────────────────────

  getMeetings(slug: string): Observable<MeetingResponse[]> {
    return this.http.get<MeetingResponse[]>(`${this.base}/${slug}/meetings`);
  }

  getAdminMeetings(slug: string): Observable<AdminMeetingResponse[]> {
    return this.http.get<AdminMeetingResponse[]>(`${this.base}/${slug}/meetings/admin`);
  }

  createMeeting(slug: string, request: CreateMeetingRequest): Observable<AdminMeetingResponse> {
    return this.http.post<AdminMeetingResponse>(`${this.base}/${slug}/meetings`, request);
  }

  updateMeeting(slug: string, meetingId: string, request: UpdateMeetingRequest): Observable<AdminMeetingResponse> {
    return this.http.put<AdminMeetingResponse>(`${this.base}/${slug}/meetings/${meetingId}`, request);
  }

  deleteMeeting(slug: string, meetingId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${slug}/meetings/${meetingId}`);
  }

  // ── Membership ──────────────────────────────────────────────────────────────

  getMembers(slug: string, page = 1, pageSize = 50): Observable<PagedResponse<GroupMemberResponse>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<GroupMemberResponse>>(`${this.base}/${slug}/members`, { params });
  }

  joinGroup(slug: string, request: JoinGroupRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/${slug}/join`, request);
  }

  leaveGroup(slug: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${slug}/members/me`);
  }

  getJoinRequests(slug: string, page = 1, pageSize = 50): Observable<PagedResponse<JoinRequestResponse>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<JoinRequestResponse>>(`${this.base}/${slug}/join-requests`, { params });
  }

  approveOrReject(slug: string, userId: string, request: ApproveJoinRequest): Observable<void> {
    const action = request.approved ? 'approve' : 'reject';
    const body = request.reason ? { reason: request.reason } : {};
    return this.http.post<void>(`${this.base}/${slug}/members/${userId}/${action}`, body);
  }

  updateMemberRole(slug: string, userId: string, request: UpdateMemberRoleRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/${slug}/members/${userId}/role`, request);
  }

  updateMemberStatus(slug: string, userId: string, request: UpdateMemberStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/${slug}/members/${userId}/status`, request);
  }

  removeMember(slug: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${slug}/members/${userId}`);
  }

  clearProbationaryStatus(slug: string, userId: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${slug}/members/${userId}/probation`, {});
  }

  // ── Phone (group-scoped) ────────────────────────────────────────────────────

  setPhoneVisibility(slug: string, request: PhoneVisibilityRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/${slug}/members/me/phone-visibility`, request);
  }

  getPhoneList(slug: string): Observable<PhoneListEntryResponse[]> {
    return this.http.get<PhoneListEntryResponse[]>(`${this.base}/${slug}/phone-list`);
  }

  getMemberDetail(slug: string, userId: string): Observable<MemberDetailResponse> {
    return this.http.get<MemberDetailResponse>(`${this.base}/${slug}/members/${userId}`);
  }
}
