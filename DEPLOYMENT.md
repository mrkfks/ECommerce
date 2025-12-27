# Production Deployment Guide

Bu kılavuz, ECommerce API'nin production ortamına deploy edilmesi için adım adım talimatlar içerir.

## 📋 Ön Hazırlık

### 1. Environment Variables Hazırlama

Production sunucusunda aşağıdaki environment variable'ları ayarlayın:

```bash
# .env dosyası oluştur
export ASPNETCORE_ENVIRONMENT=Production
export JWT_KEY="your-super-secret-key-minimum-32-characters-required"
export JWT_ISSUER="https://api.yourdomain.com"
export JWT_AUDIENCE="https://yourdomain.com"
export ConnectionStrings__DefaultConnection="Server=your-server;Database=ECommerce;User Id=sa;Password=YourPass;TrustServerCertificate=True"
export Cors__AllowedOrigins__0="https://yourdomain.com"
export Cors__AllowedOrigins__1="https://admin.yourdomain.com"
```

### 2. appsettings.Production.json Güncelleme

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Issuer": "",
    "Audience": "",
    "Key": "",
    "ExpiresInMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": []
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": 60
  }
}
```

**ÖNEMLİ**: Hassas bilgiler (Key, ConnectionStrings) environment variable'lardan okunmalı, dosyaya yazılmamalı.

## 🐳 Docker ile Deployment

### Tek Sunucuda Deployment

```bash
# 1. Repository'yi clone et
git clone https://github.com/yourusername/ecommerce.git
cd ecommerce

# 2. .env dosyası oluştur
cp .env.example .env
nano .env  # Değerleri düzenle

# 3. Docker Compose ile başlat
docker-compose up -d

# 4. Logları kontrol et
docker-compose logs -f api

# 5. Health check
curl http://localhost:5000/health
```

### Docker Image Build ve Push

```bash
# 1. Image oluştur
docker build -t yourusername/ecommerce-api:latest .

# 2. Docker Hub'a push et
docker login
docker push yourusername/ecommerce-api:latest

# 3. Sunucuda çalıştır
docker pull yourusername/ecommerce-api:latest
docker run -d \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JWT_KEY="${JWT_KEY}" \
  -e ConnectionStrings__DefaultConnection="${DB_CONNECTION}" \
  --name ecommerce-api \
  yourusername/ecommerce-api:latest
```

## 🖥️ Manuel Deployment (Linux Server)

### 1. .NET Runtime Kurulumu

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0 --runtime aspnetcore

# Path ekle
export PATH=$PATH:$HOME/.dotnet
```

### 2. Uygulama Publish

```bash
# Local makinede
cd ECommerce
dotnet publish src/Presentation/ECommerce.RestApi/ECommerce.RestApi.csproj \
  -c Release \
  -o ./publish

# Sunucuya kopyala
scp -r ./publish user@server:/var/www/ecommerce-api
```

### 3. Systemd Service Oluştur

```bash
# /etc/systemd/system/ecommerce-api.service
sudo nano /etc/systemd/system/ecommerce-api.service
```

```ini
[Unit]
Description=ECommerce API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/ecommerce-api
ExecStart=/usr/bin/dotnet /var/www/ecommerce-api/ECommerce.RestApi.dll

# Environment
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
EnvironmentFile=/etc/ecommerce-api/.env

# Restart policy
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=ecommerce-api

# Performance
TasksMax=infinity
LimitNOFILE=65536

[Install]
WantedBy=multi-user.target
```

```bash
# Service'i aktifleştir ve başlat
sudo systemctl daemon-reload
sudo systemctl enable ecommerce-api
sudo systemctl start ecommerce-api

# Status kontrol
sudo systemctl status ecommerce-api

# Logları kontrol et
sudo journalctl -u ecommerce-api -f
```

### 4. Nginx Reverse Proxy

```bash
sudo apt install nginx

# /etc/nginx/sites-available/ecommerce-api
sudo nano /etc/nginx/sites-available/ecommerce-api
```

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Health check endpoint
    location /health {
        proxy_pass http://localhost:5000/health;
        access_log off;
    }

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
    limit_req zone=api_limit burst=20;
}
```

```bash
# Site'ı aktifleştir
sudo ln -s /etc/nginx/sites-available/ecommerce-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 5. SSL Sertifikası (Let's Encrypt)

```bash
# Certbot kurulumu
sudo apt install certbot python3-certbot-nginx

# SSL sertifikası al
sudo certbot --nginx -d api.yourdomain.com

# Otomatik yenileme test et
sudo certbot renew --dry-run
```

## 🗄️ Database Setup

### SQL Server (Production)

```bash
# Docker ile SQL Server
docker run -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Password" \
  -p 1433:1433 \
  --name sql-server \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Connection String
Server=localhost,1433;Database=ECommerce;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True
```

### PostgreSQL (Alternatif)

```bash
# Docker ile PostgreSQL
docker run --name postgres \
  -e POSTGRES_USER=ecommerce \
  -e POSTGRES_PASSWORD=ecommerce123 \
  -e POSTGRES_DB=ecommerce \
  -p 5432:5432 \
  -d postgres:16-alpine

# Connection String
Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce123
```

### Migration Çalıştırma

```bash
# Production ortamında
cd /var/www/ecommerce-api
dotnet ef database update --no-build

# veya uygulama başlangıcında otomatik (zaten yapılandırılmış)
```

## 🔐 Güvenlik Checklist

- [ ] JWT_KEY environment variable'dan okunuyor
- [ ] Database şifresi güvenli ve karmaşık
- [ ] CORS sadece güvendiğiniz domainler için aktif
- [ ] HTTPS (SSL) aktif
- [ ] Rate limiting etkin
- [ ] Firewall yapılandırıldı (sadece 80, 443, 22 portları açık)
- [ ] Database yedekleme yapılandırıldı
- [ ] Loglar izleniyor
- [ ] Health checks izleniyor

## 📊 Monitoring

### Health Check URL
```
https://api.yourdomain.com/health
```

### Response
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "duration": "00:00:00.0234567"
    }
  ],
  "totalDuration": "00:00:00.0234567"
}
```

### Prometheus + Grafana (Opsiyonel)

```bash
# docker-compose.monitoring.yml
version: '3.8'
services:
  prometheus:
    image: prom/prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
  
  grafana:
    image: grafana/grafana
    ports:
      - "3000:3000"
```

## 🔄 CI/CD Pipeline

### GitHub Actions Örneği

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Build
      run: dotnet build -c Release
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Deploy to Server
      uses: appleboy/scp-action@master
      with:
        host: ${{ secrets.SERVER_HOST }}
        username: ${{ secrets.SERVER_USER }}
        key: ${{ secrets.SSH_KEY }}
        source: "./publish/*"
        target: "/var/www/ecommerce-api"
    
    - name: Restart Service
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.SERVER_HOST }}
        username: ${{ secrets.SERVER_USER }}
        key: ${{ secrets.SSH_KEY }}
        script: sudo systemctl restart ecommerce-api
```

## 🐛 Troubleshooting

### Service başlamıyor
```bash
# Logları kontrol et
sudo journalctl -u ecommerce-api -n 100 --no-pager

# Port dinliyor mu?
sudo netstat -tulpn | grep :5000

# .NET runtime var mı?
dotnet --version
```

### Database bağlantısı başarısız
```bash
# Connection string doğru mu?
echo $ConnectionStrings__DefaultConnection

# Database erişilebilir mi?
# SQL Server
sqlcmd -S localhost -U sa -P YourPassword

# PostgreSQL
psql -h localhost -U ecommerce -d ecommerce
```

### 502 Bad Gateway (Nginx)
```bash
# Service çalışıyor mu?
sudo systemctl status ecommerce-api

# Nginx error log
sudo tail -f /var/log/nginx/error.log
```

## 📞 Destek

Sorun yaşarsanız:
1. [Issues](https://github.com/yourusername/ecommerce/issues) bölümünde arayın
2. Yeni issue açın
3. [Wiki](https://github.com/yourusername/ecommerce/wiki) sayfasına bakın

## 📚 Kaynaklar

- [.NET Deployment Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Nginx Configuration](https://nginx.org/en/docs/)
- [Let's Encrypt](https://letsencrypt.org/getting-started/)
