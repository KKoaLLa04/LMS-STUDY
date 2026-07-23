import { Component, Input } from '@angular/core';
import { IconComponent } from '../icon/icon.component';
import { Activity } from '../../models/student-dashboard.model';

@Component({
  selector: 'app-activity-item',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './activity-item.component.html',
  styleUrl: './activity-item.component.scss'
})
export class ActivityItemComponent {
  @Input({ required: true }) activity!: Activity;
}
