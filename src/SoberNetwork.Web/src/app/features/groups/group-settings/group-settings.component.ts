import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { DAYS_OF_WEEK, GroupResponse, MEETING_FORMATS, MeetingFormat } from '@app/core/models';
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
  selector: 'app-group-settings',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
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

  readonly meetingFormats = MEETING_FORMATS;
  readonly daysOfWeek = DAYS_OF_WEEK;
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
    meetingSchedule: ['', [Validators.maxLength(1000)]],
    meetingDay: [null as number | null],
    meetingTime: ['', [Validators.maxLength(5)]],
    durationMinutes: [60, [Validators.required, Validators.min(1)]],
    isOpen: [true, { nonNullable: true }],
    language: ['', [Validators.maxLength(100)]],
    meetingFormats: this.createMeetingFormatControls(),
    zoomLink: ['', [Validators.maxLength(500)]],
    zoomMeetingId: ['', [Validators.maxLength(100)]],
    zoomPasscode: ['', [Validators.maxLength(100)]],
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
      meetingSchedule: this.normalizeOptionalText(value.meetingSchedule),
      meetingDay: value.meetingDay ?? null,
      meetingTime: this.normalizeOptionalText(value.meetingTime),
      durationMinutes: value.durationMinutes ?? 60,
      isOpen: value.isOpen === true,
      language: this.normalizeOptionalText(value.language),
      meetingFormats: this.serializeMeetingFormats(value.meetingFormats),
      zoomLink: this.normalizeOptionalText(value.zoomLink),
      zoomMeetingId: this.normalizeOptionalText(value.zoomMeetingId),
      zoomPasscode: this.normalizeOptionalText(value.zoomPasscode),
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
      meetingSchedule: group.meetingSchedule ?? '',
      meetingDay: group.meetingDay,
      meetingTime: group.meetingTime ?? '',
      durationMinutes: group.durationMinutes,
      isOpen: group.isOpen,
      language: group.language ?? '',
      zoomLink: group.zoomLink ?? '',
      zoomMeetingId: group.zoomMeetingId ?? '',
      zoomPasscode: group.zoomPasscode ?? '',
      timeZone: group.timeZone ?? '',
      isPublic: group.isPublic,
      requiresApproval: group.requiresApproval,
    });
    this.setMeetingFormats(group.meetingFormats);
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

  private createMeetingFormatControls() {
    return this.fb.record(
      MEETING_FORMATS.reduce((controls, format) => {
        controls[format] = new FormControl(false, { nonNullable: true });
        return controls;
      }, {} as Record<MeetingFormat, FormControl<boolean>>)
    );
  }

  private setMeetingFormats(value: string | null): void {
    const selected = new Set((value ?? '').split(',').map(item => item.trim()).filter(Boolean));
    for (const format of this.meetingFormats) {
      this.form.controls.meetingFormats.controls[format].setValue(selected.has(format));
    }
  }

  private serializeMeetingFormats(value: Record<string, boolean | null | undefined> | null | undefined): string | undefined {
    const selected = Object.entries(value ?? {})
      .filter(([, isSelected]) => isSelected)
      .map(([format]) => format);

    return selected.length > 0 ? selected.join(',') : undefined;
  }

  private normalizeOptionalText(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
