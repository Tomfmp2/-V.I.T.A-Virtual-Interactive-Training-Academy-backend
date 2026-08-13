> ⚠️ **Archivo legacy** — el contenido actualizado está en [`01-setup-local/configurar-env.md`](01-setup-local/configurar-env.md). Este archivo se conserva como referencia histórica.

# Configurar `.env` local (Development)

Guía paso a paso para que el backend VITA arranque en tu PC **sin** poner secretos en `appsettings.Development.json`.

Los valores reales (passwords de PostgreSQL, JWT y seeds) **no van en git**. Cada integrante copia la plantilla y completa la suya.

---

## Qué hace el proyecto

Al arrancar en Development:

1. Busca un archivo `.env` (en la raíz del repo o subiendo carpetas desde donde ejecutas).
2. Carga esas variables con **DotNetEnv**.
3. ASP.NET Core las lee como configuración normal (`ConnectionStrings:Default`, `Jwt:*`, `Seeds:*`).
4. Aplica migraciones y corre el seed de usuarios/catálogos demo.

Sin un `.env` completo, el API no tendrá connection string / JWT / passwords de seed y fallará al iniciar o al seedeear.

---

## Requisitos previos

- PostgreSQL local con la base `academia_cursos` (o el nombre que uses).
- .NET SDK del proyecto.
- Repo clonado en la rama `develop` (o actualizada).

---

## Paso 1 — Ubícate en la raíz del repo

```powershell
cd C:\Users\<TU_USUARIO>\Documents\Proyecto_VITA\-V.I.T.A-Virtual-Interactive-Training-Academy-backend
```

Debe existir el archivo plantilla:

```text
.env.example
```

---

## Paso 2 — Copia la plantilla a `.env`

**PowerShell:**

```powershell
Copy-Item .env.example .env
```

**CMD:**

```bat
copy .env.example .env
```

**Git Bash / macOS / Linux:**

```bash
cp .env.example .env
```

`.env` ya está en `.gitignore`: **no se sube a GitHub**.

---

## Paso 3 — Completa los valores reales

Abre `.env` en el editor y sustituye los placeholders.

### Plantilla (referencia)

```env
# PostgreSQL
ConnectionStrings__Default=Host=localhost;Port=5432;Database=academia_cursos;Username=TU_USER;Password=TU_PASSWORD

# JWT (solo Development) — la Key debe tener al menos 32 caracteres
Jwt__Key=REEMPLAZAR_CON_CLAVE_LARGA_DE_DESARROLLO_MIN_32_CHARS
Jwt__Issuer=Vita.Api
Jwt__Audience=Vita.Client
Jwt__ExpireSeconds=604800

# Seeds demo (solo Development)
Seeds__DemoUsers__AdminPassword=REEMPLAZAR
Seeds__DemoUsers__InstructorPassword=REEMPLAZAR
Seeds__DemoUsers__StudentPassword=REEMPLAZAR
```

### Qué poner en cada uno

| Variable | Qué va ahí |
| --- | --- |
| `ConnectionStrings__Default` | Tu usuario/password de PostgreSQL local y el nombre de la BD |
| `Jwt__Key` | Cadena larga de desarrollo (≥ 32 caracteres). Misma en todo el equipo de Dev si quieren tokens compatibles en pruebas compartidas |
| `Jwt__Issuer` / `Jwt__Audience` | Dejar `Vita.Api` / `Vita.Client` salvo que el equipo acuerde otra cosa |
| `Jwt__ExpireSeconds` | Tiempo de vida del token (ej. `604800` = 7 días) |
| `Seeds__DemoUsers__*Password` | Passwords de las cuentas demo (tarjeta Trello **Seeds en el API** / doc interno del equipo) |

Nota: en `.env` se usa **doble guion bajo** `__`. Equivale a `:` en appsettings (`ConnectionStrings__Default` = `ConnectionStrings:Default`).

---

## Paso 4 — Verifica que `.env` no esté tracked

```powershell
git status
```

No debe aparecer `.env` como archivo para commit. Si aparece, **no lo agregues**; revisa `.gitignore`.

---

## Paso 5 — Arranca el API

Desde la **raíz del repo**:

```powershell
dotnet run --project Vita.Api --launch-profile http
```

O desde `Vita.Api`:

```powershell
cd Vita.Api
dotnet run --launch-profile http
```

El código busca `.env` subiendo carpetas, así que funciona en ambos casos si el archivo está en la raíz del repo.

API local típico: `http://localhost:5044`  
Swagger: `http://localhost:5044/swagger`

---

## Paso 6 — Comprueba login con cuentas demo

Tras el seed, las cuentas por defecto son:

| Rol | Email |
| --- | --- |
| Admin | `admin@vita.local` |
| Instructor | `instructor@vita.local` |
| Estudiante | `estudiante@vita.local` |

Password: la que pusiste en `Seeds__DemoUsers__*Password` de tu `.env`.

Prueba rápida:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@vita.local",
  "password": "<tu-password-admin-del-env>"
}
```

---

## Errores frecuentes

| Síntoma | Causa probable | Qué hacer |
| --- | --- | --- |
| Error de conexión a PostgreSQL | User/password/host mal en `ConnectionStrings__Default` | Corregir `.env` y reiniciar el API |
| Falta password de seed / no crea demos | Faltan `Seeds__DemoUsers__*` o valores placeholder | Completar passwords reales en `.env` |
| JWT / firma inválida | `Jwt__Key` vacía, corta o distinta entre reinicios al probar tokens viejos | Key ≥ 32 chars; volver a hacer login |
| “No encuentra” configuración | Ejecutaste desde otra carpeta y no hay `.env` arriba en el path | Pon `.env` en la raíz del repo |
| Alguien subió su connection string otra vez | Diff en `appsettings.Development.json` con secretos | Rechazar en PR; los secretos van solo en `.env` |

---

## Relación con User Secrets (opcional)

Antes las passwords de seed iban con `dotnet user-secrets`. **La vía oficial ahora es `.env`.**

User Secrets sigue pudiendo funcionar como respaldo (la jerarquía de ASP.NET lo permite), pero el equipo debe unificarse en `.env` para connection string + JWT + seeds.

---

## Reset de datos demo

1. Borra o recrea la BD local `academia_cursos`.
2. Confirma que tu `.env` está completo.
3. `dotnet run --project Vita.Api` → migrate + seed de nuevo.

---

## Checklist rápido

- [ ] `Copy-Item .env.example .env`
- [ ] Completé PostgreSQL, JWT Key y las 3 passwords de seed
- [ ] `git status` no muestra `.env`
- [ ] `dotnet run --project Vita.Api` arranca
- [ ] Login con `admin@vita.local` (u otra demo) funciona

---

## Ver también

- Plantilla: `.env.example` (raíz del repo)
- Cuentas demo: `Datos-por-defecto-seeds.md`
- Seeds (histórico User Secrets): `docs/Configurar-seeds-locales.md`
