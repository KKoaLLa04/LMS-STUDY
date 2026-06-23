import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { lastValueFrom } from 'rxjs';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { CourseService } from '../../services/course.service';
import { SectionService } from '../../services/section.service';
import { LessonService } from '../../services/lesson.service';
import { ToastService } from '../../../../shared/services/toast.service';

const slideUp = [
  style({ opacity: 0, transform: 'translateY(24px)' }),
  animate('300ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 1, transform: 'translateY(0)' }))
];

const slideOut = [
  animate('200ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 0, transform: 'translateY(-20px)' }))
];

@Component({
  selector: 'app-course-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './course-edit.component.html',
  styleUrls: ['./course-edit.component.scss'],
  animations: [
    trigger('stepAnim', [
      transition(':enter', slideUp),
      transition(':leave', slideOut)
    ]),
    trigger('listStagger', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(16px)' }),
          stagger('60ms', [
            animate('280ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ], { optional: true })
      ])
    ])
  ]
})
export class CourseEditComponent implements OnInit {
  form: FormGroup;
  courseId!: number;
  currentStep = 1;
  loading = true;
  submitting = false;
  thumbnailError = false;
  priceDisplay = '0';

  // Track IDs that were removed so we can delete them on save
  deletedSectionIds: number[] = [];
  deletedLessonIds: number[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private courseService: CourseService,
    private sectionService: SectionService,
    private lessonService: LessonService,
    private router: Router,
    private toast: ToastService
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

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCourse();
  }

  private async loadCourse(): Promise<void> {
    try {
      const res = await lastValueFrom(this.courseService.getCourseById(this.courseId));
      if (!res.success || !res.data) {
        this.toast.error(res.message || 'Không tìm thấy khóa học.');
        this.router.navigate(['/courses']);
        return;
      }

      const course = res.data;
      const price = course.price ?? 0;
      this.priceDisplay = price === 0 ? '0' : price.toLocaleString('vi-VN');

      this.form.patchValue({
        title: course.title,
        description: course.description ?? '',
        thumbnail: course.thumbnail ?? '',
        price,
        status: course.status
      });

      const sectionsArray = this.form.get('sections') as FormArray;
      sectionsArray.clear();

      for (const section of (course.sections ?? [])) {
        const lessonsArray = this.fb.array(
          (section.lessons ?? []).map(lesson =>
            this.fb.group({
              id: [lesson.id],
              title: [lesson.title, [Validators.required, Validators.maxLength(255)]],
              content: [lesson.content ?? ''],
              videoUrl: [lesson.videoUrl ?? '', Validators.maxLength(500)],
              lessonType: [lesson.lessonType],
              position: [lesson.position, Validators.min(0)]
            })
          )
        );

        sectionsArray.push(
          this.fb.group({
            id: [section.id],
            title: [section.title, [Validators.required, Validators.maxLength(255)]],
            position: [section.position, Validators.min(0)],
            lessons: lessonsArray
          })
        );
      }
    } catch {
      this.toast.error('Không thể tải dữ liệu khóa học.');
      this.router.navigate(['/courses']);
    } finally {
      this.loading = false;
    }
  }

  get sections(): FormArray {
    return this.form.get('sections') as FormArray;
  }

  getSectionLessons(si: number): FormArray {
    return this.sections.at(si).get('lessons') as FormArray;
  }

  get thumbnailUrl(): string {
    return this.form.get('thumbnail')?.value ?? '';
  }

  get currentStatus(): string {
    return this.form.get('status')?.value ?? 'Draft';
  }

  onPriceInput(event: Event): void {
    const el = event.target as HTMLInputElement;
    const digits = el.value.replace(/\D/g, '');
    const num = digits === '' ? 0 : parseInt(digits, 10);
    this.form.get('price')?.setValue(num, { emitEvent: false });
    this.priceDisplay = num === 0 ? '0' : num.toLocaleString('vi-VN');
    el.value = this.priceDisplay;
  }

  setStatus(val: string): void {
    this.form.get('status')?.setValue(val);
  }

  addSection(): void {
    this.sections.push(
      this.fb.group({
        id: [null],
        title: ['', [Validators.required, Validators.maxLength(255)]],
        position: [this.sections.length + 1, Validators.min(0)],
        lessons: this.fb.array([])
      })
    );
  }

  removeSection(i: number): void {
    const sectionId = this.sections.at(i).get('id')?.value;
    if (sectionId) this.deletedSectionIds.push(sectionId);
    this.sections.removeAt(i);
  }

  addLesson(si: number): void {
    const lessons = this.getSectionLessons(si);
    lessons.push(
      this.fb.group({
        id: [null],
        title: ['', [Validators.required, Validators.maxLength(255)]],
        content: [''],
        videoUrl: ['', Validators.maxLength(500)],
        lessonType: ['Video'],
        position: [lessons.length + 1, Validators.min(0)]
      })
    );
  }

  removeLesson(si: number, li: number): void {
    const lessonId = this.getSectionLessons(si).at(li).get('id')?.value;
    if (lessonId) this.deletedLessonIds.push(lessonId);
    this.getSectionLessons(si).removeAt(li);
  }

  isStep1Valid(): boolean {
    const t = this.form.get('title');
    return !!(t && t.valid && t.value?.trim());
  }

  goNext(): void {
    if (this.currentStep === 1) {
      this.form.get('title')?.markAsTouched();
      if (!this.isStep1Valid()) return;
    }
    if (this.currentStep < 3) this.currentStep++;
  }

  goPrev(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  goToStep(n: number): void {
    if (n < this.currentStep) this.currentStep = n;
    if (n === 2 && this.isStep1Valid()) this.currentStep = 2;
    if (n === 3 && this.isStep1Valid()) this.currentStep = 3;
  }

  private firstInvalidStep(): number {
    if (this.form.get('title')?.invalid) return 1;
    for (let i = 0; i < this.sections.length; i++) {
      if (this.sections.at(i).get('title')?.invalid) return 2;
      const lessons = this.getSectionLessons(i);
      for (let j = 0; j < lessons.length; j++) {
        if (lessons.at(j).get('title')?.invalid) return 3;
      }
    }
    return 1;
  }

  async onSubmit(): Promise<void> {
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.currentStep = this.firstInvalidStep();
      this.toast.error('Vui lòng điền đầy đủ các trường bắt buộc.');
      return;
    }

    this.submitting = true;

    try {
      const v = this.form.value;

      // 1. Update course info
      const courseRes = await lastValueFrom(
        this.courseService.updateCourse(this.courseId, {
          title: v.title,
          description: v.description || undefined,
          thumbnail: v.thumbnail || undefined,
          price: v.price,
          status: v.status
        })
      );

      if (!courseRes.success) {
        this.toast.error(courseRes.message || 'Cập nhật khóa học thất bại');
        return;
      }

      // 2. Delete removed lessons then removed sections
      for (const lid of this.deletedLessonIds) {
        await lastValueFrom(this.lessonService.deleteLesson(lid));
      }
      for (const sid of this.deletedSectionIds) {
        await lastValueFrom(this.sectionService.deleteSection(sid));
      }

      // 3. Create or update sections & lessons
      for (const section of v.sections) {
        let sectionId: number;

        if (section.id) {
          // Update existing section
          const sRes = await lastValueFrom(
            this.sectionService.updateSection(section.id, { title: section.title, position: section.position })
          );
          if (!sRes.success) {
            this.toast.error(sRes.message || 'Cập nhật chương học thất bại');
            return;
          }
          sectionId = section.id;
        } else {
          // Create new section
          const sRes = await lastValueFrom(
            this.sectionService.createSection({ courseId: this.courseId, title: section.title, position: section.position })
          );
          if (!sRes.success) {
            this.toast.error(sRes.message || 'Tạo chương học thất bại');
            return;
          }
          sectionId = sRes.data!.id;
        }

        for (const lesson of section.lessons) {
          if (lesson.id) {
            // Update existing lesson
            const lRes = await lastValueFrom(
              this.lessonService.updateLesson(lesson.id, {
                title: lesson.title,
                content: lesson.content || undefined,
                videoUrl: lesson.videoUrl || undefined,
                lessonType: lesson.lessonType,
                position: lesson.position
              })
            );
            if (!lRes.success) {
              this.toast.error(lRes.message || 'Cập nhật bài học thất bại');
              return;
            }
          } else {
            // Create new lesson
            const lRes = await lastValueFrom(
              this.lessonService.createLesson({
                sectionId,
                title: lesson.title,
                content: lesson.content || undefined,
                videoUrl: lesson.videoUrl || undefined,
                lessonType: lesson.lessonType,
                position: lesson.position
              })
            );
            if (!lRes.success) {
              this.toast.error(lRes.message || 'Tạo bài học thất bại');
              return;
            }
          }
        }
      }

      this.toast.success(`Cập nhật khóa học "${v.title}" thành công!`);
      this.router.navigate(['/courses']);
    } catch (err: any) {
      this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
    } finally {
      this.submitting = false;
    }
  }
}
