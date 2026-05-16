import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, map, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthRequest, AuthResponse, RegisterRequest, User } from '../../models/user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl    = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'shopez_token';
  private readonly USER_KEY  = 'shopez_user';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getStoredUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  //  Register
  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/register`, payload).pipe(
      map(res => this.normalizeAuthResponse(res)),
      tap(res => this.handleAuthSuccess(res)),
      catchError(err => {
        console.error('Register error:', err);
        return throwError(() => err);
      })
    );
  }

  //  Login
  login(payload: AuthRequest): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/login`, payload).pipe(
      map(res => this.normalizeAuthResponse(res)),
      tap(res => this.handleAuthSuccess(res)),
      catchError(err => {
        console.error('Login error:', err);
        return throwError(() => err);
      })
    );
  }

  //  Logout
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  //  Token helpers
  getToken(): string | null {
    const t = localStorage.getItem(this.TOKEN_KEY);
    return (t && t !== 'undefined' && t !== 'null') ? t : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken() && !!this.currentUserSubject.value;
  }

  isAdmin(): boolean {
    return (this.currentUserSubject.value?.role ?? '').toUpperCase() === 'ADMIN';
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  //  Private helpers
  private normalizeAuthResponse(res: any): AuthResponse {
    if (res?.data?.token) {
      return { token: res.data.token, user: res.data.user };
    }
    if (res?.token) {
      return { token: res.token, user: res.user };
    }
    console.error('Unexpected auth response shape:', res);
    throw new Error('Invalid response from server. Expected token + user.');
  }

  private handleAuthSuccess(res: AuthResponse): void {
    if (!res?.token || !res?.user) {
      console.error('handleAuthSuccess: missing token or user', res);
      return;
    }

    if (res.user.role) {
      res.user.role = res.user.role.toUpperCase()as "CUSTOMER" | "ADMIN";
    }
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(res.user));
    this.currentUserSubject.next(res.user);
  }

  private getStoredUser(): User | null {
    try {
      const stored = localStorage.getItem(this.USER_KEY);
      if (!stored || stored === 'undefined' || stored === 'null') return null;
      return JSON.parse(stored);
    } catch {
      return null;
    }
  }
}