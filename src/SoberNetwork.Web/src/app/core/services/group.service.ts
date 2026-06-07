import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  AdminMeetingResponse,
  ApproveJoinRequest,
  AssignServiceRoleRequest,
  CreateGroupRequest,
  CreateMeetingRequest,
  EmailVisibilityRequest,
  GroupAdminContactResponse,
  GroupResponse,
  GroupServiceRoleResponse,
  GroupSummaryResponse,
  JoinGroupRequest,
  JoinRequestResponse,
  MeetingResponse,
  MeetingSortBy,
  MemberDetailResponse,
  MemberResponse,
  MemberSortBy,
  PagedResponse,
  PhoneListEntryResponse,
  PhoneVisibilityRequest,
  UpdateGroupRequest,
  UpdateMeetingRequest,
  UpdateMemberRoleRequest,
  UpdateMemberStatusRequest,
} from '@app/core/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/groups`;

  // ── Group CRUD ──────────────────────────────────────────────────────────────

  getMyGroups(): Observable<GroupResponse[]> {
    // Backend returns PagedResponse<GroupResponse>; extract items here so all consumers see a flat array.
    // pageSize=50 covers any realistic number of home groups a member would belong to.
    const params = new HttpParams().set('page', 1).set('pageSize', 50);
    return this.http.get<PagedResponse<GroupResponse>>(this.base, { params }).pipe(map(r => r.items));
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

  getGroupAdmins(slug: string): Observable<GroupAdminContactResponse[]> {
    return this.http.get<GroupAdminContactResponse[]>(`${this.base}/${slug}/admins`);
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

  getMeetings(slug: string, page = 1, pageSize = 100, search?: string, sortBy: MeetingSortBy = 'Time'): Observable<PagedResponse<MeetingResponse>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('sortBy', sortBy);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResponse<MeetingResponse>>(`${this.base}/${slug}/meetings`, { params });
  }

  getAdminMeetings(slug: string, page = 1, pageSize = 100, search?: string, sortBy: MeetingSortBy = 'Time'): Observable<PagedResponse<AdminMeetingResponse>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('sortBy', sortBy);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResponse<AdminMeetingResponse>>(`${this.base}/${slug}/meetings/admin`, { params });
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

  getMembers(slug: string, page = 1, pageSize = 100, search?: string, sortBy: MemberSortBy = 'Name', sortDescending = false): Observable<PagedResponse<MemberResponse>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('sortBy', sortBy)
      .set('sortDescending', String(sortDescending));
    if (search) params = params.set('search', search);
    return this.http.get<PagedResponse<MemberResponse>>(`${this.base}/${slug}/members`, { params });
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

  setEmailVisibility(slug: string, request: EmailVisibilityRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/${slug}/members/me/email-visibility`, request);
  }

  getServiceRoles(slug: string): Observable<GroupServiceRoleResponse[]> {
    return this.http.get<GroupServiceRoleResponse[]>(`${this.base}/${slug}/service-roles`);
  }

  assignServiceRole(slug: string, request: AssignServiceRoleRequest): Observable<GroupServiceRoleResponse> {
    return this.http.post<GroupServiceRoleResponse>(`${this.base}/${slug}/service-roles`, request);
  }

  removeServiceRole(slug: string, roleId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${slug}/service-roles/${roleId}`);
  }

  getPhoneList(slug: string): Observable<PhoneListEntryResponse[]> {
    return this.http.get<PhoneListEntryResponse[]>(`${this.base}/${slug}/phone-list`);
  }

  getMemberDetail(slug: string, userId: string): Observable<MemberDetailResponse> {
    return this.http.get<MemberDetailResponse>(`${this.base}/${slug}/members/${userId}`);
  }
}
