# Patrón de errores — VITA API

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

Todos los errores del API responden con **un único formato**:

```json
{
  "error": "Mensaje legible para el humano.",
  "statusCode": 404
}
```

---

## Códigos de estado usados

| Código | Significado | Cuándo ocurre |
| --- | --- | --- |
| `400` | Datos inválidos | Body no cumple validaciones (Data Annotations) o campo fuera de rango |
| `401` | No autenticado | No se envió token, token inválido o expirado |
| `403` | Sin permiso | Token válido pero el rol no tiene acceso, o el usuario no es dueño del recurso |
| `404` | No encontrado | El recurso solicitado no existe en la BD |
| `409` | Conflicto | Email duplicado, nombre de categoría ya existe, inscripción duplicada, etc. |
| `500` | Error interno | Excepción no controlada (el middleware la captura; no se expone stack trace) |

---

## Cómo usarlo en controllers

### 1. Hereda de `BaseApiController`

```csharp
public class MiController : BaseApiController
```

> **No** uses `ControllerBase` directamente si necesitas retornar errores con este formato.

### 2. Usa el helper `ApiError`

```csharp
return ApiError(404, "Curso no encontrado.");
return ApiError(403, "No tienes permiso para modificar este curso.");
return ApiError(409, "Ya existe una categoría con ese nombre.");
```

---

## Lo que ya está resuelto globalmente (no repetir)

| Caso | Quién lo resuelve |
| --- | --- |
| Validación 400 (Data Annotations) | `ConfigureApiBehaviorOptions` en `Program.cs` — sale automático |
| Excepciones 500 | `ErrorHandlingMiddleware` — convierte a `{ error, statusCode: 500 }` sin stack trace |

---

## Qué NO hacer

- ❌ Envelopes `{ success: true, data: {}, errors: [] }` — VITA no los usa.
- ❌ Devolver stack traces al cliente.
- ❌ Retornar errores con código 200.
- ❌ Usar `ControllerBase` en vez de `BaseApiController` cuando necesites `ApiError`.

---

## Ejemplo de flujo de error en Auth

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "no@existe.com", "password": "wrongpass" }
```

```json
HTTP/1.1 401 Unauthorized
{
  "error": "Credenciales inválidas.",
  "statusCode": 401
}
```

---

## Fuente en código

- `Vita.Api/Controllers/BaseApiController.cs` — método `ApiError`
- `Vita.Api/Middleware/ErrorHandlingMiddleware.cs` — manejo global de excepciones
- `Vita.Api/Program.cs` — configuración de `InvalidModelStateResponseFactory`
