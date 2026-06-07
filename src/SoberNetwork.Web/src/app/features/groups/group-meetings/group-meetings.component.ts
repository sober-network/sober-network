import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { DAYS_OF_WEEK, MEETING_FORMATS, LANGUAGES, US_STATES, COUNTRIES, MeetingType, AdminMeetingResponse, formatMeetingFormat } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { GroupCardComponent } from '../group-card/group-card.component';
import { GroupHeroComponent } from '../group-hero/group-hero.component';
import { GroupPageWrapperComponent } from '../group-page-wrapper/group-page-wrapper.component';
import { MeetingFormModalComponent } from '../meeting-form-modal/meeting-form-modal.component';

@Component({
  selector: 'app-group-meetings',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    GroupCardComponent,
    GroupHeroComponent,
    GroupPageWrapperComponent,
  ],
  templateUrl: './group-meetings.component.html',
  styleUrl: './group-meetings.component.scss',
})
export class GroupMeetingsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly groupService = inject(GroupService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private readonly sanitizer = inject(DomSanitizer);

  readonly daysOfWeek = DAYS_OF_WEEK;
  readonly meetingFormats = MEETING_FORMATS;
  readonly formatMeetingLabel = formatMeetingFormat;
  readonly languages = LANGUAGES;
  readonly usStates = US_STATES;
  readonly countries = COUNTRIES;
  readonly MeetingType = MeetingType;

  slug = '';
  groupName = '';
  meetings: AdminMeetingResponse[] = [];
  loading = true;
  error = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadMeetings();
    });
  }

  openCreate(): void {
    const readonly = {
      daysOfWeekLabels: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
      languages: LANGUAGES,
      states: US_STATES,
      countries: COUNTRIES,
    };

    this.dialog.open(MeetingFormModalComponent, {
      maxWidth: '100vw',
      data: {
        slug: this.slug,
        readonly,
      },
      panelClass: ['sn-modal-panel', 'sn-meeting-panel'],
    }).afterClosed().subscribe(result => {
      if (result === true) {
        this.loadMeetings();
      }
    });
  }

  openEdit(meeting: AdminMeetingResponse): void {
    const readonly = {
      daysOfWeekLabels: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
      languages: LANGUAGES,
      states: US_STATES,
      countries: COUNTRIES,
    };

    const occursOn = meeting.occursOn ? new Date(meeting.occursOn).toISOString().substring(0, 10) : null;

    this.dialog.open(MeetingFormModalComponent, {
      maxWidth: '100vw',
      data: {
        slug: this.slug,
        meetingId: meeting.id,
        meeting: {
          ...meeting,
          occursOn,
        },
        readonly,
      },
      panelClass: ['sn-modal-panel', 'sn-meeting-panel'],
    }).afterClosed().subscribe(result => {
      if (result === true) {
        this.loadMeetings();
      }
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

  meetingTypeLabel(type: MeetingType): string {
    return type === MeetingType.InPerson ? 'In-Person' : type === MeetingType.Online ? 'Online' : 'Hybrid';
  }

  meetingIcon(meeting: AdminMeetingResponse): string {
    if (meeting.meetingType === MeetingType.InPerson) return 'location_on';
    if (meeting.meetingType === MeetingType.Online) return 'video_call';
    return 'hub';
  }

  cardAccentClass(meeting: AdminMeetingResponse): string {
    if (meeting.meetingType === MeetingType.InPerson) return 'accent-green';
    if (meeting.meetingType === MeetingType.Online) return 'accent-blue';
    return 'accent-amber';
  }

  hasPhysicalLocation(meeting: AdminMeetingResponse): boolean {
    return Boolean(meeting.venueName || meeting.street || meeting.city || meeting.state || meeting.postalCode || meeting.country || meeting.location);
  }

  hasZoomDetails(meeting: AdminMeetingResponse): boolean {
    return Boolean(meeting.publicJoinUrl || meeting.zoomLink || meeting.zoomMeetingId || meeting.zoomPasscode);
  }

  mapPreviewUrl(meeting: AdminMeetingResponse): SafeResourceUrl | null {
    const query = this.mapQuery(meeting);
    if (!query) {
      return null;
    }

    return this.sanitizer.bypassSecurityTrustResourceUrl(
      `https://www.google.com/maps?q=${encodeURIComponent(query)}&z=15&output=embed`
    );
  }

  mapPreviewCaption(meeting: AdminMeetingResponse): string {
    return [meeting.venueName, this.addressLine(meeting)].filter(Boolean).join(' • ') || meeting.location || 'Map preview';
  }

  private mapQuery(meeting: AdminMeetingResponse): string | null {
    if (meeting.latitude != null && meeting.longitude != null) {
      return `${meeting.latitude.toFixed(6)},${meeting.longitude.toFixed(6)}`;
    }

    return [meeting.venueName, meeting.street, meeting.city, meeting.state, meeting.postalCode, meeting.country, meeting.location]
      .filter(Boolean)
      .join(', ') || null;
  }

  addressLine(meeting: AdminMeetingResponse): string {
    return [meeting.city, meeting.state, meeting.postalCode].filter(Boolean).join(' ');
  }

  directionsUrl(meeting: AdminMeetingResponse): string {
    const addr = [meeting.venueName, meeting.street, meeting.city, meeting.state, meeting.postalCode, meeting.country, meeting.location]
      .filter(Boolean)
      .join(', ');
    return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(addr)}`;
  }

  private loadMeetings(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      group: this.groupService.getGroup(this.slug),
      meetings: this.groupService.getAdminMeetings(this.slug),
    }).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: ({ group, meetings }) => {
        this.groupName = group.name;
        this.meetings = meetings.items;
      },
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not load meetings right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    const err = error as any;

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

    if (err?.error?.message) {
      return err.error.message;
    }

    if (err?.error?.title) {
      return err.error.title;
    }

    return fallback;
  }
}
