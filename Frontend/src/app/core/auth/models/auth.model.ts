export interface LoginRequest {
  username: string;
  password: string;
}

// Danh mục module cố định mà quyền có thể áp dụng — phải khớp với enum PermissionModule ở Backend.
export type PermissionModule =
  | 'Courses'
  | 'CourseCategories'
  | 'KhoiHocs'
  | 'Achievements'
  | 'Documents'
  | 'Quizzes'
  | 'Students'
  | 'DiscussionForums'
  | 'VirtualClassrooms';

export interface TeacherPermission {
  module: PermissionModule;
  canView: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  username: string;
  role: string;
  /** Chỉ có dữ liệu khi role === 'Teacher'. */
  permissions?: TeacherPermission[];
}

export interface CurrentUser {
  id: number;
  username: string;
  fullName: string;
  avatarUrl?: string;
  role: string;
  createdAt: string;
  /** Chỉ có dữ liệu khi role === 'Teacher'. */
  permissions?: TeacherPermission[];
}
