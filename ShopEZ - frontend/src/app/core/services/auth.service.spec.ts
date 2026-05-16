import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [AuthService]
    });
    service  = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    router   = TestBed.inject(Router);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return null when no token stored', () => {
    expect(service.getToken()).toBeNull();
  });

  it('should return false for isLoggedIn when not logged in', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('should return false for isAdmin when not logged in', () => {
    expect(service.isAdmin()).toBeFalse();
  });

  it('should store token after login', () => {
    service.login({ email: 'test@test.com', password: '123456' }).subscribe();

    httpMock.expectOne(r => r.url.includes('/auth/login'))
            .flush({ token: 'abc123', user: { id: 1, name: 'Test', email: 'test@test.com', role: 'CUSTOMER' } });

    expect(localStorage.getItem('shopez_token')).toBe('abc123');
  });

  it('should return true for isLoggedIn after login', () => {
    service.login({ email: 'test@test.com', password: '123456' }).subscribe();

    httpMock.expectOne(r => r.url.includes('/auth/login'))
            .flush({ token: 'abc123', user: { id: 1, name: 'Test', email: 'test@test.com', role: 'CUSTOMER' } });

    expect(service.isLoggedIn()).toBeTrue();
  });

  it('should return true for isAdmin when role is ADMIN', () => {
    service.login({ email: 'admin@test.com', password: '123456' }).subscribe();

    httpMock.expectOne(r => r.url.includes('/auth/login'))
            .flush({ token: 'abc123', user: { id: 2, name: 'Admin', email: 'admin@test.com', role: 'ADMIN' } });

    expect(service.isAdmin()).toBeTrue();
  });

  it('should clear token on logout', () => {
    localStorage.setItem('shopez_token', 'abc123');
    spyOn(router, 'navigate');

    service.logout();

    expect(localStorage.getItem('shopez_token')).toBeNull();
    expect(service.getCurrentUser()).toBeNull();
  });

  it('should navigate to /login on logout', () => {
    spyOn(router, 'navigate');
    service.logout();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});