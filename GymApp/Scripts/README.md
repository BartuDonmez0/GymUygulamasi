# Veritabanı Yönetim Scriptleri

Bu klasörde veritabanı yönetimi için hazırlanmış scriptler ve rehberler bulunmaktadır.

## 📁 Dosyalar

### SQL Scripts
- **`CreateDatabaseSchema.sql`** - Veritabanı şemasını oluşturan SQL script'i
- **`DropDatabase.sql`** - Veritabanını silen SQL script'i

### PowerShell Scripts
- **`DropDatabase.ps1`** - Veritabanını siler
- **`CreateDatabase.ps1`** - Veritabanını oluşturur
- **`ResetDatabase.ps1`** - Veritabanını sıfırlar (sil + oluştur + migration)
- **`RunSQLAndMigration.ps1`** - SQL script çalıştırır ve migration oluşturur
- **`ApplyMigrations.ps1`** - Migration'ları uygular

### Rehberler
- **`DatabaseCommands.md`** - Tüm veritabanı komutları rehberi
- **`ResetDatabase.md`** - Veritabanı sıfırlama rehberi
- **`ApplyMigrationsAfterSQL.md`** - SQL script sonrası migration komutları

## 🚀 Hızlı Başlangıç

### Senaryo 1: SQL Script ile Veritabanı Oluşturma

```powershell
# 1. SQL script'i çalıştır ve migration oluştur
.\GymApp\Scripts\RunSQLAndMigration.ps1

# 2. (Opsiyonel) Migration'ı uygula
.\GymApp\Scripts\ApplyMigrations.ps1
```

### Senaryo 2: Migration ile Veritabanı Oluşturma (Önerilen)

```powershell
# 1. Veritabanını sıfırla
.\GymApp\Scripts\ResetDatabase.ps1
```

### Senaryo 3: Manuel SQL Komutları

```bash
# 1. Veritabanını oluştur
psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"

# 2. SQL script'i çalıştır
psql -U postgres -h localhost -d GymApp -f GymApp/Scripts/CreateDatabaseSchema.sql

# 3. Migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj

# 4. Migration'ı uygula
dotnet ef database update --project GymApp/GymApp.csproj
```

## 📋 Detaylı Rehberler

Tüm komutlar ve detaylı açıklamalar için:
- **`DatabaseCommands.md`** dosyasına bakın
- **`ApplyMigrationsAfterSQL.md`** dosyasına bakın

## ⚙️ Gereksinimler

- PostgreSQL 12+ yüklü ve çalışıyor olmalı
- Entity Framework Core Tools kurulu olmalı:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- PowerShell 5.1+ (Windows için)
- PostgreSQL'in PATH'de olması veya tam yol ile erişilebilir olması

## 🔧 Yapılandırma

Script'lerdeki varsayılan ayarlar:
- **Host:** localhost
- **Port:** 5432
- **Database:** GymApp
- **User:** postgres
- **Password:** 2004

Bu ayarları değiştirmek için script dosyalarını düzenleyin veya `appsettings.json` dosyasındaki connection string'i kontrol edin.

