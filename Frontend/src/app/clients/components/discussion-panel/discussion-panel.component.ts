import { Component, Input, OnChanges, OnInit, SimpleChanges, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { RelativeDatePipe } from '../../pipes/relative-date.pipe';
import { DiscussionForumService } from '../../../shared/services/discussion-forum.service';
import { DiscussionPost, DiscussionPostListItem } from '../../../shared/models/discussion-post.model';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../../core/auth/auth.service';

interface FlatReply {
  id: number;
  content: string;
  authorName: string;
  createdAt: string;
  likeCount: number;
  isLikedByCurrentUser: boolean;
  depth: number;
  likeBusy?: boolean;
}

function flattenReplies(post: DiscussionPost, depth = 1): FlatReply[] {
  const mine: FlatReply = {
    id: post.id,
    content: post.content,
    authorName: post.authorName,
    createdAt: post.createdAt,
    likeCount: post.likeCount,
    isLikedByCurrentUser: post.isLikedByCurrentUser,
    depth,
  };
  const children = post.replies?.flatMap((r) => flattenReplies(r, depth + 1)) ?? [];
  return [mine, ...children];
}

@Component({
  selector: 'app-discussion-panel',
  standalone: true,
  imports: [OcIconComponent, RelativeDatePipe, FormsModule],
  templateUrl: './discussion-panel.component.html',
  styleUrl: './discussion-panel.component.scss',
})
export class DiscussionPanelComponent implements OnInit, OnChanges {
  @Input({ required: true }) courseId!: number;

  private readonly discussionService = inject(DiscussionForumService);
  private readonly toast = inject(ToastService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly posts = signal<DiscussionPostListItem[]>([]);
  readonly expandedPostId = signal<number | undefined>(undefined);
  readonly expandedDetail = signal<DiscussionPost | undefined>(undefined);
  readonly expandedReplies = signal<FlatReply[]>([]);
  readonly detailLoading = signal(false);

  /** Tên hiển thị của người đăng nhập — chỉ để gửi kèm request cho khớp dữ liệu, backend luôn
   * tự lấy lại tên thật từ tài khoản đang đăng nhập, không tin thẳng giá trị này (chặn mạo danh). */
  private currentUserFullName = '';

  readonly newPostTitle = signal('');
  readonly newPostContent = signal('');
  readonly submittingPost = signal(false);

  readonly replyContent = signal('');
  readonly submittingReply = signal(false);

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.authService.getCurrentUser().subscribe((u) => {
        this.currentUserFullName = u?.fullName ?? '';
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['courseId']) this.loadPosts();
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  goToLogin(): void {
    this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
  }

  private loadPosts(): void {
    this.loading.set(true);
    this.discussionService.getByCourse(this.courseId, 1, 20).subscribe({
      next: (res) => {
        this.posts.set(res.data?.items ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err?.error?.message ?? 'Không tải được danh sách thảo luận, vui lòng thử lại');
      },
    });
  }

  toggleExpand(post: DiscussionPostListItem): void {
    if (this.expandedPostId() === post.id) {
      this.expandedPostId.set(undefined);
      this.expandedDetail.set(undefined);
      this.expandedReplies.set([]);
      return;
    }

    this.expandedPostId.set(post.id);
    this.replyContent.set('');
    this.detailLoading.set(true);
    this.discussionService.getById(post.id).subscribe({
      next: (res) => {
        this.expandedDetail.set(res.data);
        this.expandedReplies.set(res.data ? flattenReplies(res.data).slice(1) : []);
        this.detailLoading.set(false);
      },
      error: (err) => {
        this.detailLoading.set(false);
        this.toast.error(err?.error?.message ?? 'Không tải được nội dung bài viết, vui lòng thử lại');
      },
    });
  }

  toggleLikePost(post: DiscussionPostListItem, event: Event): void {
    event.stopPropagation();
    if (!this.isLoggedIn()) {
      this.goToLogin();
      return;
    }
    const wasLiked = post.isLikedByCurrentUser;
    const request = wasLiked ? this.discussionService.unlike(post.id) : this.discussionService.like(post.id);
    request.subscribe({
      next: () => {
        this.posts.update((list) =>
          list.map((p) => (p.id === post.id ? { ...p, isLikedByCurrentUser: !wasLiked, likeCount: p.likeCount + (wasLiked ? -1 : 1) } : p))
        );
      },
      error: () => this.toast.error('Có lỗi xảy ra, vui lòng thử lại'),
    });
  }

  toggleLikeReply(reply: FlatReply): void {
    if (!this.isLoggedIn()) {
      this.goToLogin();
      return;
    }
    if (reply.likeBusy) return;
    const wasLiked = reply.isLikedByCurrentUser;
    this.patchReply(reply.id, { likeBusy: true });
    const request = wasLiked ? this.discussionService.unlike(reply.id) : this.discussionService.like(reply.id);
    request.subscribe({
      next: () => this.patchReply(reply.id, { isLikedByCurrentUser: !wasLiked, likeCount: reply.likeCount + (wasLiked ? -1 : 1), likeBusy: false }),
      error: () => {
        this.patchReply(reply.id, { likeBusy: false });
        this.toast.error('Có lỗi xảy ra, vui lòng thử lại');
      },
    });
  }

  private patchReply(id: number, patch: Partial<FlatReply>): void {
    this.expandedReplies.update((list) => list.map((r) => (r.id === id ? { ...r, ...patch } : r)));
  }

  /** Đăng bài thảo luận mới — mở cho mọi user đã đăng nhập (học sinh/giáo viên/quản trị). */
  submitPost(): void {
    const title = this.newPostTitle().trim();
    const content = this.newPostContent().trim();
    if (!title || !content || this.submittingPost()) return;

    this.submittingPost.set(true);
    this.discussionService
      .create({ title, content, authorName: this.currentUserFullName, courseId: this.courseId })
      .subscribe({
        next: (res) => {
          this.submittingPost.set(false);
          if (!res.success) {
            this.toast.error(res.message || 'Đăng bài thất bại');
            return;
          }
          this.newPostTitle.set('');
          this.newPostContent.set('');
          this.toast.success('Đã đăng bài thảo luận');
          this.loadPosts();
        },
        error: (err) => {
          this.submittingPost.set(false);
          this.toast.error(err?.error?.message ?? 'Có lỗi xảy ra, vui lòng thử lại');
        },
      });
  }

  /** Trả lời bài viết đang mở — không cần tiêu đề riêng (chỉ hiển thị nội dung ở khung reply). */
  submitReply(): void {
    const parentId = this.expandedPostId();
    const content = this.replyContent().trim();
    if (!parentId || !content || this.submittingReply()) return;

    this.submittingReply.set(true);
    this.discussionService
      .reply(parentId, {
        title: `Trả lời: ${(this.expandedDetail()?.title ?? '').slice(0, 200)}`,
        content,
        authorName: this.currentUserFullName,
        courseId: this.courseId,
      })
      .subscribe({
        next: (res) => {
          this.submittingReply.set(false);
          if (!res.success) {
            this.toast.error(res.message || 'Gửi trả lời thất bại');
            return;
          }
          this.replyContent.set('');
          this.toast.success('Đã gửi trả lời');
          this.discussionService.getById(parentId).subscribe((detailRes) => {
            this.expandedDetail.set(detailRes.data);
            this.expandedReplies.set(detailRes.data ? flattenReplies(detailRes.data).slice(1) : []);
          });
          this.loadPosts();
        },
        error: (err) => {
          this.submittingReply.set(false);
          this.toast.error(err?.error?.message ?? 'Có lỗi xảy ra, vui lòng thử lại');
        },
      });
  }
}
