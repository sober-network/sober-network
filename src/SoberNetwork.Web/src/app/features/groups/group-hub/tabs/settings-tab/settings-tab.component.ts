import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { finalize } from 'rxjs';
import { GroupResponse, US_STATES } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ConfirmDialogComponent } from '@app/shared/components/confirm-dialog/confirm-dialog.component';

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
  selector: 'app-settings-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
  ],
  templateUrl: './settings-tab.component.html',
  styleUrl: './settings-tab.component.scss',
})
export class SettingsTabComponent implements OnChanges {
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly timeZones = COMMON_TIME_ZONES;
  readonly stateOptions = US_STATES;

  @Input({ required: true }) slug!: string;
  @Input() group: GroupResponse | null = null;
  @Output() groupUpdated = new EventEmitter<GroupResponse>();
  @Output() groupDeleted = new EventEmitter<void>();

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
    districtName: ['', [Validators.maxLength(100)]],
    districtWebsiteUrl: ['', [Validators.maxLength(500)]],
    areaName: ['', [Validators.maxLength(100)]],
    areaWebsiteUrl: ['', [Validators.maxLength(500)]],
    state: ['', [Validators.maxLength(50)]],
    districtLatitude: [null as number | null],
    districtLongitude: [null as number | null],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['group']?.currentValue) {
      this.applyGroupToForm(changes['group'].currentValue as GroupResponse);
      this.cdr.markForCheck();
    }
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
    this.cdr.markForCheck();

    this.groupService.updateGroup(this.slug, {
      name: value.name?.trim(),
      description: this.normalizeOptionalText(value.description),
      timeZone: this.normalizeOptionalText(value.timeZone),
      isPublic: value.isPublic === true,
      requiresApproval: value.requiresApproval === true,
      districtName: this.normalizeOptionalText(value.districtName),
      districtWebsiteUrl: this.normalizeOptionalText(value.districtWebsiteUrl),
      areaName: this.normalizeOptionalText(value.areaName),
      areaWebsiteUrl: this.normalizeOptionalText(value.areaWebsiteUrl),
      state: this.normalizeOptionalText(value.state),
      districtLatitude: value.districtLatitude ?? undefined,
      districtLongitude: value.districtLongitude ?? undefined,
    }).pipe(
      finalize(() => {
        this.saving = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: updatedGroup => {
        this.group = updatedGroup;
        this.saveSuccess = 'Group settings saved.';
        this.applyGroupToForm(updatedGroup);
        this.groupUpdated.emit(updatedGroup);
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

    this.dialog.open(ConfirmDialogComponent, {
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
    }).afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.deleteGroup();
      }
    });
  }

  private applyGroupToForm(group: GroupResponse): void {
    this.form.reset({
      name: group.name,
      description: group.description ?? '',
      timeZone: group.timeZone ?? '',
      isPublic: group.isPublic,
      requiresApproval: group.requiresApproval,
      districtName: group.districtName ?? '',
      districtWebsiteUrl: group.districtWebsiteUrl ?? '',
      areaName: group.areaName ?? '',
      areaWebsiteUrl: group.areaWebsiteUrl ?? '',
      state: group.state ?? '',
      districtLatitude: group.districtLatitude ?? null,
      districtLongitude: group.districtLongitude ?? null,
    });
  }

  private deleteGroup(): void {
    this.deleting = true;
    this.error = '';
    this.cdr.markForCheck();

    this.groupService.deleteGroup(this.slug).pipe(
      finalize(() => {
        this.deleting = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: () => this.groupDeleted.emit(),
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
