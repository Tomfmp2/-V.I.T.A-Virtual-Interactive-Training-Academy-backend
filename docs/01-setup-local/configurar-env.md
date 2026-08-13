# Configurar `.env` local (Development)

> **Basado en rama `develop`** · **Fecha:** 13/08/2026  
> Guía paso a paso para arrancar el backend VITA en tu PC **sin** poner secretos en el repositorio.

---

## Qué hace el proyecto al arrancar

1. Busca un archivo `.env` en la raíz del repo (sube carpetas desde donde ejecutas).
2. Carga esas variables con **DotNetEnv**.
3. ASP.NET Core las lee como configuración normal (`ConnectionStrings:Default`, `Jwt:*`, `Seeds:*`).
4. Aplica migraciones pendientes (`Database.Migrate()`).
5. En `Development`: corre el seed de roles, usuarios demo y catálogos.

Sin `.env` completo el API falla al iniciar (no hay connection string / JWT) o al seedear (faltan passwords).

---

## Requisitos previos

- PostgreSQL local con la base `academia_cursos` (o el nombre que elijas).
- .NET SDK 10.
- Repo clonado en la rama `develop`.

---

## Paso 1 — Ubícate en la raíz del repo

```bash
cd ruta/al/repo/-V.I.T.A-Virtual-Interactive-Training-Academy-backend
```

Debe existir el archivo plantilla:

```
.env.example
```

---

## Paso 2 — Copia la plantilla a `.env`

**macOS / Linux / Git Bash:**
```bash
cp .env.example .env
```

**PowerShell:**
```powershell
Copy-Item .env.example .env
```

**CMD:**
```bat
copy .env.example .env
```

> `.env` está en `.gitignore`. **No se sube a GitHub nunca.**

---

## Paso 3 — Completa los valores reales

Abre `.env` y reemplaza los placeholders:

```env
# PostgreSQL
ConnectionStrings__Default=Host=localhost;Port=5432;Database=academia_cursos;Username=TU_USER;Password=TU_PASSWORD

# JWT (solo Development) — mínimo 32 caracteres
Jwt__Key=REEMPLAZAR_CON_CLAVE_LARGA_DE_DESARROLLO_MIN_32_CHARS
Jwt__Issuer=Vita.Api
Jwt__Audience=Vita.Client
Jwt__ExpireSeconds=604800

# Seeds demo (solo Development) — obtén los valores del equipo
Seeds__DemoUsers__AdminPassword=REEMPLAZAR
Seeds__DemoUsers__InstructorPassword=REEMPLAZAR
Seeds__DemoUsers__StudentPassword=REEMPLAZAR
```

### Referencia de variables

| Variable | Qué va ahí |
| --- | --- |
| `ConnectionStrings__Default` | Usuario, password y nombre de tu BD PostgreSQL local |
| `Jwt__Key` | Cadena larga (≥ 32 caracteres). Puede ser igual en todo el equipo para tokens compartidos en pruebas |
| `Jwt__Issuer` / `Jwt__Audience` | Dejar `Vita.Api` / `Vita.Client` salvo acuerdo distinto |
| `Jwt__ExpireSeconds` | Vida del token en segundos (`604800` = 7 días) |
| `Seeds__DemoUsers__*Password` | Passwords de las cuentas demo — obtener del canal del equipo |

> **Nota:** la doble barra baja `__` equivale a `:` en appsettings.  
> `ConnectionStrings__Default` == `ConnectionStrings:Default`

---

## Paso 4 — Verifica que `.env` no esté tracked

```bash
git status
```

`.env` **no** debe aparecer. Si aparece, revisa `.gitignore`.

---

## Paso 5 — Arranca el API

Desde la raíz del repo:

```bash
dotnet run --project Vita.Api --launch-profile http
```

O desde dentro de `Vita.Api/`:

```bash
dotnet run --launch-profile http
```

El código busca `.env` subiendo carpetas, funciona en ambos casos.

| Recurso | URL |
| --- | --- |
| API local | `http://localhost:5044` |
| Swagger | `http://localhost:5044/swagger` |

---

## Paso 6 — Verifica el login con una cuenta demo

Después del seed existen estas cuentas:

| Rol | Email |
| --- | --- |
| Admin | `admin@vita.local` |
| Instructor | `instructor@vita.local` |
| Estudiante | `estudiante@vita.local` |

Password: la que pusiste en `Seeds__DemoUsers__*Password` en tu `.env`.

Prueba rápida:

```http
POST http://localhost:5044/api/auth/login
Content-Type: application/json

{
  "email": "admin@vita.local",
  "password": "<tu-password-admin>"
}
```

Respuesta esperada: `200` con `token`.

---

## Errores frecuentes

| Síntoma | Causa probable | Solución |
| --- | --- | --- |
| Error de conexión a PostgreSQL | User/password/host incorrecto en `ConnectionStrings__Default` | Corregir `.env` y reiniciar |
| `Falta la contraseña de seed` | Faltan `Seeds__DemoUsers__*` o tienen valor `REEMPLAZAR` | Completar passwords reales en `.env` |
| JWT firma inválida | `Jwt__Key` vacía, corta o cambiada | Key ≥ 32 chars; volver a hacer login |
| API no arranca / no encuentra config | `.env` no está en la raíz del repo | Mover `.env` a la raíz del repo |

---

## Reset de datos demo

1. Borra o recrea la BD local `academia_cursos`.
2. Confirma que `.env` está completo.
3. `dotnet run --project Vita.Api` → migrate + seed automático.

---

## Checklist rápido

- [ ] `cp .env.example .env`
- [ ] Completé PostgreSQL, Jwt__Key y las 3 passwords de seed
- [ ] `git status` no muestra `.env`
- [ ] `dotnet run --project Vita.Api` arranca sin errores
- [ ] Login con `admin@vita.local` responde `200`

---

## Ver también

- Plantilla: [`.env.example`](../../.env.example)
- Seeds: [`configurar-seeds.md`](configurar-seeds.md)
- Probar la API: [`probar-swagger.md`](probar-swagger.md)
