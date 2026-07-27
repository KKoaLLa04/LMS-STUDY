import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Course } from '../../models/course.model';
import { OcIconComponent } from '../icon/icon.component';
import { CoursePricePipe } from '../../pipes/course-price.pipe';
import { CourseDurationPipe } from '../../pipes/course-duration.pipe';

@Component({
  selector: 'app-oc-course-card',
  standalone: true,
  imports: [RouterLink, OcIconComponent, CoursePricePipe, CourseDurationPipe],
  templateUrl: './course-card.component.html',
  styleUrl: './course-card.component.scss',
})
export class CourseCardComponent {
  @Input({ required: true }) course!: Course;
  /** 'compact' renders the narrow horizontal-scroll card used in "Đang học dở". */
  @Input() variant: 'grid' | 'compact' = 'grid';
  /** Stagger the fade-in-up entrance animation across a grid. */
  @Input() animationDelay = 0;
}
