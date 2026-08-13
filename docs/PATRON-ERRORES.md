> ⚠️ **Archivo legacy** — el contenido actualizado está en [`00-convenciones/patron-errores.md`](00-convenciones/patron-errores.md). Este archivo se conserva como referencia histórica.

# Patrón de errores del API

Todos los errores responden con este formato:
{ "error": "mensaje legible", "statusCode": 400 }

## Cómo usarlo en tus controllers
1. Hereda de `BaseApiController` (no de `ControllerBase`):
   public class CursosController : BaseApiController


Todos los errores responden con este formato:
{ "error": "mensaje legible", "statusCode": 400 }

## Cómo usarlo en tus controllers
1. Hereda de `BaseApiController` (no de `ControllerBase`):
   public class CursosController : BaseApiController

2. Para un error, usa el helper:
   return ApiError(404, "Curso no encontrado.");
   return ApiError(403, "No tienes permiso.");

## Ya resuelto globalmente (NO lo repitas)
- Validación 400: si el body no cumple las Data Annotations, sale solo.
- Excepciones 500: el middleware las convierte en { error, statusCode:500 } sin stack trace.

## Códigos
400 datos inválidos · 401 no autenticado · 403 sin permiso · 404 no encontrado · 409 conflicto

## NO hagas
- NO envelopes { success, data, errors[] }
- NO stack traces al cliente