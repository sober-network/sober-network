import { Component, OnInit, Inject, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { GroupService } from '@app/core/services/group.service';
import { MeetingType, CreateMeetingRequest, UpdateMeetingRequest } from '@app/core/models/group.models';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';

export interface MeetingFormModalData {
  slug: string;
  meetingId?: string;
  meeting?: any;
  readonly: any;
}

@Component({
  selector: 'app-meeting-form-modal',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule,
    MatSlideToggleModule, MatCheckboxModule, MatProgressSpinnerModule, BaseFormModalComponent,
  ],
  templateUrl: './meeting-form-modal.component.html',
  styleUrl: './meeting-form-modal.component.scss',
})
export class MeetingFormModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly groupService = inject(GroupService);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly dialogRef = inject(MatDialogRef<MeetingFormModalComponent>);
  private readonly data = inject(MAT_DIALOG_DATA) as MeetingFormModalData;

  form!: FormGroup;
  saving = false;
  formError = '';
  slug: string = '';
  editingId: string | undefined;
  meeting: any;
  readonly: any;

  title = 'Create Meeting';
  subtitle = 'Set up meeting details, schedule, and location.';
  icon = 'event';

  readonly MeetingType = MeetingType;

  ngOnInit(): void {
    this.slug = this.data.slug;
    this.editingId = this.data.meetingId;
    this.meeting = this.data.meeting;
    this.readonly = this.data.readonly;
    if (this.editingId) {
      this.title = 'Edit Meeting';
    }
    this.initForm();
  }

  private initForm(): void {
    const isRecurring = this.meeting?.isRecurring ?? true;
    this.form = this.fb.group({
      name: new FormControl<string>(this.meeting?.name ?? '', { validators: [Validators.required, Validators.minLength(2)], nonNullable: true }),
      description: new FormControl<string>(this.meeting?.description ?? '', { nonNullable: true }),
      isRecurring: new FormControl<boolean>(isRecurring, { nonNullable: true }),
      daysOfWeek: new FormControl<number[]>(this.meeting?.daysOfWeek ?? [], { nonNullable: true }),
      occursOn: new FormControl<string | null>(this.meeting?.occursOn ?? null),
      time: new FormControl<string>(this.meeting?.time ?? '', { validators: Validators.required, nonNullable: true }),
      durationMinutes: new FormControl<number>(this.meeting?.durationMinutes ?? 60, { nonNullable: true }),
      isOpen: new FormControl<boolean>(this.meeting?.isOpen ?? true, { nonNullable: true }),
      language: new FormControl<string>(this.meeting?.language ?? '', { nonNullable: true }),
      formats: new FormControl<string[]>(this.meeting?.formats ?? [], { nonNullable: true }),
      meetingType: new FormControl<MeetingType>(this.meeting?.meetingType ?? MeetingType.Online, { nonNullable: true }),
      venueName: new FormControl<string>(this.meeting?.venueName ?? '', { nonNullable: true }),
      street: new FormControl<string>(this.meeting?.street ?? '', { nonNullable: true }),
      street2: new FormControl<string>(this.meeting?.street2 ?? '', { nonNullable: true }),
      city: new FormControl<string>(this.meeting?.city ?? '', { nonNullable: true }),
      state: new FormControl<string>(this.meeting?.state ?? '', { nonNullable: true }),
      postalCode: new FormControl<string>(this.meeting?.postalCode ?? '', { nonNullable: true }),
      country: new FormControl<string>(this.meeting?.country ?? '', { nonNullable: true }),
      publicJoinUrl: new FormControl<string>(this.meeting?.publicJoinUrl ?? '', { nonNullable: true }),
      zoomLink: new FormControl<string>(this.meeting?.zoomLink ?? '', { nonNullable: true }),
      zoomMeetingId: new FormControl<string>(this.meeting?.zoomMeetingId ?? '', { nonNullable: true }),
      zoomPasscode: new FormControl<string>(this.meeting?.zoomPasscode ?? '', { nonNullable: true }),
      notes: new FormControl<string>(this.meeting?.notes ?? '', { nonNullable: true }),
      isActive: new FormControl<boolean>(this.meeting?.isActive ?? true, { nonNullable: true }),
    });
  }

  get isRecurring(): boolean {
    return this.form.get('isRecurring')?.value === true;
  }

  get isInPersonOrHybrid(): boolean {
    const type = this.form.get('meetingType')?.value;
    return type === MeetingType.InPerson || type === MeetingType.Hybrid;
  }

  toggleDay(index: number): void {
    const daysControl = this.form.get('daysOfWeek');
    if (!daysControl) return;
    const days = daysControl.value as number[];
    const newDays = [...days];
    if (newDays.includes(index)) {
      newDays.splice(newDays.indexOf(index), 1);
    } else {
      newDays.push(index);
    }
    daysControl.setValue(newDays);
  }

  isDaySelected(index: number): boolean {
    const days = this.form.get('daysOfWeek')?.value as number[];
    return days && days.includes(index);
  }

  updateFormat(format: string): void {
    const formatsControl = this.form.get('formats');
    if (!formatsControl) return;
    const formats = formatsControl.value as string[];
    const newFormats = [...formats];
    if (newFormats.includes(format)) {
      newFormats.splice(newFormats.indexOf(format), 1);
    } else {
      newFormats.push(format);
    }
    formatsControl.setValue(newFormats);
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
      publicJoinUrl: this.normalizeText(value.publicJoinUrl),
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

    op$.pipe(finalize(() => { this.saving = false; this.cdr.detectChanges(); }))
      .subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (err) => {
          this.formError = this.getErrorMessage(err, 'Failed to save meeting.');
        },
      });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  private normalizeText(value: string | null | undefined): string | undefined {
    return value?.trim() || undefined;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    const err = error as any;
    if (err?.error?.errors && typeof err.error.errors === 'object') {
      const messages: string[] = [];
      for (const [, fieldErrors] of Object.entries(err.error.errors)) {
        if (Array.isArray(fieldErrors)) {
          messages.push(...(fieldErrors as string[]));
        }
      }
      if (messages.length > 0) {
        return messages.join('\n');
      }
    }
    if (err?.error?.detail) {
      return err.error.detail;
    }
    return fallback;
  }
}
