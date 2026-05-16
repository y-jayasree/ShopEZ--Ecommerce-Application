import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { ActivatedRouteSnapshot } from '@angular/router';
import { authGuard } from './auth-guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authService: AuthService;
  let router: Router;

  const mockRoute = { url: [] } as unknown as ActivatedRouteSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [AuthService]
    });
    authService = TestBed.inject(AuthService);
    router      = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should allow access when token exists', () => {
    spyOn(authService, 'getToken').and.returnValue('valid-token');

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, {} as any));

    expect(result).toBeTrue();
  });

  it('should block access and redirect to /login when no token', () => {
    spyOn(authService, 'getToken').and.returnValue(null);

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, {} as any));

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], jasmine.any(Object));
  });
});