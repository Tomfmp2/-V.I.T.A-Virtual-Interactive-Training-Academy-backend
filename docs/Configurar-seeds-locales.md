> ⚠️ **Archivo legacy** — el contenido actualizado está en [`01-setup-local/configurar-seeds.md`](01-setup-local/configurar-seeds.md). Este archivo se conserva como referencia histórica.

# Configurar seeds locales (cada PC)

> **Vía oficial actual:** configura connection string, JWT y passwords de seed en un archivo `.env`.  
> Guía paso a paso: [`docs/Configurar-env-local.md`](Configurar-env-local.md).

Esta página deja el método anterior con **User Secrets** (opcional / respaldo). **No incluye las contraseñas reales** (tarjeta Trello de Seeds / doc interno del equipo).

El API, en `Development`, tras `Database.Migrate()`, crea roles, usuarios demo y catálogos. Para hashear las passwords de `admin@vita.local`, `instructor@vita.local` y `estudiante@vita.local` necesita las tres claves `Seeds:DemoUsers:*Password` (vía `.env` o User Secrets).

## Por qué

Sin estos secretos:

```text
Falta la contraseña de seed 'Seeds:DemoUsers:AdminPassword'
```

Git **no** distribuye `.env` ni User Secrets. Cada clone / cada PC = configurar una vez.

## Método recomendado: `.env`

Ver **[Configurar-env-local.md](Configurar-env-local.md)** (copiar `.env.example` → `.env` y completar valores).

## Alternativa: User Secrets (PowerShell)

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

> Si también tienes `.env`, las variables de entorno del `.env` suelen **ganar** sobre User Secrets.

## Reset

Borrar la BD local → confirmar `.env` (o secretos) → `dotnet run --project Vita.Api`.

Más detalle (con credenciales de equipo): `Instrucciones Equipo / Seeds-User-Secrets-paso-a-paso.md` en el vault del líder.
