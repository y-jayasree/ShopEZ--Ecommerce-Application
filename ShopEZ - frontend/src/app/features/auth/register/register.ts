import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {AbstractControl,FormBuilder,FormGroup,FormsModule,ReactiveFormsModule,Validators} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AlertMessages } from '../../../shared/components/alert-messages/alert-messages';
import { AuthService } from '../../../core/services/auth.service';

function passwordMatchValidator(group: FormGroup) {
  const pass    = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pass === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, AlertMessages],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register implements OnInit {
  registerForm!: FormGroup;
  isLoading    = false;
  errorMessage = '';
  showPass     = false;

  constructor(
    private fb:          FormBuilder,
    private authService: AuthService,
    private router:      Router
  ) {}

  ngOnInit(): void {
    // If already logged in, redirect away from register page
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
      return;
    }

    this.registerForm = this.fb.group({
      name:  ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
      passwords: this.fb.group(
        {
          password:        ['', [Validators.required, Validators.minLength(8)]],
          confirmPassword: ['', Validators.required]
        },
        { validators: passwordMatchValidator }
      )
    });
  }

  get passwordStrength(): number {
    const pass = this.registerForm?.get('passwords')?.get('password')?.value || '';
    let score = 0;
    if (pass.length >= 8)           score++;
    if (/[A-Z]/.test(pass))         score++;
    if (/[0-9]/.test(pass))         score++;
    if (/[^A-Za-z0-9]/.test(pass))  score++;
    return score;
  }

  get strengthLabel(): string {
    return ['', 'Weak', 'Fair', 'Good', 'Strong'][this.passwordStrength];
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading    = true;
    this.errorMessage = '';

    const { passwords, ...rest } = this.registerForm.value;

    this.authService.register({
      ...rest,
      password: passwords.password
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        const role = res?.user?.role?.toUpperCase();
        if (role === 'ADMIN') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        // 409 = email already exists
        // 400 = validation error from backend
        // 0   = network/CORS error
        if (err?.status === 409) {
          this.errorMessage = 'An account with this email already exists.';
        } else if (err?.status === 400) {
          this.errorMessage = err?.error?.message || 'Invalid registration details.';
        } else if (err?.status === 0) {
          this.errorMessage = 'Cannot reach server. Make sure your backend is running.';
        } else {
          this.errorMessage = 'Registration failed. Please try again.';
        }
      }
    });
  }

  isInvalid(field: string): boolean {
    const parts = field.split('.');
    let control: AbstractControl | null = this.registerForm;
    for (const part of parts) {
      control = control?.get(part) ?? null;
      if (!control) return false;
    }
    return !!(control.invalid && (control.dirty || control.touched));
  }
}