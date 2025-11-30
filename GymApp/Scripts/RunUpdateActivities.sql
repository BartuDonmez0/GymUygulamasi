-- =====================================================
-- ACTIVITIES TABLOSU GÜNCELLEME SCRIPT'İ
-- =====================================================
-- Bu script'i psql ile çalıştırın: psql -U postgres -d GymApp -f RunUpdateActivities.sql

-- Eğer tablo varsa, kolonları güncelle
DO $$
BEGIN
    -- description kolonu nullable ise, NOT NULL ve default değer ekle
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'activities' 
        AND column_name = 'description' 
        AND is_nullable = 'YES'
    ) THEN
        -- Önce NULL değerleri düzelt
        UPDATE activities SET description = '' WHERE description IS NULL;
        
        -- Sonra NOT NULL yap
        ALTER TABLE activities 
        ALTER COLUMN description SET NOT NULL,
        ALTER COLUMN description SET DEFAULT '';
    END IF;

    -- image_url kolonu nullable ise, NOT NULL ve default değer ekle
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'activities' 
        AND column_name = 'image_url' 
        AND is_nullable = 'YES'
    ) THEN
        -- Önce NULL değerleri düzelt
        UPDATE activities SET image_url = '' WHERE image_url IS NULL;
        
        -- Sonra NOT NULL yap
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

-- Mevcut NULL değerleri düzelt (güvenlik için tekrar)
UPDATE activities SET description = '' WHERE description IS NULL;
UPDATE activities SET image_url = '' WHERE image_url IS NULL;

