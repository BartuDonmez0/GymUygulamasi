# PowerShell script - Veritabanını silmek için
# Kullanım: .\GymApp\Scripts\DropDatabase.ps1

$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"

Write-Host "Veritabanı siliniyor..." -ForegroundColor Yellow

# PostgreSQL şifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    # Mevcut bağlantıları kes
    Write-Host "Mevcut bağlantılar kesiliyor..." -ForegroundColor Yellow
    $terminateQuery = "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '$dbName' AND pid <> pg_backend_pid();"
    psql -U $dbUser -h $dbHost -p $dbPort -c $terminateQuery 2>&1 | Out-Null
    
    # Veritabanını sil
    Write-Host "Veritabanı siliniyor..." -ForegroundColor Yellow
    $dropQuery = "DROP DATABASE IF EXISTS `"$dbName`";"
    psql -U $dbUser -h $dbHost -p $dbPort -c $dropQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Veritabanı başarıyla silindi!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Veritabanı zaten yok veya silinemedi" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Hata oluştu: $_" -ForegroundColor Red
} finally {
    # Environment variable'ı temizle
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}
