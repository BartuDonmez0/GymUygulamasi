GymApp – Spor Salonu Yönetim ve Randevu Sistemi

ASP.NET Core MVC – Web Programlama Dersi Projesi (2025–2026 Güz Dönemi)

Bu proje, Sakarya Üniversitesi Web Programlama dersi kapsamında geliştirilmiş bir Spor Salonu (Fitness Center) Yönetim ve Randevu Sistemidir.
Amaç; ders boyunca öğrenilen:

ASP.NET Core MVC

Entity Framework Core

LINQ

SQL veritabanı yönetimi

API entegrasyonu

Rol bazlı yetkilendirme

Front-End geliştirme (Bootstrap 5, HTML5, CSS3, JS)

gibi teknolojileri gerçek bir problem üzerinde uygulayarak fonksiyonel bir web uygulaması geliştirmektir.

📌 Proje Özeti

GymApp; spor salonları, eğitmenler ve üyeler için hazırlanmış bir randevu, yönetim ve takip sistemidir.
Sistem sayesinde kullanıcılar:

Hizmetleri görüntüleyebilir,

Uygun eğitmenlere göre randevu oluşturabilir,

Randevu onayı alabilir,

Yapay zekâ ile kişiselleştirilmiş egzersiz önerileri alabilir.

Yönetici (Admin) paneli sayesinde spor salonu yöneticileri:

Eğitmen,

Hizmet,

Salon,

Üyelik,

Randevu

gibi tüm birimleri yönetebilmektedir.

📂 Proje Klasör Yapısı
WebApp/
│── Entities/        # Veritabanı modelleri
│── GymApp/          # MVC projesi (Controllers, Views, Models)
│── Repositories/    # Repository Pattern katmanı
│── Services/        # İş servisleri ve iş kuralları
│── .gitignore
│── WebApp.sln


Bu yapı:

✔ Katmanlı mimari
✔ SOLID uyumlu tasarım
✔ Controller–Service–Repository hiyerarşisi

kullanılarak oluşturulmuştur.

🔧 Kullanılan Teknolojiler
Teknoloji	Açıklama
ASP.NET Core MVC	Uygulama çatısı
C#	Backend yazılım dili
Entity Framework Core	ORM – Database işlemleri
SQL Server	Veritabanı
Bootstrap 5	Arayüz tasarımı
HTML5 / CSS3 / JS / jQuery	Front-end
REST API	Veri listeleme ve filtreleme
OpenAI API (veya AI Servisi)	Yapay zekâ egzersiz/diyet önerisi
🏢 Sistem Modülleri
1️⃣ Spor Salonu Yönetimi

Salon bilgileri

Hizmet türleri (fitness, yoga, pilates…)

Ücret ve süre bilgileri

Çalışma saatleri

2️⃣ Eğitmen Yönetimi

Eğitmen ekleme / silme / güncelleme

Uzmanlık alanları

Müsaitlik saatleri

Hizmet uyumluluğu

3️⃣ Üye Yönetimi ve Randevu Sistemi

Üye kayıt/giriş

Uygun saate göre randevu oluşturma

Uygun olmayan tarihler için uyarı sistemi

Randevu detayları (hizmet, eğitmen, süre, ücret)

Admin onay mekanizması

4️⃣ REST API (LINQ ile)

Aşağıdaki işlemler API üzerinden yapılabilmektedir:

Tüm eğitmenleri listeleme

Belirli bir tarihte uygun eğitmen getirme

Üyeye ait randevuları çekme

LINQ ile filtreleme kriterleri API tarafında uygulanır.

5️⃣ Yapay Zeka Entegrasyonu

Kullanıcı:

boy/kilo/vücut tipi bilgisi girer

veya fotoğraf yükler

sistem OpenAI API üzerinden egzersiz veya diyet önerileri üretir

isteğe bağlı olarak “gelecekteki görünüm” tahmini de alınabilir

🔐 Yetkilendirme Sistemi (Authorization)

Sistemde iki rol bulunmaktadır:

Rol	Açıklama	Örnek Giriş
Admin	Panel yönetimi, CRUD işlemleri	ogrencinumara@sakarya.edu.tr
 / sau
Üye	Randevu alma, hizmet görüntüleme	Kayıt olan kullanıcı

Authorization; Controller seviyesinde [Authorize(Roles="Admin")] gibi filtrelerle uygulanmıştır.

📊 Veritabanı Modeli

Veritabanı Entity Framework Core Code First yaklaşımıyla oluşturulmuştur.

Örnek varlıklar:

GymCenter → Spor salonu bilgileri

Service → Hizmet türleri

Trainer → Eğitmen bilgileri

Member → Üyeler

Appointment → Randevu kayıtları

Örnek İlişkiler

Bir Eğitmenin birçok Randevusu olabilir

Bir Hizmet’in bir ücreti ve süresi bulunur

Üye → Eğitmen → Randevu üçlü bir yapıdadır.

Proje Sahibi

Ad Soyad: Bartu Dönmez

Sonuç

Bu proje; bir spor salonunun tüm yönetim süreçlerini dijital ortama taşıyan, rezervasyon, yönetim paneli, raporlama ve yapay zekâ entegrasyonu içeren modern bir web yazılımıdır.

Hem akademik gereksinimleri hem de gerçek hayatta kullanılabilir bir sistemi karşılayan tam kapsamlı bir MVC uygulaması ortaya konmuştur.
