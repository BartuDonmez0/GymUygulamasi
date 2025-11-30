# PowerShell script - Migration'ları uygulamak için
# Kullanım: .\GymApp\Scripts\ApplyMigrations.ps1

Write-Host "Migration'lar uygulanıyor..." -ForegroundColor Yellow

try {
    $updateResult = dotnet ef database update --project GymApp/GymApp.csproj 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migration'lar başarıyla uygulandı!" -ForegroundColor Green
    } else {
        Write-Host "✗ Migration'lar uygulanamadı!" -ForegroundColor Red
        Write-Host $updateResult -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Hata oluştu: $_" -ForegroundColor Red
    exit 1
}

