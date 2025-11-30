# Veritabanı Yönetim Komutları

## 🔴 Veritabanını Silme

### Yöntem 1: PostgreSQL Komut Satırı (psql)

```bash
# PostgreSQL'e bağlan
psql -U postgres -h localhost

# Veritabanındaki tüm bağlantıları kes
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = 'GymApp'
  AND pid <> pg_backend_pid();

# Veritabanını sil
DROP DATABASE IF EXISTS "GymApp";
```

### Yöntem 2: PowerShell Script

```powershell
# Scripts klasöründe çalıştır
.\GymApp\Scripts\DropDatabase.ps1
```

Veya manuel olarak:

```powershell
$env:PGPASSWORD = "2004"
psql -U postgres -h localhost -c "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = 'GymApp' AND pid <> pg_backend_pid();"
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS GymApp;"
```

### Yöntem 3: SQL Dosyası ile

```bash
psql -U postgres -h localhost -f GymApp/Scripts/DropDatabase.sql
```

### Yöntem 4: pgAdmin veya DBeaver

1. pgAdmin/DBeaver'i açın
2. PostgreSQL sunucusuna bağlanın
3. `GymApp` veritabanına sağ tıklayın
4. "Delete/Drop Database" seçeneğini seçin
5. Onaylayın

---

## 🟢 Yeni Veritabanı Oluşturma

### Yöntem 1: PostgreSQL Komut Satırı

```bash
# PostgreSQL'e bağlan
psql -U postgres -h localhost

# Yeni veritabanı oluştur
CREATE DATABASE "GymApp"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Turkish_Turkey.1254'
    LC_CTYPE = 'Turkish_Turkey.1254'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;
```

### Yöntem 2: PowerShell

```powershell
$env:PGPASSWORD = "2004"
psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"
```

---

## 🔄 Migration İşlemleri

### Eski Migration'ları Silme

**ÖNEMLİ:** Sadece geliştirme ortamında yapın! Production'da yapmayın!

```bash
# Migrations klasöründeki tüm .cs dosyalarını manuel olarak silin
# GymApp/Migrations/ klasöründen:
# - 20251124164409_InitalCreate.cs
# - 20251124164409_InitalCreate.Designer.cs
# - GymAppDbContextModelSnapshot.cs
```

### Yeni Migration Oluşturma

```bash
# Proje kök dizininde
cd GymApp
dotnet ef migrations add InitialCreate --project .
```

Veya proje kök dizininden:

```bash
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
```

### Migration'ı Veritabanına Uygulama

```bash
# Proje kök dizininde
cd GymApp
dotnet ef database update --project .
```

Veya:

```bash
dotnet ef database update --project GymApp/GymApp.csproj
```

### Migration'ı Geri Alma (Rollback)

```bash
# Son migration'ı geri al
dotnet ef database update ÖncekiMigrationAdı --project GymApp/GymApp.csproj

# Tüm migration'ları geri al (DİKKATLİ KULLANIN!)
dotnet ef database update 0 --project GymApp/GymApp.csproj
```

---

## 🔧 Tam Sıfırlama İşlemi (Sıfırdan Başlama)

### Adım 1: Eski Migration'ları Sil

```bash
# GymApp/Migrations/ klasöründeki tüm .cs dosyalarını silin
```

### Adım 2: Veritabanını Sil

```bash
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS GymApp;"
```

### Adım 3: Yeni Veritabanı Oluştur

```bash
psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"
```

### Adım 4: Yeni Migration Oluştur

```bash
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
```

### Adım 5: Migration'ı Uygula

```bash
dotnet ef database update --project GymApp/GymApp.csproj
```

---

## 📋 Hızlı Komutlar (Tek Seferde)

### Windows PowerShell - Tam Sıfırlama

```powershell
# Veritabanını sil ve yeniden oluştur
$env:PGPASSWORD = "2004"
psql -U postgres -h localhost -c "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = 'GymApp' AND pid <> pg_backend_pid();"
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS GymApp;"
psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"

# Migration oluştur ve uygula
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
dotnet ef database update --project GymApp/GymApp.csproj
```

### Linux/Mac Bash - Tam Sıfırlama

```bash
# Veritabanını sil ve yeniden oluştur
PGPASSWORD=2004 psql -U postgres -h localhost -c "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = 'GymApp' AND pid <> pg_backend_pid();"
PGPASSWORD=2004 psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS GymApp;"
PGPASSWORD=2004 psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"

# Migration oluştur ve uygula
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
dotnet ef database update --project GymApp/GymApp.csproj
```

---

## ⚠️ Önemli Notlar

1. **Entity Framework Core Tools Kurulumu:**

   ```bash
   dotnet tool install --global dotnet-ef
   ```

   Eğer zaten kuruluysa güncellemek için:

   ```bash
   dotnet tool update --global dotnet-ef
   ```

2. **Connection String Kontrolü:**
   `appsettings.json` dosyasında connection string'in doğru olduğundan emin olun:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=GymApp;Username=postgres;Password=2004"
     }
   }
   ```

3. **Build Hatası Durumunda:**

   ```bash
   # Önce projeyi build edin
   dotnet build GymApp/GymApp.csproj

   # Hataları düzeltin, sonra migration oluşturun
   ```

4. **Veritabanı Bağlantı Hatası:**
   - PostgreSQL servisinin çalıştığından emin olun
   - Kullanıcı adı ve şifrenin doğru olduğundan emin olun
   - Port'un 5432 olduğundan emin olun

---

## 🎯 Kullanım Senaryoları

### Senaryo 1: İlk Kez Veritabanı Oluşturma

```bash
# 1. Migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj

# 2. Veritabanını oluştur ve migration'ı uygula
dotnet ef database update --project GymApp/GymApp.csproj
```

### Senaryo 2: Mevcut Veritabanını Sıfırlama

```bash
# 1. Veritabanını sil
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS GymApp;"

# 2. Yeni veritabanı oluştur
psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"

# 3. Migration'ı uygula
dotnet ef database update --project GymApp/GymApp.csproj
```

### Senaryo 3: Yeni Değişiklikler İçin Migration

```bash
# 1. Yeni migration oluştur
dotnet ef migrations add AddNewFeature --project GymApp/GymApp.csproj

# 2. Migration'ı uygula
dotnet ef database update --project GymApp/GymApp.csproj
```

---

## 📞 Yardım

Eğer sorun yaşıyorsanız:

1. PostgreSQL servisinin çalıştığını kontrol edin
2. Connection string'i kontrol edin
3. Entity Framework Core tools'un kurulu olduğunu kontrol edin
4. Projenin build edildiğini kontrol edin
