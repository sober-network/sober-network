import { Component, OnInit, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { StatsService, PlatformStats } from '@app/core/services/stats.service';
import { GroupResponse } from '@app/core/models';

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

  groups: GroupResponse[] = [];
  loadingGroups = true;
  stats: PlatformStats = { memberCount: 0, groupCount: 0, meetingCount: 0 };

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
      next: groups => { this.groups = groups; this.loadingGroups = false; },
      error: () => { this.loadingGroups = false; },
    });
  }

  ngAfterViewInit(): void {
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
      panelClass: 'sn-login-panel',
      maxWidth:   '100vw',
      width:      '440px',
      autoFocus:  'first-tabbable',
    });
  }
}
