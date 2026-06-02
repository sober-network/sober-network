import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '@app/shared/components/navbar/navbar.component';
import { FooterComponent } from '@app/shared/components/footer/footer.component';
import { LoadingSpinnerComponent } from '@app/shared/components/loading-spinner/loading-spinner.component';
import { AuthService } from '@app/core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, FooterComponent, LoadingSpinnerComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly auth = inject(AuthService);
  restoring = true;

  ngOnInit(): void {
    this.auth.tryRestoreSession().subscribe({
      complete: () => (this.restoring = false),
      error: () => (this.restoring = false),
    });
  }
}
