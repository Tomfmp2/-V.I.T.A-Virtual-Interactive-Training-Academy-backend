using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vita.Api.Common;
using Vita.Api.Entities;

namespace Vita.Api.Data;

public static class DbSeeder
{
    private const string AdminPasswordKey = "Seeds:DemoUsers:AdminPassword";
    private const string InstructorPasswordKey = "Seeds:DemoUsers:InstructorPassword";
    private const string StudentPasswordKey = "Seeds:DemoUsers:StudentPassword";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var db = services.GetRequiredService<ApplicationDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(roleManager);
        await SeedDemoUsersAsync(userManager, configuration);
        await SeedCatalogsAsync(db);
        await SeedCategoriasAsync(db);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Instructor", "Estudiante"];

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    private static async Task SeedDemoUsersAsync(
        UserManager<Usuario> userManager,
        IConfiguration configuration)
    {
        // Sin valores por defecto: las tres claves deben existir en Development.
        var adminPassword = RequireDemoPassword(configuration, AdminPasswordKey);
        var instructorPassword = RequireDemoPassword(configuration, InstructorPasswordKey);
        var studentPassword = RequireDemoPassword(configuration, StudentPasswordKey);

        var demos = new (string Email, string Password, string Nombre, string Apellido, string Role)[]
        {
            ("admin@vita.local", adminPassword, "Admin", "Prueba", "Admin"),
            ("instructor@vita.local", instructorPassword, "Instructor", "Prueba", "Instructor"),
            ("estudiante@vita.local", studentPassword, "Estudiante", "Prueba", "Estudiante"),
        };

        foreach (var (email, password, nombre, apellido, role) in demos)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, role))
                    await userManager.AddToRoleAsync(existing, role);

                // Normaliza nombres demo antiguos en inglés ("Demo") a español.
                var needsUpdate = false;
                if (!string.Equals(existing.Nombre, nombre, StringComparison.Ordinal))
                {
                    existing.Nombre = nombre;
                    needsUpdate = true;
                }

                if (string.Equals(existing.Apellido, "Demo", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existing.Apellido, apellido, StringComparison.Ordinal))
                {
                    existing.Apellido = apellido;
                    needsUpdate = true;
                }

                if (email == "admin@vita.local" &&
                    (existing.Telefono is null || existing.CodigoPais is null))
                {
                    existing.Telefono ??= "3001234567";
                    existing.CodigoPais ??= "+57";
                    needsUpdate = true;
                }

                if (needsUpdate)
                    await userManager.UpdateAsync(existing);

                continue;
            }

            var user = new Usuario
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nombre = nombre,
                Apellido = apellido,
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                Telefono = email == "admin@vita.local" ? "3001234567" : null,
                CodigoPais = email == "admin@vita.local" ? "+57" : null,
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(
                    $"No se pudo crear el usuario demo '{email}': {errors}");
            }

            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static string RequireDemoPassword(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        throw new InvalidOperationException(
            $"Falta la contraseña de seed '{key}'. " +
            "Configúrala localmente (no la commits) con User Secrets: " +
            $"dotnet user-secrets set \"{key}\" \"<tu-password>\" --project Vita.Api " +
            "o con la variable de entorno Seeds__DemoUsers__AdminPassword / " +
            "Seeds__DemoUsers__InstructorPassword / Seeds__DemoUsers__StudentPassword. " +
            "Luego vuelve a ejecutar: dotnet run --project Vita.Api");
    }

    private static async Task SeedCatalogsAsync(ApplicationDbContext db)
    {
        if (!await db.Niveles.AnyAsync())
        {
            db.Niveles.AddRange(
                new Nivel { Nombre = "Principiante" },
                new Nivel { Nombre = "Intermedio" },
                new Nivel { Nombre = "Avanzado" });
        }

        if (!await db.EstadosCurso.AnyAsync())
        {
            db.EstadosCurso.AddRange(
                new EstadoCurso { Nombre = "Borrador" },
                new EstadoCurso { Nombre = "Publicado" });
        }

        if (!await db.TiposLeccion.AnyAsync())
        {
            db.TiposLeccion.AddRange(
                new TipoLeccion { Nombre = "Video" },
                new TipoLeccion { Nombre = "Texto" },
                new TipoLeccion { Nombre = "Recurso" });
        }

        if (!await db.EstadosInscripcion.AnyAsync())
        {
            db.EstadosInscripcion.AddRange(
                new EstadoInscripcion { Nombre = "Activa" },
                new EstadoInscripcion { Nombre = "Cancelada" });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedCategoriasAsync(ApplicationDbContext db)
    {
        string[] nombres = ["Programación", "Diseño", "Marketing Digital"];

        foreach (var nombre in nombres)
        {
            if (await db.Categorias.AnyAsync(c => c.Nombre == nombre))
                continue;

            db.Categorias.Add(new Categoria
            {
                Nombre = nombre,
                Slug = SlugHelper.Generate(nombre),
                Activo = true,
            });
        }

        await db.SaveChangesAsync();
    }
}
