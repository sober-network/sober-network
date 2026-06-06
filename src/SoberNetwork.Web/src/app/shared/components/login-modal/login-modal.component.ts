import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatDialogRef, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@app/core/services/auth.service';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';

@Component({
  selector: 'app-login-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    BaseFormModalComponent,
  ],
  templateUrl: './login-modal.component.html',
  styleUrl:    './login-modal.component.scss',
})
export class LoginModalComponent {
  private readonly fb     = inject(FormBuilder);
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  readonly dialogRef      = inject(MatDialogRef<LoginModalComponent>);

  title = 'Sign in to your account';
  subtitle = 'Welcome back';
  icon = 'lock';
  useRainbowRing = true;

  form = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  loading      = false;
  error        = '';
  hidePassword = true;

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error   = '';
    const { email, password } = this.form.getRawValue();
    this.auth.login({ email: email!, password: password! }).subscribe({
      next: () => {
        this.loading = false;
        this.dialogRef.close();
        this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.error   = err?.error?.message ?? 'Invalid email or password.';
        this.loading = false;
      },
    });
  }

  async openRegisterModal(): Promise<void> {
    this.dialogRef.close();
    const { RegisterModalComponent } = await import('../register-modal/register-modal.component');
    this.dialog.open(RegisterModalComponent, {
      panelClass: ['sn-modal-panel', 'sn-register-panel'],
      maxWidth:   '100vw',
      autoFocus:  'first-tabbable',
    });
  }

  navigateTo(path: string): void {
    this.dialogRef.close();
    this.router.navigate([path]);
  }
}
