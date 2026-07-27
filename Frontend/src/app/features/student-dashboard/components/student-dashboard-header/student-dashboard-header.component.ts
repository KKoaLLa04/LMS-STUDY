import { Component, EventEmitter, Input, Output } from '@angular/core';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-student-dashboard-header',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './student-dashboard-header.component.html',
  styleUrl: './student-dashboard-header.component.scss'
})
export class StudentDashboardHeaderComponent {
  @Input({ required: true }) dateLabel!: string;
  @Input({ required: true }) streakDays!: number;
  @Input({ required: true }) initials!: string;
  @Output() menuToggle = new EventEmitter<void>();
  @Output() logout = new EventEmitter<void>();
}
