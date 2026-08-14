#!/usr/bin/env python3
"""Seed demo VITA vía API local (≥10 cursos completos + inscripciones + reportes).

Uso:
  python3 scripts/seed_demo_via_api.py
  API_BASE=http://localhost:5044/api python3 scripts/seed_demo_via_api.py

Cuentas base (Trello):
  admin@vita.local / Admin123!
  instructor@vita.local / Instructor123!
  estudiante@vita.local / Estudiante123!
"""

from __future__ import annotations

import json
import os
import sys
import urllib.error
import urllib.request
from typing import Any

API_BASE = os.environ.get("API_BASE", "http://localhost:5044/api").rstrip("/")

ADMIN = ("admin@vita.local", "Admin123!")
INSTRUCTOR = ("instructor@vita.local", "Instructor123!")
STUDENT = ("estudiante@vita.local", "Estudiante123!")


def request(
    method: str,
    path: str,
    token: str | None = None,
    body: dict[str, Any] | None = None,
) -> tuple[int, Any]:
    data = None if body is None else json.dumps(body).encode("utf-8")
    headers = {"Content-Type": "application/json", "Accept": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(
        f"{API_BASE}{path}",
        data=data,
        headers=headers,
        method=method,
    )
    try:
        with urllib.request.urlopen(req) as resp:
            raw = resp.read().decode("utf-8")
            payload = json.loads(raw) if raw else None
            return resp.status, payload
    except urllib.error.HTTPError as exc:
        raw = exc.read().decode("utf-8")
        try:
            payload = json.loads(raw) if raw else {"error": raw}
        except json.JSONDecodeError:
            payload = {"error": raw}
        return exc.code, payload


def must_ok(status: int, payload: Any, expected: set[int], context: str) -> Any:
    if status not in expected:
        raise SystemExit(f"[FAIL] {context}: HTTP {status} → {payload}")
    return payload


def login(email: str, password: str) -> dict[str, Any]:
    status, payload = request(
        "POST", "/auth/login", body={"email": email, "password": password}
    )
    data = must_ok(status, payload, {200}, f"login {email}")
    assert isinstance(data, dict)
    return data


def find_user_id(admin_token: str, email: str) -> str | None:
    status, users = request("GET", "/users", token=admin_token)
    users = must_ok(status, users, {200}, "list users")
    for user in users:
        if user.get("email", "").lower() == email.lower():
            return user["id"]
    return None


def ensure_user(
    admin_token: str,
    *,
    nombre: str,
    apellido: str,
    email: str,
    password: str,
    rol: str,
) -> str:
    existing = find_user_id(admin_token, email)
    if existing:
        print(f"  · usuario ya existe: {email} ({rol})")
        return existing
    status, payload = request(
        "POST",
        "/users",
        token=admin_token,
        body={
            "nombre": nombre,
            "apellido": apellido,
            "email": email,
            "password": password,
            "rol": rol,
        },
    )
    data = must_ok(status, payload, {201, 200}, f"create user {email}")
    print(f"  + usuario creado: {email} ({rol})")
    return data["id"]


def find_course_by_title(token: str, title: str) -> dict[str, Any] | None:
    status, courses = request("GET", "/courses", token=token)
    courses = must_ok(status, courses, {200}, "list courses")
    for course in courses:
        if course.get("titulo") == title:
            return course
    return None


def ensure_course(
    token: str,
    *,
    title: str,
    id_categoria: int,
    id_nivel: int,
    descripcion_corta: str,
    descripcion_larga: str,
    imagen: str,
    duracion: int,
    id_instructor: str | None = None,
) -> dict[str, Any]:
    existing = find_course_by_title(token, title)
    if existing:
        print(f"  · curso ya existe: {title} (id={existing['id']})")
        return existing

    body: dict[str, Any] = {
        "titulo": title,
        "idCategoria": id_categoria,
        "idNivel": id_nivel,
        "descripcionCorta": descripcion_corta,
        "descripcionLarga": descripcion_larga,
        "imagenPortadaUrl": imagen,
        "duracionEstimadaMin": duracion,
    }
    if id_instructor:
        body["idInstructor"] = id_instructor

    status, payload = request("POST", "/courses", token=token, body=body)
    course = must_ok(status, payload, {201}, f"create course {title}")
    print(f"  + curso creado: {title} (id={course['id']})")
    return course


def publish_course(token: str, course_id: int) -> None:
    status, payload = request(
        "PATCH",
        f"/courses/{course_id}/status",
        token=token,
        body={"estado": "publicado"},
    )
    must_ok(status, payload, {200}, f"publish course {course_id}")


def ensure_lessons(token: str, course_id: int, lessons: list[dict[str, Any]]) -> None:
    status, current = request("GET", f"/courses/{course_id}/lessons", token=token)
    current = must_ok(status, current, {200}, f"list lessons {course_id}")
    by_title = {item["titulo"]: item for item in current}
    for lesson in lessons:
        if lesson["titulo"] in by_title:
            continue
        status, payload = request(
            "POST",
            f"/courses/{course_id}/lessons",
            token=token,
            body=lesson,
        )
        must_ok(status, payload, {201}, f"lesson {lesson['titulo']} on {course_id}")
        print(f"    + lección: {lesson['titulo']}")


def ensure_enrollment(student_token: str, course_id: int, label: str) -> None:
    status, payload = request(
        "POST",
        "/enrollments",
        token=student_token,
        body={"cursoId": course_id},
    )
    if status in (200, 201):
        print(f"  + inscripción: {label} → curso {course_id}")
        return
    if status == 409:
        print(f"  · ya inscrito: {label} → curso {course_id}")
        return
    raise SystemExit(f"[FAIL] enroll {label} curso {course_id}: HTTP {status} → {payload}")


def main() -> None:
    print(f"API_BASE={API_BASE}")
    admin = login(*ADMIN)
    admin_token = admin["token"]
    instructor_login = login(*INSTRUCTOR)
    instructor_token = instructor_login["token"]
    instructor_id = instructor_login["usuario"]["id"]

    print("\n== Usuarios demo ==")
    instructor2_id = ensure_user(
        admin_token,
        nombre="Laura",
        apellido="Instructora",
        email="instructor2@vita.local",
        password="Instructor123!",
        rol="Instructor",
    )
    student_specs = [
        ("Ana", "Pérez", "estudiante2@vita.local"),
        ("Bruno", "Gómez", "estudiante3@vita.local"),
        ("Carla", "Ruiz", "estudiante4@vita.local"),
        ("Diego", "López", "estudiante5@vita.local"),
        ("Elena", "Martín", "estudiante6@vita.local"),
        ("Felipe", "Soto", "estudiante7@vita.local"),
        ("Gina", "Navarro", "estudiante8@vita.local"),
    ]
    student_ids: dict[str, str] = {
        STUDENT[0]: find_user_id(admin_token, STUDENT[0]) or ""
    }
    for nombre, apellido, email in student_specs:
        student_ids[email] = ensure_user(
            admin_token,
            nombre=nombre,
            apellido=apellido,
            email=email,
            password="Estudiante123!",
            rol="Estudiante",
        )

    status, categories = request("GET", "/categories", token=admin_token)
    categories = must_ok(status, categories, {200}, "categories")
    cat_by_name = {c["nombre"]: c["id"] for c in categories}
    status, levels = request("GET", "/levels", token=admin_token)
    levels = must_ok(status, levels, {200}, "levels")
    lvl_by_name = {l["nombre"]: l["id"] for l in levels}

    prog = cat_by_name["Programación"]
    diseno = cat_by_name["Diseño"]
    mkt = cat_by_name["Marketing Digital"]
    prin = lvl_by_name["Principiante"]
    inter = lvl_by_name["Intermedio"]
    avanc = lvl_by_name["Avanzado"]

    courses_def: list[dict[str, Any]] = [
        {
            "titulo": "Fundamentos de Python para principiantes",
            "cat": prog,
            "nivel": prin,
            "corta": "Sintaxis, tipos y control de flujo con proyectos cortos.",
            "larga": "Curso introductorio a Python orientado a práctica guiada.",
            "img": "https://picsum.photos/seed/vita-python/800/450",
            "min": 480,
            "owner": "instructor",
            "lessons": [
                ("Bienvenida y entorno", 1),
                ("Variables y tipos", 2),
                ("Condicionales y bucles", 3),
                ("Funciones básicas", 4),
            ],
        },
        {
            "titulo": "JavaScript moderno y el DOM",
            "cat": prog,
            "nivel": inter,
            "corta": "ES modules, async/await y manipulación del DOM.",
            "larga": "Construye interfaces dinámicas sin framework.",
            "img": "https://picsum.photos/seed/vita-js/800/450",
            "min": 540,
            "owner": "instructor",
            "lessons": [
                ("Repaso de JS", 1),
                ("Promesas y fetch", 2),
                ("DOM y eventos", 3),
                ("Proyecto mini SPA", 4),
            ],
        },
        {
            "titulo": "API REST con .NET y Entity Framework",
            "cat": prog,
            "nivel": avanc,
            "corta": "Diseño de endpoints, Identity y persistencia.",
            "larga": "Buenas prácticas para APIs académicas productivas.",
            "img": "https://picsum.photos/seed/vita-dotnet/800/450",
            "min": 720,
            "owner": "instructor",
            "lessons": [
                ("Arquitectura de la API", 1),
                ("EF Core y migraciones", 2),
                ("Autenticación JWT", 3),
                ("Validación y errores", 4),
                ("Despliegue local", 5),
            ],
        },
        {
            "titulo": "React desde cero con TypeScript",
            "cat": prog,
            "nivel": inter,
            "corta": "Componentes, hooks y tipado estricto.",
            "larga": "Base sólida para frontends mantenibles.",
            "img": "https://picsum.photos/seed/vita-react/800/450",
            "min": 600,
            "owner": "instructor",
            "lessons": [
                ("JSX y componentes", 1),
                ("Estado y efectos", 2),
                ("Rutas y formularios", 3),
                ("Consumo de API", 4),
            ],
        },
        {
            "titulo": "SQL práctico con PostgreSQL",
            "cat": prog,
            "nivel": inter,
            "corta": "Consultas, índices y modelado relacional.",
            "larga": "Enfocado en rendimiento y claridad de datos.",
            "img": "https://picsum.photos/seed/vita-sql/800/450",
            "min": 420,
            "owner": "instructor",
            "lessons": [
                ("SELECT y filtros", 1),
                ("Joins y agregaciones", 2),
                ("Índices básicos", 3),
            ],
        },
        {
            "titulo": "Diseño UI con Figma para producto",
            "cat": diseno,
            "nivel": prin,
            "corta": "Wireframes, componentes y handoff.",
            "larga": "Flujo completo de diseño a desarrollo.",
            "img": "https://picsum.photos/seed/vita-figma/800/450",
            "min": 360,
            "owner": "instructor2",
            "lessons": [
                ("Fundamentos de Figma", 1),
                ("Sistemas de diseño", 2),
                ("Prototipos interactivos", 3),
            ],
        },
        {
            "titulo": "Identidad visual y branding digital",
            "cat": diseno,
            "nivel": inter,
            "corta": "Paleta, tipografía y consistencia de marca.",
            "larga": "Aplica branding a landings y dashboards.",
            "img": "https://picsum.photos/seed/vita-brand/800/450",
            "min": 300,
            "owner": "instructor2",
            "lessons": [
                ("Brief de marca", 1),
                ("Tipografía y color", 2),
                ("Aplicación en UI", 3),
            ],
        },
        {
            "titulo": "SEO técnico para sitios de cursos",
            "cat": mkt,
            "nivel": inter,
            "corta": "Metadatos, rendimiento y estructura semántica.",
            "larga": "Optimiza descubrimiento orgánico de contenidos.",
            "img": "https://picsum.photos/seed/vita-seo/800/450",
            "min": 280,
            "owner": "instructor2",
            "lessons": [
                ("Fundamentos SEO", 1),
                ("Core Web Vitals", 2),
                ("Schema y metadatos", 3),
            ],
        },
        {
            "titulo": "Growth marketing para academias online",
            "cat": mkt,
            "nivel": avanc,
            "corta": "Embudos, retención y métricas de aprendizaje.",
            "larga": "Estrategias orientadas a conversión educativa.",
            "img": "https://picsum.photos/seed/vita-growth/800/450",
            "min": 400,
            "owner": "instructor",
            "lessons": [
                ("Embudo de adquisición", 1),
                ("Retención de alumnos", 2),
                ("Experimentos A/B", 3),
                ("Dashboard de métricas", 4),
            ],
        },
        {
            "titulo": "Git y colaboración en equipos ágiles",
            "cat": prog,
            "nivel": prin,
            "corta": "Branching, PRs y convenciones de commit.",
            "larga": "Flujo de trabajo listo para proyectos reales.",
            "img": "https://picsum.photos/seed/vita-git/800/450",
            "min": 240,
            "owner": "instructor",
            "lessons": [
                ("Git básico", 1),
                ("Ramas y merges", 2),
                ("Pull requests", 3),
            ],
        },
        {
            "titulo": "Accesibilidad web WCAG en la práctica",
            "cat": diseno,
            "nivel": inter,
            "corta": "Contraste, teclado y lectores de pantalla.",
            "larga": "Checklist aplicable a dashboards educativos.",
            "img": "https://picsum.photos/seed/vita-a11y/800/450",
            "min": 320,
            "owner": "instructor2",
            "lessons": [
                ("Principios WCAG", 1),
                ("Navegación por teclado", 2),
                ("ARIA útil", 3),
            ],
        },
        {
            "titulo": "Curso Demo Reportes",
            "cat": diseno,
            "nivel": avanc,
            "corta": "Para probar M8",
            "larga": "Curso seed inicial para demos de reportes.",
            "img": "https://picsum.photos/seed/vita-demo/800/450",
            "min": 90,
            "owner": "instructor",
            "lessons": [
                ("Leccion 1", 1),
                ("Reportes y métricas", 2),
                ("Cierre del demo", 3),
            ],
        },
    ]

    print("\n== Cursos + lecciones ==")
    created: dict[str, int] = {}

    # Token instructor2
    instructor2_token = login("instructor2@vita.local", "Instructor123!")["token"]

    for spec in courses_def:
        owner = spec["owner"]
        if owner == "instructor":
            token = instructor_token
            course = ensure_course(
                token,
                title=spec["titulo"],
                id_categoria=spec["cat"],
                id_nivel=spec["nivel"],
                descripcion_corta=spec["corta"],
                descripcion_larga=spec["larga"],
                imagen=spec["img"],
                duracion=spec["min"],
            )
        else:
            # Admin crea asignando instructor2 (o el propio instructor2)
            token = instructor2_token
            course = ensure_course(
                token,
                title=spec["titulo"],
                id_categoria=spec["cat"],
                id_nivel=spec["nivel"],
                descripcion_corta=spec["corta"],
                descripcion_larga=spec["larga"],
                imagen=spec["img"],
                duracion=spec["min"],
            )

        course_id = int(course["id"])
        created[spec["titulo"]] = course_id
        ensure_lessons(
            token,
            course_id,
            [
                {
                    "titulo": title,
                    "descripcion": f"Contenido de {title}",
                    "recurso": f"https://example.com/vita/{course_id}/{orden}",
                    "orden": orden,
                }
                for title, orden in spec["lessons"]
            ],
        )
        publish_course(token, course_id)

    # Borrador extra solo instructor (completo)
    print("\n== Borrador instructor ==")
    draft = ensure_course(
        instructor_token,
        title="Borrador: Laboratorio privado de Docker",
        id_categoria=prog,
        id_nivel=avanc,
        descripcion_corta="Borrador interno para preparar laboratorio Docker.",
        descripcion_larga="No publicado; útil para demo de estados.",
        imagen="https://picsum.photos/seed/vita-docker/800/450",
        duracion=180,
    )
    ensure_lessons(
        instructor_token,
        int(draft["id"]),
        [
            {
                "titulo": "Outline del laboratorio",
                "descripcion": "Estructura pendiente de publicar",
                "recurso": "https://example.com/vita/docker/outline",
                "orden": 1,
            }
        ],
    )

    print("\n== Inscripciones (alimentan reportes) ==")
    student_tokens = {
        STUDENT[0]: login(*STUDENT)["token"],
    }
    for _, _, email in student_specs:
        student_tokens[email] = login(email, "Estudiante123!")["token"]

    # Mapa de inscripciones: email → títulos
    enroll_map: dict[str, list[str]] = {
        "estudiante@vita.local": [
            "Curso Demo Reportes",
            "Fundamentos de Python para principiantes",
            "JavaScript moderno y el DOM",
            "React desde cero con TypeScript",
            "Git y colaboración en equipos ágiles",
        ],
        "estudiante2@vita.local": [
            "Fundamentos de Python para principiantes",
            "SQL práctico con PostgreSQL",
            "Diseño UI con Figma para producto",
            "Growth marketing para academias online",
        ],
        "estudiante3@vita.local": [
            "API REST con .NET y Entity Framework",
            "React desde cero con TypeScript",
            "SEO técnico para sitios de cursos",
        ],
        "estudiante4@vita.local": [
            "JavaScript moderno y el DOM",
            "Identidad visual y branding digital",
            "Accesibilidad web WCAG en la práctica",
            "Curso Demo Reportes",
        ],
        "estudiante5@vita.local": [
            "Fundamentos de Python para principiantes",
            "Git y colaboración en equipos ágiles",
            "Growth marketing para academias online",
        ],
        "estudiante6@vita.local": [
            "Diseño UI con Figma para producto",
            "Identidad visual y branding digital",
            "Accesibilidad web WCAG en la práctica",
        ],
        "estudiante7@vita.local": [
            "SQL práctico con PostgreSQL",
            "API REST con .NET y Entity Framework",
            "SEO técnico para sitios de cursos",
            "Curso Demo Reportes",
        ],
        "estudiante8@vita.local": [
            "React desde cero con TypeScript",
            "JavaScript moderno y el DOM",
            "Git y colaboración en equipos ágiles",
        ],
    }

    for email, titles in enroll_map.items():
        token = student_tokens[email]
        for title in titles:
            course_id = created.get(title)
            if course_id is None:
                found = find_course_by_title(admin_token, title)
                if not found:
                    raise SystemExit(f"Curso no encontrado para inscripción: {title}")
                course_id = int(found["id"])
                created[title] = course_id
            ensure_enrollment(token, course_id, email)

    print("\n== Verificación reportes ==")
    status, r1 = request("GET", "/reports/courses-by-instructor", token=admin_token)
    r1 = must_ok(status, r1, {200}, "courses-by-instructor")
    status, r2 = request("GET", "/reports/students-by-course", token=admin_token)
    r2 = must_ok(status, r2, {200}, "students-by-course")
    status, r3 = request("GET", "/reports/top-courses?limit=10", token=admin_token)
    r3 = must_ok(status, r3, {200}, "top-courses")

    status, all_courses = request("GET", "/courses", token=admin_token)
    all_courses = must_ok(status, all_courses, {200}, "final courses")
    published = [c for c in all_courses if c.get("estado") == "Publicado"]

    print(f"Cursos totales (vista admin): {len(all_courses)}")
    print(f"Cursos publicados: {len(published)}")
    print(f"Reportes courses-by-instructor: {len(r1)} filas")
    print(f"Reportes students-by-course: {len(r2)} filas")
    print(f"Reportes top-courses: {len(r3)} filas")
    print("\nOK seed demo listo.")
    print("Cuentas clave:")
    print("  admin@vita.local / Admin123!")
    print("  instructor@vita.local / Instructor123!")
    print("  instructor2@vita.local / Instructor123!")
    print("  estudiante@vita.local … estudiante8@vita.local / Estudiante123!")


if __name__ == "__main__":
    try:
        main()
    except urllib.error.URLError as exc:
        print(f"[FAIL] No se pudo conectar a {API_BASE}: {exc}", file=sys.stderr)
        sys.exit(1)
