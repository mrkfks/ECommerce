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
  phone?: string;
  country?: string;
  address?: string;
  city?: string;
  state?: string;  // İlçe
  postalCode?: string;
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
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
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
  Cancelled = 4,
  Received = 5,
  Completed = 6
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

// Campaign Models
export interface Campaign {
  id: number;
  name: string;
  description?: string;
  startDate: Date;
  endDate: Date;
  discountPercent: number;
  bannerImageUrl?: string;
  isActive: boolean;
}

export interface ProductCampaign {
  productId: number;
  campaignId: number;
  originalPrice: number;
  discountedPrice: number;
  discountPercent?: number;
  campaign?: Campaign;
}
