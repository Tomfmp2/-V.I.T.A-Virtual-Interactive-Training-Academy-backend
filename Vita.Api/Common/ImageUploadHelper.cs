using Microsoft.AspNetCore.Http;

namespace Vita.Api.Common;

/// <summary>
/// Validación compartida de imágenes subidas (perfil, portadas de curso, etc.).
/// El Content-Type lo declara el cliente; también se comprueba la firma binaria.
/// </summary>
public static class ImageUploadHelper
{
    public const long MaxBytes = 2 * 1024 * 1024;

    public static readonly IReadOnlyDictionary<string, string> AllowedContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

    public static bool TryValidate(IFormFile? file, out string extension, out string? error)
    {
        extension = string.Empty;
        error = null;

        if (file is null || file.Length == 0)
        {
            error = "Debes seleccionar una imagen.";
            return false;
        }

        if (file.Length > MaxBytes)
        {
            error = "La imagen no puede superar 2 MB.";
            return false;
        }

        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var ext))
        {
            error = "Formato no permitido. Usa JPG, PNG o WEBP.";
            return false;
        }

        extension = ext;
        return true;
    }

    public static async Task<bool> HasImageSignatureAsync(IFormFile file, string extension)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header);
        if (read < 12)
            return false;

        return extension switch
        {
            ".jpg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".webp" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                       header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
            _ => false
        };
    }

    public static void DeleteExistingFiles(string directory, string fileNameWithoutExtension)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var existing in Directory.GetFiles(directory, $"{fileNameWithoutExtension}.*"))
            File.Delete(existing);
    }
}
