import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { ImageService } from '../../../core/services/image.service';
import { Product } from '../../../models/product';
import { Category } from '../../../models/category';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-products.html',
  styleUrl: './admin-products.css'
})
export class AdminProducts implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  loading = true;
  error = '';
  successMessage = '';
  formError = '';
  deleteError = '';
  showForm = false;
  editingId: number | null = null;
  selectedFile: File | null = null;
  submitting = false;

  form: FormGroup;

  constructor(
    private productService: ProductService,
    public imageService: ImageService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.form = this.fb.group({
      name:        ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      price:       [null, [Validators.required, Validators.min(0.01)]],
      stock:       [0,    [Validators.required, Validators.min(0)]],
      categoryId:  [null, Validators.required]
    });
  }

  ngOnInit() {
    this.loadProducts();
    this.loadCategories();
  }

  loadCategories() {
    this.productService.getCategories().subscribe({
      next: (cats) => {
        this.categories = cats;
        console.log('[AdminProducts] categories loaded:', cats);
      },
      error: (err) => console.error('[AdminProducts] Failed to load categories:', err)
    });
  }

  loadProducts() {
    this.loading = true;
    this.productService.getProducts().subscribe({
      next: res => { this.products = res.content; this.loading = false; },
      error: () => { this.error = 'Failed to load products'; this.loading = false; }
    });
  }

  openCreate() {
    this.editingId = null;
    this.formError = '';
    this.form.reset({ stock: 0 });
    this.selectedFile = null;
    this.showForm = true;
  }

  openEdit(p: Product) {
    this.editingId = p.id;
    this.formError = '';
    this.form.patchValue({
      name:        p.name,
      description: p.description,
      price:       p.price,
      stock:       p.stock,
      categoryId:  p.categoryId ?? p.category?.id
    });
    this.selectedFile = null;
    this.showForm = true;
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  submit() {
    if (this.form.invalid) return;
    this.submitting = true;
    this.formError = '';
    const val = this.form.value;

    const obs = this.editingId
      ? this.productService.updateProduct(this.editingId, val, this.selectedFile ?? undefined)
      : this.productService.createProduct(val, this.selectedFile ?? undefined);

    obs.subscribe({
      next: () => {
        this.showForm = false;
        this.successMessage = this.editingId ? 'Product updated successfully!' : 'Product created successfully!';
        setTimeout(() => this.successMessage = '', 3000);
        this.loadProducts();
        this.submitting = false;
      },
      error: (err) => {
        this.formError = err?.error?.message ?? 'Failed to save product. Please check your inputs.';
        this.submitting = false;
      }
    });
  }

  delete(id: number) {
    if (!confirm('Delete this product?')) return;
    this.deleteError = '';
    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.successMessage = 'Product deleted successfully.';
        setTimeout(() => this.successMessage = '', 3000);
        this.loadProducts();
      },
      error: (err) => {
        this.deleteError = err?.error?.message ?? 'Failed to delete product.';
        setTimeout(() => this.deleteError = '', 4000);
      }
    });
  }

  goToDashboard() {
    this.router.navigate(['/admin']);
  }

  cancelForm() {
    this.showForm = false;
    this.formError = '';
    this.form.reset();
  }
}