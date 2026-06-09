import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { finalize } from 'rxjs';
import { US_STATES } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';

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
  selector: 'app-create-group-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    BaseFormModalComponent,
  ],
  templateUrl: './create-group-modal.component.html',
  styleUrl: './create-group-modal.component.scss',
})
export class CreateGroupModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly groupService = inject(GroupService);
  readonly dialogRef = inject(MatDialogRef<CreateGroupModalComponent>);

  readonly title = 'Create a New Group';
  readonly subtitle = 'Start a welcoming home for your people.';
  readonly icon = 'group_add';
  readonly timeZones = COMMON_TIME_ZONES;
  readonly stateOptions = US_STATES;

  creating = false;
  error = '';
  private slugEdited = false;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    slug: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/)]],
    description: ['', [Validators.maxLength(500)]],
    timeZone: ['', [Validators.maxLength(100)]],
    isPublic: [true, { nonNullable: true }],
    requiresApproval: [true, { nonNullable: true }],
    state: ['', [Validators.required, Validators.maxLength(50)]],
    districtName: ['', [Validators.required, Validators.maxLength(100)]],
    districtWebsiteUrl: ['', [Validators.maxLength(500)]],
    areaName: ['', [Validators.required, Validators.maxLength(100)]],
    areaWebsiteUrl: ['', [Validators.maxLength(500)]],
    districtLatitude: [null as number | null, [Validators.required, Validators.min(-90), Validators.max(90)]],
    districtLongitude: [null as number | null, [Validators.required, Validators.min(-180), Validators.max(180)]],
  });

  constructor() {
    this.form.controls.name.valueChanges.subscribe(name => {
      if (!this.slugEdited) {
        this.form.controls.slug.setValue(this.slugify(name ?? ''), { emitEvent: false });
      }
    });
  }

  markSlugEdited(): void {
    this.slugEdited = true;
  }

  create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.creating = true;
    this.error = '';

    this.groupService.createGroup({
      name: value.name!.trim(),
      slug: value.slug!.trim(),
      description: this.normalizeOptionalText(value.description),
      timeZone: this.normalizeOptionalText(value.timeZone),
      isPublic: value.isPublic === true,
      requiresApproval: value.requiresApproval === true,
      state: value.state!.trim(),
      districtName: value.districtName!.trim(),
      districtWebsiteUrl: this.normalizeOptionalText(value.districtWebsiteUrl) ?? null,
      areaName: value.areaName!.trim(),
      areaWebsiteUrl: this.normalizeOptionalText(value.areaWebsiteUrl) ?? null,
      districtLatitude: Number(value.districtLatitude),
      districtLongitude: Number(value.districtLongitude),
    }).pipe(
      finalize(() => {
        this.creating = false;
      }),
    ).subscribe({
      next: () => {
        this.dialogRef.close(true);
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not create your group just yet. Please try again.');
      },
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  private normalizeOptionalText(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  private slugify(value: string): string {
    return value
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .replace(/-{2,}/g, '-');
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
