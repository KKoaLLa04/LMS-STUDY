export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  username: string;
  role: string;
}

export interface CurrentUser {
  id: number;
  username: string;
  fullName: string;
  avatarUrl?: string;
  role: string;
  createdAt: string;
}
