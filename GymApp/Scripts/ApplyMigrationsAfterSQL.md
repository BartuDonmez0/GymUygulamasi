# SQL Script Sonrası Migration Komutları

## 📋 Adım Adım İşlem

### 1. SQL Script'i Çalıştırma

Önce `CreateDatabaseSchema.sql` script'ini çalıştırın:

```bash
# PostgreSQL'e bağlan ve script'i çalıştır
psql -U postgres -h localhost -d GymApp -f GymApp/Scripts/CreateDatabaseSchema.sql
```

Veya PowerShell ile:
```powershell
$env:PGPASSWORD = "2004"
psql -U postgres -h localhost -d GymApp -f GymApp/Scripts/CreateDatabaseSchema.sql
```

### 2. Migration Oluşturma

SQL script'i ile veritabanı oluşturulduktan sonra, Entity Framework için migration oluşturmanız gerekiyor. Ancak bu durumda **snapshot migration** oluşturmalısınız:

```bash
# Mevcut veritabanı yapısına göre migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
```

Eğer hata alırsanız (çünkü veritabanı zaten var), şu komutu kullanın:

```bash
# Mevcut veritabanını migration olarak işaretle (snapshot)
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj --force
```

### 3. Migration'ı Uygulama (Opsiyonel)

Eğer SQL script'i ile veritabanını oluşturduysanız, migration'ı uygulamanıza gerek yok. Ancak migration'ı veritabanı ile senkronize etmek için:

```bash
# Migration'ı uygula (veritabanı zaten var, sadece migration geçmişini günceller)
dotnet ef database update --project GymApp/GymApp.csproj
```

### 4. Alternatif: Migration'ı Atla

Eğer SQL script ile veritabanını oluşturduysanız ve Entity Framework'ün migration geçmişini takip etmek istemiyorsanız:

```bash
# Migration'ı oluştur ama uygulama (sadece kod tarafında tut)
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
```

Sonra migration'ı veritabanına uygulamak yerine, sadece migration dosyalarını kod tarafında tutabilirsiniz.

---

## 🔄 Senaryolar

### Senaryo 1: SQL ile Oluştur, Migration ile Senkronize Et

```bash
# 1. SQL script'i çalıştır
psql -U postgres -h localhost -d GymApp -f GymApp/Scripts/CreateDatabaseSchema.sql

# 2. Migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj

# 3. Migration'ı uygula (geçmişi günceller)
dotnet ef database update --project GymApp/GymApp.csproj
```

### Senaryo 2: SQL ile Oluştur, Migration'ı Sadece Kod Tarafında Tut

```bash
# 1. SQL script'i çalıştır
psql -U postgres -h localhost -d GymApp -f GymApp/Scripts/CreateDatabaseSchema.sql

# 2. Migration oluştur (sadece kod tarafında)
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj

# Migration'ı uygulama (veritabanı zaten hazır)
```

### Senaryo 3: Migration ile Oluştur (Önerilen)

```bash
# 1. Migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj

# 2. Migration'ı uygula (veritabanını oluşturur)
dotnet ef database update --project GymApp/GymApp.csproj
```

---

## ⚠️ Önemli Notlar

1. **SQL Script ile Oluşturma:**
   - Veritabanı yapısını tam kontrol edersiniz
   - Migration geçmişi olmayabilir
   - Entity Framework ile senkronizasyon sorunları olabilir

2. **Migration ile Oluşturma (Önerilen):**
   - Entity Framework migration geçmişini tutar
   - Kod ve veritabanı senkronize kalır
   - Daha güvenli ve yönetilebilir

3. **Hibrit Yaklaşım:**
   - SQL ile oluştur, sonra migration ekle
   - Migration geçmişini güncelle
   - Gelecekteki değişiklikler için migration kullan

---

## 🛠️ Sorun Giderme

### Hata: "Migration already exists"
```bash
# Eski migration'ları sil
# GymApp/Migrations/ klasöründeki .cs dosyalarını silin

# Yeni migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj
```

### Hata: "Database already exists"
```bash
# Veritabanını sil
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS GymApp;"

# Yeniden oluştur
psql -U postgres -h localhost -c "CREATE DATABASE GymApp;"
```

### Hata: "No migrations found"
```bash
# Migration oluştur
dotnet ef migrations add InitialCreate --project GymApp/GymApp.csproj

# Uygula
dotnet ef database update --project GymApp/GymApp.csproj
```

