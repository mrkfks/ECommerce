# Docker Setup Guide

## Gereklilikler
- Docker (https://www.docker.com/products/docker-desktop)
- Docker Compose

## Başlatma

### Tüm Hizmetleri Başlat (API + Frontend)
```bash
docker-compose up -d
```

### Sadece Frontend'i Başlat
```bash
docker-compose up -d frontend
```

### Sadece API'yi Başlat
```bash
docker-compose up -d api
```

## Erişim

- **Frontend**: http://localhost:4200
- **API**: http://localhost:5010

## Environment Değişkenleri

`.env` dosyası oluşturup aşağıdaki değişkenleri ayarlayın:

```bash
# JWT Configuration
JWT_ISSUER=ECommerce
JWT_AUDIENCE=ECommerce.Client
JWT_KEY=your-secret-key-here-at-least-32-characters

# CORS Origins
CORS_ORIGIN_1=http://localhost:4200
CORS_ORIGIN_2=http://localhost:5041
```

## Docker Komutları

```bash
# Logs görüntüle
docker-compose logs -f frontend
docker-compose logs -f api

# Hizmetleri durdur
docker-compose down

# Hizmetleri yeniden başlat
docker-compose restart frontend
docker-compose restart api

# Konteyner içinde komut çalıştır
docker-compose exec frontend ash
docker-compose exec api bash

# Volume'leri temizle (verileri sil)
docker-compose down -v
```

## Build Optimization

### Frontend Image Boyutu
- **Build stage**: Node 20 Alpine
- **Runtime stage**: Nginx Alpine (daha hafif)
- Gzip compression etkin

### API Image Boyutu
- **.NET Runtime**: mcr.microsoft.com/dotnet/aspnet:10.0
- Multi-stage build ile optimized

## Production Deployment

Production ortamında:

1. Environment değişkenlerini ayarla
2. SSL sertifikası ekle (reverse proxy)
3. Database backup politikası belirle
4. Logging ve monitoring kuonfigürasyonu yap

```bash
# Production stack
docker-compose -f docker-compose.yml up -d
```

## Troubleshooting

### Frontend container başlamıyor
```bash
docker logs ecommerce-frontend
```

### API'ye bağlanamıyor
```bash
docker-compose exec frontend curl http://api:8080/api/category
```

### Port zaten kullanımda
```bash
# Port değiş (docker-compose.yml'de düzelt)
docker-compose down
# Veya farklı port kullan:
docker-compose up -d --publish 3000:4200
```

## Health Checks

Her hizmetin health check'i vardır. Durumunu kontrol et:

```bash
docker-compose ps
```

Health status görmek için:

```bash
docker inspect ecommerce-frontend --format='{{json .State.Health}}'
docker inspect ecommerce-api --format='{{json .State.Health}}'
```
