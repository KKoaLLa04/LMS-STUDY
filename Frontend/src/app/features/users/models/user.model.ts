import { PermissionModule, TeacherPermission } from '../../../core/auth/models/auth.model';

export type AppUserRole = 'Admin' | 'Teacher' | 'Student';
export type AppUserStatus = 'Active' | 'Inactive' | 'Banned';
export type AppUserGender = 'Male' | 'Female' | 'Other';

// Danh mục module cố định dùng cho lưới checkbox phân quyền trong form Edit User — nhãn tiếng
// Việt hiển thị cho Admin khi cấp quyền cho giáo viên.
export const TEACHER_PERMISSION_MODULES: { module: PermissionModule; label: string }[] = [
  { module: 'Courses', label: 'Khóa học' },
  { module: 'CourseCategories', label: 'Danh mục khóa học' },
  { module: 'KhoiHocs', label: 'Khối học' },
  { module: 'Achievements', label: 'Thành tích' },
  { module: 'Documents', label: 'Tài liệu' },
  { module: 'Quizzes', label: 'Quiz' },
  { module: 'Students', label: 'Học sinh' },
  { module: 'DiscussionForums', label: 'Thảo luận' },
  { module: 'VirtualClassrooms', label: 'Lớp học trực tuyến' }
];

export interface AppUser {
  id: number;
  username: string;
  email: string;
  fullName: string;
  phone: string;
  avatarUrl?: string | null;
  status: AppUserStatus;
  gender?: AppUserGender | null;
  dateOfBirth?: string | null;
  address?: string | null;
  /** Only meaningful when role === 'Teacher'. */
  subject?: string | null;
  experienceYears?: number | null;
  bio?: string | null;
  /** Only meaningful when role === 'Student'. */
  khoiHocId?: number | null;
  role: AppUserRole;
  createdAt: string;
  /** Only meaningful when role === 'Teacher'. */
  permissions?: TeacherPermission[];
  /** Only meaningful when role === 'Teacher' — read-only, managed on the Permissions page. */
  permissionGroupNames?: string[];
}

export interface CreateUserRequest {
  username: string;
  password: string;
  email: string;
  fullName: string;
  phone: string;
  avatarUrl?: string | null;
  status: AppUserStatus;
  gender?: AppUserGender | null;
  dateOfBirth?: string | null;
  address?: string | null;
  subject?: string | null;
  experienceYears?: number | null;
  bio?: string | null;
  khoiHocId?: number | null;
  role: AppUserRole;
  /** Only meaningful when role === 'Teacher'; ignored by the backend unless the caller is Admin. */
  permissions?: TeacherPermission[] | null;
}

export interface UpdateUserRequest {
  username: string;
  password?: string | null;
  email: string;
  fullName: string;
  phone: string;
  avatarUrl?: string | null;
  status: AppUserStatus;
  gender?: AppUserGender | null;
  dateOfBirth?: string | null;
  address?: string | null;
  subject?: string | null;
  experienceYears?: number | null;
  bio?: string | null;
  khoiHocId?: number | null;
  role: AppUserRole;
  /** Only meaningful when role === 'Teacher'; ignored by the backend unless the caller is Admin. */
  permissions?: TeacherPermission[] | null;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
