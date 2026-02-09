$uriGet = 'http://localhost:5001/Brand/Create'
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$res = Invoke-WebRequest -Uri $uriGet -WebSession $session -UseBasicParsing
if ($res.StatusCode -ne 200) { Write-Output "GET failed: $($res.StatusCode)"; exit 1 }
$content = $res.Content
if ($content -match 'name="__RequestVerificationToken" value="([^"]+)"') { $token = $matches[1] } else { $token = '' }
Write-Output "Token length: $($token.Length)"
$form = @{
    '__RequestVerificationToken' = $token
    'Name' = ''
    'CompanyId' = ''
    'Description' = ''
    'IsActive' = 'true'
}
$post = Invoke-WebRequest -Uri $uriGet -Method Post -Body $form -WebSession $session -UseBasicParsing -ErrorAction SilentlyContinue
if ($post -eq $null) { Write-Output 'POST returned null'; exit 1 }
Write-Output "POST Status: $($post.StatusCode)"
$content2 = $post.Content
if ($content2 -match 'Form hatası' -or $content2 -match 'Marka oluşturulurken hata oluştu' -or $content2 -match 'alert alert-danger') {
    Write-Output 'Found create error alert'
    exit 0
} else {
    Write-Output 'Create error alert NOT FOUND'
    exit 2
}
