import { CommonModule } from '@angular/common';
import { Component, Input, Output, ViewEncapsulation, EventEmitter } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { HeroComponent } from '@app/shared/components/hero-component/hero.component';
import { BreadcrumbItem, BreadcrumbsComponent } from '@app/shared/components/breadcrumbs/breadcrumbs.component';

export interface HeroTag {
  label: string;
  color: 'sky' | 'green' | 'violet' | 'amber' | 'teal' | 'rose' | 'muted';
}

@Component({
  selector: 'app-group-hero',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, BreadcrumbsComponent],
  templateUrl: './group-hero.component.html',
  styleUrls: [
    '../../../shared/components/hero-component/hero.component.scss',
    './group-hero.component.scss',
  ],
  encapsulation: ViewEncapsulation.None,
})
export class GroupHeroComponent extends HeroComponent {
  @Input() eyebrowIcon = 'groups';
  @Input() breadcrumbs: BreadcrumbItem[] = [];
  @Input() titleSuffix = '';
  @Input() descriptionItalic = false;
  @Input() tags: HeroTag[] = [];
  @Output() override plusClick = new EventEmitter<void>();
  
  readonly backgroundImageSrc = '/circle-sunset.jpg';

  override onPlusClick(): void {
    this.plusClick.emit();
  }
}
