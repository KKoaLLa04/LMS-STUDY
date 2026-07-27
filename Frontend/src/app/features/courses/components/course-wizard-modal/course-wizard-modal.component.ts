import { AfterViewInit, Component, ElementRef, EventEmitter, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { lastValueFrom } from 'rxjs';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { CourseService } from '../../services/course.service';
import { SectionService } from '../../services/section.service';
import { LessonService } from '../../services/lesson.service';
import { CreateCourseRequest } from '../../models/course.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { UploadService } from '../../../../shared/services/upload.service';
import { KhoiHocService } from '../../../khoi-hoc/services/khoi-hoc.service';
import { KhoiHoc } from '../../../khoi-hoc/models/khoi-hoc.model';

declare const bootstrap: any;

const MAX_VIDEO_SIZE = 500 * 1024 * 1024; // 500MB
const MAX_IMAGE_SIZE = 10 * 1024 * 1024; // 10MB
const DEFAULT_EMOJI = '📘';

interface UploadState {
  uploading: boolean;
  fileName?: string;
}

const slideUp = [
  style({ opacity: 0, transform: 'translateY(24px)' }),
  animate('300ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 1, transform: 'translateY(0)' }))
];

const slideOut = [
  animate('200ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 0, transform: 'translateY(-20px)' }))
];

@Component({
  selector: 'app-course-wizard-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './course-wizard-modal.component.html',
  styleUrls: ['./course-wizard-modal.component.scss'],
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
export class CourseWizardModalComponent implements AfterViewInit {
  @ViewChild('wizardModal') wizardModalEl!: ElementRef<HTMLElement>;
  @Output() saved = new EventEmitter<void>();

  mode: 'add' | 'edit' = 'add';
  courseId: number | null = null;

  form: FormGroup;
  currentStep = 1;
  loading = false;
  submitting = false;
  thumbnailError = false;
  priceDisplay = '0';
  khoiHocs: KhoiHoc[] = [];
  uploadState = new Map<AbstractControl, UploadState>();
  imageUploadState: UploadState = { uploading: false };

  deletedSectionIds: number[] = [];
  deletedLessonIds: number[] = [];

  private modal: any;

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private sectionService: SectionService,
    private lessonService: LessonService,
    private khoiHocService: KhoiHocService,
    private uploadService: UploadService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(255)]],
      emoji: [DEFAULT_EMOJI, Validators.maxLength(16)],
      teacher: ['', Validators.maxLength(255)],
      description: [''],
      thumbnail: ['', Validators.maxLength(500)],
      price: [0, [Validators.min(0)]],
      status: ['Draft'],
      khoiHocId: [null, Validators.required],
      sections: this.fb.array([])
    });
  }

  ngAfterViewInit(): void {
    this.modal = new bootstrap.Modal(this.wizardModalEl.nativeElement, {
      backdrop: 'static',
      keyboard: false
    });
  }

  get title(): string {
    return this.mode === 'add' ? 'Tạo khóa học mới' : 'Chỉnh sửa khóa học';
  }

  openCreate(): void {
    this.mode = 'add';
    this.courseId = null;
    this.sections.clear();
    this.form.reset({
      title: '',
      emoji: DEFAULT_EMOJI,
      teacher: '',
      description: '',
      thumbnail: '',
      price: 0,
      status: 'Draft',
      khoiHocId: null
    });
    this.currentStep = 1;
    this.priceDisplay = '0';
    this.thumbnailError = false;
    this.uploadState.clear();
    this.imageUploadState = { uploading: false };
    this.deletedSectionIds = [];
    this.deletedLessonIds = [];
    this.loadKhoiHocs();
    this.modal.show();
  }

  openEdit(id: number): void {
    this.mode = 'edit';
    this.courseId = id;
    this.loading = true;
    this.currentStep = 1;
    this.thumbnailError = false;
    this.uploadState.clear();
    this.imageUploadState = { uploading: false };
    this.deletedSectionIds = [];
    this.deletedLessonIds = [];
    this.loadKhoiHocs();
    this.loadCourse(id);
    this.modal.show();
  }

  close(): void {
    this.modal.hide();
  }

  private loadKhoiHocs(): void {
    this.khoiHocService.getKhoiHocs().subscribe({
      next: (res) => (this.khoiHocs = res.data ?? []),
      error: () => this.toast.error('Không thể tải danh sách khối học.')
    });
  }

  private async loadCourse(id: number): Promise<void> {
    try {
      const res = await lastValueFrom(this.courseService.getCourseById(id));
      if (!res.success || !res.data) {
        this.toast.error(res.message || 'Không tìm thấy khóa học.');
        this.modal.hide();
        return;
      }

      const course = res.data;
      const price = course.price ?? 0;
      this.priceDisplay = price === 0 ? '0' : price.toLocaleString('vi-VN');

      this.form.patchValue({
        title: course.title,
        emoji: course.emoji ?? DEFAULT_EMOJI,
        teacher: course.teacher ?? '',
        description: course.description ?? '',
        thumbnail: course.thumbnail ?? '',
        price,
        status: course.status,
        khoiHocId: course.khoiHocId ?? null
      });

      const sectionsArray = this.form.get('sections') as FormArray;
      sectionsArray.clear();

      for (const section of course.sections ?? []) {
        const lessonsArray = this.fb.array(
          (section.lessons ?? []).map((lesson) =>
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
      this.modal.hide();
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

  onImageFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.toast.error('Vui lòng chọn một file ảnh hợp lệ.');
      return;
    }
    if (file.size > MAX_IMAGE_SIZE) {
      this.toast.error('File ảnh không được vượt quá 10MB.');
      return;
    }

    this.imageUploadState = { uploading: true };
    this.uploadService.uploadImage(file).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.imageUploadState = { uploading: false };
          this.toast.error(res.message || 'Tải ảnh lên thất bại');
          return;
        }
        this.form.get('thumbnail')?.setValue(res.data.url);
        this.imageUploadState = { uploading: false, fileName: file.name };
      },
      error: (err) => {
        this.imageUploadState = { uploading: false };
        this.toast.error(err?.error?.message || 'Tải ảnh lên thất bại');
      }
    });
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
    const lessons = this.getSectionLessons(si);
    const lesson = lessons.at(li);
    const lessonId = lesson.get('id')?.value;
    if (lessonId) this.deletedLessonIds.push(lessonId);
    this.uploadState.delete(lesson);
    lessons.removeAt(li);
  }

  getUploadState(lesson: AbstractControl): UploadState {
    return this.uploadState.get(lesson) ?? { uploading: false };
  }

  onVideoFileSelected(event: Event, lesson: AbstractControl): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('video/')) {
      this.toast.error('Vui lòng chọn một file video hợp lệ.');
      return;
    }
    if (file.size > MAX_VIDEO_SIZE) {
      this.toast.error('File video không được vượt quá 500MB.');
      return;
    }

    this.uploadState.set(lesson, { uploading: true });
    this.uploadService.uploadVideo(file).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.uploadState.set(lesson, { uploading: false });
          this.toast.error(res.message || 'Tải video lên thất bại');
          return;
        }
        lesson.get('videoUrl')?.setValue(res.data.url);
        this.uploadState.set(lesson, { uploading: false, fileName: file.name });
      },
      error: (err) => {
        this.uploadState.set(lesson, { uploading: false });
        this.toast.error(err?.error?.message || 'Tải video lên thất bại');
      }
    });
  }

  clearUploadedVideo(lesson: AbstractControl): void {
    lesson.get('videoUrl')?.setValue('');
    this.uploadState.delete(lesson);
  }

  isStep1Valid(): boolean {
    const t = this.form.get('title');
    const k = this.form.get('khoiHocId');
    return !!(t && t.valid && t.value?.trim()) && !!(k && k.valid && k.value);
  }

  goNext(): void {
    if (this.currentStep === 1) {
      this.form.get('title')?.markAsTouched();
      this.form.get('khoiHocId')?.markAsTouched();
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
    if (this.form.get('title')?.invalid || this.form.get('khoiHocId')?.invalid) return 1;
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
      const payload: CreateCourseRequest = {
        title: v.title,
        description: v.description || undefined,
        thumbnail: v.thumbnail || undefined,
        teacher: v.teacher || undefined,
        emoji: v.emoji || undefined,
        price: v.price,
        status: v.status,
        khoiHocId: v.khoiHocId
      };

      let courseId: number;

      if (this.mode === 'add') {
        const courseRes = await lastValueFrom(this.courseService.createCourse(payload));
        if (!courseRes.success) {
          this.toast.error(courseRes.message || 'Tạo khóa học thất bại');
          return;
        }
        courseId = courseRes.data!.id;
      } else {
        const courseRes = await lastValueFrom(this.courseService.updateCourse(this.courseId!, payload));
        if (!courseRes.success) {
          this.toast.error(courseRes.message || 'Cập nhật khóa học thất bại');
          return;
        }
        courseId = this.courseId!;
      }

      for (const lid of this.deletedLessonIds) {
        await lastValueFrom(this.lessonService.deleteLesson(lid));
      }
      for (const sid of this.deletedSectionIds) {
        await lastValueFrom(this.sectionService.deleteSection(sid));
      }

      for (const section of v.sections) {
        let sectionId: number;

        if (section.id) {
          const sRes = await lastValueFrom(
            this.sectionService.updateSection(section.id, { title: section.title, position: section.position })
          );
          if (!sRes.success) {
            this.toast.error(sRes.message || 'Cập nhật chương học thất bại');
            return;
          }
          sectionId = section.id;
        } else {
          const sRes = await lastValueFrom(
            this.sectionService.createSection({ courseId, title: section.title, position: section.position })
          );
          if (!sRes.success) {
            this.toast.error(sRes.message || 'Tạo chương học thất bại');
            return;
          }
          sectionId = sRes.data!.id;
        }

        for (const lesson of section.lessons) {
          const lessonPayload = {
            title: lesson.title,
            content: lesson.content || undefined,
            videoUrl: lesson.videoUrl || undefined,
            lessonType: lesson.lessonType,
            position: lesson.position
          };

          if (lesson.id) {
            const lRes = await lastValueFrom(this.lessonService.updateLesson(lesson.id, lessonPayload));
            if (!lRes.success) {
              this.toast.error(lRes.message || 'Cập nhật bài học thất bại');
              return;
            }
          } else {
            const lRes = await lastValueFrom(this.lessonService.createLesson({ sectionId, ...lessonPayload }));
            if (!lRes.success) {
              this.toast.error(lRes.message || 'Tạo bài học thất bại');
              return;
            }
          }
        }
      }

      this.toast.success(
        this.mode === 'add' ? `Tạo khóa học "${v.title}" thành công!` : `Cập nhật khóa học "${v.title}" thành công!`
      );
      this.modal.hide();
      this.saved.emit();
    } catch (err: any) {
      this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
    } finally {
      this.submitting = false;
    }
  }
}
