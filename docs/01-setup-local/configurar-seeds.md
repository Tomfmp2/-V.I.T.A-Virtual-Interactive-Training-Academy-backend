# Configurar seeds locales

> **Basado en rama `develop`** · **Fecha:** 13/08/2026  
> Los seeds crean roles, usuarios demo y catálogos en Development automáticamente al iniciar el API.

---

## Qué crea el seed

Al arrancar en `Development`, después de `Database.Migrate()`, el API crea si no existen:

### Roles
| Rol |
| --- |
| `Admin` |
| `Instructor` |
| `Estudiante` |

### Usuarios demo

| Rol | Email |
| --- | --- |
| Admin | `admin@vita.local` |
| Instructor | `instructor@vita.local` |
| Estudiante | `estudiante@vita.local` |

> Las passwords **no** están en el repo. Obtenerlas del canal del equipo.

### Catálogos
- **Niveles:** Principiante, Intermedio, Avanzado
- **Estados de curso:** Borrador, Publicado
- **Tipos de lección:** Video, Texto, Recurso
- **Estados de inscripción:** Activa, Cancelada
- **Categorías iniciales:** Programación, Diseño, Marketing Digital

El seed es **idempotente**: si ya existen, no se duplican. Reiniciar el API es seguro.

---

## Cómo configurar las passwords

### Método oficial: `.env`

1. Copia la plantilla: `cp .env.example .env`
2. Completa las claves de seed en tu `.env`:

```env
Seeds__DemoUsers__AdminPassword=<password-admin>
Seeds__DemoUsers__InstructorPassword=<password-instructor>
Seeds__DemoUsers__StudentPassword=<password-student>
```

3. Arranca el API: `dotnet run --project Vita.Api`

Ver guía completa: [`configurar-env.md`](configurar-env.md)

---

### Alternativa: User Secrets (PowerShell)

Si por alguna razón no usas `.env`, puedes configurar User Secrets:

```powershell
cd Vita.Api

dotnet user-secrets set "Seeds:DemoUsers:AdminPassword" "<password-admin>"
dotnet user-secrets set "Seeds:DemoUsers:InstructorPassword" "<password-instructor>"
dotnet user-secrets set "Seeds:DemoUsers:StudentPassword" "<password-student>"

dotnet user-secrets list

cd ..
dotnet run --project Vita.Api
```

> Si tienes tanto `.env` como User Secrets, las variables de entorno del `.env` **ganan** sobre User Secrets.

---

## Error común

```
Falta la contraseña de seed 'Seeds:DemoUsers:AdminPassword'.
```

**Causa:** la clave no está en `.env` (o tiene el valor placeholder `REEMPLAZAR`).  
**Solución:** editar `.env` con los valores reales y reiniciar.

---

## Reset de datos demo

1. Borrar la BD local `academia_cursos` (o usar `DROP DATABASE academia_cursos` en psql).
2. Confirmar que `.env` está completo con passwords reales.
3. `dotnet run --project Vita.Api` → aplica migraciones y recrea el seed.

---

## Ver también

- [`configurar-env.md`](configurar-env.md) — configuración completa del `.env`
- [`probar-swagger.md`](probar-swagger.md) — usar las cuentas demo en Swagger
- `Vita.Api/Data/DbSeeder.cs` — código del seed
