import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Register } from './register';

describe('Register Component', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register, ReactiveFormsModule, RouterTestingModule, HttpClientTestingModule]
    }).compileComponents();

    fixture   = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should have invalid form when empty', () => {
    expect(component.registerForm.invalid).toBeTrue();
  });

  it('should validate name minimum length', () => {
    component.registerForm.get('name')?.setValue('A');
    component.registerForm.get('name')?.markAsTouched();
    expect(component.isInvalid('name')).toBeTrue();
  });

  it('should validate email format', () => {
    component.registerForm.get('email')?.setValue('bademail');
    component.registerForm.get('email')?.markAsTouched();
    expect(component.isInvalid('email')).toBeTrue();
  });

  it('should validate phone pattern', () => {
    component.registerForm.get('phone')?.setValue('1234567890');
    component.registerForm.get('phone')?.markAsTouched();
    expect(component.isInvalid('phone')).toBeTrue();
  });

  it('should accept valid phone starting with 9', () => {
    component.registerForm.get('phone')?.setValue('9876543210');
    expect(component.registerForm.get('phone')?.valid).toBeTrue();
  });

  it('passwordStrength should be 0 when password is empty', () => {
    expect(component.passwordStrength).toBe(0);
  });

  it('passwordStrength should increase with strong password', () => {
    component.registerForm.get('passwords')?.get('password')?.setValue('Strong@1');
    expect(component.passwordStrength).toBeGreaterThan(2);
  });

  it('strengthLabel should return correct label', () => {
    component.registerForm.get('passwords')?.get('password')?.setValue('Strong@1');
    expect(component.strengthLabel).toBeTruthy();
  });

  it('isLoading should be false initially', () => {
    expect(component.isLoading).toBeFalse();
  });
});