import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize, Subject, takeUntil } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { COUNTRIES, GroupResponse, LANGUAGES, MeetingType, US_STATES } from '@app/core/models';
import { ClientLogService } from '@app/core/services/client-log.service';
import { GroupService } from '@app/core/services/group.service';
import { GroupHeroComponent, HeroTag } from '../group-hero/group-hero.component';
import { MeetingFormModalComponent } from '../meeting-form-modal/meeting-form-modal.component';
import { MeetingsTabComponent } from './tabs/meetings-tab/meetings-tab.component';
import { MembersTabComponent } from './tabs/members-tab/members-tab.component';
import { OverviewTabComponent } from './tabs/overview-tab/overview-tab.component';
import { RequestsTabComponent } from './tabs/requests-tab/requests-tab.component';
import { SettingsTabComponent } from './tabs/settings-tab/settings-tab.component';

export type HubTab = 'overview' | 'members' | 'meetings' | 'requests' | 'settings';

@Component({
  selector: 'app-group-hub',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatProgressSpinnerModule,
    GroupHeroComponent,
    OverviewTabComponent,
    MembersTabComponent,
    MeetingsTabComponent,
    RequestsTabComponent,
    SettingsTabComponent,
  ],
  templateUrl: './group-hub.component.html',
  styleUrl: './group-hub.component.scss',
})
export class GroupHubComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly log = inject(ClientLogService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  slug = '';
  group: GroupResponse | null = null;
  loading = true;
  leaving = false;
  isAdmin = false;
  error = '';
  activeTab: HubTab = 'overview';
  meetingsReloadToken = 0;
  selectedMeetingId: string | null = null;

  get heroTags(): HeroTag[] {
    if (!this.group) return [];
    const tags: HeroTag[] = [];

    const meetingTypes = [...new Set(this.group.meetings.map(m => m.meetingType))];
    for (const type of meetingTypes) {
      if (type === MeetingType.Online) tags.push({ label: 'Online', color: 'sky' });
      else if (type === MeetingType.InPerson) tags.push({ label: 'In Person', color: 'green' });
      else if (type === MeetingType.Hybrid) tags.push({ label: 'Hybrid', color: 'teal' });
    }

    tags.push(this.group.isPublic
      ? { label: 'Public', color: 'green' }
      : { label: 'Private', color: 'muted' });

    if (this.group.requiresApproval) {
      tags.push({ label: 'Approval req.', color: 'amber' });
    }

    if (this.group.timeZone) {
      tags.push({ label: this.group.timeZone.replace(/_/g, ' '), color: 'muted' });
    }

    return tags;
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadGroup();
    });

    this.route.queryParamMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const tab = params.get('tab') as HubTab | null;
      if (tab && this.isValidTab(tab)) {
        this.activeTab = tab;
        this.cdr.markForCheck();
      } else if (!tab) {
        this.activeTab = 'overview';
        this.cdr.markForCheck();
      }
    });

    this.route.fragment.pipe(takeUntil(this.destroy$)).subscribe(fragment => {
      this.selectedMeetingId = fragment ? decodeURIComponent(fragment) : null;
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  switchTab(tab: HubTab, fragment: string | null = null): void {
    if (!this.isAdmin && (tab === 'requests' || tab === 'settings')) {
      return;
    }

    this.activeTab = tab;
    this.selectedMeetingId = fragment;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: tab === 'overview' ? null : tab },
      queryParamsHandling: 'merge',
      fragment: fragment ?? undefined,
      replaceUrl: true,
    });
    this.cdr.markForCheck();
  }

  openCreateMeeting(): void {
    const readonly = {
      daysOfWeekLabels: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
      languages: LANGUAGES,
      states: US_STATES,
      countries: COUNTRIES,
    };

    this.dialog.open(MeetingFormModalComponent, {
      maxWidth: '100vw',
      data: { slug: this.slug, readonly },
      panelClass: ['sn-modal-panel', 'sn-meeting-panel'],
    }).afterClosed().subscribe(result => {
      if (result === true) {
        this.meetingsReloadToken += 1;
        this.cdr.markForCheck();
      }
    });
  }

  onLeaveGroup(): void {
    if (!this.group || this.leaving) {
      return;
    }

    this.leaving = true;
    this.groupService.leaveGroup(this.group.slug).pipe(
      finalize(() => {
        this.leaving = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: err => {
        this.error = (err as { error?: { message?: string } })?.error?.message ?? 'Could not leave the group.';
      },
    });
  }

  onGroupUpdated(updated: GroupResponse): void {
    this.group = updated;
    this.isAdmin = updated.userRole === 'GroupAdmin';
    this.cdr.markForCheck();
  }

  onGroupDeleted(): void {
    this.router.navigate(['/dashboard']);
  }

  private isValidTab(tab: string): tab is HubTab {
    return ['overview', 'members', 'meetings', 'requests', 'settings'].includes(tab);
  }

  private loadGroup(): void {
    if (!this.slug) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();

    this.groupService.getGroup(this.slug).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: group => {
        this.group = group;
        this.isAdmin = group.userRole === 'GroupAdmin';
        if (!this.isAdmin && (this.activeTab === 'requests' || this.activeTab === 'settings')) {
          this.activeTab = 'overview';
        }
        this.log.info('GroupHub: loaded', { slug: this.slug, isAdmin: String(this.isAdmin) });
      },
      error: err => {
        this.error = (err as { error?: { message?: string } })?.error?.message ?? 'We could not load this group.';
        this.group = null;
      },
    });
  }
}
