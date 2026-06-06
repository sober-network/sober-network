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
import { DAYS_OF_WEEK, MEETING_FORMATS, LANGUAGES, US_STATES, COUNTRIES, MeetingType, AdminMeetingResponse, CreateMeetingRequest, UpdateMeetingRequest } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { Nl2brPipe } from '@app/shared/pipes/nl2br.pipe';

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
    Nl2brPipe,
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
  readonly languages = LANGUAGES;
  readonly usStates = US_STATES;
  readonly countries = COUNTRIES;
  readonly MeetingType = MeetingType;

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
    description: [''],
    meetingType: [MeetingType.Online as MeetingType],
    isRecurring: [true],
    daysOfWeek: [[] as number[]],
    occursOn: [null as string | null],
    time: ['', [Validators.required, Validators.pattern(/^([01]\d|2[0-3]):[0-5]\d$/)]],
    durationMinutes: [60, [Validators.required, Validators.min(1), Validators.max(480)]],
    isOpen: [true],
    language: [''],
    formats: [[] as string[]],
    venueName: [''],
    street: [''],
    street2: [''],
    city: [''],
    state: [''],
    postalCode: [''],
    country: ['United States' as string],
    zoomLink: [''],
    zoomMeetingId: [''],
    zoomPasscode: [''],
    publicJoinUrl: [''],
    notes: [''],
    isActive: [true],
  });

  get isRecurring(): boolean {
    return this.form.controls.isRecurring.value === true;
  }

  get meetingTypeValue(): MeetingType {
    return (this.form.controls.meetingType.value ?? MeetingType.Online) as MeetingType;
  }

  get isInPersonOrHybrid(): boolean {
    const type = this.form.controls.meetingType.value;
    return type === MeetingType.InPerson || type === MeetingType.Hybrid;
  }

  get readonly() {
    return {
      daysOfWeekLabels: DAYS_OF_WEEK,
      languages: LANGUAGES,
      usStates: US_STATES,
      countries: COUNTRIES,
    };
  }

  toggleDay(dayIndex: number): void {
    const current = this.form.controls.daysOfWeek.value ?? [];
    const updated = current.includes(dayIndex)
      ? current.filter(d => d !== dayIndex)
      : [...current, dayIndex];
    this.form.controls.daysOfWeek.setValue(updated);
  }

  isDaySelected(dayIndex: number): boolean {
    const current = this.form.controls.daysOfWeek.value ?? [];
    return current.includes(dayIndex);
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
      meetingType: MeetingType.Online,
      isRecurring: true,
      daysOfWeek: [] as number[],
      occursOn: null,
      time: '',
      durationMinutes: 60,
      isOpen: true,
      language: '',
      formats: [] as string[],
      venueName: '',
      street: '',
      street2: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'United States',
      zoomLink: '',
      zoomMeetingId: '',
      zoomPasscode: '',
      publicJoinUrl: '',
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
      meetingType: (meeting.meetingType ?? MeetingType.Online) as MeetingType,
      isRecurring: meeting.isRecurring,
      daysOfWeek: meeting.daysOfWeek ? Array.from(meeting.daysOfWeek) : [],
      occursOn: meeting.occursOn ? new Date(meeting.occursOn).toISOString().substring(0, 10) : null,
      time: meeting.time,
      durationMinutes: meeting.durationMinutes,
      isOpen: meeting.isOpen,
      language: meeting.language ?? '',
      formats: meeting.formats ?? [],
      venueName: meeting.venueName ?? '',
      street: meeting.street ?? '',
      street2: '',
      city: meeting.city ?? '',
      state: meeting.state ?? '',
      postalCode: meeting.postalCode ?? '',
      country: meeting.country ?? 'United States',
      zoomLink: meeting.zoomLink ?? '',
      zoomMeetingId: meeting.zoomMeetingId ?? '',
      zoomPasscode: meeting.zoomPasscode ?? '',
      publicJoinUrl: meeting.publicJoinUrl ?? '',
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
      daysOfWeek: value.isRecurring ? (value.daysOfWeek?.length ? value.daysOfWeek : undefined) : undefined,
      occursOn: !value.isRecurring ? (value.occursOn ?? undefined) : undefined,
      time: value.time!,
      durationMinutes: value.durationMinutes ?? 60,
      isOpen: value.isOpen === true,
      language: this.normalizeText(value.language),
      formats: value.formats?.length ? value.formats : undefined,
      meetingType: value.meetingType ?? MeetingType.Online,
      venueName: this.normalizeText(value.venueName),
      street: this.normalizeText(value.street),
      street2: this.normalizeText(value.street2),
      city: this.normalizeText(value.city),
      state: this.normalizeText(value.state),
      postalCode: this.normalizeText(value.postalCode),
      country: this.normalizeText(value.country),
      zoomLink: this.normalizeText(value.zoomLink),
      zoomMeetingId: this.normalizeText(value.zoomMeetingId),
      zoomPasscode: this.normalizeText(value.zoomPasscode),
      publicJoinUrl: this.normalizeText(value.publicJoinUrl),
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
    if (m.isRecurring && m.daysOfWeek && m.daysOfWeek.length > 0) {
      const dayNames = m.daysOfWeek.map(d => this.daysOfWeek[d] ?? 'Unknown').join(', ');
      parts.push(dayNames);
    } else if (!m.isRecurring && m.occursOn) {
      parts.push(new Date(m.occursOn).toLocaleDateString());
    }
    if (m.time) {
      parts.push(m.time);
    }
    return parts.join(' at ') || 'Time not set';
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
    const err = error as any;

    // Check for FluentValidation error structure (ProblemDetails with errors object)
    if (err?.error?.errors && typeof err.error.errors === 'object') {
      const messages: string[] = [];
      for (const [field, fieldErrors] of Object.entries(err.error.errors)) {
        if (Array.isArray(fieldErrors)) {
          messages.push(...(fieldErrors as string[]));
        }
      }
      if (messages.length > 0) {
        return messages.join('\n');
      }
    }

    // Check for single message field
    if (err?.error?.message) {
      return err.error.message;
    }

    // Check for title (ProblemDetails)
    if (err?.error?.title) {
      return err.error.title;
    }

    return fallback;
  }
}
