import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap, of, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product, ProductFilter, PagedResponse } from '../../models/product';
import { Category } from '../../models/category';
import { Review, ReviewRequest } from '../../models/review';

// shape the backend returns
interface BackendProduct {
  productId:    number;
  name:         string;
  description:  string;
  price:        number;
  imageUrl:     string;
  stock:        number;
  categoryId:   number;
  categoryName: string;
}

interface BackendListResponse {
  message: string;
  count:   number;
  data:    BackendProduct[];
}

interface BackendSingleResponse {
  message: string;
  data:    BackendProduct;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiUrl     = environment.apiUrl;
  private readonly apiBaseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}
  private mapProduct(bp: any): Product {
    if (!bp) return this.emptyProduct();

    const resolveImage = (url: string | null | undefined): string => {
      if (!url || url.trim() === '') return '';
      if (url.startsWith('http')) return url;
      return `${this.apiBaseUrl}${url}`;
    };

    // Support both camelCase and PascalCase 
    const id          = bp.productId   ?? bp.ProductId   ?? bp.id   ?? 0;
    const name        = bp.name        ?? bp.Name        ?? '';
    const description = bp.description ?? bp.Description ?? '';
    const price       = bp.price       ?? bp.Price       ?? 0;
    const origPrice   = bp.originalPrice ?? bp.OriginalPrice ?? bp.original_price ?? undefined;
    const imageUrl    = bp.imageUrl    ?? bp.ImageUrl    ?? bp.image_url ?? '';
    const stock       = bp.stock       ?? bp.Stock       ?? 0;
    const categoryId  = bp.categoryId  ?? bp.CategoryId  ?? bp.category_id ?? 0;
    const categoryName= bp.categoryName?? bp.CategoryName?? bp.category?.name ?? 'General';

    return {
      id,
      name,
      description,
      price,
      originalPrice: origPrice,
      imageUrl:      resolveImage(imageUrl),
      stock,
      categoryId,
      category:    { id: categoryId, name: categoryName },
      rating:      bp.rating      ?? bp.Rating      ?? 0,
      reviewCount: bp.reviewCount ?? bp.ReviewCount ?? 0,
      isActive:    bp.isActive    ?? bp.IsActive    ?? true
    };
  }

  private emptyProduct(): Product {
    return { id: 0, name: '', description: '', price: 0, imageUrl: '', stock: 0,
             categoryId: 0, category: { id: 0, name: '' }, rating: 0, reviewCount: 0, isActive: false };
  }

  getProducts(filter: ProductFilter = {}): Observable<PagedResponse<Product>> {
    return this.http
      .get<any>(`${this.apiUrl}/products`, { params: { page: 1, pageSize: 100 } })
      .pipe(
        tap(res => console.log('[ProductService] /products raw response:', JSON.stringify(res)?.slice(0,300))),
        map(res => {
          // Normalise .NET response into flat array
          let rawList: any[] = [];
          if (Array.isArray(res))                        rawList = res;
          else if (Array.isArray(res?.data?.items))      rawList = res.data.items;
          else if (Array.isArray(res?.data?.products))   rawList = res.data.products;
          else if (Array.isArray(res?.data))             rawList = res.data;
          else if (Array.isArray(res?.items))            rawList = res.items;
          else if (Array.isArray(res?.products))         rawList = res.products;
          else if (Array.isArray(res?.content))          rawList = res.content;
          else if (Array.isArray(res?.result))           rawList = res.result;
          else if (Array.isArray(res?.Results))          rawList = res.Results;
          else if (Array.isArray(res?.Data))             rawList = res.Data;
          console.log('[ProductService] rawList length:', rawList.length, '| first item:', JSON.stringify(rawList[0])?.slice(0,200));

          let products = rawList.map(p => this.mapProduct(p));

          // Client-side filtering
          if (filter.keyword) {
            const kw = filter.keyword.toLowerCase();
            products = products.filter(p =>
              p.name.toLowerCase().includes(kw) ||
              p.description.toLowerCase().includes(kw)
            );
          }
          if (filter.categoryId) {
            products = products.filter(p => p.categoryId === filter.categoryId);
          }
          if (filter.minPrice !== undefined) {
            products = products.filter(p => p.price >= filter.minPrice!);
          }
          if (filter.maxPrice !== undefined) {
            products = products.filter(p => p.price <= filter.maxPrice!);
          }
          if (filter.minRating !== undefined) {
            products = products.filter(p => p.rating >= filter.minRating!);
          }
          if (filter.sortBy === 'price_asc')  products.sort((a, b) => a.price - b.price);
          if (filter.sortBy === 'price_desc') products.sort((a, b) => b.price - a.price);
          if (filter.sortBy === 'rating')     products.sort((a, b) => b.rating - a.rating);

          return {
            content:       products,
            totalElements: products.length,
            totalPages:    1,
            size:          products.length,
            number:        0
          } as PagedResponse<Product>;
        }),
        catchError(err => {
          console.error('[ProductService] getProducts error:', err?.status, err?.message, err);
          return throwError(() => err);
        })
      );
  }

  getProductById(id: number): Observable<Product> {
    return this.http
      .get<any>(`${this.apiUrl}/products/${id}`)
      .pipe(map(res => this.mapProduct(res?.data ?? res)));
  }

  getFeaturedProducts(): Observable<Product[]> {
    return this.getProducts({ size: 10 }).pipe(map(res => res.content));
  }

  //  Admin CRUD
  createProduct(product: Partial<Product>, imageFile?: File): Observable<Product> {
    const form = new FormData();
    form.append('name',        product.name        ?? '');
    form.append('description', product.description ?? '');
    form.append('price',       String(product.price ?? 0));
    form.append('stock',       String(product.stock ?? 0));
    form.append('categoryId',  String(Number((product as any).categoryId ?? product.category?.id ?? 1)));
    if (imageFile) {
      form.append('imageFile', imageFile, imageFile.name);
    }

    return this.http
      .post<BackendSingleResponse>(`${this.apiUrl}/products`, form)
      .pipe(map(res => this.mapProduct(res.data)));
  }

  updateProduct(id: number, product: Partial<Product>, imageFile?: File): Observable<Product> {
    const form = new FormData();
    form.append('name',        product.name        ?? '');
    form.append('description', product.description ?? '');
    form.append('price',       String(product.price ?? 0));
    form.append('stock',       String(product.stock ?? 0));
    form.append('categoryId',  String(Number((product as any).categoryId ?? product.category?.id ?? 1)));
    if (imageFile) {
      form.append('imageFile', imageFile, imageFile.name);
    }

    return this.http
      .put<BackendSingleResponse>(`${this.apiUrl}/products/${id}`, form)
      .pipe(map(res => this.mapProduct(res.data)));
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/products/${id}`);
  }

  //  Categories
  getCategories(): Observable<Category[]> {
    return this.http.get<any>(`${this.apiUrl}/categories`).pipe(
      //log the raw response to see exactly what shape the backend sends
      tap(res => console.log('[ProductService] /categories raw:', JSON.stringify(res)?.slice(0, 400))),
      map(res => {
        let raw: any[] = [];
        if (Array.isArray(res))                      raw = res;
        else if (Array.isArray(res?.data?.items))    raw = res.data.items;
        else if (Array.isArray(res?.data))           raw = res.data;
        else if (Array.isArray(res?.items))          raw = res.items;
        else if (Array.isArray(res?.result))         raw = res.result;
        else if (Array.isArray(res?.Results))        raw = res.Results;
        else if (Array.isArray(res?.Data))           raw = res.Data;

        console.log('[ProductService] categories raw count:', raw.length);

        return raw.map((c: any) => ({
          id:          c.id          ?? c.categoryId  ?? c.CategoryId  ?? 0,
          name:        c.name        ?? c.Name        ?? '',
          description: c.description ?? c.Description ?? ''
        })) as Category[];
      }),
      catchError(err => {
        console.error('[ProductService] getCategories error:', err?.status, err?.message);
        return throwError(() => err);
      })
    );
  }

  createCategory(c: Partial<Category>): Observable<Category> {
    return this.http.post<Category>(`${this.apiUrl}/categories`, c);
  }

  updateCategory(id: number, c: Partial<Category>): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/categories/${id}`, c);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/categories/${id}`);
  }

  //  Reviews, Search
  getProductReviews(productId: number): Observable<Review[]>   { return of([]); }
  submitReview(review: ReviewRequest): Observable<Review> {
    const mock: Review = {
      id:        Date.now(),
      user:      { id: 0, name: 'You' },
      rating:    review.rating,
      title:     review.title,
      comment:   review.comment,
      createdAt: new Date().toISOString()
    };
    return of(mock);
  }
  searchSuggestions(keyword: string): Observable<string[]>      { return of([]); }
  getRelatedProducts(productId: number): Observable<Product[]>  { return of([]); }
}