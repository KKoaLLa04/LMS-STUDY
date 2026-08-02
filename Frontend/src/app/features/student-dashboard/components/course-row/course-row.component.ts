import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardCourse } from '../../models/student-dashboard.model';

@Component({
  selector: 'app-course-row',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './course-row.component.html',
  styleUrl: './course-row.component.scss'
})
export class CourseRowComponent {
  @Input({ required: true }) course!: DashboardCourse;
}
