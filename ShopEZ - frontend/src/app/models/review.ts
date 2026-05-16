export interface Review {
  id: number;
  user: { id: number; name: string };
  rating: number;
  title: string;
  comment: string;
  createdAt: string;
}

export interface ReviewRequest {
  productId: number;
  rating: number;
  title: string;
  comment: string;
}