-- =====================================================
-- ACTIVITIES TABLOSU GÜNCELLEME SCRIPT'İ
-- =====================================================
-- Bu script activities tablosunu güncel hale getirir
-- ActivityType enum'ı 1-100 arası değerleri destekler

-- Eğer tablo yoksa oluştur
CREATE TABLE IF NOT EXISTS activities (
    id SERIAL PRIMARY KEY,
    gym_center_id INTEGER NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    type INTEGER NOT NULL CHECK (type >= 1 AND type <= 100), -- ActivityType enum: 1-100 arası
    duration INTEGER NOT NULL CHECK (duration > 0), -- Dakika cinsinden, 0'dan büyük olmalı
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0), -- Fiyat 0 veya pozitif olmalı
    image_url TEXT NOT NULL DEFAULT '', -- Boş string olabilir
    CONSTRAINT fk_activities_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE
);

-- Eğer tablo varsa, kolonları güncelle
DO $$
BEGIN
    -- description kolonu yoksa veya nullable ise, NOT NULL ve default değer ekle
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'activities' 
        AND column_name = 'description' 
        AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE activities 
        ALTER COLUMN description SET NOT NULL,
        ALTER COLUMN description SET DEFAULT '';
    END IF;

    -- image_url kolonu yoksa veya nullable ise, NOT NULL ve default değer ekle
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'activities' 
        AND column_name = 'image_url' 
        AND is_nullable = 'NO'
    ) THEN
        ALTER TABLE activities 
        ALTER COLUMN image_url SET NOT NULL,
        ALTER COLUMN image_url SET DEFAULT '';
    END IF;

    -- type kolonuna CHECK constraint ekle (eğer yoksa)
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'activities' 
        AND constraint_name = 'activities_type_check'
    ) THEN
        ALTER TABLE activities 
        ADD CONSTRAINT activities_type_check CHECK (type >= 1 AND type <= 100);
    END IF;

    -- duration kolonuna CHECK constraint ekle (eğer yoksa)
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'activities' 
        AND constraint_name = 'activities_duration_check'
    ) THEN
        ALTER TABLE activities 
        ADD CONSTRAINT activities_duration_check CHECK (duration > 0);
    END IF;

    -- price kolonuna CHECK constraint ekle (eğer yoksa)
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'activities' 
        AND constraint_name = 'activities_price_check'
    ) THEN
        ALTER TABLE activities 
        ADD CONSTRAINT activities_price_check CHECK (price >= 0);
    END IF;
END $$;

-- Index'leri oluştur (eğer yoksa)
CREATE INDEX IF NOT EXISTS idx_activities_gym_center_id ON activities(gym_center_id);
CREATE INDEX IF NOT EXISTS idx_activities_type ON activities(type);

-- Mevcut NULL değerleri düzelt
UPDATE activities 
SET description = '' 
WHERE description IS NULL;

UPDATE activities 
SET image_url = '' 
WHERE image_url IS NULL;

-- ActivityType enum değerleri referansı:
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

