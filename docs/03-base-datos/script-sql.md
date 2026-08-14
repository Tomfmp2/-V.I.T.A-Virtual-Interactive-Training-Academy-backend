# Script SQL — propósito y uso correcto

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Qué es `DB/Script.sql`

Es el **modelo conceptual** de la base de datos VITA descrito en sintaxis **DBML** (Database Markup Language).

Sirve para:
- Visualizar el esquema completo en [dbdiagram.io](https://dbdiagram.io) (copiar y pegar el archivo).
- Tener una referencia rápida de tablas, columnas, relaciones y decisiones de diseño.
- Documentar acuerdos de arquitectura (FK como `string`, Identity como fuente de verdad, etc.).

---

## Qué NO es

>  `DB/Script.sql` **no** es el mecanismo de despliegue diario de la base de datos.

| Acción | Método correcto | Script.sql |
| --- | --- | --- |
| Crear el esquema inicial | Migración EF Core | No ejecutar manualmente |
| Agregar columna nueva | `dotnet ef migrations add` | No modificar la BD a mano |
| Actualizar BD local | `dotnet run` (con `Database.Migrate()`) | No usar como script SQL |

Ejecutar `Script.sql` directamente en PostgreSQL desalinearía el historial de migraciones EF Core y rompería el flujo del equipo.

---

## Cómo visualizarlo

1. Abrir [dbdiagram.io](https://dbdiagram.io).
2. Copiar el contenido de `DB/Script.sql`.
3. Pegar en el editor de dbdiagram.io.
4. El diagrama ER aparece automáticamente.

---

## Estructura del script

| Sección | Contenido |
| --- | --- |
| 1. Identity | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` |
| 2. Permisos VITA | `permisos`, `rol_permisos` (opcional / futuro) |
| 3. Catálogos | `categorias`, `niveles`, `estados_curso`, `tipos_leccion`, `estados_inscripcion` |
| 4–7. Dominio | `cursos`, `lecciones`, `inscripciones` |
| Relaciones A–C | Identity, VITA→Identity, VITA interno |

---

## Editar el script

Si el modelo de datos cambia, actualizar `Script.sql` **después** de crear y aplicar la migración EF Core correspondiente. El script es documentación; la migración es la fuente de verdad del esquema en la BD.

---

## Ver también

- Diagrama conceptual: [`modelo-identity.md`](modelo-identity.md)
- Flujo de migraciones: [`migraciones.md`](migraciones.md)
- Archivo fuente: [`DB/Script.sql`](../../DB/Script.sql)
- Documentación extensa: [`DB/Modelo-Datos-Identity.md`](../../DB/Modelo-Datos-Identity.md)
