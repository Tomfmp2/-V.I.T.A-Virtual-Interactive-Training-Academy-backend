# Módulo 10 — Configuración de perfil (Auth extendido)

> **Basado en rama `develop`** · **Fecha:** 14/08/2026

---

## Estado

✅ **Implementado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/auth/me` | Bearer · cualquier rol | Ver perfil del usuario autenticado |
| `PUT` | `/api/auth/me` | Bearer · cualquier rol | Actualizar datos personales |
| `POST` | `/api/auth/change-password` | Bearer · cualquier rol | Cambiar contraseña (self-service) |
| `POST` | `/api/auth/me/photo` | Bearer · cualquier rol | Subir foto de perfil (multipart) |

> El email **no** se modifica desde perfil (readonly en UI). La gestión de otros usuarios sigue en [`02-users.md`](02-users.md).

---

## Request / Response

### `GET /api/auth/me`

Header requerido: `Authorization: Bearer <token>`

**Response 200:**
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

| Campo | Tipo | Notas |
| --- | --- | --- |
| `fotoUrl` | `string?` | Ruta pública relativa; `null` si no hay foto |
| `telefono` | `string?` | Solo dígitos en BD |
| `codigoPais` | `string?` | Formato `+NN` |

---

### `PUT /api/auth/me`

**Request:**
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
| `telefono` | `string?` | Opcional; se normaliza a dígitos |
| `codigoPais` | `string?` | Opcional; formato `+NN` |

**Response 200:** mismo shape que `GET /api/auth/me`.

---

### `POST /api/auth/change-password`

**Request:**
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
| `nuevaContraseña` | `string` | Requerida, mínimo 8 caracteres |
| `confirmarContraseña` | `string` | Debe coincidir con `nuevaContraseña` |

**Response 200:**
```json
{
  "message": "Contraseña actualizada correctamente."
}
```

---

### `POST /api/auth/me/photo`

**Content-Type:** `multipart/form-data`  
**Campo:** `file`

| Regla | Valor |
| --- | --- |
| Tipos MIME | `image/jpeg`, `image/png`, `image/webp` |
| Tamaño máximo | 2 MB |

**Response 200:**
```json
{
  "fotoUrl": "/uploads/profiles/3fa85f64-5717-4562-b3fc-2c963f66afa6.webp"
}
```

Las imágenes se sirven en `http://localhost:5044/uploads/profiles/...` vía archivos estáticos.

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Validación de perfil o contraseña | Mensajes en español del DTO |
| `400` | Confirmación de contraseña no coincide | `"Las contraseñas no coinciden."` |
| `400` | Archivo inválido o muy grande | `"Formato no permitido. Usa JPG, PNG o WEBP."` |
| `401` | Sin token o contraseña actual incorrecta | `"No autorizado."` / `"La contraseña actual es incorrecta."` |
| `403` | Usuario inactivo | `"Usuario inactivo."` |

---

## Reglas de negocio

- Solo el usuario autenticado puede modificar **su propio** perfil.
- `telefono` se guarda sin espacios ni caracteres no numéricos.
- Al subir una nueva foto se reemplaza la anterior del mismo usuario.
- El logout sigue siendo stateless; cambiar contraseña no invalida el JWT actual.
- Usuario demo admin incluye teléfono `3001234567` y código `+57` en seeds de desarrollo.

---

## Cómo probar en Swagger

1. `POST /api/auth/login` con `admin@vita.local` → copiar token → **Authorize**.
2. `GET /api/auth/me` → verificar `telefono` y `codigoPais`.
3. `PUT /api/auth/me` → cambiar nombre o teléfono → `200`.
4. `POST /api/auth/change-password` → body con contraseñas → `200`.
5. `POST /api/auth/me/photo` → elegir archivo JPG → `200` con `fotoUrl`.
6. Abrir `http://localhost:5044` + `fotoUrl` en el navegador.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/AuthController.cs`
- Service: `Vita.Api/Services/AuthService.cs` / `IAuthService.cs`
- DTOs: `Vita.Api/Dtos/Auth/` (`MeResponse`, `UpdateProfileRequest`, `ChangePasswordRequest`, `UploadPhotoResponse`)
- Entidad: `Vita.Api/Entities/Usuario.cs` (`Telefono`, `CodigoPais`, `FotoUrl`)
- Archivos estáticos: `Vita.Api/wwwroot/uploads/profiles/`
