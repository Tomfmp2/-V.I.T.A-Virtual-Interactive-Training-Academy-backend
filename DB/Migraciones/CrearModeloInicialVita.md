# Migraci├│n inicial de VITA

## Informaci├│n general

- Migraci├│n: `CrearModeloInicialVita`
- Identificador: `20260810163913_CrearModeloInicialVita`
- Proyecto: `Vita.Api`
- Base de datos: `academia_cursos`
- Motor: PostgreSQL
- Estado: Aplicada y verificada localmente
- Fecha: 10 de agosto de 2026

## Objetivo

Crear la estructura inicial de la base de datos de VITA a partir del modelo definido en C#.

Esta migraci├│n incluye las tablas necesarias para ASP.NET Identity y las primeras entidades del dominio de la plataforma.

No se incluyeron las tablas `permisos` y `rol_permisos`, ya que en la documentaci├│n del modelo aparecen como opcionales y no forman parte del alcance actual.

## Cambios incluidos

La migraci├│n crea las tablas de ASP.NET Identity:

- `AspNetUsers`
- `AspNetRoles`
- `AspNetUserRoles`
- `AspNetUserClaims`
- `AspNetRoleClaims`
- `AspNetUserLogins`
- `AspNetUserTokens`

Tambi├®n crea las siguientes tablas del dominio:

- `categorias`
- `niveles`
- `estados_curso`
- `tipos_leccion`
- `estados_inscripcion`
- `cursos`
- `lecciones`
- `inscripciones`
- `progreso_lecciones`

Adem├ís, se crearon las claves for├íneas, los ├¡ndices y las restricciones ├║nicas definidas en el modelo.

Las relaciones hacia los usuarios de Identity utilizan el tipo `string`, de acuerdo con el tipo de identificador de `AspNetUsers`.

## Archivos generados

La migraci├│n se encuentra en:

```text
Vita.Api/Migrations/
```

Archivos principales:

```text
20260810163913_CrearModeloInicialVita.cs
20260810163913_CrearModeloInicialVita.Designer.cs
VitaDbContextModelSnapshot.cs
```

El archivo principal contiene los m├®todos `Up()` y `Down()`.

- `Up()` crea las tablas, relaciones e ├¡ndices.
- `Down()` elimina los objetos creados por la migraci├│n si se solicita revertirla.

No se modificaron manualmente los archivos generados por EF Core despu├®s de crear la migraci├│n.

## Comando utilizado

La migraci├│n se cre├│ desde la ra├¡z de la soluci├│n con el siguiente comando:

```bash
dotnet ef migrations add CrearModeloInicialVita --project Vita.Api --startup-project Vita.Api
```

El proyecto `Vita.Api` se utiliza tanto como proyecto del `DbContext` como proyecto de inicio.

## Revisi├│n de la migraci├│n

Se revis├│ el m├®todo `Up()` y se confirm├│ que contiene operaciones de creaci├│n de tablas, ├¡ndices y claves for├íneas.

No se encontraron operaciones como:

```text
DropTable
DropColumn
AlterColumn
```

La migraci├│n inicial contiene un m├®todo `Down()` que elimina las tablas creadas. Este comportamiento es esperado, pero no debe ejecutarse sobre una base que ya contenga datos importantes.

## Aplicaci├│n en PostgreSQL

La migraci├│n se aplic├│ en la base local `academia_cursos` mediante:

```bash
dotnet ef database update --project Vita.Api --startup-project Vita.Api
```

Resultado obtenido:

```text
Applying migration '20260810163913_CrearModeloInicialVita'.
Done.
```

## Verificaciones realizadas

Se verific├│ el historial de migraciones con:

```sql
SELECT *
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
```

La migraci├│n registrada fue:

```text
20260810163913_CrearModeloInicialVita
```

Tambi├®n se verific├│ la creaci├│n de las tablas de Identity y de las tablas principales del dominio.

Finalmente, se ejecut├│ la aplicaci├│n con:

```bash
dotnet run --project Vita.Api
```

La API inici├│ correctamente.

## Evidencias

Las capturas relacionadas con esta migraci├│n se encuentran en:

```text
DB/Migraciones/Evidencias/CrearModeloInicialVita/
```

Archivos:

- `01_migrations_list.png`
- `02_migration_files.png`
- `03_migration_up.png`
- `04_database_update.png`
- `05_efmigrationshistory.png`
- `06_database_tables.png`
- `07_application_running.png`

## Observaciones

- La migraci├│n se prob├│ en una base de datos local.
- La contrase├▒a de PostgreSQL se configur├│ mediante User Secrets y no se incluy├│ en el repositorio.
- Las tablas `permisos` y `rol_permisos` quedaron fuera de esta migraci├│n por decisi├│n de alcance.
- La advertencia `NU1903` relacionada con `Microsoft.OpenApi` es un pendiente independiente de esta migraci├│n.

## Estado final

La migraci├│n `CrearModeloInicialVita` qued├│:

```text
Creada
Revisada
Aplicada localmente
Verificada en PostgreSQL
Documentada
```
