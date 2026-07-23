import { Component, Input } from '@angular/core';
import { IconName } from '../../models/student-dashboard.model';

@Component({
  selector: 'app-sd-icon',
  standalone: true,
  templateUrl: './icon.component.html'
})
export class IconComponent {
  @Input() name!: IconName;
  @Input() size = 20;
  @Input() color = 'currentColor';
  @Input() strokeWidth = 2;
}
