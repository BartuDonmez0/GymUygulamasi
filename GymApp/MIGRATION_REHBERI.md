# Migration ve Veritabanı Test Rehberi

## 1. Migration Oluşturma

### Adım 1: EF Core Tools'u yükleyin (eğer yüklü değilse)

```bash
dotnet tool install --global dotnet-ef
```

### Adım 2: İlk migration'ı oluşturun

```bash
dotnet ef migrations add InitialCreate
```

Bu komut `Migrations` klasörü oluşturur ve migration dosyalarını ekler.

### Adım 3: Veritabanına uygulayın

```bash
dotnet ef database update
```

Bu komut migration'ları veritabanına uygular ve tabloları oluşturur.

## 2. Veritabanı Bağlantısını Test Etme

### Yöntem 1: Web Arayüzü ile Test

1. Uygulamayı çalıştırın:

   ```bash
   dotnet run
   ```

2. Tarayıcıda şu adrese gidin:

   ```
   https://localhost:5001/Home/TestDatabase
   ```

   veya

   ```
   http://localhost:5000/Home/TestDatabase
   ```

3. Sayfa size veritabanı bağlantı durumunu ve tablo sayılarını gösterecektir.

### Yöntem 2: Komut Satırı ile Test

```bash
dotnet ef database info
```

Bu komut veritabanı bağlantısını test eder ve migration durumunu gösterir.

## 3. Yeni Migration Oluşturma (Model Değişikliklerinden Sonra)

Model'lerinizde değişiklik yaptıktan sonra:

```bash
dotnet ef migrations add MigrationAdi
dotnet ef database update
```

## 4. Migration'ı Geri Alma

Son migration'ı geri almak için:

```bash
dotnet ef database update ÖncekiMigrationAdi
```

Tüm migration'ları silmek için:

```bash
dotnet ef migrations remove
```

## 5. Veritabanı Bağlantı String'i

`appsettings.json` dosyasında connection string'inizi kontrol edin:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=GymApp;Username=postgres;Password=2004"
}
```

## 6. PostgreSQL'de Veritabanını Kontrol Etme

PostgreSQL'de veritabanınızın oluşturulduğundan emin olun:

```sql
-- PostgreSQL'de çalıştırın
\l  -- Tüm veritabanlarını listeler
\c GymApp  -- GymApp veritabanına bağlanır
\dt  -- Tabloları listeler
```

## Sorun Giderme

### Migration oluşturulamıyor

- `Microsoft.EntityFrameworkCore.Design` paketinin yüklü olduğundan emin olun
- `dotnet restore` komutunu çalıştırın

### Veritabanına bağlanılamıyor

- PostgreSQL servisinin çalıştığından emin olun
- Connection string'deki bilgilerin doğru olduğundan emin olun
- Veritabanının oluşturulduğundan emin olun

### Tablolar oluşturulmadı

- `dotnet ef database update` komutunu çalıştırdığınızdan emin olun
- Migration dosyalarının `Migrations` klasöründe olduğunu kontrol edin
