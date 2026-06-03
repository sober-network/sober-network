import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatBadgeModule } from '@angular/material/badge';
import { ChangeDetectorRef } from '@angular/core';
import { MemberService } from '@app/core/services/member.service';
import { GroupService } from '@app/core/services/group.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '@app/shared/components/confirm-dialog/confirm-dialog.component';
import { AdminMemberResponse, GroupResponse } from '@app/core/models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatTableModule, MatTabsModule, MatChipsModule,
    MatTooltipModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDialogModule, MatBadgeModule,
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent implements OnInit {
  private readonly memberService = inject(MemberService);
  private readonly groupService  = inject(GroupService);
  private readonly snack         = inject(MatSnackBar);
  private readonly dialog        = inject(MatDialog);
  private readonly cdr           = inject(ChangeDetectorRef);

  members: AdminMemberResponse[] = [];
  groups:  GroupResponse[]        = [];
  loading = true;

  memberColumns = ['displayName', 'email', 'emailConfirmed', 'groupCount', 'createdAt', 'actions'];
  groupColumns  = ['name', 'slug', 'memberCount', 'isPublic', 'requiresApproval', 'createdAt', 'actions'];

  ngOnInit(): void {
    forkJoin({
      members: this.memberService.getAllMembers(),
      groups: this.groupService.getAllGroups(),
    }).subscribe({
      next: ({ members, groups }) => {
        this.members = members;
        this.groups = groups;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); },
    });
  }

  get totalUsers():  number { return this.members.length; }
  get totalGroups(): number { return this.groups.length; }
  get confirmedUsers(): number { return this.members.filter(m => m.emailConfirmed).length; }
  get superAdmins():    number { return this.members.filter(m => m.isSuperAdmin).length; }

  deleteMember(member: AdminMemberResponse): void {
    const data: ConfirmDialogData = {
      title:         `Delete ${member.displayName}?`,
      message:       `This will permanently delete the account for ${member.email} and remove them from all groups.`,
      confirmLabel:  'Delete User',
      dangerous:     true,
    };
    this.dialog.open(ConfirmDialogComponent, { data }).afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.memberService.adminDeleteMember(member.userId).subscribe({
        next: () => {
          this.members = this.members.filter(m => m.userId !== member.userId);
          this.snack.open(`${member.displayName} deleted`, 'OK', { duration: 3000 });
        },
        error: () => this.snack.open('Delete failed', 'OK', { duration: 3000 }),
      });
    });
  }

  deleteGroup(group: GroupResponse): void {
    const data: ConfirmDialogData = {
      title:        `Delete "${group.name}"?`,
      message:      `This will permanently delete the group and remove all ${group.memberCount} members from it.`,
      confirmLabel: 'Delete Group',
      dangerous:    true,
    };
    this.dialog.open(ConfirmDialogComponent, { data }).afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.groupService.deleteGroup(group.slug).subscribe({
        next: () => {
          this.groups = this.groups.filter(g => g.slug !== group.slug);
          this.snack.open(`"${group.name}" deleted`, 'OK', { duration: 3000 });
        },
        error: () => this.snack.open('Delete failed', 'OK', { duration: 3000 }),
      });
    });
  }
}

