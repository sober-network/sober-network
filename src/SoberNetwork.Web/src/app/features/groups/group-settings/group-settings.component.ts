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
import { GroupCardComponent } from '../group-card/group-card.component';
import { GroupHeroComponent } from '../group-hero/group-hero.component';
import { GroupPageWrapperComponent } from '../group-page-wrapper/group-page-wrapper.component';
import { GroupFormModalComponent } from '../group-form-modal/group-form-modal.component';

const COMMON_TIME_ZONES = [
  'UTC',
  'America/New_York',
  'America/Chicago',
  'America/Denver',
  'America/Los_Angeles',
  'America/Phoenix',
  'America/Anchorage',
  'Pacific/Honolulu',
  'America/Toronto',
  'America/Vancouver',
  'America/Mexico_City',
  'America/Sao_Paulo',
  'Europe/London',
  'Europe/Dublin',
  'Europe/Paris',
  'Europe/Berlin',
  'Europe/Madrid',
  'Africa/Johannesburg',
  'Asia/Kolkata',
  'Asia/Singapore',
  'Asia/Tokyo',
  'Australia/Sydney',
  'Pacific/Auckland',
];

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
    GroupCardComponent,
    GroupHeroComponent,
    GroupPageWrapperComponent,
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

  readonly timeZones = COMMON_TIME_ZONES;

  slug = '';
  group: GroupResponse | null = null;
  loading = true;
  saving = false;
  deleting = false;
  error = '';
  saveSuccess = '';

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    timeZone: ['', [Validators.maxLength(100)]],
    isPublic: [true, { nonNullable: true }],
    requiresApproval: [true, { nonNullable: true }],
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadGroup();
    });
  }

  openSettingsModal(): void {
    this.dialog.open(GroupFormModalComponent, {
      panelClass: ['sn-modal-panel', 'sn-group-panel'],
      maxWidth: '100vw',
      data: {
        slug: this.slug,
      },
      autoFocus: 'first-tabbable',
    });
  }

  save(): void {
    if (this.form.invalid || !this.group) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving = true;
    this.error = '';
    this.saveSuccess = '';

    this.groupService.updateGroup(this.slug, {
      name: value.name!.trim(),
      description: this.normalizeOptionalText(value.description),
      timeZone: this.normalizeOptionalText(value.timeZone),
      isPublic: value.isPublic === true,
      requiresApproval: value.requiresApproval === true,
    }).pipe(
      finalize(() => {
        this.saving = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: group => {
        this.group = group;
        this.saveSuccess = 'Group settings saved.';
        this.applyGroupToForm(group);
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
        this.applyGroupToForm(group);
      },
      error: err => {
        this.group = null;
        this.error = this.getErrorMessage(err, 'We could not load this group right now.');
      },
    });
  }

  private applyGroupToForm(group: GroupResponse): void {
    this.form.reset({
      name: group.name,
      description: group.description ?? '',
      timeZone: group.timeZone ?? '',
      isPublic: group.isPublic,
      requiresApproval: group.requiresApproval,
    });
  }

  private deleteGroup(): void {
    this.deleting = true;
    this.groupService.deleteGroup(this.slug).pipe(
      finalize(() => {
        this.deleting = false;
        this.cdr.detectChanges();
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

  private normalizeOptionalText(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}


