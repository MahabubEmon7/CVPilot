export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message?: string;
  errors: string[];
  timestamp: string;
}

export interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresInMinutes: number;
  user: UserSummary;
}

