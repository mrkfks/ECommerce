# API Kullanım Örnekleri

Bu dosya, ECommerce API'yi farklı platformlardan nasıl kullanacağınızı gösterir.

## 📋 İçindekiler

1. [JavaScript/TypeScript (Fetch API)](#javascript-fetch)
2. [React Örneği](#react)
3. [Vue.js Örneği](#vuejs)
4. [Angular Örneği](#angular)
5. [cURL Örnekleri](#curl)
6. [Postman Collection](#postman)

---

## 🌐 JavaScript (Fetch API) {#javascript-fetch}

### Temel Yapılandırma

```javascript
const API_BASE_URL = 'http://localhost:5000/api/v1';
let authToken = null;

// Token'ı localStorage'dan yükle
authToken = localStorage.getItem('authToken');

// API istek helper fonksiyonu
async function apiRequest(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    
    const config = {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
    };
    
    // Token varsa ekle
    if (authToken) {
        config.headers['Authorization'] = `Bearer ${authToken}`;
    }
    
    try {
        const response = await fetch(url, config);
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.detail || 'API isteği başarısız');
        }
        
        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}
```

### Login

```javascript
async function login(email, password) {
    const response = await apiRequest('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
    });
    
    authToken = response.token;
    localStorage.setItem('authToken', authToken);
    
    return response;
}

// Kullanım
login('user@example.com', 'password123')
    .then(data => console.log('Login başarılı:', data))
    .catch(err => console.error('Login hatası:', err));
```

### Ürün Listesi Çekme

```javascript
async function getProducts() {
    return await apiRequest('/product');
}

// Kullanım
getProducts()
    .then(products => console.log('Ürünler:', products))
    .catch(err => console.error('Hata:', err));
```

### Ürün Detayı

```javascript
async function getProduct(id) {
    return await apiRequest(`/product/${id}`);
}

// Kullanım
getProduct(1)
    .then(product => console.log('Ürün:', product))
    .catch(err => console.error('Hata:', err));
```

### Yeni Ürün Oluşturma

```javascript
async function createProduct(productData) {
    return await apiRequest('/product', {
        method: 'POST',
        body: JSON.stringify(productData)
    });
}

// Kullanım
const newProduct = {
    name: 'Yeni Ürün',
    description: 'Ürün açıklaması',
    price: 99.99,
    categoryId: 1,
    brandId: 1,
    companyId: 1,
    stockQuantity: 100,
    imageUrl: 'https://example.com/image.jpg'
};

createProduct(newProduct)
    .then(result => console.log('Ürün oluşturuldu:', result))
    .catch(err => console.error('Hata:', err));
```

### Ürün Arama

```javascript
async function searchProducts(searchTerm) {
    return await apiRequest(`/product/search?searchTerm=${encodeURIComponent(searchTerm)}`);
}

// Kullanım
searchProducts('laptop')
    .then(results => console.log('Arama sonuçları:', results))
    .catch(err => console.error('Hata:', err));
```

---

## ⚛️ React Örneği {#react}

### API Service (useApi Hook)

```typescript
// hooks/useApi.ts
import { useState, useCallback } from 'react';

const API_BASE_URL = 'http://localhost:5000/api/v1';

interface ApiOptions extends RequestInit {
    requiresAuth?: boolean;
}

export function useApi() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const request = useCallback(async <T>(endpoint: string, options: ApiOptions = {}): Promise<T> => {
        setLoading(true);
        setError(null);

        try {
            const token = localStorage.getItem('authToken');
            
            const config: RequestInit = {
                ...options,
                headers: {
                    'Content-Type': 'application/json',
                    ...(token && options.requiresAuth !== false ? { 
                        'Authorization': `Bearer ${token}` 
                    } : {}),
                    ...options.headers,
                },
            };

            const response = await fetch(`${API_BASE_URL}${endpoint}`, config);

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.detail || 'API isteği başarısız');
            }

            return await response.json();
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Bilinmeyen hata';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    return { request, loading, error };
}
```

### Product Service

```typescript
// services/productService.ts
import { useApi } from '../hooks/useApi';

export interface Product {
    id: number;
    name: string;
    description: string;
    price: number;
    categoryId: number;
    brandId: number;
    companyId: number;
    stockQuantity: number;
    imageUrl?: string;
}

export function useProductService() {
    const { request, loading, error } = useApi();

    const getProducts = async () => {
        return request<Product[]>('/product');
    };

    const getProduct = async (id: number) => {
        return request<Product>(`/product/${id}`);
    };

    const createProduct = async (product: Omit<Product, 'id'>) => {
        return request<Product>('/product', {
            method: 'POST',
            body: JSON.stringify(product),
        });
    };

    const updateProduct = async (id: number, product: Partial<Product>) => {
        return request<Product>(`/product/${id}`, {
            method: 'PUT',
            body: JSON.stringify(product),
        });
    };

    const deleteProduct = async (id: number) => {
        return request<void>(`/product/${id}`, {
            method: 'DELETE',
        });
    };

    return {
        getProducts,
        getProduct,
        createProduct,
        updateProduct,
        deleteProduct,
        loading,
        error,
    };
}
```

### React Component Örneği

```tsx
// components/ProductList.tsx
import React, { useEffect, useState } from 'react';
import { useProductService, Product } from '../services/productService';

export const ProductList: React.FC = () => {
    const [products, setProducts] = useState<Product[]>([]);
    const { getProducts, loading, error } = useProductService();

    useEffect(() => {
        loadProducts();
    }, []);

    const loadProducts = async () => {
        try {
            const data = await getProducts();
            setProducts(data);
        } catch (err) {
            console.error('Ürünler yüklenemedi:', err);
        }
    };

    if (loading) return <div>Yükleniyor...</div>;
    if (error) return <div>Hata: {error}</div>;

    return (
        <div className="product-list">
            <h2>Ürünler</h2>
            <div className="products">
                {products.map(product => (
                    <div key={product.id} className="product-card">
                        <img src={product.imageUrl || '/placeholder.jpg'} alt={product.name} />
                        <h3>{product.name}</h3>
                        <p>{product.description}</p>
                        <p className="price">{product.price} TL</p>
                        <p>Stok: {product.stockQuantity}</p>
                    </div>
                ))}
            </div>
        </div>
    );
};
```

---

## 🟢 Vue.js Örneği {#vuejs}

### API Service

```typescript
// services/api.ts
import axios, { AxiosInstance } from 'axios';

class ApiService {
    private api: AxiosInstance;

    constructor() {
        this.api = axios.create({
            baseURL: 'http://localhost:5000/api/v1',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        // Request interceptor - token ekle
        this.api.interceptors.request.use(
            (config) => {
                const token = localStorage.getItem('authToken');
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => Promise.reject(error)
        );

        // Response interceptor - hata yönetimi
        this.api.interceptors.response.use(
            (response) => response,
            (error) => {
                if (error.response?.status === 401) {
                    // Token geçersiz - logout
                    localStorage.removeItem('authToken');
                    window.location.href = '/login';
                }
                return Promise.reject(error);
            }
        );
    }

    async getProducts() {
        const { data } = await this.api.get('/product');
        return data;
    }

    async getProduct(id: number) {
        const { data } = await this.api.get(`/product/${id}`);
        return data;
    }

    async createProduct(product: any) {
        const { data } = await this.api.post('/product', product);
        return data;
    }

    async updateProduct(id: number, product: any) {
        const { data } = await this.api.put(`/product/${id}`, product);
        return data;
    }

    async deleteProduct(id: number) {
        const { data } = await this.api.delete(`/product/${id}`);
        return data;
    }
}

export default new ApiService();
```

### Vue Component (Composition API)

```vue
<!-- ProductList.vue -->
<template>
  <div class="product-list">
    <h2>Ürünler</h2>
    
    <div v-if="loading">Yükleniyor...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    
    <div v-else class="products">
      <div v-for="product in products" :key="product.id" class="product-card">
        <img :src="product.imageUrl || '/placeholder.jpg'" :alt="product.name" />
        <h3>{{ product.name }}</h3>
        <p>{{ product.description }}</p>
        <p class="price">{{ product.price }} TL</p>
        <p>Stok: {{ product.stockQuantity }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '@/services/api';

interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  imageUrl?: string;
}

const products = ref<Product[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const loadProducts = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    products.value = await api.getProducts();
  } catch (err: any) {
    error.value = err.message || 'Ürünler yüklenemedi';
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadProducts();
});
</script>
```

---

## 🔴 Angular Örneği {#angular}

### API Service

```typescript
// services/api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  categoryId: number;
  brandId: number;
  companyId: number;
  stockQuantity: number;
  imageUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { 'Authorization': `Bearer ${token}` } : {})
    });
  }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/product`, {
      headers: this.getHeaders()
    });
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/product/${id}`, {
      headers: this.getHeaders()
    });
  }

  createProduct(product: Omit<Product, 'id'>): Observable<Product> {
    return this.http.post<Product>(`${this.apiUrl}/product`, product, {
      headers: this.getHeaders()
    });
  }

  updateProduct(id: number, product: Partial<Product>): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/product/${id}`, product, {
      headers: this.getHeaders()
    });
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/product/${id}`, {
      headers: this.getHeaders()
    });
  }
}
```

### HTTP Interceptor (Token otomatik ekleme)

```typescript
// interceptors/auth.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('authToken');
    
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return next.handle(req);
  }
}
```

---

## 🔧 cURL Örnekleri {#curl}

### Login

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

### Ürün Listesi (Public)

```bash
curl http://localhost:5000/api/v1/product
```

### Ürün Detayı

```bash
curl http://localhost:5000/api/v1/product/1
```

### Yeni Ürün Oluşturma (Authenticated)

```bash
curl -X POST http://localhost:5000/api/v1/product \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "name": "Yeni Ürün",
    "description": "Ürün açıklaması",
    "price": 99.99,
    "categoryId": 1,
    "brandId": 1,
    "companyId": 1,
    "stockQuantity": 100,
    "imageUrl": "https://example.com/image.jpg"
  }'
```

### Health Check

```bash
curl http://localhost:5000/health
```

---

## 📮 Postman Collection {#postman}

Postman collection dosyası: [Download](./postman/ECommerce-API.postman_collection.json)

### Collection Yapısı

```
ECommerce API
├── Auth
│   ├── Login
│   └── Register
├── Products
│   ├── Get All Products
│   ├── Get Product by ID
│   ├── Search Products
│   ├── Get Products by Category
│   ├── Create Product
│   ├── Update Product
│   ├── Update Stock
│   └── Delete Product
├── Categories
│   └── ...
└── Health
    └── Health Check
```

### Environment Variables

```json
{
  "api_url": "http://localhost:5000/api/v1",
  "auth_token": ""
}
```

---

## 🔐 Token Yönetimi Best Practices

### Token Saklama (Güvenli)

```javascript
// ✅ HttpOnly Cookie (En güvenli - Backend tarafında set edilmeli)
// API'den set-cookie header ile döner

// ⚠️ localStorage (XSS'e karşı dikkatli olun)
localStorage.setItem('authToken', token);

// ❌ sessionStorage (Sayfalar arası paylaşılmaz)
```

### Token Yenileme

```javascript
async function refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    
    const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
    });
    
    const { token } = await response.json();
    localStorage.setItem('authToken', token);
    
    return token;
}
```

### Otomatik Token Ekleme (Axios)

```javascript
import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5000/api/v1'
});

api.interceptors.request.use(config => {
    const token = localStorage.getItem('authToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;
```

---

## 📱 React Native Örneği

```typescript
// services/api.ts
import AsyncStorage from '@react-native-async-storage/async-storage';

const API_BASE_URL = 'http://10.0.2.2:5000/api/v1'; // Android emulator için

export async function apiRequest(endpoint: string, options: RequestInit = {}) {
    const token = await AsyncStorage.getItem('authToken');
    
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
            ...options.headers,
        },
    });
    
    if (!response.ok) {
        throw new Error(await response.text());
    }
    
    return response.json();
}
```

---

## ⚡ WebSocket (SignalR) - Gelecek Özellik

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/notification")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", (message) => {
    console.log("Notification:", message);
});

await connection.start();
```

---

## 🎯 Hata Yönetimi

### Standart Hata Formatı

```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Validation failed",
  "instance": "/api/v1/product",
  "traceId": "0HN1NL8H3V0OL:00000001",
  "errors": {
    "Name": ["Name is required"],
    "Price": ["Price must be greater than 0"]
  }
}
```

### Hata Yakalama

```javascript
try {
    const product = await createProduct(data);
} catch (error) {
    if (error.response) {
        // API'den dönen hata
        console.error('API Error:', error.response.data);
        
        if (error.response.data.errors) {
            // Validation hataları
            Object.keys(error.response.data.errors).forEach(field => {
                console.error(`${field}: ${error.response.data.errors[field].join(', ')}`);
            });
        }
    } else {
        // Network hatası
        console.error('Network Error:', error.message);
    }
}
```

---

Daha fazla örnek ve detay için [API Documentation](http://localhost:5000/swagger) sayfasını ziyaret edin.
