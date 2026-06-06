import { CommonModule } from '@angular/common';
import { Component, Input, ViewEncapsulation } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { HeroComponent } from '@app/shared/components/hero-component/hero.component';

@Component({
  selector: 'app-group-hero',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  templateUrl: './group-hero.component.html',
  styleUrls: [
    '../../../shared/components/hero-component/hero.component.scss',
    './group-hero.component.scss',
  ],
  encapsulation: ViewEncapsulation.None,
})
export class GroupHeroComponent extends HeroComponent {
  @Input() eyebrowIcon = 'groups';
  @Input() titleSuffix = '';
  readonly backgroundImageSrc = '/circle-sunset.jpg';
}
