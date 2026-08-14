# Módulo 8 — Reports

> **Basado en rama `feature/reports-api`** · **Fecha:** 13/08/2026

---

## Estado

 **Implementado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/reports/courses-by-instructor` | Bearer · Admin | Cantidad de cursos por instructor |
| `GET` | `/api/reports/students-by-course` | Bearer · Admin / Instructor | Estudiantes activos inscritos por curso |
| `GET` | `/api/reports/top-courses` | Bearer · Admin | Ranking de cursos con más inscritos |

> Son endpoints de **solo lectura**. No crean tablas nuevas: agregan sobre `cursos` e `inscripciones`.

---

## Request / Response

### `GET /api/reports/courses-by-instructor`

**Query (opcional):** `?instructorId={string}` — filtra un instructor (Id de Identity).

**Response 200:**
```json
[
  {
    "instructorId": "a1b2c3d4-…",
    "instructor": "Instructor Demo",
    "totalCursos": 2
  }
]
```

| Campo | Tipo | Notas |
| --- | --- | --- |
| `instructorId` | `string` | Id Identity (`AspNetUsers.Id`), no `int` |
| `instructor` | `string` | `Nombre` + `Apellido` |
| `totalCursos` | `int` | Borrador + publicado |

Solo aparecen instructores con al menos un curso (salvo filtro que puede devolver `[]`).

---

### `GET /api/reports/students-by-course`

**Response 200:**
```json
[
  {
    "cursoId": 10,
    "titulo": "Introducción a C#",
    "totalEstudiantes": 34
  }
]
```

| Rol | Alcance |
| --- | --- |
| Admin | Todos los cursos |
| Instructor | Solo cursos donde `IdInstructor` = usuario del token |

`totalEstudiantes` cuenta solo inscripciones con estado **`Activa`**. Los cursos sin inscritos aparecen con `0`.

---

### `GET /api/reports/top-courses`

**Query (opcional):** `?limit=10` — default `10`, rango permitido **1–50**.

**Response 200:**
```json
[
  {
    "cursoId": 10,
    "titulo": "Introducción a C#",
    "instructor": "Instructor Demo",
    "totalInscritos": 34
  }
]
```

Orden: `totalInscritos` DESC, luego `cursoId` ASC.

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | `limit` fuera de 1–50 | El parámetro limit debe estar entre 1 y 50. |
| `401` | Sin token / token inválido | (middleware JWT) |
| `403` | Rol no autorizado | (Authorize) |

Formato de error: `{ "error": "...", "statusCode": N }`.

---

## Decisiones de diseño

- Rutas canónicas alineadas al contrato histórico del equipo (`students-by-course`, `top-courses`).
- Sin Repository dedicado: mismo patrón que Categorías / Inscripciones (`DbContext` en el service).
- Instructor acotado en servicio para `students-by-course`, no con endpoints separados.
- Conteos de inscripción solo **Activa** (seed: `Activa` / `Cancelada`).
