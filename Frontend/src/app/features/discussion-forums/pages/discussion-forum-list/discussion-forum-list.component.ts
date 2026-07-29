import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DiscussionForumService } from '../../../../shared/services/discussion-forum.service';
import { DiscussionPost, DiscussionPostListItem } from '../../../../shared/models/discussion-post.model';
import { CourseService } from '../../../courses/services/course.service';
import { CourseListItem } from '../../../courses/models/course.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { AuthService } from '../../../../core/auth/auth.service';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';

interface FlatReply {
  id: number;
  content: string;
  authorName: string;
  createdAt: string;
  depth: number;
}

function flattenReplies(post: DiscussionPost, depth = 1): FlatReply[] {
  const mine: FlatReply = { id: post.id, content: post.content, authorName: post.authorName, createdAt: post.createdAt, depth };
  const children = post.replies?.flatMap((r) => flattenReplies(r, depth + 1)) ?? [];
  return [mine, ...children];
}

@Component({
  selector: 'app-discussion-forum-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule],
  templateUrl: './discussion-forum-list.component.html',
  styleUrls: ['./discussion-forum-list.component.scss']
})
export class DiscussionForumListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('viewModal') viewModalEl!: ElementRef<HTMLElement>;

  courses: CourseListItem[] = [];
  selectedCourseId: number | null = null;
  keyword = '';

  posts: DiscussionPostListItem[] = [];
  loading = false;
  errorMessage = '';

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: DiscussionPostListItem | null = null;
  submitting = false;

  deleteTarget: { id: number; title: string } | null = null;
  deleteConfirmInput = '';
  deleting = false;

  viewingPost: DiscussionPost | null = null;
  viewingReplies: FlatReply[] = [];
  viewLoading = false;
  replyForm: FormGroup;
  replySubmitting = false;

  private formModal: any;
  private deleteModal: any;
  private viewModal: any;
  private currentUserName = '';

  constructor(
    private fb: FormBuilder,
    private discussionService: DiscussionForumService,
    private courseService: CourseService,
    private toast: ToastService,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(255)]],
      content: ['', [Validators.required]],
      authorName: ['', [Validators.required, Validators.maxLength(100)]]
    });

    this.replyForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(255)]],
      content: ['', [Validators.required]],
      authorName: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe((u) => {
      this.currentUserName = u?.fullName ?? '';
    });
    this.courseService.getCourses(1, 200).subscribe((res) => {
      this.courses = res.data?.items ?? [];
      if (this.courses.length > 0) {
        this.selectedCourseId = this.courses[0].id;
        this.loadPosts();
      }
    });
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
    this.viewModal = new bootstrap.Modal(this.viewModalEl.nativeElement);
  }

  onCourseChange(): void {
    this.loadPosts();
  }

  onSearch(): void {
    this.loadPosts();
  }

  loadPosts(): void {
    if (!this.selectedCourseId) return;
    this.loading = true;
    this.errorMessage = '';
    this.discussionService.getByCourse(this.selectedCourseId, 1, 50, this.keyword || undefined).subscribe({
      next: (res) => {
        this.posts = res.data?.items ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách bài viết. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? 'Tạo bài viết thảo luận' : 'Chỉnh sửa bài viết';
  }

  openCreateModal(): void {
    if (!this.selectedCourseId) {
      this.toast.error('Vui lòng chọn khóa học trước.');
      return;
    }
    this.modalMode = 'create';
    this.editingItem = null;
    this.form.reset({ title: '', content: '', authorName: this.currentUserName });
    this.formModal.show();
  }

  openEditModal(item: DiscussionPostListItem): void {
    this.modalMode = 'edit';
    this.editingItem = item;
    this.form.reset({ title: item.title, content: '', authorName: item.authorName });
    // Nội dung không có sẵn ở list item — tải chi tiết để điền vào form sửa.
    this.discussionService.getById(item.id).subscribe((res) => {
      if (res.data) this.form.patchValue({ content: res.data.content });
    });
    this.formModal.show();
  }

  closeFormModal(): void {
    this.formModal.hide();
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || !this.selectedCourseId) {
      this.toast.error('Vui lòng điền đầy đủ các trường bắt buộc.');
      return;
    }

    this.submitting = true;
    const v = this.form.value;

    const request$ =
      this.modalMode === 'create'
        ? this.discussionService.create({
            title: v.title.trim(),
            content: v.content.trim(),
            authorName: v.authorName.trim(),
            courseId: this.selectedCourseId
          })
        : this.discussionService.update(this.editingItem!.id, {
            title: v.title.trim(),
            content: v.content.trim()
          });

    request$.subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success) {
          this.toast.error(res.message || 'Thao tác thất bại');
          return;
        }
        this.toast.success(this.modalMode === 'create' ? 'Tạo bài viết thành công!' : 'Cập nhật bài viết thành công!');
        this.closeFormModal();
        this.loadPosts();
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  openDeleteModal(item: DiscussionPostListItem): void {
    this.deleteTarget = { id: item.id, title: item.title };
    this.deleteConfirmInput = '';
    this.deleteModal.show();
  }

  closeDeleteModal(): void {
    this.deleteModal.hide();
  }

  get isDeleteConfirmValid(): boolean {
    if (!this.deleteTarget) return false;
    return this.deleteConfirmInput.trim().toLowerCase() === this.deleteTarget.title.trim().toLowerCase();
  }

  confirmDelete(): void {
    if (!this.deleteTarget || !this.isDeleteConfirmValid) return;

    this.deleting = true;
    const { id, title } = this.deleteTarget;
    this.discussionService.delete(id).subscribe({
      next: () => {
        this.deleting = false;
        this.toast.success(`Đã xóa "${title}" thành công.`);
        this.closeDeleteModal();
        this.loadPosts();
      },
      error: (err) => {
        this.deleting = false;
        this.toast.error(err?.error?.message || 'Xóa thất bại. Vui lòng thử lại.');
      }
    });
  }

  openViewModal(item: DiscussionPostListItem): void {
    this.viewLoading = true;
    this.viewingPost = null;
    this.viewingReplies = [];
    this.replyForm.reset({ title: '', content: '', authorName: this.currentUserName });
    this.viewModal.show();
    this.loadThread(item.id);
  }

  private loadThread(id: number): void {
    this.viewLoading = true;
    this.discussionService.getById(id).subscribe({
      next: (res) => {
        this.viewingPost = res.data ?? null;
        this.viewingReplies = res.data ? flattenReplies(res.data).slice(1) : [];
        this.viewLoading = false;
      },
      error: () => {
        this.viewLoading = false;
      }
    });
  }

  closeViewModal(): void {
    this.viewModal.hide();
  }

  submitReply(): void {
    this.replyForm.markAllAsTouched();
    if (this.replyForm.invalid || !this.viewingPost) {
      this.toast.error('Vui lòng điền đầy đủ các trường bắt buộc.');
      return;
    }

    this.replySubmitting = true;
    const v = this.replyForm.value;
    this.discussionService
      .reply(this.viewingPost.id, {
        title: v.title.trim(),
        content: v.content.trim(),
        authorName: v.authorName.trim(),
        courseId: this.viewingPost.courseId
      })
      .subscribe({
        next: (res) => {
          this.replySubmitting = false;
          if (!res.success) {
            this.toast.error(res.message || 'Trả lời thất bại');
            return;
          }
          this.toast.success('Đã trả lời bài viết.');
          this.replyForm.reset({ title: '', content: '', authorName: this.currentUserName });
          this.loadThread(this.viewingPost!.id);
          this.loadPosts();
        },
        error: (err) => {
          this.replySubmitting = false;
          this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
        }
      });
  }

  deleteReply(reply: FlatReply): void {
    if (!this.viewingPost) return;
    this.discussionService.delete(reply.id).subscribe({
      next: () => {
        this.toast.success('Đã xóa trả lời.');
        this.loadThread(this.viewingPost!.id);
        this.loadPosts();
      },
      error: (err) => this.toast.error(err?.error?.message || 'Xóa thất bại.')
    });
  }
}
