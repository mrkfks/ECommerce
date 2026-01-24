# 🐳 Docker Deployment Kılavuzu

Bu belge, ECommerce projesinin Docker ile nasıl çalıştırılacağını açıklar.

## 📋 Gereksinimler

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac) veya Docker Engine (Linux)
- Docker Compose v2+
- En az 4GB RAM (önerilen: 8GB)

## 🚀 Hızlı Başlangıç

### 1. Environment Dosyasını Hazırlayın

```bash
# Örnek dosyayı kopyalayın
cp .env.docker.example .env

# .env dosyasını düzenleyin ve güvenli değerler girin
# Özellikle JWT_SECRET_KEY'i değiştirin!
```

### 2. Tüm Servisleri Başlatın

```bash
# İlk kez çalıştırma (image'ları oluşturur)
docker-compose up --build

# Arka planda çalıştırma
docker-compose up -d --build

# Sadece belirli servisleri başlatma
docker-compose up api dashboard
```

### 3. Servislere Erişim

| Servis | URL | Açıklama |
|--------|-----|----------|
| **Frontend** | http://localhost:4000 | Müşteri uygulaması |
| **API** | http://localhost:5000 | REST API |
| **Dashboard** | http://localhost:5001 | Admin paneli |
| **Nginx** | http://localhost | Reverse proxy (tüm servisler) |

## 🔧 Yararlı Komutlar

### Container Yönetimi

```bash
# Tüm container'ları durdur
docker-compose down

# Container'ları ve volume'ları sil (VERİLER SİLİNİR!)
docker-compose down -v

# Logları görüntüle
docker-compose logs -f

# Belirli servisin logları
docker-compose logs -f api

# Container içine gir
docker exec -it ecommerce-api sh
```

### Image Yönetimi

```bash
# Image'ları yeniden oluştur
docker-compose build --no-cache

# Belirli servisi yeniden oluştur
docker-compose build api

# Kullanılmayan image'ları temizle
docker image prune -a
```

### Veritabanı Yönetimi

```bash
# SQLite veritabanı volume'da saklanır
# Veriyi yedekle
docker cp ecommerce-api:/app/data/ECommerce.db ./backup/

# Veriyi geri yükle
docker cp ./backup/ECommerce.db ecommerce-api:/app/data/
```

## 🏗️ Mimari

```
                    ┌─────────────┐
                    │   Nginx     │ :80
                    │   Proxy     │
                    └──────┬──────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
    ┌──────────┐    ┌──────────┐    ┌──────────┐
    │ Frontend │    │   API    │    │Dashboard │
    │ Angular  │    │ .NET 9   │    │ .NET 9   │
    │  :4000   │    │  :5000   │    │  :5001   │
    └──────────┘    └────┬─────┘    └──────────┘
                         │
                    ┌────▼─────┐
                    │  SQLite  │
                    │ (Volume) │
                    └──────────┘
```

## 🔐 Production Güvenlik Ayarları

### 1. JWT Secret Key
```bash
# Güçlü bir secret key oluşturun
openssl rand -base64 64
```

### 2. HTTPS Yapılandırması
SSL sertifikalarını `./ssl` klasörüne koyun ve nginx yapılandırmasını güncelleyin.

### 3. Environment Variables
Production'da `.env` dosyası yerine container environment variables kullanın:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 🐛 Sorun Giderme

### Port Çakışması
```bash
# Hangi process'in portu kullandığını bul
netstat -ano | findstr :5000

# Windows'ta process'i durdur
taskkill /PID <PID> /F
```

### Container Başlamıyor
```bash
# Detaylı logları kontrol et
docker-compose logs --tail=100 api

# Container'ı interaktif modda başlat
docker-compose run --rm api sh
```

### Veritabanı Hatası
```bash
# Volume'u temizle ve yeniden başlat
docker-compose down -v
docker-compose up --build
```

## 📊 Kaynak Kullanımı İzleme

```bash
# Container kaynak kullanımı
docker stats

# Disk kullanımı
docker system df
```

## 🔄 CI/CD Entegrasyonu

GitHub Actions örneği `.github/workflows/docker.yml` dosyasında bulunabilir.

---

Sorularınız için lütfen issue açın veya ekibinizle iletişime geçin.
