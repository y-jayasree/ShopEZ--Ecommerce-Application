import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

function getRoleFromToken(token: string): string {
  try {
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));

    return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        ?? decoded['role']
        ?? decoded['Role']
        ?? '';
  } catch {
    return '';
  }
}

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router      = inject(Router);

  const token = authService.getToken();
  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  const role = getRoleFromToken(token).toUpperCase(); 
  if (role === 'ADMIN') {
    return true;
  }

  router.navigate(['/']);
  return false;
};