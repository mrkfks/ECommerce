// User & Auth Models
export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  username: string;
  role: string;
  companyId?: number;
  createdAt: Date;
  phone?: string; // Added phone property to User interface
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  username: string;
  phoneNumber: string;
  // Şirketle ilgili alanlar kaldırıldı
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  username: string;
  roles?: string[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface UserProfileUpdateRequest {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// Product Image Model
export interface ProductImage {
  id: number;
  productId: number;
  imageUrl: string;
  displayOrder?: number;
  order: number;
  isMain?: boolean;
  isPrimary: boolean;
}

// Product Models
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  originalPrice?: number;
  imageUrl: string;
  images?: ProductImage[];
  categoryId: number;
  categoryName?: string;
  brandId: number;
  brandName?: string;
  companyId: number;
  stockQuantity: number;
  rating: number;
  reviewCount: number;
  isNew?: boolean;
  discount?: number;
  isActive: boolean;
  inStock: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface ProductCreateRequest {
  name: string;
  description: string;
  price: number;
  categoryId: number;
  brandId: number;
  companyId: number;
  stockQuantity: number;
  imageUrl?: string;
}

export interface ProductUpdateRequest extends ProductCreateRequest {
  id: number;
}

// Category Models
export interface Category {
  id: number;
  name: string;
  description?: string;
  icon?: string;
  parentId?: number;
  productCount?: number;
  createdAt: Date;
}

export interface CategoryCreateRequest {
  name: string;
  description?: string;
  parentId?: number;
}

// Order Models
export interface Order {
  id: number;
  customerId: number;
  customerName: string;
  addressId: number;
  companyId: number;
  companyName: string;
  orderDate: Date;
  totalAmount: number;
  status: OrderStatus;
  statusText: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderCreateRequest {
  customerId: number;
  addressId: number;
  companyId: number;
  items: OrderItemCreateRequest[];
  shippingAddress?: AddressCreateRequest;
  cardNumber?: string;
  cardExpiry?: string;
  cardCvv?: string;
}

export interface AddressCreateRequest {
  customerId: number;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

export interface OrderItemCreateRequest {
  productId: number;
  quantity: number;
}

export enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4
}

// Cart Models
export interface CartItem {
  product: Product;
  quantity: number;
}

export interface Cart {
  items: CartItem[];
  totalItems: number;
  totalPrice: number;
}

// Brand Model
export interface Brand {
  id: number;
  name: string;
  description?: string;
  logoUrl?: string;
}

// Review Model
export interface Review {
  id: number;
  productId: number;
  customerId: number;
  customerName: string;
  rating: number;
  comment: string;
  createdAt: Date;
}

export interface ReviewCreateRequest {
  productId: number;
  rating: number;
  comment: string;
}

// Customer Model
export interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  companyId?: number;
}

export interface Address {
  id: number;
  customerId: number;
  title: string;
  addressLine: string;
  city: string;
  district: string;
  postalCode: string;
  isDefault: boolean;
}

// API Response Models
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
