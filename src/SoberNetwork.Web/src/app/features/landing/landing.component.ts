import { Component, OnInit, AfterViewInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { StatsService, PlatformStats } from '@app/core/services/stats.service';
import { DAYS_OF_WEEK, GroupResponse } from '@app/core/models';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent implements OnInit, AfterViewInit {
  readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly statsService = inject(StatsService);
  private readonly dialog = inject(MatDialog);
  private readonly cdr = inject(ChangeDetectorRef);

  groups: GroupResponse[] = [];
  loadingGroups = true;
  stats: PlatformStats = { memberCount: 0, groupCount: 0, meetingCount: 0 };
  readonly daysOfWeek = DAYS_OF_WEEK;

  readonly traditions = [
    { num: 1,  text: 'Our common welfare should come first; personal recovery depends upon A.A. unity.' },
    { num: 2,  text: 'For our group purpose there is but one ultimate authority — a loving God as He may express Himself in our group conscience. Our leaders are but trusted servants; they do not govern.' },
    { num: 3,  text: 'The only requirement for A.A. membership is a desire to stop drinking.' },
    { num: 4,  text: 'Each group should be autonomous except in matters affecting other groups or A.A. as a whole.' },
    { num: 5,  text: 'Each group has but one primary purpose — to carry its message to the alcoholic who still suffers.' },
    { num: 6,  text: 'An A.A. group ought never endorse, finance, or lend the A.A. name to any related facility or outside enterprise.' },
    { num: 7,  text: 'Every A.A. group ought to be fully self-supporting, declining outside contributions.' },
    { num: 8,  text: 'Alcoholics Anonymous should remain forever non-professional, but our service centers may employ special workers.' },
    { num: 9,  text: 'A.A., as such, ought never be organized; but we may create service boards or committees directly responsible to those they serve.' },
    { num: 10, text: 'Alcoholics Anonymous has no opinion on outside issues; hence the A.A. name ought never be drawn into public controversy.' },
    { num: 11, text: 'Our public relations policy is based on attraction rather than promotion; we need always maintain personal anonymity at the level of press, radio, and films.' },
    { num: 12, text: 'Anonymity is the spiritual foundation of all our traditions, ever reminding us to place principles before personalities.' },
  ];

  ngOnInit(): void {
    this.statsService.getStats().subscribe({
      next: stats => (this.stats = stats),
      error: () => { /* silently keep zeros — stats are decorative */ },
    });

    if (!this.auth.isLoggedIn) {
      this.loadingGroups = false;
      return;
    }
    this.groupService.getMyGroups().subscribe({
      next: groups => {
        console.log('Landing: getMyGroups returned', groups);
        this.groups = groups;
        this.loadingGroups = false;
        this.cdr.detectChanges();  // Ensure template updates before observer runs
        this.setupFadeUpObserver();  // Set up observer AFTER data loads and view updates
      },
      error: (err) => {
        console.error('Landing: getMyGroups error', err);
        this.loadingGroups = false;
      },
    });
  }

  ngAfterViewInit(): void {
    // Set up observer for other fade-up elements that don't depend on data
    // (The groups fade-up elements are handled in the subscription callback)
    this.setupFadeUpObserver();
  }

  private setupFadeUpObserver(): void {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(e => {
        if (e.isIntersecting) {
          e.target.classList.add('visible');
          observer.unobserve(e.target);
        }
      });
    }, { threshold: 0.1 });
    document.querySelectorAll('.fade-up').forEach(el => observer.observe(el));
  }

  async openRegister(): Promise<void> {
    const { RegisterModalComponent } = await import('@app/shared/components/register-modal/register-modal.component');
    this.dialog.open(RegisterModalComponent, {
      panelClass: ['sn-modal-panel', 'sn-register-panel'],
      maxWidth:   '100vw',
      autoFocus:  'first-tabbable',
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
}
