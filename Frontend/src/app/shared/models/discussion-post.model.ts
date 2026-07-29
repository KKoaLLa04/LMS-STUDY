/** Shapes returned by the backend (Backend/DTOs/DiscussionPostDto.cs). */

export interface DiscussionPost {
  id: number;
  title: string;
  content: string;
  authorName: string;
  courseId: number;
  courseName: string;
  parentPostId?: number;
  replies: DiscussionPost[];
  createdAt: string;
  updatedAt?: string;
  likeCount: number;
  isLikedByCurrentUser: boolean;
}

export interface DiscussionPostListItem {
  id: number;
  title: string;
  authorName: string;
  courseId: number;
  courseName: string;
  replyCount: number;
  createdAt: string;
  likeCount: number;
  isLikedByCurrentUser: boolean;
}

export interface CreateDiscussionPostRequest {
  title: string;
  content: string;
  authorName: string;
  courseId: number;
  parentPostId?: number;
}

export interface UpdateDiscussionPostRequest {
  title: string;
  content: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
