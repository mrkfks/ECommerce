import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map } from 'rxjs';
import { Router } from '@angular/router';
import {
  User,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RefreshTokenRequest,
  ApiResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user';
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser: boolean;

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  // Signals for reactive state
  private _isAuthenticated = signal(this.hasValidToken());
  public isAuthenticated = this._isAuthenticated.asReadonly();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    if (this.isBrowser) {
      this.checkTokenExpiration();
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>('/auth/login', credentials).pipe(
      map(response => response.data),
      tap(response => this.handleAuthResponse(response))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    console.log('Register request data:', data);
    return this.http.post<ApiResponse<AuthResponse>>('/auth/register', data).pipe(
      map(response => response.data),
      tap(response => {
        console.log('Register response:', response);
        this.handleAuthResponse(response);
      })
    );
  }

  checkEmailAvailable(email: string): Observable<{ isAvailable: boolean; message: string }> {
    return this.http.post<any>('/auth/check-email', { email }).pipe(
      map(response => response.data || response)
    );
  }

  checkUsernameAvailable(username: string): Observable<{ isAvailable: boolean; message: string }> {
    return this.http.post<any>('/auth/check-username', { username }).pipe(
      map(response => response.data || response)
    );
  }
  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
    }
    this.currentUserSubject.next(null);
    this._isAuthenticated.set(false);
    if (this.isBrowser) {
      this.router.navigate(['/login']);
    }
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    const request: RefreshTokenRequest = { refreshToken: refreshToken || '' };

    return this.http.post<ApiResponse<AuthResponse>>('/auth/refresh', request).pipe(
      map(response => response.data),
      tap(response => this.handleAuthResponse(response))
    );
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>('/auth/me');
  }

  getToken(): string | null {
    if (this.isBrowser) {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  getRefreshToken(): string | null {
    if (this.isBrowser) {
      return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }
    return null;
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  private handleAuthResponse(response: AuthResponse): void {
    if (this.isBrowser) {
      localStorage.setItem(this.TOKEN_KEY, response.accessToken);
      localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    }

    // Backend'den gelen username ve roles'ü User objesine dönüştür
    // Backend'den gelen username ve roles'ü User objesine dönüştür
    let user: User = {
      id: 0,
      email: '',
      firstName: '',
      lastName: '',
      username: '',
      role: response.roles?.[0] || 'User',
      createdAt: new Date(),
      companyId: 0
    };

    try {
      if (response.accessToken) {
        const payload = JSON.parse(atob(response.accessToken.split('.')[1]));
        user.id = payload.userId ? parseInt(payload.userId) : (payload.nameid ? parseInt(payload.nameid) : 0);
        user.email = payload.email || '';
        user.username = payload.unique_name || payload.sub || response.username || '';
        user.companyId = payload.CompanyId ? parseInt(payload.CompanyId) : 0;
        // First/Last name might not be in token, leave empty or infer
      }
    } catch (e) {
      console.error('Token decode error:', e);
    }

    // Update simple fields if available in response (override token if better)
    if (response.username) user.username = response.username;

    if (this.isBrowser) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
    this.currentUserSubject.next(user);
    this._isAuthenticated.set(true);
  }

  private getUserFromStorage(): User | null {
    if (this.isBrowser) {
      const userJson = localStorage.getItem(this.USER_KEY);
      if (!userJson || userJson === 'undefined') {
        return null;
      }
      try {
        return JSON.parse(userJson);
      } catch (err) {
        // Bozuk veri varsa temizle ki parse hatası tekrar etmesin
        localStorage.removeItem(this.USER_KEY);
        return null;
      }
    }
    return null;
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    return !!token;
  }

  private checkTokenExpiration(): void {
    // Token expiration kontrolü - JWT decode ile yapılabilir
    const token = this.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const expirationDate = new Date(payload.exp * 1000);
        if (expirationDate < new Date()) {
          this.logout();
        }
      } catch {
        // Token decode edilemedi
      }
    }
  }
}
