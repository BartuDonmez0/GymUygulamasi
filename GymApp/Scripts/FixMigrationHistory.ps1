# PowerShell script - Migration history'yi düzelt ve eksik kolonları ekle
# Kullanım: .\GymApp\Scripts\FixMigrationHistory.ps1

$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"
$sqlFile = "GymApp\Scripts\FixMigrationHistory.sql"

Write-Host "Migration history düzeltiliyor ve eksik kolonlar ekleniyor..." -ForegroundColor Yellow

# PostgreSQL şifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    # SQL dosyasını çalıştır
    psql -U $dbUser -h $dbHost -p $dbPort -d $dbName -f $sqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration history basariyla duzeltildi ve kolonlar eklendi!" -ForegroundColor Green
    } else {
        Write-Host "Islem basarisiz oldu!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Hata olustu: $_" -ForegroundColor Red
    exit 1
} finally {
    # Environment variable'ı temizle
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

