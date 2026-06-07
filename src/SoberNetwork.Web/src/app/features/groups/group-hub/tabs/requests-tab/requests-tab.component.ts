import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { JoinRequestResponse } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-requests-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './requests-tab.component.html',
  styleUrl: './requests-tab.component.scss',
})
export class RequestsTabComponent implements OnInit, OnChanges {
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) slug!: string;

  requests: JoinRequestResponse[] = [];
  loading = true;
  error = '';
  actionUserId: string | null = null;
  totalCount = 0;
  pageIndex = 0;
  pageSize = 10;

  ngOnInit(): void {
    this.loadRequests();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['slug'] && !changes['slug'].firstChange) {
      this.pageIndex = 0;
      this.loadRequests();
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadRequests();
  }

  approve(request: JoinRequestResponse): void {
    this.actionUserId = request.userId;
    this.cdr.markForCheck();

    this.groupService.approveOrReject(this.slug, request.userId, { approved: true }).pipe(
      finalize(() => {
        this.actionUserId = null;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: () => this.loadRequests(),
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not approve that request right now.');
      },
    });
  }

  confirmReject(request: JoinRequestResponse): void {
    this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '440px',
      data: {
        title: 'Reject request?',
        message: `Are you sure you want to decline ${request.displayName}'s request to join this group?`,
        confirmLabel: 'Reject request',
        dangerous: true,
      },
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.actionUserId = request.userId;
      this.cdr.markForCheck();

      this.groupService.approveOrReject(this.slug, request.userId, { approved: false }).pipe(
        finalize(() => {
          this.actionUserId = null;
          this.cdr.markForCheck();
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
    if (!this.slug) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();

    this.groupService.getJoinRequests(this.slug, this.pageIndex + 1, this.pageSize).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: response => {
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
