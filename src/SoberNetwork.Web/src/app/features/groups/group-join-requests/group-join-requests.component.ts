import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { JoinRequestResponse } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';
import { GroupCardComponent } from '../group-card/group-card.component';
import { GroupHeroComponent } from '../group-hero/group-hero.component';
import { GroupPageWrapperComponent } from '../group-page-wrapper/group-page-wrapper.component';

@Component({
  selector: 'app-group-join-requests',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    GroupCardComponent,
    GroupHeroComponent,
    GroupPageWrapperComponent,
  ],
  templateUrl: './group-join-requests.component.html',
  styleUrl: './group-join-requests.component.scss',
})
export class GroupJoinRequestsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly cdr = inject(ChangeDetectorRef);

  slug = '';
  groupName = '';
  requests: JoinRequestResponse[] = [];
  loading = true;
  error = '';
  actionUserId: string | null = null;
  totalCount = 0;
  pageIndex = 0;
  pageSize = 10;

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.pageIndex = 0;
      this.loadRequests();
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadRequests();
  }

  approve(request: JoinRequestResponse): void {
    this.actionUserId = request.userId;
    this.groupService.approveOrReject(this.slug, request.userId, { approved: true }).pipe(
      finalize(() => {
        this.actionUserId = null;
      })
    ).subscribe({
      next: () => this.loadRequests(),
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not approve that request right now.');
      },
    });
  }

  confirmReject(request: JoinRequestResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '440px',
      data: {
        title: 'Reject request?',
        message: `Are you sure you want to decline ${request.displayName}'s request to join ${this.groupName}?`,
        confirmLabel: 'Reject request',
        dangerous: true,
      },
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.actionUserId = request.userId;
      this.groupService.approveOrReject(this.slug, request.userId, { approved: false }).pipe(
        finalize(() => {
          this.actionUserId = null;
        })
      ).subscribe({
        next: () => this.loadRequests(),
        error: err => {
          this.error = this.getErrorMessage(err, 'We could not reject that request right now.');
        },
      });
    });
  }

  trackByUserId(_: number, request: JoinRequestResponse): string {
    return request.userId;
  }

  private loadRequests(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      group: this.groupService.getGroup(this.slug),
      response: this.groupService.getJoinRequests(this.slug, this.pageIndex + 1, this.pageSize),
    }).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: ({ group, response }) => {
        this.groupName = group.name;
        this.requests = response.items;
        this.totalCount = response.totalCount;
      },
      error: err => {
        this.requests = [];
        this.totalCount = 0;
        this.error = this.getErrorMessage(err, 'We could not load pending requests right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
