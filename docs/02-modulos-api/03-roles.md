# Módulo 3 — Roles

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

✅ **Implementado** (parcial — solo lectura)

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/roles` | Bearer · Admin | Listar todos los roles disponibles |

---

## Request / Response

### `GET /api/roles`

No requiere body.

**Response 200:**
```json
[
  { "id": "a1b2c3d4-...", "name": "Admin" },
  { "id": "e5f6g7h8-...", "name": "Instructor" },
  { "id": "i9j0k1l2-...", "name": "Estudiante" }
]
```

> El shape de la respuesta viene del `RoleDto` interno (`Id`, `Name`). El frontend debe usar el campo `name` para mostrar o enviar roles a otros endpoints.

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `401` | Token ausente o inválido | (respuesta estándar de JWT) |
| `403` | Token válido pero no es Admin | (respuesta estándar de Authorization) |

---

## Reglas de negocio

- Los roles son gestionados por ASP.NET Core Identity.
- Los tres roles existentes (`Admin`, `Instructor`, `Estudiante`) se crean en el seed al iniciar el API en Development.
- Este endpoint es de **solo lectura**; crear o eliminar roles no está expuesto en la API.
- Los valores válidos de rol para otros módulos (crear usuario, cambiar rol) deben coincidir exactamente con los devueltos por este endpoint.

---

## Cómo probar en Swagger

1. Hacer login con `admin@vita.local` y autorizar en Swagger.
2. `GET /api/roles` → **Try it out** → **Execute** → `200` con los tres roles.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/RolesController.cs`
- Service: `Vita.Api/Services/RoleService.cs` / `IRoleService.cs`
- DTO: `Vita.Api/Dtos/Roles/RoleDto.cs` (`Id`, `Name`)
- Seed: `Vita.Api/Data/DbSeeder.cs` (`SeedRolesAsync`)
