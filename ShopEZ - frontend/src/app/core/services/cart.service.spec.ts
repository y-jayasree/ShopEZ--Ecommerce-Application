import { CartService } from './cart.service';
import { Product } from '../../models/product';

const mockProduct = (id: number, price: number): Product => ({
  id, name: `Product ${id}`, description: '', price,
  imageUrl: '', stock: 10, categoryId: 1,
  category: { id: 1, name: 'Test' }, rating: 0, reviewCount: 0, isActive: true,
});

describe('CartService', () => {
  let service: CartService;

  beforeEach(() => {
    localStorage.clear();
    service = new CartService();
  });

  afterEach(() => localStorage.clear());

  it('should create service', () => {
    expect(service).toBeTruthy();
  });

  it('should start with empty cart', () => {
    const cart = service.getCart();
    expect(cart.items.length).toBe(0);
    expect(cart.totalItems).toBe(0);
    expect(cart.totalPrice).toBe(0);
  });

  //  addToCart 
  it('should add a product to cart', () => {
    service.addToCart(mockProduct(1, 100));
    expect(service.getCart().items.length).toBe(1);
  });

  it('should default quantity to 1', () => {
    service.addToCart(mockProduct(1, 100));
    expect(service.getCart().items[0].quantity).toBe(1);
  });

  it('should add with a custom quantity', () => {
    service.addToCart(mockProduct(1, 100), 3);
    expect(service.getCart().items[0].quantity).toBe(3);
  });

  it('should accumulate quantity for the same product', () => {
    service.addToCart(mockProduct(1, 100));
    service.addToCart(mockProduct(1, 100), 2);
    expect(service.getCart().items.length).toBe(1);
    expect(service.getCart().items[0].quantity).toBe(3);
  });

  it('should keep distinct products as separate items', () => {
    service.addToCart(mockProduct(1, 100));
    service.addToCart(mockProduct(2, 200));
    expect(service.getCart().items.length).toBe(2);
  });

  //  totals 
  it('should calculate totalPrice correctly', () => {
    service.addToCart(mockProduct(1, 50), 2);
    service.addToCart(mockProduct(2, 30), 1);
    expect(service.getCart().totalPrice).toBe(130);
  });

  it('should calculate totalItems correctly', () => {
    service.addToCart(mockProduct(1, 50), 2);
    service.addToCart(mockProduct(2, 30), 3);
    expect(service.getCart().totalItems).toBe(5);
  });

  it('getItemCount should return totalItems', () => {
    service.addToCart(mockProduct(1, 50), 4);
    expect(service.getItemCount()).toBe(4);
  });

  //  removeFromCart 
  it('should remove an existing product', () => {
    service.addToCart(mockProduct(1, 100));
    service.removeFromCart(1);
    expect(service.getCart().items.length).toBe(0);
  });

  it('should not change cart when removing unknown product', () => {
    service.addToCart(mockProduct(1, 100));
    service.removeFromCart(99);
    expect(service.getCart().items.length).toBe(1);
  });

  it('should recalculate totals after removal', () => {
    service.addToCart(mockProduct(1, 100));
    service.addToCart(mockProduct(2, 50));
    service.removeFromCart(1);
    expect(service.getCart().totalPrice).toBe(50);
    expect(service.getCart().totalItems).toBe(1);
  });

  //  updateQuantity 
  it('should update quantity for a product', () => {
    service.addToCart(mockProduct(1, 100));
    service.updateQuantity(1, 5);
    expect(service.getCart().items[0].quantity).toBe(5);
  });

  it('should remove product when quantity set to 0', () => {
    service.addToCart(mockProduct(1, 100));
    service.updateQuantity(1, 0);
    expect(service.getCart().items.length).toBe(0);
  });

  it('should remove product when quantity set to negative', () => {
    service.addToCart(mockProduct(1, 100));
    service.updateQuantity(1, -1);
    expect(service.getCart().items.length).toBe(0);
  });

  //  clearCart 
  it('should clear all items', () => {
    service.addToCart(mockProduct(1, 100));
    service.addToCart(mockProduct(2, 200));
    service.clearCart();
    expect(service.getCart().items.length).toBe(0);
    expect(service.getCart().totalPrice).toBe(0);
    expect(service.getCart().totalItems).toBe(0);
  });

  it('should remove cart key from localStorage on clear', () => {
    service.addToCart(mockProduct(1, 100));
    service.clearCart();
    expect(localStorage.getItem('shopez_cart')).toBeNull();
  });

  //  isInCart 
  it('should return true when product is in cart', () => {
    service.addToCart(mockProduct(1, 100));
    expect(service.isInCart(1)).toBeTrue();
  });

  it('should return false when product is not in cart', () => {
    expect(service.isInCart(99)).toBeFalse();
  }); 
  it('should persist cart to localStorage', () => {
    service.addToCart(mockProduct(1, 100));
    const stored = localStorage.getItem('shopez_cart');
    expect(stored).toBeTruthy();
    expect(JSON.parse(stored!).items.length).toBe(1);
  });

  it('should load cart from localStorage on init', () => {
    const cart = { items: [{ product: mockProduct(1, 100), quantity: 2 }], totalItems: 2, totalPrice: 200 };
    localStorage.setItem('shopez_cart', JSON.stringify(cart));
    const newService = new CartService();
    expect(newService.getCart().items.length).toBe(1);
    expect(newService.getCart().totalPrice).toBe(200);
  });

  it('should start empty when localStorage has corrupted data', () => {
    localStorage.setItem('shopez_cart', 'not-valid-json{{{');
    const newService = new CartService();
    expect(newService.getCart().items.length).toBe(0);
  });

  //  cart observable 
  it('should emit updated cart via cart$', () => {
    let emitted: any = null;
    service.cart$.subscribe(c => (emitted = c));
    service.addToCart(mockProduct(1, 100));
    expect(emitted.items.length).toBe(1);
  });
});