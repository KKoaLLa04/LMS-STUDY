import { Component, Input } from '@angular/core';
import { OcIconName } from '../../models/icon-name.type';

/** Inline-SVG icon set for the client area (no lucide-angular dependency),
 * following the same @switch pattern as app-sd-icon in student-dashboard. */
@Component({
  selector: 'app-oc-icon',
  standalone: true,
  templateUrl: './icon.component.html',
})
export class OcIconComponent {
  @Input() name!: OcIconName;
  @Input() size = 18;
  @Input() color = 'currentColor';
  @Input() strokeWidth = 2.75;
}
