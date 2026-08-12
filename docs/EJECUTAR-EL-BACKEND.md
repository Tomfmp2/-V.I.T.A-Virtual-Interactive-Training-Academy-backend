# Ejecutar el backend

Guía única para levantar VITA en local (Windows). Primera vez: unos **10 minutos**.

## Ruta rápida

```powershell
git clone <repo>
cd <repo>
.\setup-dev.ps1
```

El script pide la contraseña de `postgres` y las tres contraseñas de usuarios demo (tarjeta Trello **Seeds en el API**). Al terminar deja el API corriendo; Swagger: `http://localhost:5044/swagger`.

Si PowerShell bloquea scripts:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\setup-dev.ps1
```

O usa la [ruta manual](#ruta-manual-paso-a-paso).

## Requisitos previos

- SDK .NET 10
- PostgreSQL 13 o superior, instalado y corriendo

Verificar:

```powershell
dotnet --version
Get-Service -Name "postgresql*"
```

Ten a mano antes de empezar:

- Contraseña del rol `postgres` (la definiste al instalar PostgreSQL)
- Tres contraseñas de usuarios demo (tarjeta Trello **Seeds en el API**)

El script **no modifica `appsettings.Development.json`** y no hace falta
editarlo. La contraseña del superusuario solo se usa para la conexión
administrativa del momento y no se guarda en ningún archivo. El script 01
fija la contraseña de `vita_user` en `1234`, que es exactamente la que ya
está en `appsettings.Development.json`, así que ambos lados quedan
alineados sin intervención manual.

## Ruta manual (paso a paso)

Misma secuencia que el script, sin automatizar.

### Paso 1 — Base de datos

`psql` suele **no** estar en el PATH. Ajusta la versión si no es 18:

```powershell
$env:Path += ";C:\Program Files\PostgreSQL\18\bin"
```

**ADVERTENCIA:** el primer script **DESTRUYE** la base `academia_cursos` y todos sus datos locales.

```powershell
psql -U postgres -h localhost -d postgres -f DB/setup-local-01-crear-base.sql
psql -U postgres -h localhost -d academia_cursos -f DB/setup-local-02-permisos.sql
```

Si tu superusuario no se llama `postgres`, cambia `-U postgres` por tu rol
en ambos comandos.

`psql` sin `-U` ni `-d` intenta conectarse con el usuario de Windows y falla. Siempre pasa ambos.

La verificación del paso 02 debe devolver `true` / `true` (o `t` / `t`) en `puede_crear` y `puede_usar`.

### Paso 2 — Secretos del seeder

Los User Secrets **no** viajan por git. Una vez por PC. Sin ellos el API aplica migraciones y luego falla con `Falta la contraseña de seed`.

```powershell
cd Vita.Api

dotnet user-secrets set "Seeds:DemoUsers:AdminPassword" "<password-admin>"
dotnet user-secrets set "Seeds:DemoUsers:InstructorPassword" "<password-instructor>"
dotnet user-secrets set "Seeds:DemoUsers:StudentPassword" "<password-student>"

dotnet user-secrets list

cd ..
```

Valores de `<password-*>`: tarjeta Trello **Seeds en el API**.

### Paso 3 — Levantar

```powershell
dotnet restore
dotnet run --project Vita.Api
```

## Verificar que quedó bien

- El API imprime `Now listening on: http://localhost:5044`
- `http://localhost:5044/swagger` abre
- Esta consulta muestra `vita_user` en todas las filas:

```sql
SELECT tablename, tableowner
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY tablename;
```

- `POST /api/auth/login` con `admin@vita.local` (y la password de seed) → **200** y un token
- `GET /api/roles` con ese token → los 3 roles (`Admin`, `Instructor`, `Estudiante`)

## Solución de problemas

| Síntoma | Qué hacer |
| --- | --- |
| `Npgsql.PostgresException 42501` permiso denegado a `__EFMigrationsHistory` | No corriste el paso 02, o las tablas las creó otro rol. Repite el paso 1 completo. No ejecutes `DB/Script.sql`. |
| `Falta la contraseña de seed 'Seeds:DemoUsers:AdminPassword'` | Faltan User Secrets. Repite el paso 2 (o `.\setup-dev.ps1` sin `-SkipSecrets`). |
| `no se puede eliminar la base de datos actual` | Estás conectado a `academia_cursos` en el paso 01. Conéctate a `postgres` (`-d postgres`). |
| `la base de datos está siendo utilizada por otros usuarios` | Detén el API (Ctrl+C) y cierra conexiones de DBeaver. Vuelve a ejecutar el paso 01. |
| `no se puede eliminar el rol «vita_user» porque otros objetos dependen de él` | Versión antigua del script 01. Actualiza la rama: el script ya no elimina el rol. |
| `autentificación password falló para el usuario «postgres»` | Contraseña del superusuario incorrecta, o tu superusuario se llama distinto. Usa `.\setup-dev.ps1 -SuperUser "<tu-rol>"`. |
| `DROP DATABASE cannot run inside a transaction block` | En DBeaver: pon la conexión en **Auto** (no Manual Commit) y ejecuta con **Alt+X**. |
| `psql` no se reconoce como comando | Agrégalo al PATH (ver paso 1) o pasa `-PsqlPath` a `setup-dev.ps1`. |
| autentificación password falló para el usuario «<usuario-windows>» | Faltó `-U postgres`. Siempre usa `-U` y `-d`. |
| Login **401** con usuarios demo | Usuarios preexistentes con otra password; el seeder no reconcilia contraseñas. Recrea la base (paso 01) y vuelve a arrancar. |
| `no se puede cargar el archivo setup-dev.ps1 porque la ejecución de scripts está deshabilitada` | Usa la ruta manual, o: `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` |

## Alternativa con DBeaver (paso 1)

1. Conexión con el usuario **postgres**.
2. Script 01: editor SQL sobre la base **postgres** → pega `DB/setup-local-01-crear-base.sql` → **Alt+X** (no Ctrl+Enter).
3. Script 02: editor SQL sobre **academia_cursos** → pega `DB/setup-local-02-permisos.sql` → **Alt+X**.
4. La conexión debe estar en modo **Auto**, no Manual Commit.

Los comandos que empiezan con `\` (por ejemplo `\c`, `\dt`) son exclusivos de **psql**; DBeaver no los entiende.

## Nota importante

`DB/Script.sql` es **material de referencia** del modelo de datos. **No** lo ejecutes para montar el entorno: el esquema lo generan las migraciones de EF Core (`db.Database.Migrate()`). Ejecutar el script SQL y las migraciones juntos provoca el conflicto de dueños (`postgres` vs `vita_user`) y el error **42501**.

Más detalle de seeds: `docs/Configurar-seeds-locales.md`.
