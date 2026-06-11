import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginCredentials } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly tokenKey = 'idx_token';
  private readonly baseUrl = `${environment.serverUrl}/api/auth`;

  // Manage login state reactively with Signals
  private tokenSignal = signal<string | null>(
    typeof window !== 'undefined' ? localStorage.getItem(this.tokenKey) : null
  );
  
  public isAuthenticated = signal<boolean>(!!this.tokenSignal());

  login(credentials: LoginCredentials): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/login`, credentials, { responseType: 'text' as 'json' }).pipe(
      tap((token) => {
        if (typeof window !== 'undefined') {
          localStorage.setItem(this.tokenKey, token);
        }
        this.tokenSignal.set(token);
        this.isAuthenticated.set(true);
      })
    );
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
    }
    this.tokenSignal.set(null);
    this.isAuthenticated.set(false);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }
}