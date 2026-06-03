import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GroupService } from '@app/core/services/group.service';
import { AuthService } from '@app/core/services/auth.service';
import { MemberDetailResponse } from '@app/core/models';
import { ChangeDetectorRef } from '@angular/core';
import { switchMap, of } from 'rxjs';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatDividerModule, MatProgressSpinnerModule,
  ],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.scss',
})
export class MemberDetailComponent implements OnInit {
  private readonly route  = inject(ActivatedRoute);
  private readonly groups = inject(GroupService);
  private readonly auth   = inject(AuthService);
  private readonly cdr    = inject(ChangeDetectorRef);

  member: MemberDetailResponse | null = null;
  loading = true;
  error   = '';

  ngOnInit(): void {
    const userId   = this.route.snapshot.paramMap.get('userId')!;
    // Prefer ?group= query param; fall back to first of user's groups
    const groupSlug = this.route.snapshot.queryParamMap.get('group');

    const slug$ = groupSlug
      ? of(groupSlug)
      : this.groups.getMyGroups().pipe(
          switchMap(gs => gs.length ? of(gs[0].slug) : of(null as string | null))
        );

    slug$.pipe(
      switchMap(slug => slug ? this.groups.getMemberDetail(slug, userId) : of(null))
    ).subscribe({
      next: m => { this.member = m; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.error = 'Could not load member profile.'; this.loading = false; this.cdr.detectChanges(); },
    });
  }

  get isOwnProfile(): boolean {
    return this.auth.currentUser?.userId === this.member?.userId;
  }

  sobrietyLabel(daysSober: number | null): string {
    if (daysSober === null) return '';
    if (daysSober === 0) return 'Today is day one';
    if (daysSober === 1) return '1 day sober';
    return `${daysSober} days sober`;
  }
}
