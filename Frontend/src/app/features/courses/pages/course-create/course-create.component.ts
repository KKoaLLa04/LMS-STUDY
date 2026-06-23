import { Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { lastValueFrom } from 'rxjs';
import { CourseService } from '../../services/course.service';
import { SectionService } from '../../services/section.service';
import { LessonService } from '../../services/lesson.service';

@Component({
  selector: 'app-course-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './course-create.component.html'
})
export class CourseCreateComponent {
  form: FormGroup;
  submitting = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private sectionService: SectionService,
    private lessonService: LessonService,
    private router: Router
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(255)]],
      description: [''],
      thumbnail: ['', Validators.maxLength(500)],
      price: [0, [Validators.min(0)]],
      status: ['Draft'],
      sections: this.fb.array([])
    });
  }

  get sections(): FormArray {
    return this.form.get('sections') as FormArray;
  }

  getSectionLessons(sectionIndex: number): FormArray {
    return this.sections.at(sectionIndex).get('lessons') as FormArray;
  }

  addSection(): void {
    this.sections.push(
      this.fb.group({
        title: ['', [Validators.required, Validators.maxLength(255)]],
        position: [this.sections.length + 1, Validators.min(0)],
        lessons: this.fb.array([])
      })
    );
  }

  removeSection(index: number): void {
    this.sections.removeAt(index);
  }

  addLesson(sectionIndex: number): void {
    const lessons = this.getSectionLessons(sectionIndex);
    lessons.push(
      this.fb.group({
        title: ['', [Validators.required, Validators.maxLength(255)]],
        content: [''],
        videoUrl: ['', Validators.maxLength(500)],
        lessonType: ['Video'],
        position: [lessons.length + 1, Validators.min(0)]
      })
    );
  }

  removeLesson(sectionIndex: number, lessonIndex: number): void {
    this.getSectionLessons(sectionIndex).removeAt(lessonIndex);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    try {
      const formValue = this.form.value;

      const courseRes = await lastValueFrom(
        this.courseService.createCourse({
          title: formValue.title,
          description: formValue.description || undefined,
          thumbnail: formValue.thumbnail || undefined,
          price: formValue.price,
          status: formValue.status
        })
      );

      if (courseRes.httpStatusCode !== 200) {
        this.errorMessage = courseRes.message || 'Tạo khóa học thất bại';
        return;
      }

      const courseId = courseRes.data!.id;

      for (const section of formValue.sections) {
        const sectionRes = await lastValueFrom(
          this.sectionService.createSection({
            courseId,
            title: section.title,
            position: section.position
          })
        );

        if (sectionRes.httpStatusCode !== 200) {
          this.errorMessage = sectionRes.message || 'Tạo chương học thất bại';
          return;
        }

        const sectionId = sectionRes.data!.id;

        for (const lesson of section.lessons) {
          const lessonRes = await lastValueFrom(
            this.lessonService.createLesson({
              sectionId,
              title: lesson.title,
              content: lesson.content || undefined,
              videoUrl: lesson.videoUrl || undefined,
              lessonType: lesson.lessonType,
              position: lesson.position
            })
          );

          if (lessonRes.httpStatusCode !== 200) {
            this.errorMessage = lessonRes.message || 'Tạo bài học thất bại';
            return;
          }
        }
      }

      this.successMessage = 'Tạo khóa học thành công!';
      setTimeout(() => this.router.navigate(['/dashboard']), 1500);
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.';
    } finally {
      this.submitting = false;
    }
  }
}
