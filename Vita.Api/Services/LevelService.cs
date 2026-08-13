using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vita.Api.Data;
using Vita.Api.Dtos.Levels;
using Vita.Api.Entities;

namespace Vita.Api.Services;

public class LevelService : ILevelService
{
    private readonly ApplicationDbContext _db;

    public LevelService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<LevelResponse>> GetAllAsync()
    {
        return await _db.Niveles
            .AsNoTracking()
            .OrderBy(n => n.Nombre)
            .Select(n => new LevelResponse
            {
                Id = n.IdNivel,
                Nombre = n.Nombre
            })
            .ToListAsync();
    }

    public async Task<LevelResponse?> GetByIdAsync(int id)
    {
        return await _db.Niveles
            .AsNoTracking()
            .Where(n => n.IdNivel == id)
            .Select(n => new LevelResponse
            {
                Id = n.IdNivel,
                Nombre = n.Nombre
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LevelResult> CreateAsync(LevelRequest request)
    {
        var nombre = request.Nombre.Trim();
        var existe = await _db.Niveles
            .AnyAsync(n => n.Nombre.ToLower() == nombre.ToLower());

        if (existe)
            return new LevelResult { Outcome = LevelOutcome.NombreExists };

        var nivel = new Nivel
        {
            Nombre = nombre
        };

        _db.Niveles.Add(nivel);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return new LevelResult { Outcome = LevelOutcome.NombreExists };
        }

        return new LevelResult
        {
            Outcome = LevelOutcome.Success,
            Level = MapToResponse(nivel)
        };
    }

    public async Task<LevelResult> UpdateAsync(int id, LevelRequest request)
    {
        var nivel = await _db.Niveles.FindAsync(id);
        if (nivel is null)
            return new LevelResult { Outcome = LevelOutcome.NotFound };

        var nombre = request.Nombre.Trim();
        var existe = await _db.Niveles
            .AnyAsync(n => n.IdNivel != id && n.Nombre.ToLower() == nombre.ToLower());

        if (existe)
            return new LevelResult { Outcome = LevelOutcome.NombreExists };

        nivel.Nombre = nombre;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return new LevelResult { Outcome = LevelOutcome.NombreExists };
        }

        return new LevelResult
        {
            Outcome = LevelOutcome.Success,
            Level = MapToResponse(nivel)
        };
    }

    public async Task<LevelResult> DeleteAsync(int id)
    {
        var nivel = await _db.Niveles.FindAsync(id);
        if (nivel is null)
            return new LevelResult { Outcome = LevelOutcome.NotFound };

        var enUso = await _db.Cursos.AnyAsync(c => c.IdNivel == id);
        if (enUso)
            return new LevelResult { Outcome = LevelOutcome.InUse };

        _db.Niveles.Remove(nivel);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return new LevelResult { Outcome = LevelOutcome.InUse };
        }

        return new LevelResult { Outcome = LevelOutcome.Success };
    }

    private static LevelResponse MapToResponse(Nivel nivel) => new()
    {
        Id = nivel.IdNivel,
        Nombre = nivel.Nombre
    };
}
