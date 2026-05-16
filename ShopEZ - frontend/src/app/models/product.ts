import { Category } from "./category";

export interface Product {
  id:             number;
  name:           string;
  description:    string;
  price:          number;
  originalPrice?: number;
  stock:          number;
  imageUrl:       string;
  images?:        string[];
  category:       Category;
  categoryId?:    number;
  rating:         number;
  reviewCount:    number;
  brand?:         string;
  tags?:          string[];
  createdAt?:     string;
  isActive:       boolean;
}

export interface ProductFilter {
  categoryId?: number;
  minPrice?:   number;
  maxPrice?:   number;
  minRating?:  number;
  keyword?:    string;
  sortBy?:     'price_asc' | 'price_desc' | 'newest' | 'popularity' | 'rating';
  page?:       number;
  size?:       number;
}

export interface PagedResponse<T> {
  content:       T[];
  totalElements: number;
  totalPages:    number;
  size:          number;
  number:        number;
}