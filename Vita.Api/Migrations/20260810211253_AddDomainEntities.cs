using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    icono_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "estados_curso",
                columns: table => new
                {
                    id_estado_curso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_curso", x => x.id_estado_curso);
                });

            migrationBuilder.CreateTable(
                name: "estados_inscripcion",
                columns: table => new
                {
                    id_estado_inscripcion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_inscripcion", x => x.id_estado_inscripcion);
                });

            migrationBuilder.CreateTable(
                name: "niveles",
                columns: table => new
                {
                    id_nivel = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_niveles", x => x.id_nivel);
                });

            migrationBuilder.CreateTable(
                name: "tipos_leccion",
                columns: table => new
                {
                    id_tipo_leccion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_leccion", x => x.id_tipo_leccion);
                });

            migrationBuilder.CreateTable(
                name: "cursos",
                columns: table => new
                {
                    id_curso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_instructor = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    id_nivel = table.Column<int>(type: "integer", nullable: false),
                    id_estado_curso = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    descripcion_corta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    descripcion_larga = table.Column<string>(type: "text", nullable: true),
                    imagen_portada_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    duracion_estimada_min = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cursos", x => x.id_curso);
                    table.ForeignKey(
                        name: "FK_cursos_AspNetUsers_id_instructor",
                        column: x => x.id_instructor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cursos_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cursos_estados_curso_id_estado_curso",
                        column: x => x.id_estado_curso,
                        principalTable: "estados_curso",
                        principalColumn: "id_estado_curso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cursos_niveles_id_nivel",
                        column: x => x.id_nivel,
                        principalTable: "niveles",
                        principalColumn: "id_nivel",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inscripciones",
                columns: table => new
                {
                    id_inscripcion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_estudiante = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    id_curso = table.Column<int>(type: "integer", nullable: false),
                    id_estado_inscripcion = table.Column<int>(type: "integer", nullable: false),
                    fecha_inscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscripciones", x => x.id_inscripcion);
                    table.ForeignKey(
                        name: "FK_inscripciones_AspNetUsers_id_estudiante",
                        column: x => x.id_estudiante,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscripciones_cursos_id_curso",
                        column: x => x.id_curso,
                        principalTable: "cursos",
                        principalColumn: "id_curso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscripciones_estados_inscripcion_id_estado_inscripcion",
                        column: x => x.id_estado_inscripcion,
                        principalTable: "estados_inscripcion",
                        principalColumn: "id_estado_inscripcion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lecciones",
                columns: table => new
                {
                    id_leccion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curso = table.Column<int>(type: "integer", nullable: false),
                    id_tipo_leccion = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resumen = table.Column<string>(type: "text", nullable: true),
                    contenido = table.Column<string>(type: "text", nullable: true),
                    recurso_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    duracion_min = table.Column<int>(type: "integer", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lecciones", x => x.id_leccion);
                    table.ForeignKey(
                        name: "FK_lecciones_cursos_id_curso",
                        column: x => x.id_curso,
                        principalTable: "cursos",
                        principalColumn: "id_curso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lecciones_tipos_leccion_id_tipo_leccion",
                        column: x => x.id_tipo_leccion,
                        principalTable: "tipos_leccion",
                        principalColumn: "id_tipo_leccion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categorias_slug",
                table: "categorias",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_categoria",
                table: "cursos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_estado_curso",
                table: "cursos",
                column: "id_estado_curso");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_instructor",
                table: "cursos",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_nivel",
                table: "cursos",
                column: "id_nivel");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_slug",
                table: "cursos",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estados_curso_nombre",
                table: "estados_curso",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estados_inscripcion_nombre",
                table: "estados_inscripcion",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inscripciones_id_curso",
                table: "inscripciones",
                column: "id_curso");

            migrationBuilder.CreateIndex(
                name: "IX_inscripciones_id_estado_inscripcion",
                table: "inscripciones",
                column: "id_estado_inscripcion");

            migrationBuilder.CreateIndex(
                name: "IX_inscripciones_id_estudiante_id_curso",
                table: "inscripciones",
                columns: new[] { "id_estudiante", "id_curso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lecciones_id_curso_orden",
                table: "lecciones",
                columns: new[] { "id_curso", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lecciones_id_tipo_leccion",
                table: "lecciones",
                column: "id_tipo_leccion");

            migrationBuilder.CreateIndex(
                name: "IX_niveles_nombre",
                table: "niveles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_leccion_nombre",
                table: "tipos_leccion",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inscripciones");

            migrationBuilder.DropTable(
                name: "lecciones");

            migrationBuilder.DropTable(
                name: "estados_inscripcion");

            migrationBuilder.DropTable(
                name: "cursos");

            migrationBuilder.DropTable(
                name: "tipos_leccion");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "estados_curso");

            migrationBuilder.DropTable(
                name: "niveles");
        }
    }
}
