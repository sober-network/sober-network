import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { GroupService } from '@app/core/services/group.service';
import { GroupResponse } from '@app/core/models';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';

const COMMON_TIME_ZONES = [
  'UTC', 'America/New_York', 'America/Chicago', 'America/Denver', 'America/Los_Angeles',
  'America/Phoenix', 'America/Anchorage', 'Pacific/Honolulu', 'America/Toronto', 'America/Vancouver',
  'America/Mexico_City', 'America/Sao_Paulo', 'Europe/London', 'Europe/Dublin', 'Europe/Paris',
  'Europe/Berlin', 'Europe/Madrid', 'Africa/Johannesburg', 'Asia/Kolkata', 'Asia/Singapore',
  'Asia/Tokyo', 'Australia/Sydney', 'Pacific/Auckland',
];

@Component({
  selector: 'app-group-form-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    BaseFormModalComponent,
  ],
  templateUrl: './group-form-modal.component.html',
  styleUrl: './group-form-modal.component.scss',
})
export class GroupFormModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly groupService = inject(GroupService);
  private readonly data = inject(MAT_DIALOG_DATA) as { slug: string };
  readonly dialogRef = inject(MatDialogRef<GroupFormModalComponent>);

  readonly timeZones = COMMON_TIME_ZONES;

  title = 'Edit Group Settings';
  subtitle = 'Update the group name, description, and other settings.';
  icon = 'settings';

  slug: string = '';
  group: GroupResponse | null = null;
  loading = true;
  saving = false;
  error = '';

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    timeZone: ['', [Validators.maxLength(100)]],
    isPublic: [true, { nonNullable: true }],
    requiresApproval: [true, { nonNullable: true }],
  });

  ngOnInit(): void {
    this.slug = this.data?.slug || '';
    this.loadGroup();
  }

  private loadGroup(): void {
    this.groupService
      .getGroup(this.slug)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (group) => {
          this.group = group;
          this.form.patchValue({
            name: group.name,
            description: group.description,
            timeZone: group.timeZone,
            isPublic: group.isPublic,
            requiresApproval: group.requiresApproval,
          });
        },
        error: (err) => {
          this.error = 'Failed to load group settings.';
        },
      });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';

    const formValue = this.form.getRawValue();
    const payload: any = {
      name: formValue.name,
      description: formValue.description,
      timeZone: formValue.timeZone,
      isPublic: formValue.isPublic,
      requiresApproval: formValue.requiresApproval,
    };

    this.groupService
      .updateGroup(this.slug, payload)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Failed to save group settings.';
        },
      });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
