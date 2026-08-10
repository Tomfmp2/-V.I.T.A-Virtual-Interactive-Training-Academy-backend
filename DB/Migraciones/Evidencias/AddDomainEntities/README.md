# Evidencias — AddDomainEntities

Fecha: 2026-08-10  
Rama: `feature/add-domain-entities`  
Base: `origin/develop`

## Compilación

```text
dotnet build
→ Compilación correcta. 0 Errores.
```

## Migraciones listadas

```text
dotnet ef migrations list --project Vita.Api --startup-project Vita.Api
→ 20260810155411_InitialIdentity (Pending)
→ 20260810211253_AddDomainEntities (Pending)
```

## database update (academia_cursos)

```text
Applying migration '20260810155411_InitialIdentity'
42P07: la relación «AspNetRoles» ya existe
```

Historial local previo:

```text
20260810163913_CrearModeloInicialVita
```

## Tablas presentes (consulta solo lectura)

Identity: AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetRoleClaims, AspNetUserClaims, AspNetUserLogins, AspNetUserTokens  

Dominio: categorias, cursos, lecciones, inscripciones, niveles, estados_curso, tipos_leccion, estados_inscripcion  

## Revisión Up/Down

- Up: solo CreateTable dominio/catálogos + índices + FKs Restrict a AspNetUsers  
- Down: solo DropTable de dominio/catálogos  
- Sin AlterColumn / DropColumn sobre AspNet*

## Nota

No se adjuntan connection strings ni contraseñas.
