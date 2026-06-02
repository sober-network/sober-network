import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterModule, MatButtonModule, MatIconModule],
  template: `
    <div class="not-found-container">
      <mat-icon class="not-found-icon">search_off</mat-icon>
      <h1>404 — Page Not Found</h1>
      <p>The page you're looking for doesn't exist or may have moved.</p>
      <a mat-raised-button color="primary" routerLink="/">Go Home</a>
    </div>
  `,
  styles: [`
    .not-found-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 80px 24px;
      text-align: center;
      gap: 16px;
    }
    .not-found-icon {
      font-size: 72px;
      width: 72px;
      height: 72px;
      opacity: 0.3;
    }
    h1 { margin: 0; font-size: 1.8rem; }
    p { margin: 0; opacity: 0.65; }
  `],
})
export class NotFoundComponent {}
