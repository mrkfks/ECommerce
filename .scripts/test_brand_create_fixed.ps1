$dashboardUrl = "http://localhost:5001"
$loginUrl = "$dashboardUrl/Auth/Login"
$createBrandUrl = "$dashboardUrl/Brand/Create"

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

Write-Host "Step 1: Giriş yapılıyor..."
$loginResponse = Invoke-WebRequest -Uri $loginUrl -WebSession $session -UseBasicParsing
if ($loginResponse.StatusCode -ne 200) {
    Write-Host "Login GET failed: $($loginResponse.StatusCode)"
    exit 1
}

# Extract CSRF token from login form
$loginContent = $loginResponse.Content
$tokenPattern = 'name="__RequestVerificationToken" value="([^"]+)"'
if ($loginContent -match $tokenPattern) {
    $csrfToken = $matches[1]
    Write-Host "CSRF token alındı"
} else {
    Write-Host "ERROR: CSRF token bulunamadı"
    exit 1
}

# Login
$loginBody = @{
    '__RequestVerificationToken' = $csrfToken
    'Email' = 'superadmin'
    'Password' = '123123'
}

$loginPost = Invoke-WebRequest -Uri $loginUrl -Method Post -Body $loginBody -WebSession $session -UseBasicParsing -ErrorAction SilentlyContinue
Write-Host "Login response: $($loginPost.StatusCode)"

Write-Host "Step 2: Marka sayfasına erişiliyor..."
$brandCreateGet = Invoke-WebRequest -Uri $createBrandUrl -WebSession $session -UseBasicParsing
if ($brandCreateGet.StatusCode -ne 200) {
    Write-Host "Brand Create GET failed"
    exit 1
}
Write-Host "✓ Marka sayfasına erişildi"

# Extract CSRF token from brand create form
$brandContent = $brandCreateGet.Content
if ($brandContent -match $tokenPattern) {
    $brandCsrfToken = $matches[1]
    Write-Host "✓ Brand CSRF token alındı"
} else {
    Write-Host "ERROR: Brand CSRF token bulunamadı"
    exit 1
}

Write-Host "Step 3: Marka oluşturuluyor..."
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$brandName = "TestMarkaFix$timestamp"

$brandCreateBody = @{
    '__RequestVerificationToken' = $brandCsrfToken
    'Name' = $brandName
    'Description' = 'Test description'
    'IsActive' = 'true'
    'CompanyId' = '1'
}

$brandCreatePost = Invoke-WebRequest -Uri $createBrandUrl -Method Post -Body $brandCreateBody -WebSession $session -UseBasicParsing -ErrorAction SilentlyContinue
if ($brandCreatePost -eq $null) {
    Write-Host "ERROR: POST yanıt alınamadı"
    exit 1
}

Write-Host "Response Status: $($brandCreatePost.StatusCode)"

# Check if success
if ($brandCreatePost.StatusCode -eq 200) {
    $responseContent = $brandCreatePost.Content
    if ($responseContent -match 'Marka başarıyla oluşturuldu' -or $responseContent -match 'alert-success') {
        Write-Host "SUCCESS: Marka başarıyla oluşturuldu!"
        exit 0
    } elseif ($responseContent -match 'CreateError' -or $responseContent -match 'alert-danger') {
        Write-Host "ERROR: Create formunda hata mesajı gösterildi"
        exit 1
    } else {
        Write-Host "OK: Sayfa returned 200"
        exit 0
    }
} else {
    Write-Host "Unexpected status: $($brandCreatePost.StatusCode)"
    exit 1
}
