import { Component, Input } from '@angular/core';
import { Course } from '../../models/student-dashboard.model';

@Component({
  selector: 'app-course-row',
  standalone: true,
  templateUrl: './course-row.component.html',
  styleUrl: './course-row.component.scss'
})
export class CourseRowComponent {
  @Input({ required: true }) course!: Course;
}
