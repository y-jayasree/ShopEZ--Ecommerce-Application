import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AlertMessages } from '../../../shared/components/alert-messages/alert-messages';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone:true,
  imports: [CommonModule, RouterModule,FormsModule, ReactiveFormsModule, AlertMessages],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  showPassword = false;
  sessionExpired = false;
  private returnUrl = '/';
 
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}
 
  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
 
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    this.sessionExpired = this.route.snapshot.queryParams['reason'] === 'session_expired';
  }
 
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
 
    this.isLoading = true;
    this.errorMessage = '';
 
    this.authService.login(this.loginForm.value).subscribe({
      next: res => {
        this.isLoading = false;
        if (res.user.role === 'ADMIN') {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate([this.returnUrl]);
        }
      },
      error: err => {
        this.isLoading = false;
        this.errorMessage = err.status === 401
          ? 'Invalid email or password. Please try again.'
          : 'Login failed. Please try again later.';
      }
    });
  }
 
  isFieldInvalid(field: string): boolean {
    const control = this.loginForm.get(field);
    return !!(control?.invalid && (control.dirty || control.touched));
  }
}
 
