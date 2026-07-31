export type AppUserRole = 'Admin' | 'Teacher' | 'Student';
export type AppUserStatus = 'Active' | 'Inactive' | 'Banned';
export type AppUserGender = 'Male' | 'Female' | 'Other';

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
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
