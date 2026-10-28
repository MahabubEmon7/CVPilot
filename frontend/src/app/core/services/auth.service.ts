import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, AuthResponse, UserSummary } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);

  private readonly TOKEN_KEY = 'cvpilot_access_token';
  private readonly USER_KEY = 'cvpilot_user_data';

  readonly currentUser = signal<UserSummary | null>(this.getStoredUser());
  readonly isAuthenticated = computed(() => !!this.currentUser());

  private getStoredUser(): UserSummary | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserSummary;
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setSession(auth: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, auth.accessToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(auth.user));
    this.currentUser.set(auth.user);
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  register(data: { fullName: string; email: string; password: string }): Observable<ApiResponse<AuthResponse>> {
    return this.api.post<AuthResponse>('auth/register', data).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  login(data: { email: string; password: string }): Observable<ApiResponse<AuthResponse>> {
    return this.api.post<AuthResponse>('auth/login', data).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }
}

