# PowerShell script - Activities tablosunu güncellemek için
# Kullanım: .\GymApp\Scripts\UpdateActivitiesTable.ps1

$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"
$sqlFile = "GymApp\Scripts\UpdateActivitiesTable.sql"

Write-Host "Activities tablosu guncelleniyor..." -ForegroundColor Yellow

# PostgreSQL sifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    # SQL dosyasini calistir
    psql -U $dbUser -h $dbHost -p $dbPort -d $dbName -f $sqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Activities tablosu basariyla guncellendi!" -ForegroundColor Green
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

