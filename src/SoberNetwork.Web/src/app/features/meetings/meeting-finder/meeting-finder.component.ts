import {
  Component, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef, inject, signal, computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import * as L from 'leaflet';
import { MeetingService } from '../../../core/services/meeting.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  PublicMeetingSearchResponse, MeetingSearchParams, MeetingType, TimeBlock,
  MEETING_FORMATS, DAYS_OF_WEEK, DAY_ABBR
} from '../../../core/models/group.models';

@Component({
  selector: 'app-meeting-finder',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './meeting-finder.component.html',
  styleUrl: './meeting-finder.component.scss',
})
export class MeetingFinderComponent implements OnInit, OnDestroy, AfterViewInit {
  private readonly meetingService = inject(MeetingService);
  private readonly authService = inject(AuthService);
  private readonly destroy$ = new Subject<void>();
  private readonly search$ = new Subject<void>();

  @ViewChild('mapContainer') mapContainerRef!: ElementRef<HTMLDivElement>;
  private map: L.Map | null = null;
  private markers: L.LayerGroup = L.layerGroup();
  private radiusCircle: L.Circle | null = null;

  // ── Exposed constants for template ───────────────────────────────────────────
  readonly FORMATS = MEETING_FORMATS;
  readonly DAYS = DAY_ABBR;
  readonly MeetingType = MeetingType;
  readonly TimeBlock = TimeBlock;

  // ── Filter state ──────────────────────────────────────────────────────────────
  selectedDays = signal<number[]>([]);
  selectedTimeBlock = signal<TimeBlock | undefined>(undefined);
  selectedFormats = signal<string[]>([]);
  selectedMeetingType = signal<MeetingType | undefined>(undefined);
  isOpen = signal<boolean | undefined>(undefined);

  // ── Location state ────────────────────────────────────────────────────────────
  addressInput = '';
  radiusMiles = 25;
  lat = signal<number | undefined>(undefined);
  lon = signal<number | undefined>(undefined);
  locationLabel = signal<string>('');
  geoError = signal<string>('');

  // ── Results state ─────────────────────────────────────────────────────────────
  results = signal<PublicMeetingSearchResponse[]>([]);
  loading = signal(false);
  error = signal('');
  view = signal<'list' | 'map'>('list');

  // ── Derived ───────────────────────────────────────────────────────────────────
  grouped = computed(() => {
    const byDay = new Map<number | null, PublicMeetingSearchResponse[]>();
    for (const m of this.results()) {
      const key = m.dayOfWeek ?? null;
      if (!byDay.has(key)) byDay.set(key, []);
      byDay.get(key)!.push(m);
    }
    return byDay;
  });

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe(() => this.doSearch());
    this.doSearch();
    this.tryLoadUserAddress();
  }

  ngAfterViewInit(): void {
    if (this.view() === 'map') this.initMap();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.map?.remove();
  }

  // ── Filter toggles ────────────────────────────────────────────────────────────

  toggleDay(day: number): void {
    const current = this.selectedDays();
    this.selectedDays.set(current.includes(day) ? current.filter(d => d !== day) : [...current, day]);
    this.scheduleSearch();
  }

  toggleFormat(fmt: string): void {
    const current = this.selectedFormats();
    this.selectedFormats.set(current.includes(fmt) ? current.filter(f => f !== fmt) : [...current, fmt]);
    this.scheduleSearch();
  }

  setTimeBlock(block: TimeBlock | undefined): void {
    this.selectedTimeBlock.set(this.selectedTimeBlock() === block ? undefined : block);
    this.scheduleSearch();
  }

  setMeetingType(type: MeetingType | undefined): void {
    this.selectedMeetingType.set(this.selectedMeetingType() === type ? undefined : type);
    this.scheduleSearch();
  }

  setOpenFilter(value: boolean | undefined): void {
    this.isOpen.set(this.isOpen() === value ? undefined : value);
    this.scheduleSearch();
  }

  clearFilters(): void {
    this.selectedDays.set([]);
    this.selectedTimeBlock.set(undefined);
    this.selectedFormats.set([]);
    this.selectedMeetingType.set(undefined);
    this.isOpen.set(undefined);
    this.scheduleSearch();
  }

  // ── Location ──────────────────────────────────────────────────────────────────

  useMyLocation(): void {
    this.geoError.set('');
    if (!navigator.geolocation) {
      this.geoError.set('Geolocation is not supported by your browser.');
      return;
    }
    navigator.geolocation.getCurrentPosition(
      pos => {
        this.lat.set(pos.coords.latitude);
        this.lon.set(pos.coords.longitude);
        this.locationLabel.set('Current location');
        this.scheduleSearch();
      },
      () => this.geoError.set('Unable to retrieve your location. Please enter an address.')
    );
  }

  geocodeAddress(): void {
    if (!this.addressInput.trim()) return;
    this.geoError.set('');
    const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(this.addressInput)}&format=json&limit=1`;
    fetch(url, { headers: { 'Accept-Language': 'en', 'User-Agent': 'SoberNetwork/1.0' } })
      .then(r => r.json())
      .then((data: { lat: string; lon: string; display_name: string }[]) => {
        if (!data?.length) { this.geoError.set('Address not found. Try a zip code or city, state.'); return; }
        this.lat.set(parseFloat(data[0].lat));
        this.lon.set(parseFloat(data[0].lon));
        this.locationLabel.set(data[0].display_name);
        this.scheduleSearch();
      })
      .catch(() => this.geoError.set('Geocoding failed. Please try again.'));
  }

  clearLocation(): void {
    this.lat.set(undefined);
    this.lon.set(undefined);
    this.locationLabel.set('');
    this.addressInput = '';
    this.scheduleSearch();
  }

  onRadiusChange(): void { this.scheduleSearch(); }

  // ── View toggle ───────────────────────────────────────────────────────────────

  switchView(v: 'list' | 'map'): void {
    this.view.set(v);
    if (v === 'map') setTimeout(() => this.initMap(), 50);
  }

  // ── Google Maps directions URL ────────────────────────────────────────────────

  directionsUrl(m: PublicMeetingSearchResponse): string {
    const addr = [m.street, m.city, m.state, m.postalCode].filter(Boolean).join(', ');
    return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(addr)}`;
  }

  dayLabel(m: PublicMeetingSearchResponse): string {
    if (m.isRecurring && m.dayOfWeek !== null) return DAYS_OF_WEEK[m.dayOfWeek];
    if (m.occursOn) return new Date(m.occursOn).toLocaleDateString('en-US', { weekday: 'long', month: 'short', day: 'numeric' });
    return '';
  }

  meetingTypeLabel(t: MeetingType): string {
    return t === MeetingType.InPerson ? 'In-Person' : t === MeetingType.Online ? 'Online' : 'Hybrid';
  }

  // ── Private ───────────────────────────────────────────────────────────────────

  private scheduleSearch(): void { this.search$.next(); }

  private doSearch(): void {
    this.loading.set(true);
    this.error.set('');
    const params: MeetingSearchParams = {};
    const days = this.selectedDays();
    if (days.length) params.days = days;
    const tb = this.selectedTimeBlock();
    if (tb !== undefined) params.timeBlock = tb;
    const fmts = this.selectedFormats();
    if (fmts.length) params.formats = fmts;
    const mt = this.selectedMeetingType();
    if (mt !== undefined) params.meetingType = mt;
    const open = this.isOpen();
    if (open !== undefined) params.isOpen = open;
    const lat = this.lat(), lon = this.lon();
    if (lat !== undefined && lon !== undefined) {
      params.lat = lat; params.lon = lon; params.radiusMiles = this.radiusMiles;
    }
    this.meetingService.searchPublicMeetings(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: r => { this.results.set(r); this.loading.set(false); this.updateMapMarkers(); },
        error: () => { this.error.set('Failed to load meetings. Please try again.'); this.loading.set(false); },
      });
  }

  private tryLoadUserAddress(): void {
    if (!this.authService.isLoggedIn) return;
    this.meetingService.getMyMailingAddress().pipe(takeUntil(this.destroy$)).subscribe({
      next: addr => {
        if (addr.mailingLatitude && addr.mailingLongitude) {
          this.lat.set(addr.mailingLatitude);
          this.lon.set(addr.mailingLongitude);
          const label = [addr.mailingCity, addr.mailingState].filter(Boolean).join(', ');
          this.locationLabel.set(label || 'Saved address');
          this.scheduleSearch();
        }
      },
      error: () => { /* no-op: unenrolled users skip */ },
    });
  }

  private initMap(): void {
    if (!this.mapContainerRef?.nativeElement || this.map) return;
    this.map = L.map(this.mapContainerRef.nativeElement).setView([39.5, -98.35], 4);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 18,
    }).addTo(this.map);
    this.markers.addTo(this.map);
    this.updateMapMarkers();
  }

  private updateMapMarkers(): void {
    if (!this.map) return;
    this.markers.clearLayers();
    this.radiusCircle?.remove();
    this.radiusCircle = null;

    const lat = this.lat(), lon = this.lon();
    if (lat !== undefined && lon !== undefined) {
      this.radiusCircle = L.circle([lat, lon], {
        radius: this.radiusMiles * 1609.34,
        color: '#4A90D9', fillColor: '#4A90D9', fillOpacity: 0.08, weight: 1
      }).addTo(this.map);
      this.map.setView([lat, lon], 11);
    }

    const inPersonPin = L.icon({
      iconUrl: 'assets/marker-icon.png', shadowUrl: 'assets/marker-shadow.png',
      iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34],
    });

    for (const m of this.results()) {
      if (m.latitude == null || m.longitude == null) continue;
      const popup = L.popup().setContent(this.buildPopupHtml(m));
      L.marker([m.latitude, m.longitude], { icon: inPersonPin })
        .bindPopup(popup)
        .addTo(this.markers);
    }

    if (!lat && this.results().length > 0) {
      const bounds = this.markers.getLayers()
        .filter((l): l is L.Marker => l instanceof L.Marker)
        .map(m => m.getLatLng());
      if (bounds.length) this.map.fitBounds(L.latLngBounds(bounds), { padding: [30, 30] });
    }
  }

  private buildPopupHtml(m: PublicMeetingSearchResponse): string {
    const addr = [m.street, m.city, m.state].filter(Boolean).join(', ');
    const dir = this.directionsUrl(m);
    const groupUrl = `/groups/${m.groupSlug}`;
    return `
      <div class="map-popup">
        <strong>${m.name}</strong><br>
        <span class="text-muted">${this.dayLabel(m)} · ${m.time}</span><br>
        ${m.venueName ? `<em>${m.venueName}</em><br>` : ''}
        ${addr ? `${addr}<br>` : ''}
        <small>${m.isOpen ? 'Open' : 'Closed'} · ${this.meetingTypeLabel(m.meetingType)}</small><br>
        <a href="${dir}" target="_blank" rel="noopener">Get Directions</a>
        &nbsp;|&nbsp;<a href="${groupUrl}">Group Page</a>
      </div>`;
  }
}
