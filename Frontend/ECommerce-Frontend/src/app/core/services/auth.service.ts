import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { 
  User, 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse, 
  RefreshTokenRequest 
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  // Signals for reactive state
  private _isAuthenticated = signal(this.hasValidToken());
  public isAuthenticated = this._isAuthenticated.asReadonly();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.checkTokenExpiration();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/auth/login', credentials).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    console.log('Register request data:', data);
    return this.http.post<AuthResponse>('/auth/register', data).pipe(
      tap(response => {
        console.log('Register response:', response);
        this.handleAuthResponse(response);
      })
    );
  }

  checkEmailAvailable(email: string): Observable<{ isAvailable: boolean; message: string }> {
    return this.http.post<{ isAvailable: boolean; message: string }>('/auth/check-email', { email });
  }

  checkUsernameAvailable(username: string): Observable<{ isAvailable: boolean; message: string }> {
    return this.http.post<{ isAvailable: boolean; message: string }>('/auth/check-username', { username });
  }
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this._isAuthenticated.set(false);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    const request: RefreshTokenRequest = { refreshToken: refreshToken || '' };
    
    return this.http.post<AuthResponse>('/auth/refresh', request).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>('/auth/me');
  }

  getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  getRefreshToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }
    return null;
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    
    // Backend'den gelen username ve roles'ü User objesine dönüştür
    const user: User = {
      id: 0, // Backend userID döndürmüyor, JWT'den decode edebilirsiniz
      email: '', // Backend email döndürmüyor
      firstName: '',
      lastName: '',
      role: response.roles?.[0] || 'User',
      createdAt: new Date()
    };
    
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
    this._isAuthenticated.set(true);
  }

  private getUserFromStorage(): User | null {
    if (typeof window !== 'undefined') {
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
