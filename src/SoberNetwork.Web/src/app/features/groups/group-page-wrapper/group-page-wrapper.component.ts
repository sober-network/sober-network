import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PageWrapperComponent } from '@app/shared/components/page-wrapper-component/page-wrapper.component';

@Component({
  selector: 'app-group-page-wrapper',
  standalone: true,
  imports: [CommonModule],
  templateUrl: '../../../shared/components/page-wrapper-component/page-wrapper.component.html',
  styleUrls: [
    '../../../shared/components/page-wrapper-component/page-wrapper.component.scss',
    './group-page-wrapper.component.scss',
  ],
})
export class GroupPageWrapperComponent extends PageWrapperComponent {}
