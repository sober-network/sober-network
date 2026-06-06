import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CardComponent } from '@app/shared/components/card-component/card.component';

@Component({
  selector: 'app-group-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: '../../../shared/components/card-component/card.component.html',
  styleUrls: [
    '../../../shared/components/card-component/card.component.scss',
    './group-card.component.scss',
  ],
})
export class GroupCardComponent extends CardComponent {}
