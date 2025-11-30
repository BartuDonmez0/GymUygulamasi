-- PostgreSQL veritabanını silmek için kullanılacak script
-- Dikkat: Bu script tüm veritabanını siler!

-- Önce tüm bağlantıları kes
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = 'GymApp'
  AND pid <> pg_backend_pid();

-- Veritabanını sil
DROP DATABASE IF EXISTS "GymApp";

-- Yeni veritabanı oluştur (opsiyonel - migration ile de oluşturulabilir)
-- CREATE DATABASE "GymApp";

