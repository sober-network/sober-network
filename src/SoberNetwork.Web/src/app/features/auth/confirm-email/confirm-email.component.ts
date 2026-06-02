import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@app/core/services/auth.service';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  templateUrl: './confirm-email.component.html',
  styleUrl: './confirm-email.component.scss',
})
export class ConfirmEmailComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  loading = true;
  success = false;
  error = '';

  ngOnInit(): void {
    const token  = this.route.snapshot.queryParamMap.get('token') ?? '';
    const userId = this.route.snapshot.queryParamMap.get('userId') ?? '';
    if (!token || !userId) {
      this.error = 'Invalid confirmation link.';
      this.loading = false;
      return;
    }
    this.auth.confirmEmail({ token, userId }).subscribe({
      next: () => { this.success = true; this.loading = false; },
      error: err => {
        this.error = err?.error?.message ?? 'Confirmation failed. The link may have expired.';
        this.loading = false;
      },
    });
  }
}

