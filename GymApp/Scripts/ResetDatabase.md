# Veritabanını Sıfırlama Rehberi

## Yöntem 1: Entity Framework Migration ile

### Adım 1: Eski Migration'ları Sil

```bash
# Migrations klasöründeki tüm migration dosyalarını silin
# (GymApp/Migrations/ klasöründeki .cs dosyaları)
```

### Adım 2: Veritabanını Sil

PostgreSQL'de:

```sql
DROP DATABASE IF EXISTS "GymApp";
CREATE DATABASE "GymApp";
```

Veya PowerShell script kullanarak:

```powershell
.\Scripts\DropDatabase.ps1
```

### Adım 3: Yeni Migration Oluştur

```bash
dotnet ef migrations add InitialCreate --project GymApp
```

### Adım 4: Veritabanını Oluştur

```bash
dotnet ef database update --project GymApp
```

## Yöntem 2: Manuel SQL Script ile

1. PostgreSQL'e bağlanın
2. `DropDatabase.sql` scriptini çalıştırın
3. Yeni migration oluşturun ve uygulayın

## Yöntem 3: Visual Studio Package Manager Console

```powershell
# Eski migration'ları sil
Remove-Migration

# Veritabanını sil (manuel olarak PostgreSQL'den)

# Yeni migration oluştur
Add-Migration InitialCreate

# Veritabanını güncelle
Update-Database
```
