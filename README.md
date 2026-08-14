# VITA — Backend

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

API REST de **V.I.T.A** (Virtual Interactive Training Academy), plataforma de cursos
en línea con autenticación por JWT y tres roles: administrador, instructor y
estudiante.

Proyecto 3 · CAMPUSLANDS

---

## Índice

- [Stack](#stack)
- [Requisitos](#requisitos)
- [Cómo iniciar](#cómo-iniciar)
- [Cómo usar](#cómo-usar)
- [Configuración](#configuración)
- [Usuarios de prueba](#usuarios-de-prueba)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Módulos y endpoints](#módulos-y-endpoints)
- [Roles y reglas de negocio](#roles-y-reglas-de-negocio)
- [Contrato de errores](#contrato-de-errores)
- [Archivos subidos](#archivos-subidos)
- [Cómo verificar](#cómo-verificar)
- [Cómo actualizar](#cómo-actualizar)
- [Base de datos y migraciones](#base-de-datos-y-migraciones)
- [Cómo desplegar](#cómo-desplegar)
- [Convenciones](#convenciones)
- [Documentación](#documentación)
- [Solución de problemas](#solución-de-problemas)

---

## Stack

| Capa | Tecnología |
| --- | --- |
| Entorno de ejecución | .NET 10 |
| API | ASP.NET Core Web API en C# |
| Persistencia | Entity Framework Core con Npgsql |
| Base de datos | PostgreSQL |
| Autenticación | ASP.NET Core Identity con JWT Bearer |
| Documentación interactiva | Swagger mediante Swashbuckle |
| Configuración local | DotNetEnv, archivo `.env` |
| Arquitectura | Capas en un solo proyecto: Controllers, Services, Repositories |

Regla de dependencias:

```
Controllers -> Services -> Repositories -> DbContext
```

Los controladores no acceden nunca a la persistencia de forma directa. Esa
separación es la que permite probar la lógica de negocio sin levantar la base de
datos.

---

## Requisitos

| Herramienta | Versión | Comprobación |
| --- | --- | --- |
| SDK de .NET | 10 | `dotnet --version` |
| PostgreSQL | 14 o superior | `psql --version` |

---

## Cómo iniciar

### 1. Clonar

```bash
git clone <url-del-repositorio>
cd V.I.T.A-Virtual-Interactive-Training-Academy-backend
```

### 2. Crear la base de datos

```bash
createdb academia_cursos
```

**No ejecutes `DB/Script.sql`.** Ese archivo es material de referencia del modelo de
datos. El esquema lo generan las migraciones de EF Core al arrancar. Ejecutar el
script y las migraciones a la vez provoca un conflicto de propietarios de objetos
entre `postgres` y `vita_user`.

### 3. Configurar el entorno

```bash
cp .env.example .env
```

Completa los valores en `.env`:

```bash
ConnectionStrings__Default=Host=localhost;Port=5432;Database=academia_cursos;Username=postgres;Password=<tu-password>

Jwt__Key=<clave-de-al-menos-32-caracteres>
Jwt__Issuer=Vita.Api
Jwt__Audience=Vita.Client
Jwt__ExpireSeconds=604800

Seeds__DemoUsers__AdminPassword=<password-admin>
Seeds__DemoUsers__InstructorPassword=<password-instructor>
Seeds__DemoUsers__StudentPassword=<password-estudiante>
```

La doble barra baja equivale a los dos puntos de `appsettings.json`:
`ConnectionStrings__Default` corresponde a `ConnectionStrings:Default`.

El archivo `.env` se busca desde el directorio de trabajo hacia arriba, así que
funciona tanto si arrancas desde la raíz como desde `Vita.Api`.

### 4. Restaurar y arrancar

```bash
dotnet restore
dotnet run --project Vita.Api
```

Al arrancar en Development, el API aplica las migraciones pendientes y crea los datos
de prueba si no existen.

### 5. Comprobar

```
http://localhost:5044/swagger
```

### Alternativa en Windows

Hay un script que automatiza base de datos, secretos y arranque:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\setup-dev.ps1
```

La primera línea autoriza la ejecución de scripts solo en esa ventana de PowerShell,
que los bloquea por defecto. No cambia la configuración del sistema ni requiere
permisos de administrador.

Detalle y solución de problemas en [`docs/EJECUTAR-EL-BACKEND.md`](docs/EJECUTAR-EL-BACKEND.md).

---

## Cómo usar

### Prefijo y autenticación

Todos los endpoints cuelgan de `/api`. Los protegidos esperan la cabecera:

```
Authorization: Bearer <token>
```

El token se obtiene en `POST /api/auth/login` y por defecto dura siete días.

### Flujo en Swagger

1. Abre `http://localhost:5044/swagger`.
2. Ejecuta `POST /api/auth/login` con una cuenta de prueba.
3. Copia el valor de `token` de la respuesta.
4. Pulsa **Authorize** e introduce `Bearer <token>`.
5. Ya puedes ejecutar el resto de endpoints con esa identidad.

Guía ampliada en [`docs/01-setup-local/probar-swagger.md`](docs/01-setup-local/probar-swagger.md).

### Ejemplo con curl

```bash
TOKEN=$(curl -s -X POST http://localhost:5044/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@vita.local","password":"<password>"}' \
 | python3 -c "import sys,json; print(json.load(sys.stdin)['token'])")

curl -s http://localhost:5044/api/auth/me -H "Authorization: Bearer $TOKEN"
```

---

## Configuración

Claves que cada máquina necesita **fuera del control de versiones**:

| Clave | Uso |
| --- | --- |
| `ConnectionStrings:Default` | Cadena de conexión a PostgreSQL |
| `Jwt:Key` | Clave de firma del token, mínimo 32 caracteres |
| `Jwt:Issuer` | Emisor del token |
| `Jwt:Audience` | Audiencia del token |
| `Jwt:ExpireSeconds` | Vigencia del token en segundos |
| `Seeds:DemoUsers:AdminPassword` | Contraseña de la cuenta demo de administrador |
| `Seeds:DemoUsers:InstructorPassword` | Contraseña de la cuenta demo de instructor |
| `Seeds:DemoUsers:StudentPassword` | Contraseña de la cuenta demo de estudiante |

Tres formas de proporcionarlas, en orden de precedencia creciente:

| Mecanismo | Cuándo conviene |
| --- | --- |
| `appsettings.json` | Solo valores no sensibles |
| Archivo `.env` | Desarrollo local, es la vía habitual del equipo |
| User Secrets o variables de entorno | Alternativa cuando no se quiere archivo en disco |

Con User Secrets, desde la carpeta que contiene el `.csproj`:

```bash
cd Vita.Api
dotnet user-secrets set "Seeds:DemoUsers:AdminPassword" "<password>"
dotnet user-secrets list
```

Ninguna contraseña debe subirse al repositorio. `.env` está ignorado por git;
`.env.example` sí se versiona, con valores de ejemplo.

Si falta alguna contraseña de seed, el API aplica las migraciones y después falla con
`Falta la contraseña de seed`. Es un fallo deliberado y con mensaje explícito: es
preferible a arrancar con credenciales por defecto.

---

## Usuarios de prueba

En Development, después de aplicar migraciones, el seed crea los datos que no
existan:

- Los roles `Admin`, `Instructor` y `Estudiante`
- Una cuenta por rol
- Categorías y niveles de ejemplo

| Cuenta | Rol |
| --- | --- |
| `admin@vita.local` | Admin |
| `instructor@vita.local` | Instructor |
| `estudiante@vita.local` | Estudiante |

Las contraseñas salen de la configuración local y no figuran en este README.

El seed es idempotente: reiniciar el API no duplica registros. **No se ejecuta en
Production.**

Para reiniciar los datos: borra la base, confirma que la configuración sigue en su
sitio y vuelve a arrancar.

---

## Estructura del proyecto

```
Vita.sln
Vita.Api/
├── Controllers/     # Endpoints HTTP
├── Services/        # Lógica de negocio y reglas
├── Repositories/    # Acceso a datos con EF Core
├── Entities/        # Entidades del modelo
├── Dtos/            # Contratos de entrada y salida
├── Data/            # DbContext y seed
├── Config/          # Identity, JWT, Swagger
├── Identity/        # Personalización de Identity, mensajes en español
├── Middleware/      # Manejo global de errores
├── Migrations/      # Migraciones de EF Core
├── wwwroot/         # Archivos estáticos servidos, incluidas las fotos de perfil
├── Program.cs
└── appsettings.json
DB/                  # Script SQL de referencia, no de despliegue
docs/                # Documentación técnica
scripts/             # Utilidades auxiliares
```

---

## Módulos y endpoints

| Módulo | Endpoints | Documentación |
| --- | --- | --- |
| Autenticación | `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/logout`, `GET /api/auth/me` | [01-auth.md](docs/02-modulos-api/01-auth.md) |
| Configuración de perfil | `PUT /api/auth/me`, `POST /api/auth/me/photo`, `POST /api/auth/change-password` | [10-profile-settings.md](docs/02-modulos-api/10-profile-settings.md) |
| Usuarios | `GET`, `POST /api/users`, `GET`, `PUT /api/users/{id}`, `PATCH /api/users/{id}/role`, `PATCH /api/users/{id}/status` | [02-users.md](docs/02-modulos-api/02-users.md) |
| Roles | `GET /api/roles` | [03-roles.md](docs/02-modulos-api/03-roles.md) |
| Categorías | CRUD en `/api/categories` | [04-categories.md](docs/02-modulos-api/04-categories.md) |
| Niveles | CRUD en `/api/levels` | [09-levels.md](docs/02-modulos-api/09-levels.md) |
| Cursos | CRUD en `/api/courses`, `GET /api/courses/me`, `PATCH /api/courses/{id}/status` | [05-courses.md](docs/02-modulos-api/05-courses.md) |
| Lecciones | CRUD en `/api/courses/{courseId}/lessons` | [06-lessons.md](docs/02-modulos-api/06-lessons.md) |
| Inscripciones | `POST /api/enrollments`, `GET /api/enrollments/me` | [07-enrollments.md](docs/02-modulos-api/07-enrollments.md) |
| Reportes | `GET /api/reports/courses-by-instructor`, `/students-by-course`, `/top-courses` | [08-reports.md](docs/02-modulos-api/08-reports.md) |

Resumen ejecutivo de todos los endpoints con sus roles en
[`docs/00-convenciones/contrato-api.md`](docs/00-convenciones/contrato-api.md).

---

## Roles y reglas de negocio

| Acción | Admin | Instructor | Estudiante |
| --- | --- | --- | --- |
| Gestionar usuarios y roles | Sí | No | No |
| Gestionar categorías y niveles | Sí | No | No |
| Crear, editar y borrar cursos | Sí | Solo los propios | No |
| Publicar un curso | No | Solo los propios | No |
| Gestionar lecciones | Sí | Solo en cursos propios | No |
| Inscribirse en un curso | No | No | Sí |
| Ver reportes | Todos | Solo de sus cursos | No |
| Gestionar su perfil | Sí | Sí | Sí |

Reglas que conviene destacar porque no se deducen de la tabla:

**Publicar es exclusivo del instructor asignado.** Un administrador administra el
contenido, pero declarar un curso terminado corresponde a quien lo produce. El
intento devuelve 403 con `Solo el instructor asignado puede publicar el curso.`

**Un curso necesita al menos una lección para publicarse.** En otro caso responde
400.

**Un curso publicado no se puede borrar.** Hay que pasarlo antes a borrador. Es una
salvaguarda frente a la eliminación de contenido con estudiantes inscritos.

**Las cuentas no se borran, se desactivan.** Así se conserva la integridad de cursos,
inscripciones e histórico. Una cuenta desactivada no puede iniciar sesión.

**El rol viaja dentro del JWT.** Un cambio de rol no afecta a una sesión ya abierta
hasta que su token caduque.

---

## Contrato de errores

Todas las respuestas de error comparten la misma forma:

```json
{ "error": "El nombre debe tener entre 3 y 100 caracteres.", "statusCode": 400 }
```

| Código | Significado |
| --- | --- |
| 400 | Datos inválidos o regla de negocio incumplida |
| 401 | Falta el token, es inválido o la contraseña actual no coincide |
| 403 | Autenticado pero sin permiso para la operación |
| 404 | El recurso no existe |
| 409 | Conflicto, por ejemplo un nombre duplicado o un recurso en uso |

El mensaje viene redactado en español y es apto para mostrarse al usuario final. Un
middleware centraliza el formato, de modo que ningún controlador construye respuestas
de error a mano.

Detalle en [`docs/00-convenciones/patron-errores.md`](docs/00-convenciones/patron-errores.md).

---

## Archivos subidos

Las fotos de perfil son el único archivo que acepta el API.

| Aspecto | Comportamiento |
| --- | --- |
| Ubicación | `Vita.Api/wwwroot/uploads/profiles/{userId}.{extensión}` |
| En base de datos | Solo la ruta relativa, en `AspNetUsers.FotoUrl` |
| URL pública | `http://localhost:5044/uploads/profiles/{userId}.{extensión}` |
| Formatos | JPG, PNG y WEBP |
| Tamaño máximo | 2 MB |
| Versiones anteriores | Se borran al subir una nueva, incluso si cambia la extensión |

La validación no se conforma con el `Content-Type`, que lo declara el cliente y se
puede falsear: se comprueba además la firma binaria del archivo. Un archivo de texto
renombrado a `.png` se rechaza con 400.

El nombre del archivo es el identificador del usuario, así que cada cuenta tiene como
máximo una foto y no se acumulan huérfanos.

Los archivos subidos están excluidos del control de versiones. La carpeta se conserva
en el repositorio mediante un `.gitkeep`.

---

## Cómo verificar

```bash
dotnet build                      # debe terminar con 0 advertencias y 0 errores
dotnet list package --vulnerable  # sin paquetes vulnerables
dotnet run --project Vita.Api     # el log no debe mostrar warnings ni excepciones
```

Comprobación funcional rápida, con el API levantado:

```bash
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5044/swagger/v1/swagger.json
```

Debe responder 200 y el documento debe incluir todos los endpoints del apartado
anterior.

---

## Cómo actualizar

### Traer cambios del repositorio

```bash
git pull
dotnet restore
dotnet run --project Vita.Api    # aplica las migraciones nuevas al arrancar
```

Las migraciones se aplican solas en Development, así que un `git pull` que incluya un
cambio de esquema no requiere pasos manuales.

### Actualizar paquetes NuGet

```bash
dotnet list package --outdated
dotnet add package <paquete> --version <version>
dotnet build
```

Revisa siempre las vulnerabilidades después:

```bash
dotnet list package --vulnerable --include-transitive
```

Actualiza los paquetes de uno en uno. Subir varios a la vez hace que un fallo de
compilación sea difícil de atribuir.

### Cambiar el modelo de datos

1. Modifica la entidad en `Entities/`.
2. Crea la migración:

```bash
dotnet ef migrations add <NombreDescriptivo> --project Vita.Api
```

3. Revisa el archivo generado antes de aplicarlo. Es el momento de detectar una
   columna que se borraría por error.
4. Aplícala:

```bash
dotnet ef database update --project Vita.Api
```

5. Actualiza el DTO, el servicio y la documentación del módulo.

Flujo completo y convenciones en
[`docs/03-base-datos/migraciones.md`](docs/03-base-datos/migraciones.md).

---

## Base de datos y migraciones

| Comando | Qué hace |
| --- | --- |
| `dotnet ef migrations add <Nombre> --project Vita.Api` | Crea una migración |
| `dotnet ef migrations list --project Vita.Api` | Lista las migraciones y su estado |
| `dotnet ef database update --project Vita.Api` | Aplica las pendientes |
| `dotnet ef migrations remove --project Vita.Api` | Elimina la última, si no está aplicada |

En Development, `Program.cs` llama a `Database.Migrate()` al arrancar, así que basta
con ejecutar el API.

Documentación del modelo en
[`docs/03-base-datos/modelo-identity.md`](docs/03-base-datos/modelo-identity.md).

---

## Cómo desplegar

```bash
dotnet publish Vita.Api -c Release -o ./publish
```

Antes de publicar:

| Requisito | Motivo |
| --- | --- |
| Definir la configuración por variables de entorno | El `.env` es solo para desarrollo |
| Usar una `Jwt:Key` distinta de la local | La clave de desarrollo no debe salir del equipo |
| `ASPNETCORE_ENVIRONMENT=Production` | Desactiva Swagger, el seed y la política CORS abierta |
| Aplicar las migraciones | En Production no se aplican automáticamente |
| Persistir `wwwroot/uploads` | Si no, las fotos se pierden en cada despliegue |
| Servir por HTTPS | La redirección a HTTPS solo se activa fuera de Development |

Sobre el último punto: en Development la redirección está desactivada a propósito,
porque el host local es solo HTTP y redirigir rompería el consumo desde el frontend.

En Production hay que revisar además la política CORS: la actual acepta cualquier
origen `localhost` y **solo se registra en Development**.

---

## Convenciones

- Commits con Conventional Commits: `feat(auth): ...`, `fix(courses): ...`
- Ramas: `feature/<ID>-descripcion`
- Integración a `main` solo por pull request
- Errores siempre en formato `{ "error": "...", "statusCode": N }`
- Un controlador no accede a la persistencia: pasa por un servicio
- Los mensajes de validación se escriben en español, listos para la interfaz

---

## Documentación

Índice completo en **[docs/README.md](docs/README.md)**.

| Sección | Contenido |
| --- | --- |
| [docs/00-convenciones/](docs/00-convenciones/) | Contrato de la API y patrón de errores |
| [docs/01-setup-local/](docs/01-setup-local/) | Configurar el entorno, los seeds y probar Swagger |
| [docs/02-modulos-api/](docs/02-modulos-api/) | Detalle de los diez módulos implementados |
| [docs/03-base-datos/](docs/03-base-datos/) | Modelo Identity, migraciones y script SQL |
| [docs/EJECUTAR-EL-BACKEND.md](docs/EJECUTAR-EL-BACKEND.md) | Guía única para levantar el backend |

---

## Solución de problemas

| Síntoma | Causa | Solución |
| --- | --- | --- |
| `Falta la contraseña de seed` | No están las claves `Seeds:DemoUsers:*` | Completarlas en `.env` o en User Secrets |
| `Address already in use` en 5044 | Otra instancia sigue viva | `pkill -f Vita.Api` y volver a arrancar |
| `Failed to bind to address http://127.0.0.1:5000` | Se arrancó con `--no-launch-profile` sin definir la URL | Definir `ASPNETCORE_URLS=http://127.0.0.1:5044` |
| No conecta con PostgreSQL | Cadena de conexión incorrecta o servicio parado | Verificar `ConnectionStrings__Default` y el estado del servicio |
| Aviso de puerto HTTPS al arrancar | Redirección a HTTPS sin puerto HTTPS configurado | Ya resuelto: la redirección solo se activa fuera de Development |
| Conflicto de propietarios en las tablas | Se ejecutó `DB/Script.sql` junto con las migraciones | Recrear la base y dejar que solo actúen las migraciones |
| 401 con un token recién obtenido | `Jwt:Key`, `Issuer` o `Audience` cambiaron tras emitirlo | Volver a iniciar sesión |
| La foto de perfil devuelve 404 | Falta la carpeta de subidas o los archivos estáticos | Comprobar `wwwroot/uploads/profiles` y `UseStaticFiles` |

---

## Equipo

| Rol | Responsabilidad |
| --- | --- |
| Líder | Coordinación, pull requests e integración |
| Backend 1 | Autenticación, Identity, JWT y middlewares |
| Backend 2 | Módulos de dominio |
| Base de datos | Esquema, seeds y alineación con Identity |
