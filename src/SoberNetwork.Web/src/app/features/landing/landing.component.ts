import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { GroupResponse } from '@app/core/models';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatCardModule, MatIconModule,
    MatChipsModule, MatDividerModule, MatProgressSpinnerModule,
  ],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);

  groups: GroupResponse[] = [];
  loadingGroups = true;

  readonly traditions = [
    { num: 1,  text: 'Our common welfare comes first.' },
    { num: 2,  text: 'One ultimate authority — a loving God as expressed through group conscience.' },
    { num: 3,  text: 'The only requirement for membership is a desire to stop drinking.' },
    { num: 4,  text: 'Each group is autonomous except in matters affecting A.A. as a whole.' },
    { num: 5,  text: 'Each group has but one primary purpose — to carry its message to the alcoholic who still suffers.' },
    { num: 6,  text: 'An A.A. group ought never endorse, finance or lend its name to any outside enterprise.' },
    { num: 7,  text: 'Every A.A. group ought to be fully self-supporting, declining outside contributions.' },
    { num: 8,  text: 'A.A. should remain forever non-professional.' },
    { num: 9,  text: 'A.A. ought never be organized, but may create boards or committees directly responsible to those they serve.' },
    { num: 10, text: 'A.A. has no opinion on outside issues.' },
    { num: 11, text: 'Our public relations policy is based on attraction rather than promotion.' },
    { num: 12, text: 'Anonymity is the spiritual foundation of all our traditions.' },
  ];

  ngOnInit(): void {
    if (!this.auth.isLoggedIn) {
      this.loadingGroups = false;
      return;
    }
    this.groupService.getMyGroups().subscribe({
      next: groups => { this.groups = groups; this.loadingGroups = false; },
      error: () => { this.loadingGroups = false; },
    });
  }
}
