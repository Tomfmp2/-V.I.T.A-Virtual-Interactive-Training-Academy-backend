# Módulo 2 — Users

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

 **Implementado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/users` | Bearer · Admin | Listar todos los usuarios |
| `GET` | `/api/users/{id}` | Bearer · Admin | Ver usuario por ID |
| `POST` | `/api/users` | Bearer · Admin | Crear usuario con rol específico |
| `PUT` | `/api/users/{id}` | Bearer · Admin | Actualizar nombre, apellido y email |
| `PATCH` | `/api/users/{id}/status` | Bearer · Admin | Activar / desactivar usuario (borrado lógico) |
| `PATCH` | `/api/users/{id}/role` | Bearer · Admin | Cambiar rol de un usuario |

> Todos los endpoints requieren rol `Admin`. Un 403 se retorna si el token no tiene ese rol.

---

## Request / Response

### `GET /api/users`

**Response 200:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombre": "Admin",
    "apellido": "Demo",
    "email": "admin@vita.local",
    "rol": "Admin",
    "activo": true
  }
]
```

---

### `POST /api/users`

**Request:**
```json
{
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@correo.com",
  "password": "Password123",
  "rol": "Instructor"
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `nombre` | `string` | Requerido, 3–100 caracteres |
| `apellido` | `string` | Requerido, 3–100 caracteres |
| `email` | `string` | Requerido, formato email |
| `password` | `string` | Requerido, mínimo 8 caracteres |
| `rol` | `string` | Requerido, máx 50 chars. Valores: `Admin`, `Instructor`, `Estudiante` |

**Response 201:**
```json
{
  "id": "...",
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@correo.com",
  "rol": "Instructor",
  "activo": true
}
```

---

### `PUT /api/users/{id}`

**Request:**
```json
{
  "nombre": "Juan Carlos",
  "apellido": "Pérez López",
  "email": "juancarlos@correo.com"
}
```

**Response 200:** `UserResponse` actualizado.

---

### `PATCH /api/users/{id}/status`

**Request:**
```json
{
  "activo": false
}
```

**Response 200:** `UserResponse` con `activo` actualizado.

---

### `PATCH /api/users/{id}/role`

**Request:**
```json
{
  "rol": "Admin"
}
```

**Response 200:** `UserResponse` con el nuevo rol.

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Body inválido o rol fuera de los permitidos | `"Rol inválido. Usa Admin, Instructor o Estudiante."` |
| `404` | Usuario no encontrado | `"Usuario no encontrado."` |
| `409` | Email ya registrado | `"El email ya está registrado."` |

---

## Reglas de negocio

- Solo el rol `Admin` puede acceder a este módulo.
- El campo `activo: false` es borrado lógico: el usuario no se elimina de la BD, solo se marca como inactivo.
- Los roles válidos son exactamente: `Admin`, `Instructor`, `Estudiante` (sensible a mayúsculas).
- El endpoint `POST /api/users` es para que el Admin cree usuarios con roles específicos, no para auto-registro (ver `POST /api/auth/register`).

---

## Cómo probar en Swagger

1. Hacer login con `admin@vita.local` y autorizar en Swagger.
2. `GET /api/users` → **Execute** → ver lista de usuarios.
3. `POST /api/users` → body con nombre, email, password y rol → `201`.
4. `PATCH /api/users/{id}/status` → body `{ "activo": false }` → `200`.
5. `PATCH /api/users/{id}/role` → body `{ "rol": "Instructor" }` → `200`.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/UsersController.cs`
- Service: `Vita.Api/Services/AdminUserService.cs` / `IAdminUserService.cs`
- DTOs: `Vita.Api/Dtos/Users/` (`CreateUserRequest`, `UpdateUserRequest`, `UpdateUserStatusRequest`, `UpdateUserRoleRequest`, `UserResponse`)
