import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { adminGuard } from './admin-guard';
import { AuthService } from '../services/auth.service';

// helps to build a fake JWT with a given role
function makeToken(role: string): string {
  const payload = btoa(JSON.stringify({ role }));
  return `header.${payload}.signature`;
}

describe('adminGuard', () => {
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [AuthService]
    });
    authService = TestBed.inject(AuthService);
    router      = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should allow access when role is ADMIN', () => {
    spyOn(authService, 'getToken').and.returnValue(makeToken('ADMIN'));

    const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any));

    expect(result).toBeTrue();
  });

  it('should block and redirect to / when role is CUSTOMER', () => {
    spyOn(authService, 'getToken').and.returnValue(makeToken('CUSTOMER'));

    const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any));

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should redirect to /login when no token', () => {
    spyOn(authService, 'getToken').and.returnValue(null);

    const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any));

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});