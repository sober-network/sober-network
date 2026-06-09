import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DAYS_OF_WEEK, GroupResponse } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ClientLogService } from '@app/core/services/client-log.service';
import { CreateGroupModalComponent } from '../create-group-modal/create-group-modal.component';

@Component({
  selector: 'app-groups-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './groups-dashboard.component.html',
  styleUrl: './groups-dashboard.component.scss',
})
export class GroupsDashboardComponent implements OnInit {
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly log = inject(ClientLogService);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly daysOfWeek = DAYS_OF_WEEK;

  groups: GroupResponse[] = [];
  loading = true;
  error = '';

  ngOnInit(): void {
    this.log.info('GroupsDashboard.ngOnInit');
    this.loadGroups();
  }

  openCreateGroupDialog(): void {
    this.dialog.open(CreateGroupModalComponent, {
      panelClass: ['sn-modal-panel', 'sn-create-group-panel'],
      maxWidth: '100vw',
      autoFocus: 'first-tabbable',
    }).afterClosed().subscribe({
      next: created => {
        if (created) {
          this.loadGroups();
        }
      },
    });
  }

  trackBySlug(_: number, group: GroupResponse): string {
    return group.slug;
  }

  descriptionPreview(group: GroupResponse): string {
    return group.description || 'A welcoming place to stay connected between meetings.';
  }

  meetingSummary(group: GroupResponse): string | null {
    if (!group.meetings || group.meetings.length === 0) {
      return null;
    }

    const m = group.meetings[0];
    const parts: string[] = [];

    if (m.isRecurring && m.daysOfWeek && m.daysOfWeek.length > 0) {
      const dayNames = m.daysOfWeek.map(d => this.daysOfWeek[d] ?? 'Unknown').join(', ');
      parts.push(dayNames);
    } else if (!m.isRecurring && m.occursOn) {
      parts.push(new Date(m.occursOn).toLocaleDateString());
    }

    if (m.time) {
      parts.push(m.time);
    }

    if (m.durationMinutes > 0) {
      parts.push(`${m.durationMinutes} min`);
    }

    const suffix = group.meetings.length > 1 ? ` +${group.meetings.length - 1} more` : '';
    return parts.length > 0 ? parts.join(' • ') + suffix : null;
  }

  getGroupIconColor(index: number): string {
    const colors = ['violet', 'rose', 'sky', 'amber', 'green', 'teal', 'indigo', 'coral'];
    return colors[index % colors.length];
  }

  private loadGroups(): void {
    this.loading = true;
    this.error = '';
    this.log.info('GroupsDashboard.loadGroups: subscribing');

    this.groupService.getMyGroups().pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
        this.log.info('GroupsDashboard.loadGroups: finalize', { loading: 'false' });
      })
    ).subscribe({
      next: groups => {
        this.log.info('GroupsDashboard.loadGroups: next', { count: String(groups.length) });
        this.groups = groups;
      },
      error: err => {
        this.log.error('GroupsDashboard.loadGroups: error', { message: String(err) });
        this.groups = [];
        this.error = this.getErrorMessage(err, 'We could not load your groups right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
