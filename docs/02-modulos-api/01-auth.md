# Módulo 1 — Auth

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

✅ **Implementado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Público | Registrar nuevo usuario (rol `Estudiante` por defecto) |
| `POST` | `/api/auth/login` | Público | Autenticar y recibir JWT |
| `GET` | `/api/auth/me` | Bearer · cualquier rol | Ver perfil del usuario autenticado |
| `POST` | `/api/auth/logout` | Bearer · cualquier rol | Cierre de sesión stateless |

---

## Request / Response

### `POST /api/auth/register`

**Request:**
```json
{
  "nombre": "Ana",
  "apellido": "Torres",
  "email": "ana@correo.com",
  "password": "Secreta123"
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `nombre` | `string` | Requerido, 3–100 caracteres |
| `apellido` | `string` | Requerido, 3–255 caracteres |
| `email` | `string` | Requerido, formato email válido |
| `password` | `string` | Requerido, mínimo 8 caracteres |

**Response 201:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombre": "Ana",
  "apellido": "Torres",
  "email": "ana@correo.com",
  "rol": "Estudiante",
  "activo": true
}
```

---

### `POST /api/auth/login`

**Request:**
```json
{
  "email": "ana@correo.com",
  "password": "Secreta123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEn": 604800,
  "usuario": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombre": "Ana",
    "apellido": "Torres",
    "email": "ana@correo.com",
    "rol": "Estudiante"
  }
}
```

---

### `GET /api/auth/me`

Header requerido: `Authorization: Bearer <token>`

**Response 200:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombre": "Ana",
  "apellido": "Torres",
  "email": "ana@correo.com",
  "rol": "Estudiante",
  "activo": true
}
```

---

### `POST /api/auth/logout`

Header requerido: `Authorization: Bearer <token>`

**Response 200:**
```json
{
  "message": "Sesion cerrada"
}
```

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Body inválido (campos faltantes o formato incorrecto) | `"El campo Email no es una dirección de correo electrónico válida."` |
| `401` | Credenciales incorrectas | `"Credenciales inválidas."` |
| `403` | Usuario inactivo intenta login | `"Usuario inactivo."` |
| `409` | Email ya registrado | `"El email ya está registrado."` |

---

## Reglas de negocio

- El registro asigna automáticamente el rol `Estudiante`. No se puede elegir rol en `register`.
- El login emite un JWT con los claims: `sub` (userId), `email`, `role`.
- El userId del token mapea a `ClaimTypes.NameIdentifier` (claim `sub`).
- **Logout es stateless:** el servidor no invalida el token. El frontend debe borrarlo localmente.
- La duración del token se configura con `Jwt__ExpireSeconds` en `.env` (por defecto 604800 = 7 días).
- Contraseña: mínimo 8 caracteres. No requiere mayúsculas, dígitos ni caracteres especiales (configurado en `Program.cs`).

---

## Cómo probar en Swagger

1. Ir a `http://localhost:5044/swagger`.
2. `POST /api/auth/register` → **Try it out** → completar body → **Execute** → `201`.
3. `POST /api/auth/login` → **Try it out** → mismas credenciales → `200` → copiar `token`.
4. Clic en **Authorize 🔒** → pegar token → **Authorize** → **Close**.
5. `GET /api/auth/me` → **Try it out** → **Execute** → `200` con perfil.
6. `POST /api/auth/logout` → **Execute** → `200`.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/AuthController.cs`
- Service: `Vita.Api/Services/AuthService.cs` / `IAuthService.cs`
- DTOs: `Vita.Api/Dtos/Auth/` (`RegisterRequest`, `LoginRequest`, `LoginResponse`, `MeResponse`, `RegisterResponse`)
