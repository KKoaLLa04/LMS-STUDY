import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Course } from '../../models/course.model';
import { ClientCourseService } from '../../services/client-course.service';
import { isCourseInProgress } from '../../utils/course-progress.util';
import { CourseCarouselComponent } from '../../components/course-carousel/course-carousel.component';
import { CategoryChipsComponent } from '../../components/category-chips/category-chips.component';
import { CourseCardComponent } from '../../components/course-card/course-card.component';
import { OcIconComponent } from '../../components/icon/icon.component';

const CATEGORY_ALL = 'Tất cả';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [
    CourseCarouselComponent,
    CategoryChipsComponent,
    CourseCardComponent,
    OcIconComponent,
  ],
  templateUrl: './course-list.component.html',
  styleUrl: './course-list.component.scss',
})
export class CourseListComponent implements OnInit {
  private readonly courseService = inject(ClientCourseService);

  readonly categories = signal<string[]>([CATEGORY_ALL]);
  readonly courses = signal<Course[]>([]);
  readonly searchQuery = this.courseService.searchQuery;
  readonly activeCategory = signal(CATEGORY_ALL);

  readonly featuredCourses = computed(() => this.courses().filter((c) => c.featured));
  readonly inProgressCourses = computed(() => this.courses().filter(isCourseInProgress));

  readonly filteredCourses = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    const category = this.activeCategory();
    return this.courses().filter(
      (course) =>
        (category === CATEGORY_ALL || course.subject === category) &&
        course.title.toLowerCase().includes(query)
    );
  });

  ngOnInit(): void {
    this.courseService.getCourses().subscribe((courses) => this.courses.set(courses));
    this.courseService.getCategories().subscribe((categories) => this.categories.set(categories));
  }

  animationDelayFor(index: number): number {
    return (index % 6) * 0.06;
  }
}
