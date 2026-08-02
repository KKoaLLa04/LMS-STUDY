import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { QuizLibraryService } from '../../services/quiz.service';
import { QuizItem } from '../../models/quiz.model';
import { QuizQuestionAdmin } from '../../../courses/models/course.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { RichTextEditorComponent } from '../../../../shared/components/rich-text-editor/rich-text-editor.component';
import { AuthService } from '../../../../core/auth/auth.service';

declare const bootstrap: any;

@Component({
  selector: 'app-quiz-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, RichTextEditorComponent],
  templateUrl: './quiz-list.component.html',
  styleUrls: ['./quiz-list.component.scss']
})
export class QuizListComponent implements OnInit, AfterViewInit {
  @ViewChild('infoModal') infoModalEl!: ElementRef<HTMLElement>;
  @ViewChild('questionsModal') questionsModalEl!: ElementRef<HTMLElement>;

  items: QuizItem[] = [];
  loading = false;
  errorMessage = '';

  // Modal 1: Tạo mới / sửa Tiêu đề, Mô tả
  infoForm: FormGroup;
  infoMode: 'add' | 'edit' = 'add';
  editingInfoItem: QuizItem | null = null;
  submittingInfo = false;

  // Modal 2: Quản lý câu hỏi (nhập tay và/hoặc import Excel)
  editingItem: QuizItem | null = null;
  loadingQuestions = false;
  submitting = false;
  importing = false;
  importWarnings: string[] = [];
  questionsForm: FormGroup;

  private infoModal: any;
  private questionsModal: any;

  constructor(
    private fb: FormBuilder,
    private quizService: QuizLibraryService,
    private toast: ToastService,
    public auth: AuthService
  ) {
    this.infoForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(255)]],
      description: ['']
    });
    this.questionsForm = this.fb.group({ questions: this.fb.array([]) });
  }

  ngOnInit(): void {
    this.loadItems();
  }

  ngAfterViewInit(): void {
    this.infoModal = new bootstrap.Modal(this.infoModalEl.nativeElement);
    this.questionsModal = new bootstrap.Modal(this.questionsModalEl.nativeElement);
  }

  loadItems(): void {
    this.loading = true;
    this.errorMessage = '';
    this.quizService.getAll().subscribe({
      next: (res) => {
        this.items = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách quiz.';
        this.loading = false;
      }
    });
  }

  // ── Modal 1: Tạo mới / sửa thông tin quiz ──

  openCreateModal(): void {
    this.infoMode = 'add';
    this.editingInfoItem = null;
    this.infoForm.reset({ title: '', description: '' });
    this.infoModal.show();
  }

  openEditInfoModal(item: QuizItem): void {
    this.infoMode = 'edit';
    this.editingInfoItem = item;
    this.infoForm.reset({ title: item.title, description: item.description ?? '' });
    this.infoModal.show();
  }

  closeInfoModal(): void {
    this.infoModal.hide();
  }

  onSubmitInfo(): void {
    this.infoForm.markAllAsTouched();
    if (this.infoForm.invalid) return;

    this.submittingInfo = true;
    const v = this.infoForm.value;
    const payload = { title: v.title, description: v.description || undefined };

    const req$ = this.infoMode === 'edit' && this.editingInfoItem
      ? this.quizService.update(this.editingInfoItem.id, payload)
      : this.quizService.create(payload);

    req$.subscribe({
      next: (res) => {
        this.submittingInfo = false;
        if (!res.success) {
          this.toast.error(res.message || 'Lưu quiz thất bại');
          return;
        }
        this.toast.success(this.infoMode === 'edit' ? `Cập nhật quiz "${v.title}" thành công!` : `Tạo quiz "${v.title}" thành công!`);
        this.closeInfoModal();
        this.loadItems();
      },
      error: (err) => {
        this.submittingInfo = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  deleteItem(item: QuizItem): void {
    if (!confirm(`Xóa quiz "${item.title}"?`)) return;

    this.quizService.delete(item.id).subscribe({
      next: (res) => {
        if (!res.success) {
          this.toast.error(res.message || 'Xóa quiz thất bại');
          return;
        }
        this.toast.success('Đã xóa quiz.');
        this.loadItems();
      },
      error: (err) => {
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  // ── Modal 2: Quản lý câu hỏi ──

  get questions(): FormArray {
    return this.questionsForm.get('questions') as FormArray;
  }

  getOptions(qi: number): FormArray {
    return this.questions.at(qi).get('options') as FormArray;
  }

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

  private buildQuestionsArray(questionsData: QuizQuestionAdmin[]): FormArray {
    return this.fb.array(
      questionsData.map((q, i) =>
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

  async openQuestionsModal(item: QuizItem): Promise<void> {
    this.editingItem = item;
    this.loadingQuestions = true;
    this.importWarnings = [];
    this.questionsModal.show();

    try {
      const res = await lastValueFrom(this.quizService.getQuestions(item.id));
      this.questionsForm.setControl('questions', this.buildQuestionsArray(res.data ?? []));
    } catch {
      this.toast.error('Không thể tải danh sách câu hỏi.');
    } finally {
      this.loadingQuestions = false;
    }
  }

  closeQuestionsModal(): void {
    this.questionsModal.hide();
  }

  addQuestion(): void {
    this.questions.push(
      this.fb.group({
        id: [null],
        text: ['', Validators.required],
        orderNumber: [this.questions.length + 1],
        allowMultipleAnswers: [false],
        options: this.buildOptionsArray([])
      })
    );
  }

  removeQuestion(qi: number): void {
    this.questions.removeAt(qi);
  }

  addOption(qi: number): void {
    const options = this.getOptions(qi);
    options.push(this.fb.group({ id: [null], text: ['', Validators.required], isCorrect: [false], orderNumber: [options.length + 1] }));
  }

  removeOption(qi: number, oi: number): void {
    this.getOptions(qi).removeAt(oi);
  }

  onOptionCorrectChange(qi: number, oi: number): void {
    const question = this.questions.at(qi);
    const options = this.getOptions(qi);
    const allowMultiple = question.get('allowMultipleAnswers')?.value;
    const clickedIsCorrect = options.at(oi).get('isCorrect')?.value;

    if (!allowMultiple && clickedIsCorrect) {
      options.controls.forEach((opt, idx) => {
        if (idx !== oi) opt.get('isCorrect')?.setValue(false, { emitEvent: false });
      });
    }
  }

  onAllowMultipleChange(qi: number): void {
    const question = this.questions.at(qi);
    if (question.get('allowMultipleAnswers')?.value) return;

    const options = this.getOptions(qi);
    let keptFirst = false;
    options.controls.forEach((opt) => {
      if (opt.get('isCorrect')?.value) {
        if (keptFirst) opt.get('isCorrect')?.setValue(false, { emitEvent: false });
        keptFirst = true;
      }
    });
  }

  // ── Import câu hỏi từ file Excel (kết hợp với câu hỏi đã nhập tay, không thay thế) ──

  downloadTemplate(): void {
    this.quizService.downloadTemplate().subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'mau-import-quiz.xlsx';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.toast.error('Không thể tải file mẫu.')
    });
  }

  onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    this.importing = true;
    this.importWarnings = [];
    this.quizService.importExcel(file).subscribe({
      next: (res) => {
        this.importing = false;
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Import file thất bại');
          return;
        }
        const startIndex = this.questions.length;
        res.data.questions.forEach((q, i) => {
          this.questions.push(
            this.fb.group({
              id: [null],
              text: [q.text, Validators.required],
              orderNumber: [startIndex + i + 1],
              allowMultipleAnswers: [q.allowMultipleAnswers],
              options: this.buildOptionsArray(q.options)
            })
          );
        });
        this.importWarnings = res.data.warnings ?? [];
        this.toast.success(`Đã thêm ${res.data.questions.length} câu hỏi từ file import.`);
      },
      error: (err) => {
        this.importing = false;
        this.toast.error(err?.error?.message || 'Import file thất bại');
      }
    });
  }

  async onSubmit(): Promise<void> {
    if (!this.editingItem) return;

    this.submitting = true;
    try {
      const res = await lastValueFrom(
        this.quizService.replaceQuestions(this.editingItem.id, this.questionsForm.value.questions)
      );
      if (!res.success) {
        this.toast.error(res.message || 'Lưu câu hỏi thất bại');
        return;
      }
      this.toast.success(`Lưu câu hỏi quiz "${this.editingItem.title}" thành công!`);
      this.closeQuestionsModal();
      this.loadItems();
    } catch (err: any) {
      this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
    } finally {
      this.submitting = false;
    }
  }
}
