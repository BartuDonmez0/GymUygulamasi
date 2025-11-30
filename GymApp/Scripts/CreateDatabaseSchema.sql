-- =====================================================
-- GymApp Veritabanı Şema Oluşturma Scripti
-- PostgreSQL için hazırlanmıştır
-- =====================================================

-- Önce mevcut tabloları sil (eğer varsa)
DROP TABLE IF EXISTS chat_messages CASCADE;
DROP TABLE IF EXISTS ai_recommendations CASCADE;
DROP TABLE IF EXISTS appointments CASCADE;
DROP TABLE IF EXISTS trainer_working_hours CASCADE;
DROP TABLE IF EXISTS trainer_activities CASCADE;
DROP TABLE IF EXISTS activities CASCADE;
DROP TABLE IF EXISTS gym_center_photos CASCADE;
DROP TABLE IF EXISTS trainers CASCADE;
DROP TABLE IF EXISTS gym_centers CASCADE;
DROP TABLE IF EXISTS members CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- =====================================================
-- 1. USERS TABLOSU (Admin ve Üye hesapları için)
-- =====================================================
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Member', -- 'Admin' veya 'Member'
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);

-- =====================================================
-- 2. MEMBERS TABLOSU (Üyeler)
-- =====================================================
CREATE TABLE members (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(20) NOT NULL,
    password VARCHAR(255) NOT NULL,
    registration_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    user_id INTEGER,
    CONSTRAINT fk_members_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX idx_members_email ON members(email);
CREATE INDEX idx_members_user_id ON members(user_id);

-- =====================================================
-- 3. GYM_CENTERS TABLOSU (Spor Salonları)
-- =====================================================
CREATE TABLE gym_centers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    address TEXT NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255) NOT NULL,
    working_hours VARCHAR(255),
    advertisement TEXT,
    image_url VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT FALSE -- Aktiviteler olmadan açık olmamalı
);

CREATE INDEX idx_gym_centers_is_active ON gym_centers(is_active);
CREATE INDEX idx_gym_centers_name ON gym_centers(name);

-- =====================================================
-- 4. GYM_CENTER_WORKING_HOURS TABLOSU (Spor Salonu Çalışma Saatleri)
-- =====================================================
CREATE TABLE gym_center_working_hours (
    id SERIAL PRIMARY KEY,
    gym_center_id INTEGER NOT NULL,
    day_of_week INTEGER NOT NULL, -- 0=Pazar, 1=Pazartesi, ..., 6=Cumartesi
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    CONSTRAINT fk_gym_center_working_hours_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE,
    CONSTRAINT chk_gym_center_working_hours_time CHECK (end_time > start_time)
);

CREATE INDEX idx_gym_center_working_hours_gym_center_id ON gym_center_working_hours(gym_center_id);
CREATE INDEX idx_gym_center_working_hours_day ON gym_center_working_hours(day_of_week);

-- =====================================================
-- 5. ACTIVITIES TABLOSU (Aktiviteler)
-- =====================================================
CREATE TABLE activities (
    id SERIAL PRIMARY KEY,
    gym_center_id INTEGER NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    type INTEGER NOT NULL CHECK (type >= 1 AND type <= 100), -- ActivityType enum: 1-100 arası değerler
    duration INTEGER NOT NULL CHECK (duration > 0), -- Dakika cinsinden, 0'dan büyük olmalı
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0), -- Fiyat 0 veya pozitif olmalı
    image_url TEXT NOT NULL DEFAULT '', -- Boş string olabilir
    CONSTRAINT fk_activities_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE
);

CREATE INDEX idx_activities_gym_center_id ON activities(gym_center_id);
CREATE INDEX idx_activities_type ON activities(type);

-- ActivityType enum değerleri (1-100):
-- 1=Fitness, 2=Yoga, 3=Pilates, 4=Crossfit, 5=Zumba, 6=Spinning, 7=Kickboxing,
-- 8=Kişisel Antrenman, 9=Grup Antrenmanı, 10=Kardiyovasküler, 11=Güç Antrenmanı,
-- 12=Esneklik, 13=Dans, 14=Aerobik, 15=Vücut Geliştirme, 16=Boks, 17=Muay Thai,
-- 18=Brezilya Jiu Jitsu, 19=Karate, 20=Tae Kwon Do, 21=Yüzme, 22=Su Aerobiği,
-- 23=Su Topu, 24=Basketbol, 25=Futbol, 26=Voleybol, 27=Tenis, 28=Badminton,
-- 29=Masa Tenisi, 30=Squash, 31=Raketbol, 32=Koşu, 33=Jogging, 34=Yürüyüş,
-- 35=Doğa Yürüyüşü, 36=Bisiklet, 37=Kapalı Bisiklet, 38=Kürek, 39=Tırmanış,
-- 40=Kaya Tırmanışı, 41=Bouldering, 42=Jimnastik, 43=Kalistenik, 44=TRX,
-- 45=Kettlebell, 46=Fonksiyonel Antrenman, 47=HIIT, 48=Tabata, 49=Devre Antrenmanı,
-- 50=Boot Camp, 51=Barre, 52=Bale, 53=Modern Dans, 54=Latin Dans, 55=Hip Hop,
-- 56=Oryantal Dans, 57=Pole Dance, 58=Aerial Yoga, 59=Hot Yoga, 60=Power Yoga,
-- 61=Yin Yoga, 62=Ashtanga Yoga, 63=Vinyasa Yoga, 64=Hatha Yoga, 65=Pilates Mat,
-- 66=Reformer Pilates, 67=Barre Pilates, 68=Meditasyon, 69=Farkındalık, 70=Tai Chi,
-- 71=Qigong, 72=Dövüş Sanatları, 73=Güreş, 74=Judo, 75=Aikido, 76=Capoeira,
-- 77=MMA, 78=Öz Savunma, 79=Krav Maga, 80=Kilo Verme, 81=Kas Geliştirme,
-- 82=Rehabilitasyon, 83=Fizik Tedavi, 84=Yaşlılar İçin Fitness, 85=Çocuk Fitness,
-- 86=Hamile Yoga, 87=Doğum Sonrası Yoga, 88=Spor Masajı, 89=Esneklik Dersi,
-- 90=Esneklik Antrenmanı, 91=Mobilite Antrenmanı, 92=Core Antrenmanı, 93=Karın Antrenmanı,
-- 94=Kalça Antrenmanı, 95=Bacak Antrenmanı, 96=Kol Antrenmanı, 97=Sırt Antrenmanı,
-- 98=Göğüs Antrenmanı, 99=Omuz Antrenmanı, 100=Tüm Vücut Antrenmanı

-- =====================================================
-- 6. TRAINERS TABLOSU (Antrenörler)
-- =====================================================
CREATE TABLE trainers (
    id SERIAL PRIMARY KEY,
    gym_center_id INTEGER NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(20) NOT NULL,
    specialization VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    CONSTRAINT fk_trainers_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE
);

CREATE INDEX idx_trainers_gym_center_id ON trainers(gym_center_id);
CREATE INDEX idx_trainers_email ON trainers(email);

-- =====================================================
-- 7. TRAINER_ACTIVITIES TABLOSU (Many-to-Many: Antrenör-Aktivite)
-- =====================================================
CREATE TABLE trainer_activities (
    id SERIAL PRIMARY KEY,
    trainer_id INTEGER NOT NULL,
    activity_id INTEGER NOT NULL,
    CONSTRAINT fk_trainer_activities_trainer FOREIGN KEY (trainer_id) REFERENCES trainers(id) ON DELETE CASCADE,
    CONSTRAINT fk_trainer_activities_activity FOREIGN KEY (activity_id) REFERENCES activities(id) ON DELETE CASCADE,
    CONSTRAINT uk_trainer_activities UNIQUE (trainer_id, activity_id) -- Aynı antrenör-aktivite çifti tekrar edemez
);

CREATE INDEX idx_trainer_activities_trainer_id ON trainer_activities(trainer_id);
CREATE INDEX idx_trainer_activities_activity_id ON trainer_activities(activity_id);

-- =====================================================
-- 8. TRAINER_WORKING_HOURS TABLOSU (Antrenör Çalışma Saatleri)
-- =====================================================
CREATE TABLE trainer_working_hours (
    id SERIAL PRIMARY KEY,
    trainer_id INTEGER NOT NULL,
    day_of_week INTEGER NOT NULL, -- 0=Pazar, 1=Pazartesi, ..., 6=Cumartesi
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    CONSTRAINT fk_trainer_working_hours_trainer FOREIGN KEY (trainer_id) REFERENCES trainers(id) ON DELETE CASCADE,
    CONSTRAINT chk_trainer_working_hours_time CHECK (end_time > start_time)
);

CREATE INDEX idx_trainer_working_hours_trainer_id ON trainer_working_hours(trainer_id);
CREATE INDEX idx_trainer_working_hours_day ON trainer_working_hours(day_of_week);

-- =====================================================
-- 9. APPOINTMENTS TABLOSU (Randevular)
-- =====================================================
CREATE TABLE appointments (
    id SERIAL PRIMARY KEY,
    member_id INTEGER NOT NULL,
    trainer_id INTEGER NOT NULL,
    activity_id INTEGER NOT NULL,
    gym_center_id INTEGER NOT NULL,
    appointment_date DATE NOT NULL,
    appointment_time TIME NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    status INTEGER NOT NULL DEFAULT 1, -- 1=Pending, 2=Approved, 3=Rejected, 4=Completed
    CONSTRAINT fk_appointments_member FOREIGN KEY (member_id) REFERENCES members(id) ON DELETE CASCADE,
    CONSTRAINT fk_appointments_trainer FOREIGN KEY (trainer_id) REFERENCES trainers(id) ON DELETE CASCADE,
    CONSTRAINT fk_appointments_activity FOREIGN KEY (activity_id) REFERENCES activities(id) ON DELETE CASCADE,
    CONSTRAINT fk_appointments_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE
);

CREATE INDEX idx_appointments_member_id ON appointments(member_id);
CREATE INDEX idx_appointments_trainer_id ON appointments(trainer_id);
CREATE INDEX idx_appointments_activity_id ON appointments(activity_id);
CREATE INDEX idx_appointments_gym_center_id ON appointments(gym_center_id);
CREATE INDEX idx_appointments_date ON appointments(appointment_date);
CREATE INDEX idx_appointments_status ON appointments(status);

-- =====================================================
-- 10. AI_RECOMMENDATIONS TABLOSU (AI Önerileri)
-- =====================================================
CREATE TABLE ai_recommendations (
    id SERIAL PRIMARY KEY,
    member_id INTEGER NOT NULL,
    recommendation_type VARCHAR(100) NOT NULL, -- 'Exercise', 'Diet', vb.
    content TEXT NOT NULL,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ai_recommendations_member FOREIGN KEY (member_id) REFERENCES members(id) ON DELETE CASCADE
);

CREATE INDEX idx_ai_recommendations_member_id ON ai_recommendations(member_id);
CREATE INDEX idx_ai_recommendations_created_date ON ai_recommendations(created_date);

-- =====================================================
-- 11. CHAT_MESSAGES TABLOSU (AI Chatbot Mesajları)
-- =====================================================
CREATE TABLE chat_messages (
    id SERIAL PRIMARY KEY,
    member_id INTEGER NOT NULL,
    message TEXT NOT NULL,
    response TEXT, -- AI'dan gelen cevap
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_chat_messages_member FOREIGN KEY (member_id) REFERENCES members(id) ON DELETE CASCADE
);

CREATE INDEX idx_chat_messages_member_id ON chat_messages(member_id);
CREATE INDEX idx_chat_messages_created_date ON chat_messages(created_date);

-- =====================================================
-- 12. GYM_CENTER_PHOTOS TABLOSU (Spor Salonu Fotoğrafları)
-- =====================================================
CREATE TABLE gym_center_photos (
    id SERIAL PRIMARY KEY,
    gym_center_id INTEGER NOT NULL,
    photo_url VARCHAR(500) NOT NULL,
    CONSTRAINT fk_gym_center_photos_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE
);

CREATE INDEX idx_gym_center_photos_gym_center_id ON gym_center_photos(gym_center_id);

-- =====================================================
-- VERİ GİRİŞİ (Örnek Admin Kullanıcısı)
-- =====================================================

-- Admin kullanıcısı oluştur
INSERT INTO users (email, password, role, created_date) 
VALUES ('G231210561@sakarya.edu.tr', 'sau', 'Admin', CURRENT_TIMESTAMP);

-- =====================================================
-- AÇIKLAMALAR
-- =====================================================

-- ActivityType Enum Değerleri:
-- 1 = Fitness
-- 2 = Yoga
-- 3 = Pilates
-- 4 = Crossfit
-- 5 = Zumba
-- 6 = Spinning
-- 7 = Kickboxing
-- 8 = PersonalTraining
-- 9 = GroupTraining
-- 10 = Cardio
-- 11 = StrengthTraining
-- 12 = Stretching
-- 13 = Dance

-- AppointmentStatus Enum Değerleri:
-- 1 = Pending (Beklemede)
-- 2 = Approved (Onaylandı)
-- 3 = Rejected (Reddedildi)
-- 4 = Completed (Tamamlandı)

-- DayOfWeek Enum Değerleri:
-- 0 = Sunday (Pazar)
-- 1 = Monday (Pazartesi)
-- 2 = Tuesday (Salı)
-- 3 = Wednesday (Çarşamba)
-- 4 = Thursday (Perşembe)
-- 5 = Friday (Cuma)
-- 6 = Saturday (Cumartesi)

-- =====================================================
-- SCRIPT TAMAMLANDI
-- =====================================================

