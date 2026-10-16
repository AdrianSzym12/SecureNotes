
-- =====================================================
-- SecureNotes - konfiguracja lokalnych baz danych
-- Uruchom w phpMyAdmin jako administrator MariaDB.
-- Przed wykonaniem ustaw w³asne lokalne has³o.
-- Nie uruchamiaj skryptu na serwerze produkcyjnym.
--
-- Skrypt przygotowuje:
-- 1. securenotes_db      - g³ówna baza aplikacji
-- 2. securenotes_test_db - baza testów automatycznych
-- =====================================================

-- 1. Utworzenie g³ównej bazy aplikacji
CREATE DATABASE IF NOT EXISTS `securenotes_db`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- 2. Utworzenie bazy testów automatycznych
CREATE DATABASE IF NOT EXISTS `securenotes_test_db`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- 3. Utworzenie lokalnego u¿ytkownika MariaDB
CREATE USER IF NOT EXISTS 'securenotes_migrator'@'localhost'
    IDENTIFIED BY 'UZUPELNIJ_WLASNE_HASLO';

-- 4. Uprawnienia do g³ównej bazy aplikacji
GRANT SELECT, INSERT, UPDATE, DELETE,
      CREATE, ALTER, INDEX, DROP, REFERENCES
ON `securenotes_db`.*
TO 'securenotes_migrator'@'localhost';

-- 5. Uprawnienia do bazy testów automatycznych
GRANT SELECT, INSERT, UPDATE, DELETE,
      CREATE, ALTER, INDEX, DROP, REFERENCES
ON `securenotes_test_db`.*
TO 'securenotes_migrator'@'localhost';

-- 6. Kontrola przyznanych uprawnieñ
SHOW GRANTS FOR 'securenotes_migrator'@'localhost';
