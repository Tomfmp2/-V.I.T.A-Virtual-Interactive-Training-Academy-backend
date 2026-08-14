using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
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

    private static readonly (byte R, byte G, byte B)[] CoverPalette =
    [
        (0, 230, 118),
        (16, 185, 129),
        (34, 197, 94),
        (6, 182, 212),
        (59, 130, 246),
        (168, 85, 247),
        (244, 63, 94),
        (251, 146, 60)
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var db = services.GetRequiredService<ApplicationDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();

        await SeedRolesAsync(roleManager);
        await SeedDemoUsersAsync(userManager, configuration);
        await SeedCatalogsAsync(db);
        await SeedCategoriasAsync(db);
        await SeedCourseCoversAsync(db, environment);
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

    /// <summary>
    /// En Development, asigna una portada local a cursos que aún no tienen imagen
    /// (útil para ver el catálogo con portadas sin subirlas a mano).
    /// </summary>
    private static async Task SeedCourseCoversAsync(
        ApplicationDbContext db,
        IWebHostEnvironment environment)
    {
        var cursos = await db.Cursos
            .Where(c => c.ImagenPortadaUrl == null || c.ImagenPortadaUrl == "")
            .OrderBy(c => c.IdCurso)
            .ToListAsync();

        if (cursos.Count == 0)
            return;

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var coversDir = Path.Combine(webRoot, "uploads", "covers");
        Directory.CreateDirectory(coversDir);

        for (var i = 0; i < cursos.Count; i++)
        {
            var curso = cursos[i];
            var color = CoverPalette[i % CoverPalette.Length];
            var fileName = $"{curso.IdCurso}.png";
            var physicalPath = Path.Combine(coversDir, fileName);
            await File.WriteAllBytesAsync(physicalPath, CreateSolidPng(960, 540, color.R, color.G, color.B));
            curso.ImagenPortadaUrl = $"/uploads/covers/{fileName}";
            curso.UpdateAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static byte[] CreateSolidPng(int width, int height, byte r, byte g, byte b)
    {
        var row = new byte[1 + width * 3];
        for (var x = 0; x < width; x++)
        {
            var offset = 1 + x * 3;
            row[offset] = r;
            row[offset + 1] = g;
            row[offset + 2] = b;
        }

        using var raw = new MemoryStream();
        for (var y = 0; y < height; y++)
            raw.Write(row);

        var compressed = CompressZlib(raw.ToArray());
        using var png = new MemoryStream();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        WritePngChunk(png, "IHDR"u8.ToArray(), BuildIhdr(width, height));
        WritePngChunk(png, "IDAT"u8.ToArray(), compressed);
        WritePngChunk(png, "IEND"u8.ToArray(), []);
        return png.ToArray();
    }

    private static byte[] BuildIhdr(int width, int height)
    {
        var data = new byte[13];
        WriteInt32Be(data, 0, width);
        WriteInt32Be(data, 4, height);
        data[8] = 8;  // bit depth
        data[9] = 2;  // truecolor RGB
        return data;
    }

    private static void WritePngChunk(Stream stream, byte[] type, byte[] data)
    {
        WriteInt32Be(stream, data.Length);
        stream.Write(type);
        stream.Write(data);
        var crcPayload = new byte[type.Length + data.Length];
        Buffer.BlockCopy(type, 0, crcPayload, 0, type.Length);
        Buffer.BlockCopy(data, 0, crcPayload, type.Length, data.Length);
        WriteInt32Be(stream, (int)Crc32(crcPayload));
    }

    private static byte[] CompressZlib(byte[] raw)
    {
        using var output = new MemoryStream();
        output.WriteByte(0x78);
        output.WriteByte(0x9C);
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
            deflate.Write(raw);
        var adler = Adler32(raw);
        WriteInt32Be(output, (int)adler);
        return output.ToArray();
    }

    private static void WriteInt32Be(Stream stream, int value)
    {
        stream.WriteByte((byte)((value >> 24) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)(value & 0xFF));
    }

    private static void WriteInt32Be(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)((value >> 24) & 0xFF);
        buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 3] = (byte)(value & 0xFF);
    }

    private static uint Adler32(byte[] data)
    {
        uint a = 1, b = 0;
        foreach (var value in data)
        {
            a = (a + value) % 65521;
            b = (b + a) % 65521;
        }
        return (b << 16) | a;
    }

    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var value in data)
        {
            crc ^= value;
            for (var i = 0; i < 8; i++)
            {
                var mask = (crc & 1) != 0 ? 0xEDB88320u : 0u;
                crc = (crc >> 1) ^ mask;
            }
        }
        return crc ^ 0xFFFFFFFF;
    }
}
