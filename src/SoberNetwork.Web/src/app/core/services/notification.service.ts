import { Injectable, inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { NotificationResponse, NotificationCountResponse } from '../models/notification.models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = environment.apiUrl;
  private readonly destroy$ = new Subject<void>();

  private hubConnection: signalR.HubConnection | null = null;
  private readonly _unreadCount$ = new BehaviorSubject<number>(0);
  readonly unreadCount$ = this._unreadCount$.asObservable();

  get unreadCount(): number { return this._unreadCount$.value; }

  constructor() {
    // Start/stop SignalR connection when auth state changes
    this.authService.currentUser$.pipe(takeUntil(this.destroy$)).subscribe(user => {
      if (user?.accessToken) {
        this.startConnection(user.accessToken);
      } else {
        this.stopConnection();
      }
    });
  }

  private startConnection(accessToken: string): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/hubs/notifications`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.on('notificationCount', (count: number) => {
      this._unreadCount$.next(count);
    });

    this.hubConnection.start()
      .then(() => this.fetchUnreadCount())
      .catch(err => console.warn('SignalR connection failed:', err));
  }

  private stopConnection(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
    this._unreadCount$.next(0);
  }

  fetchUnreadCount(): void {
    this.http.get<NotificationCountResponse>(`${this.baseUrl}/api/notifications/count`)
      .subscribe({ next: r => this._unreadCount$.next(r.count), error: () => {} });
  }

  getNotifications() {
    return this.http.get<NotificationResponse[]>(`${this.baseUrl}/api/notifications`);
  }

  markAllRead() {
    return this.http.post<void>(`${this.baseUrl}/api/notifications/mark-read`, {});
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopConnection();
  }
}
