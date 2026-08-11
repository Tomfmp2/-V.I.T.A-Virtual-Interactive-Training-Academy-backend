# Configurar seeds locales (cada PC)

Guía del repositorio. **No incluye las contraseñas reales** (están en la tarjeta Trello de Seeds y en el doc interno del equipo).

El API, en `Development`, tras `Database.Migrate()`, crea roles, usuarios demo y catálogos. Para hashear las passwords de `admin@vita.local`, `instructor@vita.local` y `estudiante@vita.local` necesita tres secretos **en esa máquina**.

## Por qué

Sin estos secretos:

```text
Falta la contraseña de seed 'Seeds:DemoUsers:AdminPassword'
```

Git **no** distribuye User Secrets. Cada clone / cada PC = configurar una vez.

## Pasos (PowerShell)

```powershell
cd <ruta-del-repo>\Vita.Api

dotnet user-secrets set "Seeds:DemoUsers:AdminPassword" "<password-admin>"
dotnet user-secrets set "Seeds:DemoUsers:InstructorPassword" "<password-instructor>"
dotnet user-secrets set "Seeds:DemoUsers:StudentPassword" "<password-student>"

dotnet user-secrets list

cd ..
dotnet run --project Vita.Api
```

Valores de `<password-*>`: tarjeta Trello **Seeds en el API**.

## Reset

Borrar la BD local → confirmar secretos → `dotnet run --project Vita.Api`.

Más detalle (con credenciales de equipo): `Instrucciones Equipo / Seeds-User-Secrets-paso-a-paso.md` en el vault del líder.
