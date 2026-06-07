import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { finalize } from 'rxjs';
import {
  GroupAdminContactResponse,
  GroupResponse,
  GroupServiceRoleResponse,
  MemberProfileResponse,
} from '@app/core/models';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { MemberService } from '@app/core/services/member.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-overview-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
  ],
  templateUrl: './overview-tab.component.html',
  styleUrl: './overview-tab.component.scss',
})
export class OverviewTabComponent implements OnInit, OnChanges {
  private readonly groupService = inject(GroupService);
  private readonly memberService = inject(MemberService);
  private readonly authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) group!: GroupResponse;
  @Input({ required: true }) slug!: string;
  @Input() isAdmin = false;
  @Output() switchToMeetings = new EventEmitter<void>();
  @Output() leaveGroup = new EventEmitter<void>();

  admins: GroupAdminContactResponse[] = [];
  serviceRoles: GroupServiceRoleResponse[] = [];
  myProfile: MemberProfileResponse | null = null;

  phoneShared = false;
  emailShared = false;
  togglingPhone = false;
  togglingEmail = false;
  loadingRoles = false;

  ngOnInit(): void {
    this.loadTabData();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.group) {
      this.phoneShared = this.group.userIsPhoneShared;
      this.emailShared = this.group.userIsEmailShared;
    }

    if (changes['slug'] && !changes['slug'].firstChange) {
      this.loadTabData();
    }
  }

  get currentUserEmail(): string {
    return this.authService.currentUser?.email ?? '';
  }

  groupAccess(): string {
    return this.group?.isPublic ? 'Public' : 'Private';
  }

  approvalPolicy(): string {
    return this.group?.requiresApproval ? 'Approval required' : 'Open to join';
  }

  createdDate(): string {
    return this.group?.createdAt
      ? new Date(this.group.createdAt).toLocaleDateString(undefined, { dateStyle: 'medium' })
      : '—';
  }

  memberSinceDate(): string {
    const joinedAt = (this.group as GroupResponse & { joinedAt?: string | null }).joinedAt;
    return joinedAt
      ? new Date(joinedAt).toLocaleDateString(undefined, { dateStyle: 'medium' })
      : '—';
  }

  humanize(value: string | null | undefined): string {
    if (!value) return '—';
    return value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, c => c.toUpperCase());
  }

  formatNextMeetingDate(): string {
    if (!this.group?.nextMeeting) return '';
    return new Date(this.group.nextMeeting.nextOccurrence).toLocaleDateString(undefined, {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
    });
  }

  formatNextMeetingTime(): string {
    if (!this.group?.nextMeeting) return '';
    return new Date(this.group.nextMeeting.nextOccurrence).toLocaleTimeString(undefined, {
      hour: 'numeric',
      minute: '2-digit',
    });
  }

  roleLabel(roleType: string, customTitle: string | null): string {
    if (roleType === 'Other' && customTitle) return customTitle;

    const labels: Record<string, string> = {
      GSR: 'G.S.R.',
      Secretary: 'Secretary',
      Treasurer: 'Treasurer',
      IntergroupRep: 'Intergroup Rep',
      LiteratureRep: 'Literature Rep',
      GrapevineRep: 'Grapevine Rep',
      MeetingChair: 'Meeting Chair',
      ChipsPerson: 'Chips Person',
      Other: 'Other',
    };

    return labels[roleType] ?? roleType;
  }

  roleColorClass(roleType: string): string {
    const map: Record<string, string> = {
      GSR: 'role-violet',
      Secretary: 'role-sky',
      Treasurer: 'role-green',
      IntergroupRep: 'role-amber',
      LiteratureRep: 'role-teal',
      GrapevineRep: 'role-rose',
      MeetingChair: 'role-indigo',
      ChipsPerson: 'role-coral',
      Other: 'role-muted',
    };

    return map[roleType] ?? 'role-muted';
  }

  adminEmailHref(email: string): string {
    return `mailto:${email}`;
  }

  adminPhoneHref(phone: string | null): string | null {
    if (!phone) return null;
    const normalized = phone.replace(/[^\d+]/g, '');
    return normalized ? `tel:${normalized}` : null;
  }

  togglePhone(checked: boolean): void {
    this.togglingPhone = true;
    this.cdr.markForCheck();

    this.groupService.setPhoneVisibility(this.slug, { isShared: checked }).pipe(
      finalize(() => {
        this.togglingPhone = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: () => {
        this.phoneShared = checked;
        this.group.userIsPhoneShared = checked;
      },
      error: () => {
        this.phoneShared = !checked;
      },
    });
  }

  toggleEmail(checked: boolean): void {
    this.togglingEmail = true;
    this.cdr.markForCheck();

    this.groupService.setEmailVisibility(this.slug, { isShared: checked }).pipe(
      finalize(() => {
        this.togglingEmail = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: () => {
        this.emailShared = checked;
        this.group.userIsEmailShared = checked;
      },
      error: () => {
        this.emailShared = !checked;
      },
    });
  }

  confirmLeave(): void {
    this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '440px',
      data: {
        title: 'Leave this group?',
        message: `You can always come back later if the door is open. Are you sure you want to leave ${this.group.name}?`,
        confirmLabel: 'Leave group',
        dangerous: true,
      },
    }).afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.leaveGroup.emit();
      }
    });
  }

  trackByUserId(_: number, item: GroupAdminContactResponse | GroupServiceRoleResponse): string {
    return item.userId;
  }

  private loadTabData(): void {
    if (!this.slug) {
      return;
    }

    this.loadAdmins();
    this.loadServiceRoles();
    this.loadMyProfile();
  }

  private loadAdmins(): void {
    this.groupService.getGroupAdmins(this.slug).subscribe({
      next: admins => {
        this.admins = admins;
        this.cdr.markForCheck();
      },
      error: () => {
        this.admins = [];
        this.cdr.markForCheck();
      },
    });
  }

  private loadServiceRoles(): void {
    this.loadingRoles = true;
    this.cdr.markForCheck();

    this.groupService.getServiceRoles(this.slug).pipe(
      finalize(() => {
        this.loadingRoles = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: roles => {
        this.serviceRoles = [...roles].sort((a, b) => a.displayOrder - b.displayOrder || a.displayName.localeCompare(b.displayName));
      },
      error: () => {
        this.serviceRoles = [];
      },
    });
  }

  private loadMyProfile(): void {
    this.memberService.getMyProfile().subscribe({
      next: profile => {
        this.myProfile = profile;
        this.cdr.markForCheck();
      },
      error: () => {
        this.myProfile = null;
        this.cdr.markForCheck();
      },
    });
  }
}
