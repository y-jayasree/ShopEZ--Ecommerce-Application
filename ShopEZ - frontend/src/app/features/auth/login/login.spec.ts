import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Login } from './login';

describe('Login Component', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login, ReactiveFormsModule, RouterTestingModule, HttpClientTestingModule]
    }).compileComponents();

    fixture   = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should have invalid form when empty', () => {
    expect(component.loginForm.invalid).toBeTrue();
  });

  it('should mark form valid with correct email and password', () => {
    component.loginForm.setValue({ email: 'test@test.com', password: '123456' });
    expect(component.loginForm.valid).toBeTrue();
  });

  it('should show error when email is invalid', () => {
    component.loginForm.get('email')?.setValue('not-an-email');
    component.loginForm.get('email')?.markAsTouched();
    expect(component.isFieldInvalid('email')).toBeTrue();
  });

  it('should show error when password is less than 6 chars', () => {
    component.loginForm.get('password')?.setValue('123');
    component.loginForm.get('password')?.markAsTouched();
    expect(component.isFieldInvalid('password')).toBeTrue();
  });

  it('should not submit when form is invalid', () => {
    spyOn(component as any, 'authService');
    component.onSubmit();
    expect(component.loginForm.touched).toBeTrue();
  });

  it('isLoading should be false initially', () => {
    expect(component.isLoading).toBeFalse();
  });

  it('errorMessage should be empty initially', () => {
    expect(component.errorMessage).toBe('');
  });
});