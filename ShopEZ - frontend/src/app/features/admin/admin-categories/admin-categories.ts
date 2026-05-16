import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { Category } from '../../../models/category';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-categories.html',
  styleUrl: './admin-categories.css'
})
export class AdminCategories implements OnInit {
  categories: Category[] = [];
  loading = true;
  error = '';
  successMessage = '';
  formError = '';
  showForm = false;
  editingId: number | null = null;
  submitting = false;

  form: FormGroup;

  constructor(
    private productService: ProductService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.form = this.fb.group({
      name:        ['', [Validators.required, Validators.minLength(2)]],
      description: ['']
    });
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.error = '';
    this.productService.getCategories().subscribe({
      next: cats => { this.categories = cats; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Failed to load categories. Is the backend running?'; }
    });
  }

  openCreate() {
    this.editingId = null;
    this.formError = '';
    this.form.reset();
    this.showForm = true;
  }

  openEdit(c: Category) {
    this.editingId = c.id;
    this.formError = '';
    this.form.patchValue({ name: c.name, description: c.description });
    this.showForm = true;
  }

  submit() {
    if (this.form.invalid) return;
    this.submitting = true;
    this.formError = '';

    const obs = this.editingId
      ? this.productService.updateCategory(this.editingId, this.form.value)
      : this.productService.createCategory(this.form.value);

    obs.subscribe({
      next: () => {
        this.showForm = false;
        this.successMessage = this.editingId ? 'Category updated!' : 'Category created!';
        setTimeout(() => this.successMessage = '', 3000);
        this.load();
        this.submitting = false;
      },
      error: (err) => {
        this.formError = err?.error?.message ?? 'Failed to save category. Please try again.';
        this.submitting = false;
      }
    });
  }

  delete(id: number) {
    if (!confirm('Delete this category?')) return;
    this.productService.deleteCategory(id).subscribe({
      next: () => {
        this.successMessage = 'Category deleted.';
        setTimeout(() => this.successMessage = '', 3000);
        this.load();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Cannot delete — category may have products assigned.';
        setTimeout(() => this.error = '', 4000);
      }
    });
  }

  goToDashboard() {
    this.router.navigate(['/admin']);
  }

  cancel() {
    this.showForm = false;
    this.formError = '';
    this.form.reset();
  }
}