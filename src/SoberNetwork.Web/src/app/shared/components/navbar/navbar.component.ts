import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { NavigationEnd } from '@angular/router';
import { catchError, combineLatest, distinctUntilChanged, filter, map, Observable, of, startWith, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { CurrentUser, GroupResponse } from '@app/core/models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent {
  private readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly router = inject(Router);

  currentUser$: Observable<CurrentUser | null> = this.auth.currentUser$;
  myGroups: GroupResponse[] = [];

  constructor() {
    const nav$ = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      startWith(null),
    );

    combineLatest([this.auth.currentUser$.pipe(distinctUntilChanged((a, b) => a?.userId === b?.userId)), nav$]).pipe(
      map(([user]) => user),
      switchMap(user => user ? this.groupService.getMyGroups().pipe(catchError(() => of([]))) : of([])),
      takeUntilDestroyed(),
    ).subscribe(groups => (this.myGroups = groups));
  }

  navigateToGroup(slug: string): void {
    this.router.navigate(['/groups', slug]);
  }

  logout(): void {
    this.auth.logout();
  }
}
