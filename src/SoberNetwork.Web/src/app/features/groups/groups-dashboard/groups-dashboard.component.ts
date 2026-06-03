import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { DAYS_OF_WEEK, GroupResponse, MEETING_FORMATS, MeetingFormat } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ClientLogService } from '@app/core/services/client-log.service';

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
  selector: 'app-groups-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
  ],
  templateUrl: './groups-dashboard.component.html',
  styleUrl: './groups-dashboard.component.scss',
})
export class GroupsDashboardComponent implements OnInit {
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly log = inject(ClientLogService);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild('createGroupDialog') private createGroupDialog?: TemplateRef<unknown>;

  readonly meetingFormats = MEETING_FORMATS;
  readonly daysOfWeek = DAYS_OF_WEEK;
  readonly timeZones = COMMON_TIME_ZONES;

  groups: GroupResponse[] = [];
  loading = true;
  creating = false;
  error = '';
  createError = '';

  private slugEdited = false;

  readonly createForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    slug: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/)]],
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

  constructor() {
    this.createForm.controls.name.valueChanges.subscribe(name => {
      if (!this.slugEdited) {
        this.createForm.controls.slug.setValue(this.slugify(name ?? ''), { emitEvent: false });
      }
    });
  }

  ngOnInit(): void {
    this.log.info('GroupsDashboard.ngOnInit');
    this.loadGroups();
  }

  openCreateGroupDialog(): void {
    if (!this.createGroupDialog) {
      return;
    }

    this.slugEdited = false;
    this.createError = '';
    this.createForm.reset({
      name: '',
      slug: '',
      description: '',
      meetingSchedule: '',
      meetingDay: null,
      meetingTime: '',
      durationMinutes: 60,
      isOpen: true,
      language: '',
      zoomLink: '',
      zoomMeetingId: '',
      zoomPasscode: '',
      timeZone: '',
      isPublic: true,
      requiresApproval: true,
    });
    this.resetMeetingFormats();

    this.dialog.open(this.createGroupDialog, {
      width: '720px',
      maxWidth: '95vw',
    });
  }

  markSlugEdited(): void {
    this.slugEdited = true;
  }

  submitCreate(dialogRef: MatDialogRef<unknown>): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const value = this.createForm.getRawValue();
    this.creating = true;
    this.createError = '';

    this.groupService.createGroup({
      name: value.name!.trim(),
      slug: value.slug!.trim(),
      description: this.normalizeOptionalText(value.description),
      meetingSchedule: this.normalizeOptionalText(value.meetingSchedule),
      meetingDay: value.meetingDay ?? undefined,
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
        this.creating = false;
      })
    ).subscribe({
      next: () => {
        dialogRef.close();
        this.loadGroups();
      },
      error: err => {
        this.createError = this.getErrorMessage(err, 'We could not create your group just yet. Please try again.');
      },
    });
  }

  trackBySlug(_: number, group: GroupResponse): string {
    return group.slug;
  }

  descriptionPreview(group: GroupResponse): string {
    return group.description || 'A welcoming place to stay connected between meetings.';
  }

  meetingSummary(group: GroupResponse): string | null {
    const parts: string[] = [];

    if (group.meetingDay !== null && group.meetingDay !== undefined) {
      parts.push(this.daysOfWeek[group.meetingDay] ?? 'Scheduled meeting');
    }

    if (group.meetingTime) {
      parts.push(group.meetingTime);
    }

    if (group.durationMinutes > 0) {
      parts.push(`${group.durationMinutes} min`);
    }

    return parts.length > 0 ? parts.join(' • ') : null;
  }

  private loadGroups(): void {
    this.loading = true;
    this.error = '';
    this.log.info('GroupsDashboard.loadGroups: subscribing');

    this.groupService.getMyGroups().pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
        this.log.info('GroupsDashboard.loadGroups: finalize', { loading: 'false' });
      })
    ).subscribe({
      next: groups => {
        this.log.info('GroupsDashboard.loadGroups: next', { count: String(groups.length) });
        this.groups = groups;
      },
      error: err => {
        this.log.error('GroupsDashboard.loadGroups: error', { message: String(err) });
        this.groups = [];
        this.error = this.getErrorMessage(err, 'We could not load your groups right now.');
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

  private resetMeetingFormats(): void {
    for (const format of this.meetingFormats) {
      this.createForm.controls.meetingFormats.controls[format].setValue(false);
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
