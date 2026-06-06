import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-hero-component',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.scss',
})
export class HeroComponent {
  @Input() eyebrowText = '';
  @Input() title = '';
  @Input() description = '';
  @Input() backButtonRoute: string | readonly unknown[] = ['/dashboard'];
  @Input() backButtonText = '';
  @Input() showSettingsButton = false;
  @Input() settingsButtonText = '';
  @Input() showPlusButton = false;
  @Input() plusButtonText = '';
  
  @Output() settingsClick = new EventEmitter<void>();
  @Output() plusClick = new EventEmitter<void>();

  onSettingsClick(): void {
    this.settingsClick.emit();
  }

  onPlusClick(): void {
    this.plusClick.emit();
  }
}
