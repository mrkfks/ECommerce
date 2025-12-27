#!/bin/bash

# ECommerce API Quick Start Script
# Bu script projeyi hızlıca başlatmak için kullanılır

set -e  # Hata durumunda dur

echo "🚀 ECommerce API Quick Start"
echo "============================"
echo ""

# Renk kodları
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# .NET kontrolü
echo "📋 Kontroller yapılıyor..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK bulunamadı!${NC}"
    echo "Lütfen .NET 10.0 SDK'yı yükleyin: https://dotnet.microsoft.com/download"
    exit 1
fi

echo -e "${GREEN}✅ .NET SDK bulundu: $(dotnet --version)${NC}"

# Bağımlılıkları restore et
echo ""
echo "📦 Bağımlılıklar yükleniyor..."
dotnet restore

# Database migration
echo ""
echo "🗄️  Database migration çalıştırılıyor..."
cd src/Presentation/ECommerce.RestApi
dotnet ef database update --no-build || true
cd ../../..

# Build
echo ""
echo "🔨 Proje derleniyor..."
dotnet build -c Debug

# Başarı mesajı
echo ""
echo -e "${GREEN}✅ Tüm işlemler başarıyla tamamlandı!${NC}"
echo ""
echo "🎯 Uygulamayı başlatmak için:"
echo -e "${YELLOW}   dotnet run --project src/Presentation/ECommerce.RestApi${NC}"
echo ""
echo "📚 Swagger UI:"
echo "   http://localhost:5000/swagger"
echo ""
echo "🏥 Health Check:"
echo "   http://localhost:5000/health"
echo ""
echo "🐳 Docker ile başlatmak için:"
echo -e "${YELLOW}   docker-compose up -d${NC}"
echo ""
