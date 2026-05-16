import { Injectable } from '@angular/core';
import {HttpRequest,HttpHandler,HttpEvent,HttpInterceptor,HttpErrorResponse} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp ? (payload.exp * 1000) < Date.now() : false;
  } catch {
    return true;
  }
}

function getValidToken(): string | null {
  const t = localStorage.getItem('shopez_token');
  return (t && t !== 'undefined' && t !== 'null') ? t : null;
}

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private router: Router) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Reads token at request time to attach to header
    const token = getValidToken();

    const authReq = token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {

        if (error.status === 401 && !req.url.includes('/auth/')) {
          // Re-read token at ERROR time (not at request time).
          const tokenAtErrorTime = getValidToken();

          // Only logout if a token exists AND it is genuinely expired
          if (tokenAtErrorTime && isTokenExpired(tokenAtErrorTime)) {
            localStorage.removeItem('shopez_token');
            localStorage.removeItem('shopez_user');
            this.router.navigate(['/login'], {
              queryParams: { returnUrl: this.router.url, reason: 'session_expired' }
            });
          }
        }

        if (error.status === 403 && !req.url.includes('/auth/')) {
          this.router.navigate(['/']);
        }

        return throwError(() => error);
      })
    );
  }
}