export interface User {
  id: number;
  name: string;
  email: string;
  role: 'CUSTOMER' | 'ADMIN';
  phone?: string;
  createdAt?: string;
}
 
export interface AuthRequest {
  email: string;
  password: string;
}
 
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  phone?: string;
}
 
export interface AuthResponse {
  token: string;
  user: User;
}