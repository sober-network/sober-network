import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { GroupResponse } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-group-settings',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
  ],
  templateUrl: './group-settings.component.html',
  styleUrl: './group-settings.component.scss',
})
export class GroupSettingsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  slug = '';
  group: GroupResponse | null = null;
  loading = true;
  saving = false;
  deleting = false;
  error = '';
  saveSuccess = '';

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    description: ['', [Validators.maxLength(500)]],
    isPublic: [true, { nonNullable: true }],
    requiresApproval: [true, { nonNullable: true }],
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadGroup();
    });
  }

  save(): void {
    if (this.form.invalid || !this.group) {
      this.form.markAllAsTouched();
      return;
    }

    const { name, description, isPublic, requiresApproval } = this.form.getRawValue();
    const isGroupPublic = isPublic === true;
    const approvalRequired = requiresApproval === true;
    this.saving = true;
    this.error = '';
    this.saveSuccess = '';

    this.groupService.updateGroup(this.slug, {
      name: name!.trim(),
      description: description?.trim() || undefined,
      isPublic: isGroupPublic,
      requiresApproval: approvalRequired,
    }).pipe(
      finalize(() => {
        this.saving = false;
      })
    ).subscribe({
      next: group => {
        this.group = group;
        this.saveSuccess = 'Group settings saved.';
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not save your changes right now.');
      },
    });
  }

  confirmDelete(): void {
    if (!this.group || this.deleting) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '480px',
      maxWidth: '95vw',
      data: {
        title: 'Delete group?',
        message: `This will permanently remove ${this.group.name}. Type the group name to confirm.`,
        confirmLabel: 'Delete group',
        dangerous: true,
        confirmationText: this.group.name,
        confirmationPlaceholder: 'Type the group name',
      },
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.deleteGroup();
    });
  }

  private loadGroup(): void {
    this.loading = true;
    this.error = '';
    this.saveSuccess = '';

    this.groupService.getGroup(this.slug).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: group => {
        this.group = group;
        this.form.reset({
          name: group.name,
          description: group.description ?? '',
          isPublic: group.isPublic,
          requiresApproval: group.requiresApproval,
        });
      },
      error: err => {
        this.group = null;
        this.error = this.getErrorMessage(err, 'We could not load this group right now.');
      },
    });
  }

  private deleteGroup(): void {
    this.deleting = true;
    this.groupService.deleteGroup(this.slug).pipe(
      finalize(() => {
        this.deleting = false;
      })
    ).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not delete this group right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
