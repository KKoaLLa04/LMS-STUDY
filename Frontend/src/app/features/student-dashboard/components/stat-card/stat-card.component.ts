import { Component, Input } from '@angular/core';
import { IconComponent } from '../icon/icon.component';
import { Stat } from '../../models/student-dashboard.model';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './stat-card.component.html',
  styleUrl: './stat-card.component.scss'
})
export class StatCardComponent {
  @Input({ required: true }) stat!: Stat;
}
