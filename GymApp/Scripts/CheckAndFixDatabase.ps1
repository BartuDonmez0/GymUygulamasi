# PowerShell script - Veritabani kontrol ve duzeltme
# Kullanim: .\GymApp\Scripts\CheckAndFixDatabase.ps1

$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"
$sqlFile = "GymApp\Scripts\CheckAndFixDatabase.sql"

Write-Host "Veritabani kontrol ediliyor ve duzeltiliyor..." -ForegroundColor Yellow

# PostgreSQL sifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    # SQL dosyasini calistir
    psql -U $dbUser -h $dbHost -p $dbPort -d $dbName -f $sqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Veritabani basariyla kontrol edildi ve duzeltildi!" -ForegroundColor Green
        
        # Veritabani durumunu goster
        Write-Host "`nVeritabani durumu:" -ForegroundColor Cyan
        psql -U $dbUser -h $dbHost -p $dbPort -d $dbName -c "SELECT 'gym_centers' as tablo, COUNT(*) as kayit_sayisi FROM gym_centers UNION ALL SELECT 'activities' as tablo, COUNT(*) as kayit_sayisi FROM activities;"
    } else {
        Write-Host "Islem basarisiz oldu!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Hata olustu: $_" -ForegroundColor Red
    exit 1
} finally {
    # Environment variable'i temizle
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

