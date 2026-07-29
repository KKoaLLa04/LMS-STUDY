import { AfterViewInit, Component, ElementRef, EventEmitter, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { lastValueFrom } from 'rxjs';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { CourseService } from '../../services/course.service';
import { SectionService } from '../../services/section.service';
import { LessonService } from '../../services/lesson.service';
import { QuizQuestionService } from '../../services/quiz-question.service';
import { CreateCourseRequest, QuizQuestionAdmin } from '../../models/course.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { UploadService } from '../../../../shared/services/upload.service';
import { KhoiHocService } from '../../../khoi-hoc/services/khoi-hoc.service';
import { KhoiHoc } from '../../../khoi-hoc/models/khoi-hoc.model';
import { CourseCategoryService } from '../../../course-categories/services/course-category.service';
import { CourseCategory } from '../../../course-categories/models/course-category.model';
import { DocumentService } from '../../../documents/services/document.service';
import { DocumentItem } from '../../../documents/models/document.model';
import { QuizLibraryService } from '../../../quizzes/services/quiz.service';
import { QuizItem } from '../../../quizzes/models/quiz.model';
import { RichTextEditorComponent } from '../../../../shared/components/rich-text-editor/rich-text-editor.component';

declare const bootstrap: any;

const MAX_VIDEO_SIZE = 500 * 1024 * 1024; // 500MB
const MAX_IMAGE_SIZE = 10 * 1024 * 1024; // 10MB
const MAX_DOCUMENT_SIZE = 50 * 1024 * 1024; // 50MB
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
  imports: [CommonModule, ReactiveFormsModule, RichTextEditorComponent],
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
  categories: CourseCategory[] = [];
  sharedDocuments: DocumentItem[] = [];
  sharedQuizzes: QuizItem[] = [];
  uploadState = new Map<AbstractControl, UploadState>();
  documentUploadState = new Map<AbstractControl, UploadState>();
  imageUploadState: UploadState = { uploading: false };
  previewVideoUploadState: UploadState = { uploading: false };

  deletedSectionIds: number[] = [];
  deletedLessonIds: number[] = [];

  private modal: any;

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private sectionService: SectionService,
    private lessonService: LessonService,
    private quizQuestionService: QuizQuestionService,
    private khoiHocService: KhoiHocService,
    private courseCategoryService: CourseCategoryService,
    private documentService: DocumentService,
    private quizLibraryService: QuizLibraryService,
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
      khoiHocId: [null],
      categoryId: [null],
      isFeatured: [false],
      previewVideoUrl: ['', Validators.maxLength(500)],
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
      khoiHocId: null,
      categoryId: null,
      isFeatured: false,
      previewVideoUrl: ''
    });
    this.currentStep = 1;
    this.priceDisplay = '0';
    this.thumbnailError = false;
    this.uploadState.clear();
    this.documentUploadState.clear();
    this.imageUploadState = { uploading: false };
    this.previewVideoUploadState = { uploading: false };
    this.deletedSectionIds = [];
    this.deletedLessonIds = [];
    this.loadKhoiHocs();
    this.loadCategories();
    this.loadSharedDocuments();
    this.loadSharedQuizzes();
    this.modal.show();
  }

  openEdit(id: number): void {
    this.mode = 'edit';
    this.courseId = id;
    this.loading = true;
    this.currentStep = 1;
    this.thumbnailError = false;
    this.uploadState.clear();
    this.documentUploadState.clear();
    this.imageUploadState = { uploading: false };
    this.previewVideoUploadState = { uploading: false };
    this.deletedSectionIds = [];
    this.deletedLessonIds = [];
    this.loadKhoiHocs();
    this.loadCategories();
    this.loadSharedDocuments();
    this.loadSharedQuizzes();
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

  private loadCategories(): void {
    this.courseCategoryService.getCategories().subscribe({
      next: (res) => (this.categories = res.data ?? []),
      error: () => this.toast.error('Không thể tải danh sách danh mục khóa học.')
    });
  }

  private loadSharedDocuments(): void {
    this.documentService.getAll().subscribe({
      next: (res) => (this.sharedDocuments = res.data ?? []),
      error: () => this.toast.error('Không thể tải danh sách tài liệu chung.')
    });
  }

  private loadSharedQuizzes(): void {
    this.quizLibraryService.getAll().subscribe({
      next: (res) => (this.sharedQuizzes = res.data ?? []),
      error: () => this.toast.error('Không thể tải danh sách quiz chung.')
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
        khoiHocId: course.khoiHocId ?? null,
        categoryId: course.categoryId ?? null,
        isFeatured: course.isFeatured ?? false,
        previewVideoUrl: course.previewVideoUrl ?? ''
      });

      const sectionsArray = this.form.get('sections') as FormArray;
      sectionsArray.clear();

      for (const section of course.sections ?? []) {
        const lessonsArray = this.fb.array<FormGroup>([]);

        for (const lesson of section.lessons ?? []) {
          let questions: QuizQuestionAdmin[] = [];
          if (lesson.lessonType === 'Quiz') {
            const qRes = await lastValueFrom(this.quizQuestionService.getQuestions(lesson.id));
            questions = qRes.data ?? [];
          }

          lessonsArray.push(
            this.fb.group({
              id: [lesson.id],
              title: [lesson.title, [Validators.required, Validators.maxLength(255)]],
              content: [lesson.content ?? ''],
              videoUrl: [lesson.videoUrl ?? '', Validators.maxLength(500)],
              documentUrl: [lesson.documentUrl ?? '', Validators.maxLength(500)],
              lessonType: [lesson.lessonType],
              position: [lesson.position, Validators.min(0)],
              durationMinutes: [lesson.durationMinutes ?? 0, Validators.min(0)],
              documentId: [lesson.documentId ?? null],
              quizId: [lesson.quizId ?? null],
              questions: this.buildQuestionsArray(questions)
            })
          );
        }

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

  onPreviewVideoFileSelected(event: Event): void {
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

    this.previewVideoUploadState = { uploading: true };
    this.uploadService.uploadVideo(file).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.previewVideoUploadState = { uploading: false };
          this.toast.error(res.message || 'Tải video lên thất bại');
          return;
        }
        this.form.get('previewVideoUrl')?.setValue(res.data.url);
        this.previewVideoUploadState = { uploading: false, fileName: file.name };
      },
      error: (err) => {
        this.previewVideoUploadState = { uploading: false };
        this.toast.error(err?.error?.message || 'Tải video lên thất bại');
      }
    });
  }

  clearPreviewVideo(): void {
    this.form.get('previewVideoUrl')?.setValue('');
    this.previewVideoUploadState = { uploading: false };
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
        documentUrl: ['', Validators.maxLength(500)],
        lessonType: ['Video'],
        position: [lessons.length + 1, Validators.min(0)],
        durationMinutes: [0, Validators.min(0)],
        documentId: [null],
        quizId: [null],
        questions: this.buildQuestionsArray([])
      })
    );
  }

  removeLesson(si: number, li: number): void {
    const lessons = this.getSectionLessons(si);
    const lesson = lessons.at(li);
    const lessonId = lesson.get('id')?.value;
    if (lessonId) this.deletedLessonIds.push(lessonId);
    this.uploadState.delete(lesson);
    this.documentUploadState.delete(lesson);
    lessons.removeAt(li);
  }

  // ── Câu hỏi Quiz (chỉ hiển thị khi lessonType === 'Quiz') ──

  private buildOptionsArray(options: { id?: number; text: string; isCorrect: boolean; orderNumber: number }[]): FormArray {
    const source = options.length > 0 ? options : [
      { text: '', isCorrect: false, orderNumber: 1 },
      { text: '', isCorrect: false, orderNumber: 2 }
    ];
    return this.fb.array(
      source.map((o) =>
        this.fb.group({
          id: [o.id ?? null],
          text: [o.text, Validators.required],
          isCorrect: [o.isCorrect],
          orderNumber: [o.orderNumber]
        })
      )
    );
  }

  private buildQuestionsArray(questions: QuizQuestionAdmin[]): FormArray {
    return this.fb.array(
      questions.map((q, i) =>
        this.fb.group({
          id: [q.id ?? null],
          text: [q.text, Validators.required],
          orderNumber: [q.orderNumber ?? i + 1],
          allowMultipleAnswers: [q.allowMultipleAnswers],
          options: this.buildOptionsArray(q.options)
        })
      )
    );
  }

  getLessonQuestions(si: number, li: number): FormArray {
    return this.getSectionLessons(si).at(li).get('questions') as FormArray;
  }

  getQuestionOptions(si: number, li: number, qi: number): FormArray {
    return this.getLessonQuestions(si, li).at(qi).get('options') as FormArray;
  }

  addQuestion(si: number, li: number): void {
    const questions = this.getLessonQuestions(si, li);
    questions.push(
      this.fb.group({
        id: [null],
        text: ['', Validators.required],
        orderNumber: [questions.length + 1],
        allowMultipleAnswers: [false],
        options: this.buildOptionsArray([])
      })
    );
  }

  removeQuestion(si: number, li: number, qi: number): void {
    this.getLessonQuestions(si, li).removeAt(qi);
  }

  addOption(si: number, li: number, qi: number): void {
    const options = this.getQuestionOptions(si, li, qi);
    options.push(this.fb.group({ id: [null], text: ['', Validators.required], isCorrect: [false], orderNumber: [options.length + 1] }));
  }

  removeOption(si: number, li: number, qi: number, oi: number): void {
    this.getQuestionOptions(si, li, qi).removeAt(oi);
  }

  // Với câu hỏi 1 đáp án đúng, chọn đáp án này thì tự bỏ chọn các đáp án còn lại
  // (mô phỏng hành vi radio bằng checkbox, vì mỗi option là một FormGroup độc lập trong FormArray).
  onOptionCorrectChange(si: number, li: number, qi: number, oi: number): void {
    const question = this.getLessonQuestions(si, li).at(qi);
    const options = this.getQuestionOptions(si, li, qi);
    const allowMultiple = question.get('allowMultipleAnswers')?.value;
    const clickedIsCorrect = options.at(oi).get('isCorrect')?.value;

    if (!allowMultiple && clickedIsCorrect) {
      options.controls.forEach((opt, idx) => {
        if (idx !== oi) opt.get('isCorrect')?.setValue(false, { emitEvent: false });
      });
    }
  }

  onAllowMultipleChange(si: number, li: number, qi: number): void {
    const question = this.getLessonQuestions(si, li).at(qi);
    if (question.get('allowMultipleAnswers')?.value) return;

    // Vừa tắt "nhiều đáp án đúng" — chỉ giữ lại đáp án đúng đầu tiên, bỏ chọn các đáp án còn lại.
    const options = this.getQuestionOptions(si, li, qi);
    let keptFirst = false;
    options.controls.forEach((opt) => {
      if (opt.get('isCorrect')?.value) {
        if (keptFirst) opt.get('isCorrect')?.setValue(false, { emitEvent: false });
        keptFirst = true;
      }
    });
  }

  // ── Upload tài liệu (chỉ hiển thị khi lessonType === 'Document') ──

  getDocumentUploadState(lesson: AbstractControl): UploadState {
    return this.documentUploadState.get(lesson) ?? { uploading: false };
  }

  onDocumentFileSelected(event: Event, lesson: AbstractControl): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (file.size > MAX_DOCUMENT_SIZE) {
      this.toast.error('File tài liệu không được vượt quá 50MB.');
      return;
    }

    this.documentUploadState.set(lesson, { uploading: true });
    this.uploadService.uploadDocument(file).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.documentUploadState.set(lesson, { uploading: false });
          this.toast.error(res.message || 'Tải tài liệu lên thất bại');
          return;
        }
        lesson.get('documentUrl')?.setValue(res.data.url);
        this.documentUploadState.set(lesson, { uploading: false, fileName: file.name });
      },
      error: (err) => {
        this.documentUploadState.set(lesson, { uploading: false });
        this.toast.error(err?.error?.message || 'Tải tài liệu lên thất bại');
      }
    });
  }

  clearUploadedDocument(lesson: AbstractControl): void {
    lesson.get('documentUrl')?.setValue('');
    this.documentUploadState.delete(lesson);
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
      const payload: CreateCourseRequest = {
        title: v.title,
        description: v.description || undefined,
        thumbnail: v.thumbnail || undefined,
        teacher: v.teacher || undefined,
        emoji: v.emoji || undefined,
        price: v.price,
        status: v.status,
        khoiHocId: v.khoiHocId ?? undefined,
        categoryId: v.categoryId ?? undefined,
        isFeatured: v.isFeatured ?? false,
        previewVideoUrl: v.previewVideoUrl || undefined
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
            documentUrl: lesson.documentUrl || undefined,
            lessonType: lesson.lessonType,
            position: lesson.position,
            durationMinutes: lesson.durationMinutes ?? 0,
            documentId: lesson.documentId || undefined,
            quizId: lesson.quizId || undefined
          };

          let lessonId: number;
          if (lesson.id) {
            const lRes = await lastValueFrom(this.lessonService.updateLesson(lesson.id, lessonPayload));
            if (!lRes.success) {
              this.toast.error(lRes.message || 'Cập nhật bài học thất bại');
              return;
            }
            lessonId = lesson.id;
          } else {
            const lRes = await lastValueFrom(this.lessonService.createLesson({ sectionId, ...lessonPayload }));
            if (!lRes.success) {
              this.toast.error(lRes.message || 'Tạo bài học thất bại');
              return;
            }
            lessonId = lRes.data!.id;
          }

          // Nếu lesson gắn quiz có sẵn (quizId), câu hỏi được quản lý ở trang "Quiz chung" —
          // không ghi đè bằng danh sách câu hỏi rỗng/local ở đây.
          if (lesson.lessonType === 'Quiz' && !lesson.quizId) {
            const qRes = await lastValueFrom(this.quizQuestionService.replaceQuestions(lessonId, lesson.questions ?? []));
            if (!qRes.success) {
              this.toast.error(qRes.message || 'Lưu câu hỏi quiz thất bại');
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
