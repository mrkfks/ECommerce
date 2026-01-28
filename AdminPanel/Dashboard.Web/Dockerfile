# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src



# Proje dosyalarını (.csproj) klasör yapılarıyla birlikte kopyalıyoruz
# NuGet restore işleminin cache mekanizmasını kullanabilmesi için bu adım önemlidir.
COPY ["ECommerce.slnx", "./"]
COPY ["src/Core/ECommerce.Domain/ECommerce.Domain.csproj", "src/Core/ECommerce.Domain/"]
COPY ["src/Core/ECommerce.Application/ECommerce.Application.csproj", "src/Core/ECommerce.Application/"]
COPY ["src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/Infrastructure/ECommerce.Infrastructure/"]
COPY ["AdminPanel/Dashboard.Web/Dashboard.Web.csproj", "AdminPanel/Dashboard.Web/"]

# Bağımlılıkları geri yükle
RUN dotnet restore "AdminPanel/Dashboard.Web/Dashboard.Web.csproj"

# Tüm kaynak kodları kopyala
# Docker Context '.' olduğu için root dizindeki her şeyi Docker imajına alırız
COPY . .

# Dashboard klasörüne geç ve projeyi yayınla
WORKDIR "/src/AdminPanel/Dashboard.Web"
RUN dotnet publish "Dashboard.Web.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Güvenlik için uygulama kullanıcısı oluştur
RUN adduser --disabled-password --gecos "" appuser

# Yayınlanmış dosyaları kopyala
COPY --from=build /app/publish .

# Dosya yetkilerini ayarla
RUN chown -R appuser:appuser /app

# Uygulamayı appuser kullanıcısı ile çalıştır
USER appuser

# Ortam değişkenleri
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Render üzerinde port eşleşmesi için EXPOSE önemli
EXPOSE 5001

ENTRYPOINT ["dotnet", "Dashboard.Web.dll"]
