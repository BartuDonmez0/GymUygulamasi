#  GymApp - Spor Salonu Yönetim Sistemi

ASP.NET Core MVC tabanlı, modern bir spor salonu yönetim ve randevu sistemi.

##  Özellikler

###  Temel Özellikler

- **Spor Salonu Yönetimi**: Salon bilgileri, çalışma saatleri ve fotoğrafları
- **Antrenör Yönetimi**: Antrenör profilleri, uzmanlık alanları ve çalışma saatleri
- **Aktivite Yönetimi**: Spor aktiviteleri ve türleri
- **Randevu Sistemi**: Üye-antrenör randevu yönetimi
- **Üye Yönetimi**: Üye kayıtları ve profil yönetimi

###  AI Entegrasyonu

- **Gemini AI**: Google Gemini API ile yapay zeka destekli öneriler
- **Akıllı Öneriler**: Kişiselleştirilmiş egzersiz ve antrenman önerileri

###  Chat Sistemi

- **AI Chatbot**: Üyeler Gemini AI ile sohbet edebilir
- **Mesaj Geçmişi**: Tüm chat mesajları kaydedilir ve görüntülenebilir
- **Anlık Yanıtlar**: AI'dan hızlı ve akıllı yanıtlar alınır

###  Admin Paneli

- **Yönetim Paneli**: Tüm sistem verilerinin yönetimi
- **Mesaj Yönetimi**: Chat mesajlarının görüntülenmesi ve yönetimi
- **Rol Bazlı Yetkilendirme**: Admin ve User rolleri

##  Teknolojiler

- **.NET 9.0**: Backend framework
- **ASP.NET Core MVC**: Web framework
- **Entity Framework Core**: ORM
- **PostgreSQL**: Veritabanı
- **Bootstrap**: Frontend framework
- **Gemini AI API**: Yapay zeka entegrasyonu

##  Proje Yapısı

```
GymUygulamasi/
├── Entities/          # Veritabanı entity'leri
├── Repositories/      # Veri erişim katmanı (Repository Pattern)
├── Services/          # İş mantığı katmanı
└── GymApp/           # MVC uygulaması
    ├── Controllers/   # Controller'lar
    ├── Views/         # Razor view'lar
    ├── Data/          # DbContext
    └── Areas/Admin/   # Admin paneli
```

##  Kurulum

1. **Gereksinimler**

   - .NET 9.0 SDK
   - PostgreSQL veritabanı
   - Gemini API anahtarı

2. **Veritabanı Yapılandırması**

   - `appsettings.json` dosyasında connection string'i düzenleyin
   - Migration'ları çalıştırın: `dotnet ef database update`

3. **API Anahtarı**

   - `appsettings.json` dosyasında Gemini API anahtarınızı ekleyin

4. **Çalıştırma**
   ```bash
   dotnet run
   ```

## 📝 Notlar

- Chat sistemi, üyelerin AI ile iletişim kurmasını sağlar
- Tüm chat mesajları veritabanında saklanır
- Admin panelinden tüm mesajlar görüntülenebilir
