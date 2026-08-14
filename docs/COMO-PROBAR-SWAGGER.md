>  **Archivo legacy** — el contenido actualizado está en [`01-setup-local/probar-swagger.md`](01-setup-local/probar-swagger.md). Este archivo se conserva como referencia histórica.

# Cómo probar la API en Swagger (con Bearer)

Guía para probar los endpoints de autenticación desde la UI de Swagger, usando el botón **Authorize** con token JWT.

## URL local

```
http://localhost:5044/swagger
```

## Requisitos previos

1. PostgreSQL local corriendo con la base de datos `academia_cursos`.
2. Secretos de seed configurados (ver [`Configurar-seeds-locales.md`](./Configurar-seeds-locales.md)).
3. Levantar la API:
   ```bash
   dotnet run --project Vita.Api
   ```

## Flujo de prueba: register → login → Authorize → me → logout

### 1. Registrarse — `POST /api/auth/register`
"Try it out" → body:
```json
{
  "nombre": "Ana",
  "apellido": "Torres",
  "email": "ana@correo.com",
  "password": "Secreta123"
}
```
Respuesta esperada: **201** con el usuario creado (sin password).

### 2. Login — `POST /api/auth/login`
Body:
```json
{
  "email": "ana@correo.com",
  "password": "Secreta123"
}
```
Respuesta esperada: **200** con un `token`. **Copia el valor de `token`.**

### 3. Autorizar — botón **Authorize** (arriba a la derecha)
1. Clic en **Authorize**.
2. Pega el `token` (solo el token; Swagger le agrega `Bearer ` automáticamente).
3. Clic en **Authorize** → **Close**.

### 4. Ver mi perfil — `GET /api/auth/me`
"Try it out" → Execute.
Respuesta esperada: **200** con tus datos (`id`, `nombre`, `email`, `rol`, `activo`).
> Funciona porque el candado está cerrado (el token viaja en el header `Authorization`).

### 5. Logout — `POST /api/auth/logout`
Execute → **200** con `{ "message": "Sesión cerrada" }`.
> Logout es *stateless*: el servidor no invalida el token. En el frontend, aquí se borra el token guardado.

## Notas

- Sin autorizar (candado abierto), `me` y `logout` devuelven **401**.
- Con credenciales incorrectas, `login` devuelve **401**; con email duplicado, `register` devuelve **409**; con datos inválidos, **400**.
- Todos los errores siguen el formato `{ "error": "...", "statusCode": ... }`.
- La vigencia del token depende de `Jwt:ExpireSeconds` en la configuración.
