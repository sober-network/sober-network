import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { finalize } from 'rxjs';
import {
  AdminMeetingResponse,
  COUNTRIES,
  DAYS_OF_WEEK,
  LANGUAGES,
  MeetingResponse,
  MeetingSortBy,
  MeetingType,
  US_STATES,
  formatMeetingFormat,
} from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { MeetingFormModalComponent } from '../../../meeting-form-modal/meeting-form-modal.component';
import { MiniMapComponent } from '@app/shared/components/mini-map/mini-map.component';

type HubMeeting = AdminMeetingResponse | MeetingResponse;

@Component({
  selector: 'app-meetings-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MiniMapComponent],
  templateUrl: './meetings-tab.component.html',
  styleUrl: './meetings-tab.component.scss',
})
export class MeetingsTabComponent implements OnInit, OnChanges {
  private readonly groupService = inject(GroupService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private readonly sanitizer = inject(DomSanitizer);

  readonly daysOfWeek = DAYS_OF_WEEK;
  readonly formatMeetingLabel = formatMeetingFormat;

  @Input({ required: true }) slug!: string;
  @Input() isAdmin = false;
  @Input() reloadToken = 0;
  @Input() selectedMeetingId: string | null = null;

  allMeetings: HubMeeting[] = [];
  filteredMeetings: HubMeeting[] = [];
  loading = true;
  error = '';

  searchQuery = '';
  sortBy: MeetingSortBy = 'Time';

  readonly sortOptions: MeetingSortBy[] = ['Time', 'Name', 'Type'];

  ngOnInit(): void {
    this.loadMeetings();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['slug'] && !changes['slug'].firstChange) || (changes['reloadToken'] && !changes['reloadToken'].firstChange)) {
      this.loadMeetings();
    }

    if (changes['selectedMeetingId'] && !changes['selectedMeetingId'].firstChange) {
      this.scrollToSelectedMeeting();
    }
  }

  onSearch(): void {
    this.applyFilter();
    this.cdr.markForCheck();
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.applyFilter();
    this.cdr.markForCheck();
  }

  onSortChange(sortBy: MeetingSortBy): void {
    this.sortBy = sortBy;
    this.loadMeetings();
  }

  openCreate(): void {
    if (!this.isAdmin) {
      return;
    }

    const readonly = {
      daysOfWeekLabels: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
      languages: LANGUAGES,
      states: US_STATES,
      countries: COUNTRIES,
    };

    this.dialog.open(MeetingFormModalComponent, {
      maxWidth: '100vw',
      data: { slug: this.slug, readonly },
      panelClass: ['sn-modal-panel', 'sn-meeting-panel'],
    }).afterClosed().subscribe(result => {
      if (result === true) {
        this.loadMeetings();
      }
    });
  }

  openEdit(meeting: HubMeeting): void {
    if (!this.isAdmin) {
      return;
    }

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
        meeting: { ...meeting, occursOn },
        readonly,
      },
      panelClass: ['sn-modal-panel', 'sn-meeting-panel'],
    }).afterClosed().subscribe(result => {
      if (result === true) {
        this.loadMeetings();
      }
    });
  }

  delete(meeting: HubMeeting): void {
    if (!this.isAdmin) {
      return;
    }

    if (!confirm(`Delete "${meeting.name}"? This cannot be undone.`)) {
      return;
    }

    this.groupService.deleteMeeting(this.slug, meeting.id).subscribe({
      next: () => this.loadMeetings(),
      error: err => {
        this.error = this.getErrorMessage(err, 'We could not delete this meeting right now.');
        this.cdr.markForCheck();
      },
    });
  }

  formatWhen(meeting: HubMeeting): string {
    const parts: string[] = [];

    if (meeting.isRecurring && meeting.daysOfWeek?.length) {
      parts.push(meeting.daysOfWeek.map(day => this.daysOfWeek[day] ?? 'Unknown').join(', '));
    } else if (!meeting.isRecurring && meeting.occursOn) {
      parts.push(new Date(meeting.occursOn).toLocaleDateString());
    }

    if (meeting.time) {
      parts.push(meeting.time);
    }

    return parts.join(' at ') || 'Time not set';
  }

  meetingTypeLabel(type: MeetingType): string {
    return type === MeetingType.InPerson ? 'In-Person' : type === MeetingType.Online ? 'Online' : 'Hybrid';
  }

  meetingEmoji(meeting: HubMeeting): string {
    if (meeting.meetingType === MeetingType.Online) return '💻';
    if (meeting.meetingType === MeetingType.Hybrid) return '🪜';
    return '📖';
  }

  meetingIconClass(meeting: HubMeeting): string {
    if (!meeting.isActive) return 'icon-gray';
    if (meeting.meetingType === MeetingType.Online) return 'icon-sky';
    if (meeting.meetingType === MeetingType.Hybrid) return 'icon-teal';
    return 'icon-green';
  }

  meetingIcon(meeting: HubMeeting): string {
    if (meeting.meetingType === MeetingType.InPerson) return 'location_on';
    if (meeting.meetingType === MeetingType.Online) return 'video_call';
    return 'hub';
  }

  cardAccentClass(meeting: HubMeeting): string {
    if (meeting.meetingType === MeetingType.InPerson) return 'accent-green';
    if (meeting.meetingType === MeetingType.Online) return 'accent-blue';
    return 'accent-amber';
  }

  hasPhysicalLocation(meeting: HubMeeting): boolean {
    return Boolean(meeting.venueName || meeting.street || meeting.city || meeting.state || meeting.postalCode || meeting.country || meeting.location);
  }

  hasZoomDetails(meeting: HubMeeting): boolean {
    return Boolean(meeting.publicJoinUrl || meeting.zoomLink || meeting.zoomMeetingId || meeting.zoomPasscode);
  }

  mapPreviewUrl(meeting: HubMeeting): SafeResourceUrl | null {
    const query = this.mapQuery(meeting);
    if (!query) {
      return null;
    }

    return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.google.com/maps?q=${encodeURIComponent(query)}&z=15&output=embed`);
  }

  mapPreviewCaption(meeting: HubMeeting): string {
    return [meeting.venueName, this.addressLine(meeting)].filter(Boolean).join(' • ') || meeting.location || 'Map preview';
  }

  addressLine(meeting: HubMeeting): string {
    return [meeting.city, meeting.state, meeting.postalCode].filter(Boolean).join(' ');
  }

  /** Full address string passed to MiniMapComponent for geocoding when lat/lng are absent. */
  meetingAddress(meeting: HubMeeting): string {
    return [meeting.street, meeting.city, meeting.state, meeting.postalCode, meeting.country, meeting.location]
      .filter(Boolean)
      .join(', ');
  }

  directionsUrl(meeting: HubMeeting): string {
    const destination = [meeting.venueName, meeting.street, meeting.city, meeting.state, meeting.postalCode, meeting.country, meeting.location]
      .filter(Boolean)
      .join(', ');
    return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(destination)}`;
  }

  trackById(_: number, meeting: HubMeeting): string {
    return meeting.id;
  }

  private mapQuery(meeting: HubMeeting): string | null {
    if (meeting.latitude != null && meeting.longitude != null) {
      return `${meeting.latitude.toFixed(6)},${meeting.longitude.toFixed(6)}`;
    }

    return [meeting.venueName, meeting.street, meeting.city, meeting.state, meeting.postalCode, meeting.country, meeting.location]
      .filter(Boolean)
      .join(', ') || null;
  }

  private loadMeetings(): void {
    if (!this.slug) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();

    const request$ = this.isAdmin
      ? this.groupService.getAdminMeetings(this.slug, 1, 200, undefined, this.sortBy)
      : this.groupService.getMeetings(this.slug, 1, 200, undefined, this.sortBy);

    request$.pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: response => {
        this.allMeetings = response.items as HubMeeting[];
        this.applyFilter();
        this.scrollToSelectedMeeting();
      },
      error: err => {
        this.allMeetings = [];
        this.filteredMeetings = [];
        this.error = this.getErrorMessage(err, 'We could not load meetings right now.');
      },
    });
  }

  private applyFilter(): void {
    const query = this.searchQuery.trim().toLowerCase();
    this.filteredMeetings = query
      ? this.allMeetings.filter(meeting =>
          [meeting.name, meeting.description ?? '', this.meetingTypeLabel(meeting.meetingType)]
            .join(' ')
            .toLowerCase()
            .includes(query))
      : [...this.allMeetings];
  }

  private scrollToSelectedMeeting(): void {
    if (!this.selectedMeetingId) {
      return;
    }

    const element = document.getElementById(this.selectedMeetingId);
    if (!element) {
      return;
    }

    requestAnimationFrame(() => {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    const err = error as { error?: { errors?: Record<string, string[]>; message?: string; title?: string } };

    if (err.error?.errors) {
      const messages = Object.values(err.error.errors).flat();
      if (messages.length > 0) {
        return messages.join('\n');
      }
    }

    return err.error?.message ?? err.error?.title ?? fallback;
  }
}
