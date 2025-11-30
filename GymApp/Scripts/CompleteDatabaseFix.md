# Veritabanı Güncelleme ve Düzeltme Rehberi

## Sorun
Aktivite kaydetme sırasında "GymCenter field is required" hatası alınıyor. Bu sorun şu nedenlerden kaynaklanabilir:
1. Veritabanı tabloları eksik veya yanlış yapılandırılmış
2. Migration'lar uygulanmamış
3. Model binding sorunu

## Çözüm Adımları

### 1. Veritabanını Kontrol Et ve Düzelt

**PowerShell Script ile (Önerilen):**
```powershell
powershell -ExecutionPolicy Bypass -File GymApp\Scripts\CheckAndFixDatabase.ps1
```

**Manuel SQL ile:**
```bash
psql -U postgres -d GymApp -f GymApp\Scripts\CheckAndFixDatabase.sql
```

### 2. Migration'ları Kontrol Et

```bash
dotnet ef migrations list --project GymApp/GymApp.csproj
```

Eğer migration'lar "Pending" durumundaysa:
```bash
dotnet ef database update --project GymApp/GymApp.csproj
```

### 3. Activities Tablosunu Güncelle

```bash
psql -U postgres -d GymApp -f GymApp\Scripts\UpdateActivitiesTable.sql
```

veya PowerShell ile:
```powershell
powershell -ExecutionPolicy Bypass -File GymApp\Scripts\UpdateActivitiesTable.ps1
```

### 4. Veritabanı Durumunu Kontrol Et

```sql
-- Spor salonlarını kontrol et
SELECT id, name FROM gym_centers;

-- Activities tablosunu kontrol et
SELECT * FROM activities LIMIT 5;

-- Tablo yapısını kontrol et
\d activities
```

## Beklenen Sonuç

Tüm script'ler başarıyla çalıştıktan sonra:
- ✅ `gym_centers` tablosu mevcut ve kayıtlar var
- ✅ `activities` tablosu mevcut ve doğru yapılandırılmış
- ✅ `description` ve `image_url` kolonları NOT NULL
- ✅ `type` kolonu 1-100 arası değerleri kabul ediyor
- ✅ Migration history güncel

## Sorun Devam Ederse

1. Uygulamayı kapatıp yeniden başlatın
2. Tarayıcı cache'ini temizleyin
3. Tarayıcı konsolunu açın (F12) ve hataları kontrol edin
4. Visual Studio Output penceresinde ModelState hatalarını kontrol edin

