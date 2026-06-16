import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models';

const API_URL = 'http://localhost:5000/api/auth';
const TOKEN_KEY = 'todo_token';
const USER_KEY = 'todo_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly isLoggedInSignal = signal(this.hasToken());

  readonly isLoggedIn = this.isLoggedInSignal.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  login(dto: LoginRequest) {
    return this.http.post<AuthResponse>(`${API_URL}/login`, dto).pipe(
      tap(response => this.saveSession(response))
    );
  }

  register(dto: RegisterRequest) {
    return this.http.post<AuthResponse>(`${API_URL}/register`, dto).pipe(
      tap(response => this.saveSession(response))
    );
  }

  logout() {
    const token = this.getToken();
    if (token) {
      this.http.post(`${API_URL}/logout`, {}).subscribe({ error: () => {} });
    }
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.isLoggedInSignal.set(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUsername(): string | null {
    return localStorage.getItem(USER_KEY);
  }

  private saveSession(response: AuthResponse) {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, response.username);
    this.isLoggedInSignal.set(true);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
  }
}
