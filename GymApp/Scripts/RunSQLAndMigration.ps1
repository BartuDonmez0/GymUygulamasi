# PowerShell Script - SQL Script Çalıştırma ve Migration İşlemleri
# Kullanım: .\GymApp\Scripts\RunSQLAndMigration.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SQL Script ve Migration İşlemi" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# PostgreSQL bağlantı bilgileri
$dbName = "GymApp"
$dbUser = "postgres"
$dbPassword = "2004"
$dbHost = "localhost"
$dbPort = "5432"
$sqlScriptPath = "GymApp/Scripts/CreateDatabaseSchema.sql"

# PostgreSQL şifresini environment variable olarak ayarla
$env:PGPASSWORD = $dbPassword

try {
    # Veritabanının var olup olmadığını kontrol et
    Write-Host "[1/4] Veritabanı kontrol ediliyor..." -ForegroundColor Yellow
    $dbExists = psql -U $dbUser -h $dbHost -p $dbPort -lqt | Select-String -Pattern "^\s*$dbName\s"
    
    if ($dbExists) {
        Write-Host "⚠ Veritabanı zaten mevcut. Devam ediliyor..." -ForegroundColor Yellow
    } else {
        Write-Host "[2/4] Veritabanı oluşturuluyor..." -ForegroundColor Yellow
        $createQuery = "CREATE DATABASE `"$dbName`";"
        psql -U $dbUser -h $dbHost -p $dbPort -c $createQuery
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Veritabanı oluşturuldu" -ForegroundColor Green
        } else {
            Write-Host "✗ Veritabanı oluşturulamadı!" -ForegroundColor Red
            exit 1
        }
    }
    
    # SQL script'ini çalıştır
    Write-Host "[3/4] SQL script çalıştırılıyor..." -ForegroundColor Yellow
    if (Test-Path $sqlScriptPath) {
        psql -U $dbUser -h $dbHost -p $dbPort -d $dbName -f $sqlScriptPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ SQL script başarıyla çalıştırıldı" -ForegroundColor Green
        } else {
            Write-Host "✗ SQL script çalıştırılamadı!" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "✗ SQL script dosyası bulunamadı: $sqlScriptPath" -ForegroundColor Red
        exit 1
    }
    
    # Migration oluştur
    Write-Host "[4/4] Migration oluşturuluyor..." -ForegroundColor Yellow
    $migrationResult = dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migration oluşturuldu" -ForegroundColor Green
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  İşlem Başarıyla Tamamlandı!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Not: Migration'ı uygulamak için:" -ForegroundColor Yellow
        Write-Host "  dotnet ef database update --project GymApp/GymApp.csproj" -ForegroundColor Cyan
    } else {
        Write-Host "⚠ Migration oluşturulurken uyarı/hatalar oluştu:" -ForegroundColor Yellow
        Write-Host $migrationResult -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Eğer 'migration already exists' hatası alıyorsanız," -ForegroundColor Yellow
        Write-Host "GymApp/Migrations/ klasöründeki eski migration dosyalarını silin." -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "✗ Hata oluştu: $_" -ForegroundColor Red
    exit 1
} finally {
    # Environment variable'ı temizle
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

