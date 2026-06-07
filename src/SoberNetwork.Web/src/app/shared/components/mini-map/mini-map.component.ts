import { Component, Input, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';

const localIcon = L.icon({
  iconUrl: 'marker-icon.png',
  iconRetinaUrl: 'marker-icon-2x.png',
  shadowUrl: 'marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

@Component({
  selector: 'app-mini-map',
  standalone: true,
  imports: [CommonModule],
  template: `<div #mapContainer [style.height]="height" style="width:100%;display:block;"></div>`,
  styles: [`:host { display: block; width: 100%; }`],
})
export class MiniMapComponent implements AfterViewInit, OnDestroy {
  @Input() latitude: number | null = null;
  @Input() longitude: number | null = null;
  /** Full address string used for geocoding when lat/lng are not available. */
  @Input() address: string | null = null;
  @Input() zoomLevel: number = 14;
  @Input() label: string | null = null;
  @Input() height: string = '170px';

  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;

  private map: L.Map | null = null;
  private observer: ResizeObserver | null = null;

  ngAfterViewInit(): void {
    const hasCoords = this.latitude != null && this.longitude != null;
    const hasAddress = Boolean(this.address?.trim());
    if (!hasCoords && !hasAddress) return;

    const container = this.mapContainer.nativeElement;

    // ResizeObserver fires once the container has real pixel dimensions —
    // reliable even inside *ngFor + OnPush, unlike setTimeout(0).
    this.observer = new ResizeObserver(entries => {
      const rect = entries[0]?.contentRect;
      if (rect && rect.width > 0 && rect.height > 0) {
        this.observer?.disconnect();
        this.observer = null;
        if (hasCoords) {
          this.initMap(container, this.latitude!, this.longitude!);
        } else {
          this.geocodeAndInit(container);
        }
      }
    });
    this.observer.observe(container);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    this.map?.remove();
    this.map = null;
  }

  private geocodeAndInit(container: HTMLElement): void {
    const query = encodeURIComponent(this.address!);
    fetch(`https://nominatim.openstreetmap.org/search?q=${query}&format=json&limit=1`, {
      headers: { 'Accept-Language': 'en' },
    })
      .then(r => r.json())
      .then((results: { lat: string; lon: string }[]) => {
        if (results.length > 0) {
          this.initMap(container, parseFloat(results[0].lat), parseFloat(results[0].lon));
          return;
        }
        // Street-level lookup failed — fall back to city/state/zip (last 3 comma-separated parts).
        const parts = this.address!.split(',').map(s => s.trim()).filter(Boolean);
        if (parts.length > 1) {
          const fallback = encodeURIComponent(parts.slice(-3).join(', '));
          return fetch(`https://nominatim.openstreetmap.org/search?q=${fallback}&format=json&limit=1`, {
            headers: { 'Accept-Language': 'en' },
          })
            .then(r2 => r2.json())
            .then((r2: { lat: string; lon: string }[]) => {
              if (r2.length > 0) {
                this.initMap(container, parseFloat(r2[0].lat), parseFloat(r2[0].lon));
              }
            });
        }
        return;
      })
      .catch(() => { /* silently fail — no map rendered */ });
  }

  private initMap(container: HTMLElement, lat: number, lon: number): void {
    if (this.map) return;
    this.map = L.map(container, { attributionControl: false, zoomControl: false })
      .setView([lat, lon], this.zoomLevel);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(this.map);
    L.marker([lat, lon], { icon: localIcon, title: this.label || '' }).addTo(this.map);
    setTimeout(() => this.map?.invalidateSize(), 50);
  }
}
