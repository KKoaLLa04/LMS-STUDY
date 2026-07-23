import { Component, Input } from '@angular/core';
import { Deadline } from '../../models/student-dashboard.model';

@Component({
  selector: 'app-deadline-item',
  standalone: true,
  templateUrl: './deadline-item.component.html',
  styleUrl: './deadline-item.component.scss'
})
export class DeadlineItemComponent {
  @Input({ required: true }) deadline!: Deadline;
}
