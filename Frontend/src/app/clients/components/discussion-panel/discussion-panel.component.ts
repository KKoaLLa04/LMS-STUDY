import { Component, Input, OnChanges, SimpleChanges, inject, signal } from '@angular/core';
import { OcIconComponent } from '../../components/icon/icon.component';
import { RelativeDatePipe } from '../../pipes/relative-date.pipe';
import { DiscussionForumService } from '../../../shared/services/discussion-forum.service';
import { DiscussionPost, DiscussionPostListItem } from '../../../shared/models/discussion-post.model';
import { ToastService } from '../../../shared/services/toast.service';

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
  imports: [OcIconComponent, RelativeDatePipe],
  templateUrl: './discussion-panel.component.html',
  styleUrl: './discussion-panel.component.scss',
})
export class DiscussionPanelComponent implements OnChanges {
  @Input({ required: true }) courseId!: number;

  private readonly discussionService = inject(DiscussionForumService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);
  readonly posts = signal<DiscussionPostListItem[]>([]);
  readonly expandedPostId = signal<number | undefined>(undefined);
  readonly expandedDetail = signal<DiscussionPost | undefined>(undefined);
  readonly expandedReplies = signal<FlatReply[]>([]);
  readonly detailLoading = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['courseId']) this.loadPosts();
  }

  private loadPosts(): void {
    this.loading.set(true);
    this.discussionService.getByCourse(this.courseId, 1, 20).subscribe({
      next: (res) => {
        this.posts.set(res.data?.items ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
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
    this.detailLoading.set(true);
    this.discussionService.getById(post.id).subscribe({
      next: (res) => {
        this.expandedDetail.set(res.data);
        this.expandedReplies.set(res.data ? flattenReplies(res.data).slice(1) : []);
        this.detailLoading.set(false);
      },
      error: () => this.detailLoading.set(false),
    });
  }

  toggleLikePost(post: DiscussionPostListItem, event: Event): void {
    event.stopPropagation();
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
}
