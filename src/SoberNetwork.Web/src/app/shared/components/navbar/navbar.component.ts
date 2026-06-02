import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { CurrentUser, GroupResponse } from '@app/core/models';
import { Observable } from 'rxjs';

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
export class NavbarComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly router = inject(Router);

  currentUser$: Observable<CurrentUser | null> = this.auth.currentUser$;
  myGroups: GroupResponse[] = [];

  ngOnInit(): void {
    this.auth.currentUser$.subscribe(user => {
      if (user) {
        this.groupService.getMyGroups().subscribe({
          next: groups => (this.myGroups = groups),
          error: () => (this.myGroups = []),
        });
      } else {
        this.myGroups = [];
      }
    });
  }

  navigateToGroup(slug: string): void {
    this.router.navigate(['/groups', slug]);
  }

  logout(): void {
    this.auth.logout();
  }
}
