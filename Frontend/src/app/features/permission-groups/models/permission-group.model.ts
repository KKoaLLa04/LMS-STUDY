import { TeacherPermission } from '../../../core/auth/models/auth.model';

export interface PermissionGroup {
  id: number;
  name: string;
  description?: string | null;
  memberCount: number;
}

export interface PermissionGroupMember {
  id: number;
  username: string;
  fullName: string;
}

export interface PermissionGroupDetail extends PermissionGroup {
  modulePermissions: TeacherPermission[];
  members: PermissionGroupMember[];
}

export interface SavePermissionGroupRequest {
  name: string;
  description?: string | null;
  modulePermissions: TeacherPermission[];
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
