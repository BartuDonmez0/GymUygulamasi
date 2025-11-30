-- =====================================================
-- VERİTABANI KONTROL VE DÜZELTME SCRIPT'İ
-- =====================================================
-- Bu script veritabanını kontrol eder ve eksikleri düzeltir

-- 1. gym_centers tablosu kontrolü
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_name = 'gym_centers'
    ) THEN
        RAISE EXCEPTION 'gym_centers tablosu bulunamadı!';
    END IF;
END $$;

-- 2. activities tablosu kontrolü ve düzeltme
DO $$
BEGIN
    -- activities tablosu yoksa oluştur
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_name = 'activities'
    ) THEN
        CREATE TABLE activities (
            id SERIAL PRIMARY KEY,
            gym_center_id INTEGER NOT NULL,
            name VARCHAR(255) NOT NULL,
            description TEXT NOT NULL DEFAULT '',
            type INTEGER NOT NULL CHECK (type >= 1 AND type <= 100),
            duration INTEGER NOT NULL CHECK (duration > 0),
            price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
            image_url TEXT NOT NULL DEFAULT '',
            CONSTRAINT fk_activities_gym_center FOREIGN KEY (gym_center_id) REFERENCES gym_centers(id) ON DELETE CASCADE
        );
        
        CREATE INDEX idx_activities_gym_center_id ON activities(gym_center_id);
        CREATE INDEX idx_activities_type ON activities(type);
    ELSE
        -- Tablo varsa kolonları güncelle
        -- description kolonu nullable ise düzelt
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'activities' 
            AND column_name = 'description' 
            AND is_nullable = 'YES'
        ) THEN
            UPDATE activities SET description = '' WHERE description IS NULL;
            ALTER TABLE activities 
            ALTER COLUMN description SET NOT NULL,
            ALTER COLUMN description SET DEFAULT '';
        END IF;

        -- image_url kolonu nullable ise düzelt
        IF EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'activities' 
            AND column_name = 'image_url' 
            AND is_nullable = 'YES'
        ) THEN
            UPDATE activities SET image_url = '' WHERE image_url IS NULL;
            ALTER TABLE activities 
            ALTER COLUMN image_url SET NOT NULL,
            ALTER COLUMN image_url SET DEFAULT '';
        END IF;

        -- type CHECK constraint ekle (eğer yoksa)
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.table_constraints 
            WHERE table_name = 'activities' 
            AND constraint_name = 'activities_type_check'
        ) THEN
            ALTER TABLE activities 
            ADD CONSTRAINT activities_type_check CHECK (type >= 1 AND type <= 100);
        END IF;
    END IF;
END $$;

-- 3. working_hours_json kolonları kontrolü
DO $$
BEGIN
    -- trainers tablosuna working_hours_json ekle (eğer yoksa)
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'trainers' 
        AND column_name = 'working_hours_json'
    ) THEN
        ALTER TABLE trainers 
        ADD COLUMN working_hours_json TEXT NOT NULL DEFAULT '[]';
    END IF;

    -- gym_centers tablosuna working_hours_json ekle (eğer yoksa)
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'gym_centers' 
        AND column_name = 'working_hours_json'
    ) THEN
        ALTER TABLE gym_centers 
        ADD COLUMN working_hours_json TEXT NOT NULL DEFAULT '[]';
    END IF;
END $$;

-- 4. Mevcut NULL değerleri düzelt
UPDATE activities SET description = '' WHERE description IS NULL;
UPDATE activities SET image_url = '' WHERE image_url IS NULL;

-- 5. Migration history kontrolü
DO $$
BEGIN
    -- Migration history tablosu yoksa oluştur
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_name = '__EFMigrationsHistory'
    ) THEN
        CREATE TABLE "__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        );
    END IF;

    -- Eksik migration kayıtlarını ekle
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    SELECT '20251129184151_InitialCreate', '9.0.0'
    WHERE NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20251129184151_InitialCreate'
    );

    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    SELECT '20251129190638_AddWorkingHoursJson', '9.0.0'
    WHERE NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20251129190638_AddWorkingHoursJson'
    );
END $$;

-- 6. Veritabanı durumunu göster
SELECT 
    'gym_centers' as table_name,
    COUNT(*) as record_count
FROM gym_centers
UNION ALL
SELECT 
    'activities' as table_name,
    COUNT(*) as record_count
FROM activities;

