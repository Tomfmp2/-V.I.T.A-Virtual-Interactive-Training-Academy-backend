-- =============================================================================
-- setup-local-01-crear-base.sql
-- =============================================================================
-- Qué hace:
--   Destruye y recrea la base local academia_cursos y el rol vita_user,
--   dejando vita_user como dueño de la base (alineado con appsettings.Development).
--
-- Conectar como:  rol superusuario (típicamente postgres)
-- Base destino:   postgres  (NO academia_cursos)
--
-- ADVERTENCIA:
--   ESTE SCRIPT DESTRUYE LA BASE academia_cursos Y TODOS SUS DATOS LOCALES.
--   Solo úsalo en entornos de desarrollo. No lo ejecutes contra bases compartidas
--   ni de producción.
-- =============================================================================

-- Cierra sesiones activas sobre academia_cursos (excepto la conexión actual).
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'academia_cursos'
  AND pid <> pg_backend_pid();

DROP DATABASE IF EXISTS academia_cursos WITH (FORCE);
DROP ROLE IF EXISTS vita_user;

CREATE ROLE vita_user WITH LOGIN PASSWORD '1234';
CREATE DATABASE academia_cursos OWNER vita_user;