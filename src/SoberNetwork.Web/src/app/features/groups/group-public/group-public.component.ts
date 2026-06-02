import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GroupResponse } from '@app/core/models';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';

@Component({
  selector: 'app-group-public',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './group-public.component.html',
  styleUrl: './group-public.component.scss',
})
export class GroupPublicComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);

  @ViewChild('joinDialog') private joinDialog?: TemplateRef<unknown>;

  slug = '';
  group: GroupResponse | null = null;
  loading = true;
  joining = false;
  notFound = false;
  error = '';
  joinError = '';
  joinRequested = false;

  readonly joinForm = this.fb.group({
    message: ['', [Validators.maxLength(500)]],
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadGroup();
    });
  }

  openJoinDialog(): void {
    if (!this.joinDialog) {
      return;
    }

    this.joinError = '';
    this.joinForm.reset({ message: '' });

    this.dialog.open(this.joinDialog, {
      width: '520px',
      maxWidth: '95vw',
    });
  }

  submitJoin(dialogRef: MatDialogRef<unknown>): void {
    const message = this.joinForm.controls.message.value?.trim();
    this.joining = true;
    this.joinError = '';

    this.groupService.joinGroup(this.slug, { message: message || undefined }).pipe(
      finalize(() => {
        this.joining = false;
      })
    ).subscribe({
      next: () => {
        this.joinRequested = true;
        dialogRef.close();
      },
      error: err => {
        this.joinError = this.getErrorMessage(err, 'We could not send your request just now. Please try again.');
      },
    });
  }

  private loadGroup(): void {
    this.loading = true;
    this.notFound = false;
    this.error = '';
    this.group = null;
    this.joinRequested = false;

    this.groupService.getGroup(this.slug).pipe(
      finalize(() => {
        this.loading = false;
      })
    ).subscribe({
      next: group => {
        this.group = group;
      },
      error: err => {
        if ((err as HttpErrorResponse).status === 404) {
          this.notFound = true;
          return;
        }

        this.error = this.getErrorMessage(err, 'We could not load this group right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
