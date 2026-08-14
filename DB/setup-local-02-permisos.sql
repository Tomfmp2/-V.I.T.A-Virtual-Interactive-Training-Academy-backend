-- =============================================================================
-- setup-local-02-permisos.sql
-- =============================================================================
-- Qué hace:
--   Transfiere el esquema public a vita_user y le otorga permisos CREATE/USAGE
--   para que EF Core pueda crear tablas e __EFMigrationsHistory al arrancar.
--
-- Conectar como:  rol superusuario (típicamente postgres)
-- Base destino:   academia_cursos
--
-- ADVERTENCIA:
--   Este script asume que academia_cursos y vita_user ya existen (paso 01).
--   Ejecutarlo sin el paso previo fallará.
-- =============================================================================

-- Desde PostgreSQL 15, el esquema public ya no otorga permisos de escritura por
-- defecto a roles normales. Sin ALTER SCHEMA OWNER, EF Core no puede crear
-- tablas aunque vita_user sea dueño de la base.
ALTER SCHEMA public OWNER TO vita_user;
GRANT ALL ON SCHEMA public TO vita_user;

-- Verificación: ambas columnas deben devolver true (t en psql).
SELECT
    has_schema_privilege('vita_user', 'public', 'CREATE') AS puede_crear,
    has_schema_privilege('vita_user', 'public', 'USAGE')  AS puede_usar;