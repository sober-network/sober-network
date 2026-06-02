import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { catchError, finalize, forkJoin, map, of, switchMap } from 'rxjs';
import { PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { GroupMemberResponse, MembershipStatus } from '@app/core/models';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';

interface GroupMemberView extends GroupMemberResponse {
  sobrietyDate: string | null;
}

@Component({
  selector: 'app-group-members',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './group-members.component.html',
  styleUrl: './group-members.component.scss',
})
export class GroupMembersComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly membershipStatuses: MembershipStatus[] = ['Active', 'Probationary', 'Suspended', 'Banned'];

  slug = '';
  groupName = '';
  members: GroupMemberView[] = [];
  loading = true;
  isAdmin = false;
  error = '';
  actionUserId: string | null = null;
  totalCount = 0;
  pageIndex = 0;
  pageSize = 10;

  get currentUserId(): string {
    return this.auth.currentUser?.userId ?? '';
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.pageIndex = 0;
      this.loadMembers();
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadMembers();
  }

  toggleRole(member: GroupMemberView): void {
    if (this.isSelf(member)) {
      return;
    }

    const nextRole = member.role === 'GroupAdmin' ? 'Member' : 'GroupAdmin';
    this.actionUserId = member.userId;

    this.groupService.updateMemberRole(this.slug, member.userId, { role: nextRole }).pipe(
      finalize(() => {
        this.actionUserId = null;
      })
    ).subscribe({
      next: () => this.loadMembers(),
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not update that role right now.');
      },
    });
  }

  updateStatus(member: GroupMemberView, status: MembershipStatus): void {
    if (this.isSelf(member) || member.status === status) {
      return;
    }

    this.actionUserId = member.userId;
    this.groupService.updateMemberStatus(this.slug, member.userId, { status }).pipe(
      finalize(() => {
        this.actionUserId = null;
      })
    ).subscribe({
      next: () => {
        member.status = status;
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not update that membership status right now.');
      },
    });
  }

  confirmRemove(member: GroupMemberView): void {
    if (this.isSelf(member)) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '440px',
      data: {
        title: 'Remove member?',
        message: `Are you sure you want to remove ${member.displayName} from ${this.groupName}?`,
        confirmLabel: 'Remove member',
        dangerous: true,
      },
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.actionUserId = member.userId;
      this.groupService.removeMember(this.slug, member.userId).pipe(
        finalize(() => {
          this.actionUserId = null;
        })
      ).subscribe({
        next: () => this.loadMembers(),
        error: err => {
          this.error = this.getErrorMessage(err, 'We could not remove that member right now.');
        },
      });
    });
  }

  isSelf(member: GroupMemberView): boolean {
    return member.userId === this.currentUserId;
  }

  roleClass(member: GroupMemberView): string {
    return member.role === 'GroupAdmin' ? 'role-admin' : 'role-member';
  }

  statusClass(status: MembershipStatus): string {
    return `status-${status.toLowerCase()}`;
  }

  trackByUserId(_: number, member: GroupMemberView): string {
    return member.userId;
  }

  private loadMembers(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      group: this.groupService.getGroup(this.slug),
      isAdmin: this.groupService.getJoinRequests(this.slug, 1, 1).pipe(
        map(() => true),
        catchError(() => of(false))
      ),
    }).pipe(
      switchMap(({ group, isAdmin }) => {
        this.groupName = group.name;
        this.isAdmin = isAdmin;
        return this.groupService.getMembers(this.slug, this.pageIndex + 1, this.pageSize);
      }),
      switchMap(response => {
        this.totalCount = response.totalCount;

        if (response.items.length === 0) {
          return of([] as GroupMemberView[]);
        }

        return forkJoin(
          response.items.map(member => this.groupService.getMemberDetail(this.slug, member.userId).pipe(
            map(detail => ({
              ...member,
              sobrietyDate: detail.sobriety?.sobrietyDate ?? null,
            })),
            catchError(() => of({
              ...member,
              sobrietyDate: null,
            }))
          ))
        );
      }),
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: members => {
        this.members = members;
      },
      error: err => {
        this.members = [];
        this.totalCount = 0;
        this.error = this.getErrorMessage(err, 'We could not load the member list right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
