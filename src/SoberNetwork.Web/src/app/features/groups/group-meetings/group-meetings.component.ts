import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { DAYS_OF_WEEK, MEETING_FORMATS, AdminMeetingResponse, CreateMeetingRequest, UpdateMeetingRequest } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';

@Component({
  selector: 'app-group-meetings',
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
  templateUrl: './group-meetings.component.html',
  styleUrl: './group-meetings.component.scss',
})
export class GroupMeetingsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly groupService = inject(GroupService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly daysOfWeek = DAYS_OF_WEEK;
  readonly meetingFormats = MEETING_FORMATS;

  slug = '';
  meetings: AdminMeetingResponse[] = [];
  loading = true;
  saving = false;
  error = '';
  formError = '';

  /** null = form closed; undefined = creating; string = editing by id */
  editingId: string | undefined | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    isRecurring: [true, { nonNullable: true }],
    dayOfWeek: [null as number | null],
    occursOn: [null as string | null],
    time: ['', [Validators.required, Validators.pattern(/^([01]\d|2[0-3]):[0-5]\d$/)]],
    durationMinutes: [60, [Validators.required, Validators.min(1), Validators.max(480)]],
    isOpen: [true, { nonNullable: true }],
    language: ['', [Validators.maxLength(100)]],
    formats: [[] as string[]],
    location: ['', [Validators.maxLength(300)]],
    zoomLink: ['', [Validators.maxLength(500)]],
    zoomMeetingId: ['', [Validators.maxLength(100)]],
    zoomPasscode: ['', [Validators.maxLength(100)]],
    notes: ['', [Validators.maxLength(2000)]],
    isActive: [true, { nonNullable: true }],
  });

  get isRecurring(): boolean {
    return this.form.controls.isRecurring.value === true;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadMeetings();
    });
  }

  openCreate(): void {
    this.editingId = undefined;
    this.formError = '';
    this.form.reset({
      name: '',
      description: '',
      isRecurring: true,
      dayOfWeek: null,
      occursOn: null,
      time: '',
      durationMinutes: 60,
      isOpen: true,
      language: '',
      formats: [] as string[],
      location: '',
      zoomLink: '',
      zoomMeetingId: '',
      zoomPasscode: '',
      notes: '',
      isActive: true,
    });
  }

  openEdit(meeting: AdminMeetingResponse): void {
    this.editingId = meeting.id;
    this.formError = '';
    this.form.reset({
      name: meeting.name,
      description: meeting.description ?? '',
      isRecurring: meeting.isRecurring,
      dayOfWeek: meeting.dayOfWeek ?? null,
      occursOn: meeting.occursOn ? new Date(meeting.occursOn).toISOString().substring(0, 10) : null,
      time: meeting.time,
      durationMinutes: meeting.durationMinutes,
      isOpen: meeting.isOpen,
      language: meeting.language ?? '',
      formats: meeting.formats ?? [],
      location: meeting.location ?? '',
      zoomLink: meeting.zoomLink ?? '',
      zoomMeetingId: meeting.zoomMeetingId ?? '',
      zoomPasscode: meeting.zoomPasscode ?? '',
      notes: meeting.notes ?? '',
      isActive: meeting.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId = null;
    this.formError = '';
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: CreateMeetingRequest | UpdateMeetingRequest = {
      name: value.name!.trim(),
      description: this.normalizeText(value.description),
      isRecurring: value.isRecurring === true,
      dayOfWeek: value.isRecurring ? (value.dayOfWeek ?? undefined) : undefined,
      occursOn: !value.isRecurring ? (value.occursOn ?? undefined) : undefined,
      time: value.time!,
      durationMinutes: value.durationMinutes ?? 60,
      isOpen: value.isOpen === true,
      language: this.normalizeText(value.language),
      formats: value.formats?.length ? value.formats : undefined,
      location: this.normalizeText(value.location),
      zoomLink: this.normalizeText(value.zoomLink),
      zoomMeetingId: this.normalizeText(value.zoomMeetingId),
      zoomPasscode: this.normalizeText(value.zoomPasscode),
      notes: this.normalizeText(value.notes),
      isActive: value.isActive === true,
    };

    this.saving = true;
    this.formError = '';

    const op$ = this.editingId
      ? this.groupService.updateMeeting(this.slug, this.editingId, request as UpdateMeetingRequest)
      : this.groupService.createMeeting(this.slug, request as CreateMeetingRequest);

    op$.pipe(
      finalize(() => {
        this.saving = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.editingId = null;
        this.loadMeetings();
      },
      error: err => {
        this.formError = this.getErrorMessage(err, 'We could not save the meeting right now.');
      },
    });
  }

  delete(meeting: AdminMeetingResponse): void {
    if (!confirm(`Delete "${meeting.name}"? This cannot be undone.`)) {
      return;
    }

    this.groupService.deleteMeeting(this.slug, meeting.id).subscribe({
      next: () => this.loadMeetings(),
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not delete this meeting right now.');
      },
    });
  }

  formatWhen(m: AdminMeetingResponse): string {
    const parts: string[] = [];
    if (m.isRecurring && m.dayOfWeek !== null && m.dayOfWeek !== undefined) {
      parts.push(this.daysOfWeek[m.dayOfWeek] ?? 'Scheduled');
    } else if (!m.isRecurring && m.occursOn) {
      parts.push(new Date(m.occursOn).toLocaleDateString());
    }
    if (m.time) {
      parts.push(m.time);
    }
    return parts.join(' • ') || 'Time not set';
  }

  private loadMeetings(): void {
    this.loading = true;
    this.error = '';

    this.groupService.getAdminMeetings(this.slug).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: meetings => {
        this.meetings = meetings;
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not load meetings right now.');
      },
    });
  }

  private normalizeText(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
