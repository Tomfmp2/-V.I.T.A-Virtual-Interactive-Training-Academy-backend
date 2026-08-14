# Cómo probar la API en Swagger

> **Basado en rama `develop`** · **Fecha:** 13/08/2026
> Guía para probar cualquier endpoint desde Swagger UI usando autenticación JWT Bearer.

---

## Requisitos previos

1. PostgreSQL local corriendo con la base `academia_cursos`.
2. `.env` completo con connection string, JWT key y passwords de seed.
   → Ver [`configurar-env.md`](configurar-env.md)
3. API levantada:

```bash
dotnet run --project Vita.Api --launch-profile http
```

---

## URL de Swagger

```
http://localhost:5044/swagger
```

---

## Flujo básico: login → Authorize → probar endpoint protegido

### 1. Obtener un token — `POST /api/auth/login`

En Swagger: despliega `Auth` → `POST /api/auth/login` → **Try it out**.

Body:
```json
{
  "email": "admin@vita.local",
  "password": "<tu-password-admin-del-env>"
}
```

Respuesta esperada: `200` con el campo `token`.

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEn": 604800,
  "usuario": {
    "id": "...",
    "nombre": "Admin",
    "apellido": "Demo",
    "email": "admin@vita.local",
    "rol": "Admin"
  }
}
```

**Copia el valor de `token`.**

---

### 2. Autorizar en Swagger

1. Clic en el botón **Authorize** (arriba a la derecha).
2. Pega el token copiado (solo el token; Swagger agrega `Bearer ` automáticamente).
3. Clic en **Authorize** → **Close**.

El candado se cierra: todos los requests siguientes incluirán el header `Authorization: Bearer <token>`.

---

### 3. Ver mi perfil — `GET /api/auth/me`

`Auth` → `GET /api/auth/me` → **Try it out** → **Execute**.

Respuesta esperada: `200` con tus datos.

```json
{
  "id": "...",
  "nombre": "Admin",
  "apellido": "Demo",
  "email": "admin@vita.local",
  "rol": "Admin",
  "activo": true
}
```

---

### 4. Probar un endpoint de otro módulo — ejemplo: categorías

`Categories` → `GET /api/categories` → **Try it out** → **Execute**.

Respuesta esperada: `200` con el listado de categorías.

> Con el token de Admin también puedes crear, editar y eliminar categorías.

---

### 5. Logout — `POST /api/auth/logout`

`Auth` → `POST /api/auth/logout` → **Execute**.

Respuesta: `200 { "message": "Sesion cerrada" }`.

> Logout es **stateless**: el servidor no invalida el token. En producción, el frontend borra el token localmente.

---

## Flujo por rol

| Rol | Qué puede hacer |
| --- | --- |
| **Admin** | Todos los módulos. Gestión de usuarios, roles y categorías |
| **Instructor** | Ver cursos, crear y gestionar sus propios cursos y lecciones |
| **Estudiante** | Ver cursos publicados, inscribirse, ver sus inscripciones |

Para probar cada rol: haz login con el email correspondiente y autoriza con su token.

---

## Errores comunes en Swagger

| Síntoma | Causa | Solución |
| --- | --- | --- |
| `401` en `/api/auth/me` | Candado abierto (sin token) | Hacer login y usar el botón **Authorize** |
| `401` en login | Credenciales incorrectas | Verificar email y password en `.env` |
| `409` en register | Email ya registrado | Usar otro email |
| `400` con detalles | Body no cumple validaciones | Revisar campos requeridos en Swagger |
| `403` al crear curso | Token es de Admin o Estudiante | Usar token de Instructor |

---

## Ver también

- [`configurar-env.md`](configurar-env.md)
- [`configurar-seeds.md`](configurar-seeds.md)
- [`../00-convenciones/contrato-api.md`](../00-convenciones/contrato-api.md) — tabla completa de endpoints
- [`../00-convenciones/patron-errores.md`](../00-convenciones/patron-errores.md) — formato de errores
