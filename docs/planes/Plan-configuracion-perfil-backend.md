# Plan Backend — Configuración de perfil (endpoints y conexiones faltantes)

| | |
| --- | --- |
| **Repo** | `V.I.T.A-Virtual-Interactive-Training-Academy-backend` |
| **Base** | Rama `develop` |
| **Fecha** | 14/08/2026 |
| **Relacionados** | [`01-auth.md`](../02-modulos-api/01-auth.md) · [`02-users.md`](../02-modulos-api/02-users.md) · [`contrato-api.md`](../00-convenciones/contrato-api.md) · FE `src/pages/PerfilPage.tsx` |
| **Objetivo** | Hacer funcional el apartado **Configuración de perfil** (UI ya existe en el frontend) |
| **Principio** | Extender el módulo Auth existente · no duplicar lógica Admin · cambios aditivos al contrato |

---

## 0. Veredicto (leer antes de codear)

| Pregunta | Respuesta |
| --- | --- |
| ¿La pantalla de perfil existe en el FE? | **Sí** — `PerfilPage.tsx`, ruta menú `settings` en `HomePage`. |
| ¿Está conectada a la API? | **No** — `handleSubmit` solo muestra *"Cambios guardados en esta sesión."* |
| ¿Qué endpoints BE existen hoy para perfil propio? | Solo **`GET /api/auth/me`** (parcial). |
| ¿Sirve `PUT /api/users/{id}` para el usuario logueado? | **No** — requiere rol **Admin** y opera sobre cualquier usuario. |
| ¿Hay campos en BD para teléfono? | **No** — faltan en entidad `Usuario`. |
| ¿Hay campo para foto? | **Sí** — `FotoUrl` en `AspNetUsers`, pero sin endpoint de subida. |
| ¿Hay cambio de contraseña self-service? | **No** — Identity lo soporta vía `UserManager`, falta exponerlo. |

**Matriz pantalla ↔ backend**

| Sección UI (`PerfilPage`) | Campo / acción | Estado BE | Acción requerida |
| --- | --- | --- | --- |
| Foto de perfil | Subir imagen + preview | `FotoUrl` existe · sin upload | `POST /api/auth/me/photo` + archivos estáticos |
| Detalles personales | Nombre, apellido | Parcial (`GET /me`) | `PUT /api/auth/me` |
| Detalles personales | Correo (readonly) | ✅ en `GET /me` | Ninguna (MVP: no permitir cambio de email) |
| Detalles personales | Teléfono + código país | ❌ no en modelo | Migración + `PUT /api/auth/me` + ampliar `GET /me` |
| Cambiar contraseña | Actual / nueva / confirmar | ❌ | `POST /api/auth/change-password` |
| Guardar cambios | Submit del formulario | ❌ sin endpoint | Cablear FE a los endpoints anteriores |

---

## 1. Principios no negociables

1. **Misma capa:** `Controllers → Services → UserManager / ApplicationDbContext` (reutilizar `AuthService` o extraer `ProfileService` solo si el archivo crece demasiado).
2. **Misma base HTTP:** heredar `BaseApiController`, errores `{ "error": "…" }`, validación con DataAnnotations + `[ErrorMessage]` en español.
3. **Auth:** `[Authorize]` · el usuario solo puede modificar **su propio** perfil (userId desde `ClaimTypes.NameIdentifier` / `sub`).
4. **Contrato aditivo:** ampliar `MeResponse` con campos nuevos (`fotoUrl`, `telefono`, `codigoPais`) **sin romper** consumidores actuales.
5. **Email fuera de alcance MVP:** la UI ya lo muestra readonly; no abrir cambio de email hasta tener flujo de verificación.
6. **Rol Admin no necesario:** perfil propio ≠ gestión de usuarios (`/api/users`).
7. **Docs en la misma PR:** este plan → ficha [`10-profile-settings.md`](../02-modulos-api/10-profile-settings.md) · actualizar `contrato-api.md` y `docs/README.md`.
8. **Commits:** Conventional Commits, cuerpo en español, sin Co-authored-by de agente.

```text
Patrón a copiar:
  Dtos/Auth/UpdateProfileRequest.cs, ChangePasswordRequest.cs
  Services/AuthService.cs (o IProfileService + ProfileService)
  Controllers/AuthController.cs (rutas bajo /api/auth)
  Entities/Usuario.cs + migración EF
  Program.cs → UseStaticFiles si aplica
```

---

## 2. Alcance

### Incluye (MVP perfil)

| ID | Capacidad | Prioridad |
| --- | --- | --- |
| BE-PR1 | Migración: `Telefono`, `CodigoPais` en `Usuario` | P0 |
| BE-PR2 | Ampliar `GET /api/auth/me` (`fotoUrl`, `telefono`, `codigoPais`) | P0 |
| BE-PR3 | `PUT /api/auth/me` — actualizar datos personales | P0 |
| BE-PR4 | `POST /api/auth/change-password` — cambio de contraseña | P0 |
| BE-PR5 | `POST /api/auth/me/photo` — subir foto de perfil | P1 |
| BE-PR6 | Servir archivos estáticos (`/uploads/...`) | P1 |
| FE-PR1 | Cablear `PerfilPage` + `authApi` + `AuthContext` | P0 (FE, tras BE-PR2–4) |

### Excluye (explícito)

| Ítem | Motivo |
| --- | --- |
| Cambio de email desde perfil | UI readonly; requiere verificación / riesgo de cuenta |
| `PUT /api/users/{id}` para self-service | Duplicaría reglas Admin; mejor ruta dedicada bajo `/auth` |
| Biografía editable en UI | Campo existe en entidad pero no hay sección en la pantalla |
| CDN / S3 / Azure Blob | MVP local con `wwwroot/uploads` |
| Recorte / redimensionado de imagen | Opcional post-MVP |
| Invalidar JWT tras cambio de contraseña | Logout stateless actual; el FE puede forzar re-login |

---

## 3. Modelo de datos

### 3.1 Estado actual (`Usuario`)

```csharp
// Vita.Api/Entities/Usuario.cs
public string Nombre { get; set; }
public string Apellido { get; set; }
public string? FotoUrl { get; set; }
public string? Biografia { get; set; }
public bool Activo { get; set; }
public DateTime CreatedAt { get; set; }
// Hereda Email, PasswordHash, etc. de IdentityUser
```

### 3.2 Campos a agregar

| Campo | Tipo | MaxLength | Nullable | Ejemplo |
| --- | --- | --- | --- | --- |
| `Telefono` | `string` | 20 | Sí | `"3001234567"` (sin espacios en BD) |
| `CodigoPais` | `string` | 10 | Sí | `"+57"` |

**Migración sugerida:** `AddTelefonoYCodigoPaisToUsuario`

**Seeds:** opcionalmente poblar teléfono demo del admin (`3001234567`, `+57`) en `DbSeeder` para validar la UI.

### 3.3 Foto de perfil

- Persistir en `Usuario.FotoUrl` la **URL pública** relativa, p. ej. `/uploads/profiles/{userId}.jpg`.
- Al reemplazar foto, borrar archivo anterior si existía (mismo userId, distinta extensión).

---

## 4. Contratos API a implementar

### 4.1 Ampliar `GET /api/auth/me` (BE-PR2)

**Auth:** Bearer · cualquier rol activo  
**Cambio:** respuesta aditiva (compatible hacia atrás).

**Response 200 (ampliado):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombre": "Admin",
  "apellido": "Prueba",
  "email": "admin@vita.local",
  "rol": "Admin",
  "activo": true,
  "fotoUrl": "/uploads/profiles/abc123.jpg",
  "telefono": "3001234567",
  "codigoPais": "+57"
}
```

| Campo nuevo | Tipo | Notas |
| --- | --- | --- |
| `fotoUrl` | `string?` | `null` si no hay foto |
| `telefono` | `string?` | Solo dígitos recomendado en BE |
| `codigoPais` | `string?` | Formato `+NN` |

**Archivos:** `MeResponse.cs`, `AuthService.GetMeAsync`, opcionalmente `UsuarioLoginDto` en login para coherencia del avatar en header.

---

### 4.2 `PUT /api/auth/me` (BE-PR3)

**Auth:** Bearer · cualquier rol  
**Body:**
```json
{
  "nombre": "Admin",
  "apellido": "Prueba",
  "telefono": "300 123 4567",
  "codigoPais": "+57"
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `nombre` | `string` | Requerido, 3–100 caracteres |
| `apellido` | `string` | Requerido, 3–100 caracteres |
| `telefono` | `string?` | Opcional; si viene, normalizar (quitar espacios) y validar 6–15 dígitos |
| `codigoPais` | `string?` | Opcional; debe empezar por `+` y 1–4 dígitos |

**Response 200:** mismo shape que `GET /api/auth/me` (perfil actualizado).

**Errores:**

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Validación | Mensajes en español del DTO |
| `401` | Sin token / usuario inexistente | `"No autorizado."` |
| `403` | Usuario inactivo | `"Usuario inactivo."` |

**Reglas:**
- No modificar `email`, `rol` ni `activo`.
- Trim en nombre/apellido antes de guardar.

---

### 4.3 `POST /api/auth/change-password` (BE-PR4)

**Auth:** Bearer · cualquier rol  
**Body:**
```json
{
  "contraseñaActual": "Admin123!",
  "nuevaContraseña": "NuevaAdmin123!",
  "confirmarContraseña": "NuevaAdmin123!"
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `contraseñaActual` | `string` | Requerida |
| `nuevaContraseña` | `string` | Requerida, mínimo 8 caracteres (misma política que registro) |
| `confirmarContraseña` | `string` | Debe coincidir con `nuevaContraseña` |

**Response 200:**
```json
{
  "message": "Contraseña actualizada correctamente."
}
```

**Errores:**

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Confirmación no coincide | `"Las contraseñas no coinciden."` |
| `400` | Política Identity | Traducidos vía `SpanishIdentityErrorDescriber` |
| `401` | Contraseña actual incorrecta | `"La contraseña actual es incorrecta."` |
| `403` | Usuario inactivo | `"Usuario inactivo."` |

**Implementación sugerida:**
```csharp
await _userManager.ChangePasswordAsync(usuario, request.ContraseñaActual, request.NuevaContraseña);
```

**Nota FE:** si el usuario no rellena ningún campo de contraseña, no llamar este endpoint (solo `PUT /me`).

---

### 4.4 `POST /api/auth/me/photo` (BE-PR5)

**Auth:** Bearer · cualquier rol  
**Content-Type:** `multipart/form-data`  
**Campo:** `file` (único archivo)

**Validaciones:**

| Regla | Valor |
| --- | --- |
| Tipos MIME permitidos | `image/jpeg`, `image/png`, `image/webp` |
| Tamaño máximo | 2 MB |
| Dimensiones | Sin validar en MVP |

**Response 200:**
```json
{
  "fotoUrl": "/uploads/profiles/3fa85f64-5717-4562-b3fc-2c963f66afa6.webp"
}
```

**Errores:**

| Código | Cuándo | Mensaje |
| --- | --- | --- |
| `400` | Sin archivo / tipo inválido / muy grande | Mensaje específico en español |
| `401` | Sin token | `"No autorizado."` |

**Almacenamiento (BE-PR6):**
- Carpeta: `wwwroot/uploads/profiles/`
- Nombre: `{userId}{ext}` (sobrescribe anterior)
- `Program.cs`: `app.UseStaticFiles()` (verificar que esté activo)
- URL base en FE: `import.meta.env.VITE_API_URL` sin `/api` + `fotoUrl`

---

## 5. Dónde tocar en código (backend)

| Archivo | Cambio |
| --- | --- |
| `Entities/Usuario.cs` | Propiedades `Telefono`, `CodigoPais` |
| `Migrations/YYYYMMDDHHMMSS_AddTelefonoYCodigoPaisToUsuario.cs` | Nueva migración |
| `Dtos/Auth/MeResponse.cs` | Campos `FotoUrl`, `Telefono`, `CodigoPais` |
| `Dtos/Auth/UpdateProfileRequest.cs` | **Nuevo** |
| `Dtos/Auth/ChangePasswordRequest.cs` | **Nuevo** |
| `Services/IAuthService.cs` | `UpdateProfileAsync`, `ChangePasswordAsync`, `UploadPhotoAsync` |
| `Services/AuthService.cs` | Implementación |
| `Controllers/AuthController.cs` | `PUT me`, `POST change-password`, `POST me/photo` |
| `Program.cs` | `UseStaticFiles`, límite `FormOptions` / `MultipartBodyLengthLimit` si hace falta |
| `Data/DbSeeder.cs` | (Opcional) teléfono demo admin |
| `docs/02-modulos-api/10-profile-settings.md` | Ficha del módulo ✅ tras implementar |
| `docs/00-convenciones/contrato-api.md` | Tabla Auth ampliada |
| `docs/README.md` | Entrada módulo 10 |

### Outcomes sugeridos (estilo existente)

```csharp
enum ProfileOutcome { Success, NotFound, Inactive, ValidationError, WrongPassword, FileInvalid }
```

El controller hace `switch` → `ApiError` / `Ok` igual que `AuthController.Register`.

---

## 6. Plan de conexión frontend (referencia para la PR FE)

> El cableado vive en el repo frontend; se documenta aquí para cerrar el flujo completo.

### 6.1 Tipos (`src/types/auth.ts`)

Ampliar `User`:
```typescript
export interface User {
  id: string;
  nombre: string;
  apellido?: string;
  email: string;
  rol: string;
  activo?: boolean;
  fotoUrl?: string | null;
  telefono?: string | null;
  codigoPais?: string | null;
}
```

### 6.2 API client (`src/api/authApi.ts`)

| Función | Método | Ruta |
| --- | --- | --- |
| `getMeApi` | GET | `/auth/me` (ya existe — tipar respuesta ampliada) |
| `updateProfileApi` | PUT | `/auth/me` |
| `changePasswordApi` | POST | `/auth/change-password` |
| `uploadProfilePhotoApi` | POST multipart | `/auth/me/photo` |

### 6.3 `PerfilPage.tsx`

| Acción actual | Cambio |
| --- | --- |
| Init desde `useAuth().user` | Al montar, opcional `getMeApi()` para datos frescos (`telefono`, `fotoUrl`) |
| `handleSubmit` mock | 1) Si hay foto pendiente → `uploadProfilePhotoApi` 2) `updateProfileApi` 3) Si hay passwords → `changePasswordApi` |
| Mensaje éxito local | Reemplazar por feedback real + `getApiErrorMessage` en errores |
| Avatar | Mostrar `fotoUrl` del API (URL absoluta) si no hay preview local |
| Post-guardado | Actualizar `AuthContext` (`setUser` / refetch `getMeApi`) |

### 6.4 Header / sidebar avatar

Si el header usa inicial del nombre, extender para mostrar `fotoUrl` cuando exista (misma URL base estática).

---

## 7. Orden de implementación recomendado

```text
Fase 1 — Datos y lectura (BE)
  └─ Migración Telefono + CodigoPais
  └─ Ampliar MeResponse + GetMeAsync
  └─ Probar GET /me en Swagger

Fase 2 — Escritura perfil (BE)
  └─ UpdateProfileRequest + PUT /api/auth/me
  └─ ChangePasswordRequest + POST /api/auth/change-password
  └─ Probar con admin@vita.local

Fase 3 — Foto (BE)
  └─ wwwroot/uploads + POST /api/auth/me/photo
  └─ Verificar URL en navegador

Fase 4 — Frontend
  └─ types + authApi
  └─ PerfilPage cableado
  └─ AuthContext refresh
  └─ Probar flujo completo los 3 roles

Fase 5 — Documentación
  └─ 10-profile-settings.md (estado ✅)
  └─ contrato-api.md + README
```

---

## 8. Checklist de pruebas

### Backend (Swagger)

- [ ] `GET /api/auth/me` devuelve `apellido`, `fotoUrl`, `telefono`, `codigoPais`
- [ ] `PUT /api/auth/me` actualiza nombre/apellido/teléfono y persiste tras nuevo GET
- [ ] `PUT /api/auth/me` con teléfono inválido → `400` en español
- [ ] `POST /api/auth/change-password` con contraseña actual incorrecta → `401`
- [ ] `POST /api/auth/change-password` OK → login con nueva contraseña funciona
- [ ] `POST /api/auth/me/photo` con JPG < 2MB → `fotoUrl` accesible por HTTP
- [ ] `POST /api/auth/me/photo` con PDF → `400`
- [ ] Usuario inactivo → `403` en endpoints de escritura

### Frontend (manual)

- [ ] Abrir Configuración como Admin / Instructor / Estudiante
- [ ] Campos precargados desde API
- [ ] Guardar solo datos personales sin tocar contraseña
- [ ] Cambiar contraseña con validación de confirmación
- [ ] Subir foto y ver avatar actualizado en perfil y header
- [ ] Correo sigue readonly
- [ ] Errores API visibles en UI (no solo consola)

---

## 9. Estimación de esfuerzo

| Bloque | Esfuerzo |
| --- | --- |
| BE Fase 1–2 (migración + CRUD perfil + password) | ~4–6 h |
| BE Fase 3 (upload + estáticos) | ~2–3 h |
| FE cableado | ~3–4 h |
| Docs + pruebas | ~1–2 h |
| **Total** | **~10–15 h** |

---

## 10. Riesgos y mitigaciones

| Riesgo | Mitigación |
| --- | --- |
| Romper `GET /me` en consumidores actuales | Solo campos nuevos opcionales |
| Path traversal en upload | Guardar solo como `{userId}{ext}` validado |
| Disco lleno en dev | Límite 2 MB + borrar foto anterior |
| FE no construye URL de imagen | Documentar `VITE_API_BASE` sin sufijo `/api` |
| Duplicar validación Admin vs perfil | Perfil no toca email; Admin sigue en `/api/users` |

---

## 11. Definition of Done

- [ ] Endpoints BE-PR2 a BE-PR5 implementados y en Swagger
- [ ] Migración aplicada en local (`dotnet ef database update`)
- [ ] Mensajes de error en español
- [ ] `PerfilPage` persiste cambios contra API real (PR FE separada o mismo sprint)
- [ ] Ficha `10-profile-settings.md` creada con estado ✅
- [ ] `contrato-api.md` actualizado
- [ ] Checklist §8 pasado con cuenta demo `admin@vita.local`

---

## Fuente en código (estado actual)

| Área | Ubicación |
| --- | --- |
| Controller Auth | `Vita.Api/Controllers/AuthController.cs` |
| Service Auth | `Vita.Api/Services/AuthService.cs` |
| Entidad usuario | `Vita.Api/Entities/Usuario.cs` |
| Admin update (referencia) | `Vita.Api/Services/AdminUserService.cs` |
| UI perfil (FE) | `V.I.T.A-Virtual-Interactive-Training-Academy/src/pages/PerfilPage.tsx` |
| Client HTTP auth (FE) | `V.I.T.A-Virtual-Interactive-Training-Academy/src/api/authApi.ts` |
