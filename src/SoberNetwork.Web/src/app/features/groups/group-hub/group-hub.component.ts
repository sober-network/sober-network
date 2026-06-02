import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { catchError, finalize, forkJoin, map, of } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GroupResponse } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ClientLogService } from '@app/core/services/client-log.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-group-hub',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './group-hub.component.html',
  styleUrl: './group-hub.component.scss',
})
export class GroupHubComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly log = inject(ClientLogService);
  private readonly cdr = inject(ChangeDetectorRef);

  slug = '';
  group: GroupResponse | null = null;
  loading = true;
  leaving = false;
  isAdmin = false;
  error = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.log.info('GroupHub.ngOnInit', { slug: this.slug });
      this.loadGroup();
    });
  }

  confirmLeave(): void {
    if (!this.group || this.leaving) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '440px',
      data: {
        title: 'Leave this group?',
        message: `You can always come back later if the door is open. Are you sure you want to leave ${this.group.name}?`,
        confirmLabel: 'Leave group',
        dangerous: true,
      },
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.leaveGroup();
      }
    });
  }

  private loadGroup(): void {
    this.loading = true;
    this.error = '';
    this.log.info('GroupHub.loadGroup: subscribing', { slug: this.slug });

    forkJoin({
      group: this.groupService.getGroup(this.slug),
      isAdmin: this.groupService.getJoinRequests(this.slug, 1, 1).pipe(
        map(() => true),
        catchError(() => of(false))
      ),
    }).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
        this.log.info('GroupHub.loadGroup: finalize', { slug: this.slug });
      })
    ).subscribe({
      next: result => {
        this.log.info('GroupHub.loadGroup: next', { groupName: result.group.name, isAdmin: result.isAdmin });
        this.group = result.group;
        this.isAdmin = result.isAdmin;
      },
      error: err => {
        this.log.error('GroupHub.loadGroup: error', { message: String(err) });
        this.group = null;
        this.error = this.getErrorMessage(err, 'We could not load this group right now.');
      },
    });
  }

  private leaveGroup(): void {
    if (!this.group) {
      return;
    }

    this.leaving = true;
    this.groupService.leaveGroup(this.group.slug).pipe(
      finalize(() => {
        this.leaving = false;
      })
    ).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not leave the group right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
