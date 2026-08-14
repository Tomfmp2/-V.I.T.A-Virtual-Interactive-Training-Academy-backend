#!/usr/bin/env python3
"""Asigna portadas JPG/PNG a los cursos existentes vía POST /api/courses/{id}/cover.

Uso:
  python scripts/seed_course_covers.py
  API_BASE=http://localhost:5044/api python scripts/seed_course_covers.py

Login: admin@vita.local (password de seed local / Trello).
"""

from __future__ import annotations

import json
import os
import struct
import sys
import urllib.error
import urllib.request
import zlib
from pathlib import Path
from typing import Any

API_BASE = os.environ.get("API_BASE", "http://localhost:5044/api").rstrip("/")
ADMIN_EMAIL = os.environ.get("VITA_ADMIN_EMAIL", "admin@vita.local")
ADMIN_PASSWORD = os.environ.get("VITA_ADMIN_PASSWORD", "Admin123!")

COLORS = [
    (0, 230, 118),
    (16, 185, 129),
    (34, 197, 94),
    (6, 182, 212),
    (59, 130, 246),
    (168, 85, 247),
    (244, 63, 94),
    (251, 146, 60),
]


def png_bytes(width: int, height: int, rgb: tuple[int, int, int]) -> bytes:
    def chunk(tag: bytes, data: bytes) -> bytes:
        crc = zlib.crc32(tag + data) & 0xFFFFFFFF
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", crc)

    raw = b"".join(b"\x00" + bytes(rgb) * width for _ in range(height))
    return (
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0))
        + chunk(b"IDAT", zlib.compress(raw, 9))
        + chunk(b"IEND", b"")
    )


def request_json(
    method: str,
    path: str,
    token: str | None = None,
    body: dict[str, Any] | None = None,
) -> tuple[int, Any]:
    data = None if body is None else json.dumps(body).encode("utf-8")
    headers = {"Accept": "application/json"}
    if body is not None:
        headers["Content-Type"] = "application/json"
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(
        f"{API_BASE}{path}", data=data, headers=headers, method=method
    )
    try:
        with urllib.request.urlopen(req) as resp:
            raw = resp.read().decode("utf-8")
            return resp.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as exc:
        raw = exc.read().decode("utf-8")
        try:
            payload = json.loads(raw) if raw else {"error": raw}
        except json.JSONDecodeError:
            payload = {"error": raw}
        return exc.code, payload


def upload_cover(token: str, course_id: int, image: bytes, filename: str) -> tuple[int, Any]:
    boundary = "----vitaCoverBoundary7MA4YWxkTrZu0gW"
    body = (
        f"--{boundary}\r\n"
        f'Content-Disposition: form-data; name="file"; filename="{filename}"\r\n'
        f"Content-Type: image/png\r\n\r\n"
    ).encode("utf-8") + image + f"\r\n--{boundary}--\r\n".encode("utf-8")

    req = urllib.request.Request(
        f"{API_BASE}/courses/{course_id}/cover",
        data=body,
        headers={
            "Authorization": f"Bearer {token}",
            "Content-Type": f"multipart/form-data; boundary={boundary}",
            "Accept": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req) as resp:
            raw = resp.read().decode("utf-8")
            return resp.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as exc:
        raw = exc.read().decode("utf-8")
        try:
            payload = json.loads(raw) if raw else {"error": raw}
        except json.JSONDecodeError:
            payload = {"error": raw}
        return exc.code, payload


def main() -> int:
    status, login = request_json(
        "POST", "/auth/login", body={"email": ADMIN_EMAIL, "password": ADMIN_PASSWORD}
    )
    if status != 200 or not isinstance(login, dict) or not login.get("token"):
        print(f"[FAIL] login HTTP {status}: {login}", file=sys.stderr)
        print(
            "Tip: exporta VITA_ADMIN_PASSWORD con la password de seed local.",
            file=sys.stderr,
        )
        return 1

    token = login["token"]
    status, courses = request_json("GET", "/courses", token=token)
    if status != 200 or not isinstance(courses, list):
        print(f"[FAIL] list courses HTTP {status}: {courses}", file=sys.stderr)
        return 1

    out_dir = Path(__file__).resolve().parents[1] / "Vita.Api" / "wwwroot" / "uploads" / "covers"
    out_dir.mkdir(parents=True, exist_ok=True)

    ok = 0
    for index, course in enumerate(courses):
        course_id = int(course["id"])
        color = COLORS[index % len(COLORS)]
        image = png_bytes(960, 540, color)
        filename = f"{course_id}.png"
        (out_dir / filename).write_bytes(image)

        status, payload = upload_cover(token, course_id, image, filename)
        if status != 200:
            print(f"[FAIL] cover course {course_id}: HTTP {status} → {payload}", file=sys.stderr)
            continue

        print(f"[OK] curso {course_id} ({course.get('titulo')}) → {payload.get('imagenPortadaUrl')}")
        ok += 1

    print(f"Listo: {ok}/{len(courses)} portadas asignadas.")
    return 0 if ok == len(courses) else 2


if __name__ == "__main__":
    raise SystemExit(main())
