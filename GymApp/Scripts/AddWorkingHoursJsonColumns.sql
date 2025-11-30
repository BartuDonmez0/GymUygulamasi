-- Add working_hours_json column to trainers table if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'trainers' 
        AND column_name = 'working_hours_json'
    ) THEN
        ALTER TABLE trainers 
        ADD COLUMN working_hours_json TEXT NOT NULL DEFAULT '[]';
    END IF;
END $$;

-- Add working_hours_json column to gym_centers table if it doesn't exist
DO $$
BEGIN
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

