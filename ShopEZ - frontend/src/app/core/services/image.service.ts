import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ImageService {
  private readonly base = environment.apiBaseUrl;
  private readonly fallback = 'https://placehold.co/200x200/f8f9fa/999?text=ShopEZ';

  resolve(url: string | null | undefined): string {
    if (!url || url.trim() === '') return this.fallback;
    if (url.startsWith('http'))   return url;
    return `${this.base}${url}`;
  }

  getFallback(): string {
    return this.fallback;
  }

  onError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img.src !== this.fallback) {
      img.src = this.fallback;
    }
  }
}