# PowerShell Script - Veritabanını Sıfırlama ve Yeniden Oluşturma
# Kullanım: .\GymApp\Scripts\ResetDatabase.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Veritabanı Sıfırlama İşlemi" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# PostgreSQL bağlantı bilgileri
$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"

# PostgreSQL şifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    Write-Host "[1/5] Mevcut bağlantıları kesiliyor..." -ForegroundColor Yellow
    $terminateQuery = "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '$dbName' AND pid <> pg_backend_pid();"
    psql -U $dbUser -h $dbHost -p $dbPort -c $terminateQuery 2>&1 | Out-Null
    
    Write-Host "[2/5] Veritabanı siliniyor..." -ForegroundColor Yellow
    $dropQuery = "DROP DATABASE IF EXISTS `"$dbName`";"
    psql -U $dbUser -h $dbHost -p $dbPort -c $dropQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Veritabanı silindi" -ForegroundColor Green
    } else {
        Write-Host "⚠ Veritabanı zaten yok veya silinemedi" -ForegroundColor Yellow
    }
    
    Write-Host "[3/5] Yeni veritabanı oluşturuluyor..." -ForegroundColor Yellow
    $createQuery = "CREATE DATABASE `"$dbName`";"
    psql -U $dbUser -h $dbHost -p $dbPort -c $createQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Veritabanı oluşturuldu" -ForegroundColor Green
    } else {
        Write-Host "✗ Veritabanı oluşturulamadı!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "[4/5] Migration oluşturuluyor..." -ForegroundColor Yellow
    $migrationResult = dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migration oluşturuldu" -ForegroundColor Green
    } else {
        Write-Host "✗ Migration oluşturulamadı!" -ForegroundColor Red
        Write-Host $migrationResult -ForegroundColor Red
        exit 1
    }
    
    Write-Host "[5/5] Migration uygulanıyor..." -ForegroundColor Yellow
    $updateResult = dotnet ef database update --project GymApp/GymApp.csproj 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migration uygulandı" -ForegroundColor Green
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  İşlem Başarıyla Tamamlandı!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
    } else {
        Write-Host "✗ Migration uygulanamadı!" -ForegroundColor Red
        Write-Host $updateResult -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "✗ Hata oluştu: $_" -ForegroundColor Red
    exit 1
} finally {
    # Environment variable'ı temizle
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

