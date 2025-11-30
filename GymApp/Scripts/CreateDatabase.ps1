# PowerShell script - Veritabanını oluşturmak için
# Kullanım: .\GymApp\Scripts\CreateDatabase.ps1

$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"

Write-Host "Veritabanı oluşturuluyor..." -ForegroundColor Yellow

# PostgreSQL şifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    # Veritabanını oluştur
    $createQuery = "CREATE DATABASE `"$dbName`" WITH OWNER = postgres ENCODING = 'UTF8' LC_COLLATE = 'Turkish_Turkey.1254' LC_CTYPE = 'Turkish_Turkey.1254' TABLESPACE = pg_default CONNECTION LIMIT = -1;"
    psql -U $dbUser -h $dbHost -p $dbPort -c $createQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Veritabanı başarıyla oluşturuldu!" -ForegroundColor Green
    } else {
        Write-Host "✗ Veritabanı oluşturulamadı!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Hata oluştu: $_" -ForegroundColor Red
    exit 1
} finally {
    # Environment variable'ı temizle
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

