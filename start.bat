@echo off
REM ECommerce API Quick Start Script (Windows)
REM Bu script projeyi hızlıca başlatmak için kullanılır

echo.
echo =============================================
echo   ECommerce API Quick Start (Windows)
echo =============================================
echo.

REM .NET kontrolü
echo Kontroller yapiliyor...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] .NET SDK bulunamadi!
    echo Lutfen .NET 10.0 SDK'yi yukleyin: https://dotnet.microsoft.com/download
    exit /b 1
)

dotnet --version
echo [OK] .NET SDK bulundu
echo.

REM Bağımlılıkları restore et
echo Bagimliliklar yukleniyor...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Bagimliliklar yuklenemedi!
    exit /b 1
)
echo [OK] Bagimliliklar yuklendi
echo.

REM Database migration
echo Database migration calistiriliyor...
cd src\Presentation\ECommerce.RestApi
dotnet ef database update --no-build
cd ..\..\..
echo [OK] Migration tamamlandi
echo.

REM Build
echo Proje derleniyor...
dotnet build -c Debug
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Proje derlenemedi!
    exit /b 1
)
echo [OK] Proje derlendi
echo.

REM Başarı mesajı
echo =============================================
echo [BASARILI] Tum islemler tamamlandi!
echo =============================================
echo.
echo Uygulamayi baslatmak icin:
echo    dotnet run --project src\Presentation\ECommerce.RestApi
echo.
echo Swagger UI:
echo    http://localhost:5000/swagger
echo.
echo Health Check:
echo    http://localhost:5000/health
echo.
echo Docker ile baslatmak icin:
echo    docker-compose up -d
echo.
pause
