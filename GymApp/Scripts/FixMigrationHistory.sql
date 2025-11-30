-- Migration history'yi düzelt ve eksik kolonları ekle

-- 1. InitialCreate migration'ını history'ye ekle (eğer yoksa)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20251129184151_InitialCreate', '9.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory" 
    WHERE "MigrationId" = '20251129184151_InitialCreate'
);

-- 2. AddWorkingHoursJson migration'ını history'ye ekle (eğer yoksa)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20251129190638_AddWorkingHoursJson', '9.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory" 
    WHERE "MigrationId" = '20251129190638_AddWorkingHoursJson'
);

-- 3. Eksik kolonları ekle (eğer yoksa)
DO $$
BEGIN
    -- trainers tablosuna working_hours_json ekle
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'trainers' 
        AND column_name = 'working_hours_json'
    ) THEN
        ALTER TABLE trainers 
        ADD COLUMN working_hours_json TEXT NOT NULL DEFAULT '[]';
    END IF;

    -- gym_centers tablosuna working_hours_json ekle
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'gym_centers' 
        AND column_name = 'working_hours_json'
    ) THEN
        ALTER TABLE gym_centers 
        ADD COLUMN working_hours_json TEXT NOT NULL DEFAULT '[]';
    END IF;
END $$;

