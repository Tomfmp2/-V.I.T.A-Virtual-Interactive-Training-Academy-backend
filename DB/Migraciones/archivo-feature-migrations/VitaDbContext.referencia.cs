using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vita.Api.Entities;

namespace Vita.Api.Config;

public class VitaDbContext : IdentityDbContext<ApplicationUser>
{
    public VitaDbContext(DbContextOptions<VitaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Nivel> Niveles => Set<Nivel>();
    public DbSet<EstadoCurso> EstadosCurso => Set<EstadoCurso>();
    public DbSet<TipoLeccion> TiposLeccion => Set<TipoLeccion>();
    public DbSet<EstadoInscripcion> EstadosInscripcion => Set<EstadoInscripcion>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Leccion> Lecciones => Set<Leccion>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
    public DbSet<ProgresoLeccion> ProgresosLeccion => Set<ProgresoLeccion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.FotoUrl).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(450);
        });

        builder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(120).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            entity.Property(e => e.IconoUrl).HasColumnName("icono_url").HasMaxLength(255);
            entity.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        builder.Entity<Nivel>(entity =>
        {
            entity.ToTable("niveles");
            entity.HasKey(e => e.IdNivel);
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        builder.Entity<EstadoCurso>(entity =>
        {
            entity.ToTable("estados_curso");
            entity.HasKey(e => e.IdEstadoCurso);
            entity.Property(e => e.IdEstadoCurso).HasColumnName("id_estado_curso");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        builder.Entity<TipoLeccion>(entity =>
        {
            entity.ToTable("tipos_leccion");
            entity.HasKey(e => e.IdTipoLeccion);
            entity.Property(e => e.IdTipoLeccion).HasColumnName("id_tipo_leccion");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        builder.Entity<EstadoInscripcion>(entity =>
        {
            entity.ToTable("estados_inscripcion");
            entity.HasKey(e => e.IdEstadoInscripcion);
            entity.Property(e => e.IdEstadoInscripcion).HasColumnName("id_estado_inscripcion");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        builder.Entity<Curso>(entity =>
        {
            entity.ToTable("cursos");
            entity.HasKey(e => e.IdCurso);
            entity.Property(e => e.IdCurso).HasColumnName("id_curso");
            entity.Property(e => e.IdInstructor).HasColumnName("id_instructor").HasMaxLength(450).IsRequired();
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria").IsRequired();
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel").IsRequired();
            entity.Property(e => e.IdEstadoCurso).HasColumnName("id_estado_curso").IsRequired();
            entity.Property(e => e.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(220).IsRequired();
            entity.Property(e => e.DescripcionCorta).HasColumnName("descripcion_corta").HasMaxLength(300);
            entity.Property(e => e.DescripcionLarga).HasColumnName("descripcion_larga");
            entity.Property(e => e.ImagenPortadaUrl).HasColumnName("imagen_portada_url").HasMaxLength(255);
            entity.Property(e => e.DuracionEstimadaMin).HasColumnName("duracion_estimada_min");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.Instructor)
                .WithMany(u => u.CursosComoInstructor)
                .HasForeignKey(e => e.IdInstructor)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Cursos)
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Nivel)
                .WithMany(n => n.Cursos)
                .HasForeignKey(e => e.IdNivel)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EstadoCurso)
                .WithMany(s => s.Cursos)
                .HasForeignKey(e => e.IdEstadoCurso)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Leccion>(entity =>
        {
            entity.ToTable("lecciones");
            entity.HasKey(e => e.IdLeccion);
            entity.Property(e => e.IdLeccion).HasColumnName("id_leccion");
            entity.Property(e => e.IdCurso).HasColumnName("id_curso").IsRequired();
            entity.Property(e => e.IdTipoLeccion).HasColumnName("id_tipo_leccion").IsRequired();
            entity.Property(e => e.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Resumen).HasColumnName("resumen");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.RecursoUrl).HasColumnName("recurso_url").HasMaxLength(255);
            entity.Property(e => e.Orden).HasColumnName("orden").HasDefaultValue(1).IsRequired();
            entity.Property(e => e.DuracionMin).HasColumnName("duracion_min");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").IsRequired();
            entity.HasIndex(e => new { e.IdCurso, e.Orden }).IsUnique();

            entity.HasOne(e => e.Curso)
                .WithMany(c => c.Lecciones)
                .HasForeignKey(e => e.IdCurso)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoLeccion)
                .WithMany(t => t.Lecciones)
                .HasForeignKey(e => e.IdTipoLeccion)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Inscripcion>(entity =>
        {
            entity.ToTable("inscripciones");
            entity.HasKey(e => e.IdInscripcion);
            entity.Property(e => e.IdInscripcion).HasColumnName("id_inscripcion");
            entity.Property(e => e.IdEstudiante).HasColumnName("id_estudiante").HasMaxLength(450).IsRequired();
            entity.Property(e => e.IdCurso).HasColumnName("id_curso").IsRequired();
            entity.Property(e => e.IdEstadoInscripcion).HasColumnName("id_estado_inscripcion").IsRequired();
            entity.Property(e => e.FechaInscripcion).HasColumnName("fecha_inscripcion").IsRequired();
            entity.HasIndex(e => new { e.IdEstudiante, e.IdCurso }).IsUnique();

            entity.HasOne(e => e.Estudiante)
                .WithMany(u => u.Inscripciones)
                .HasForeignKey(e => e.IdEstudiante)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Curso)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(e => e.IdCurso)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EstadoInscripcion)
                .WithMany(s => s.Inscripciones)
                .HasForeignKey(e => e.IdEstadoInscripcion)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProgresoLeccion>(entity =>
        {
            entity.ToTable("progreso_lecciones");
            entity.HasKey(e => e.IdProgreso);
            entity.Property(e => e.IdProgreso).HasColumnName("id_progreso");
            entity.Property(e => e.IdEstudiante).HasColumnName("id_estudiante").HasMaxLength(450).IsRequired();
            entity.Property(e => e.IdLeccion).HasColumnName("id_leccion").IsRequired();
            entity.Property(e => e.Completada).HasColumnName("completada").HasDefaultValue(true).IsRequired();
            entity.Property(e => e.VistoEn).HasColumnName("visto_en").IsRequired();
            entity.HasIndex(e => new { e.IdEstudiante, e.IdLeccion }).IsUnique();

            entity.HasOne(e => e.Estudiante)
                .WithMany(u => u.ProgresosLeccion)
                .HasForeignKey(e => e.IdEstudiante)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Leccion)
                .WithMany(l => l.Progresos)
                .HasForeignKey(e => e.IdLeccion)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
