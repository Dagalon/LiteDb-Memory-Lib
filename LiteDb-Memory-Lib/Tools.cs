using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LiteDb_Memory_Lib;

/// <summary>
/// Métodos auxiliares compartidos en toda la biblioteca.
/// </summary>
public static class Tools
{
    /// <summary>
    /// Lee un archivo JSON desde disco y lo deserializa al tipo solicitado.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapeará el archivo JSON.</typeparam>
    /// <param name="jsonPath">Ruta absoluta o relativa del archivo JSON.</param>
    /// <exception cref="ArgumentException">Se lanza cuando <paramref name="jsonPath"/> es nulo o solo contiene espacios.</exception>
    /// <exception cref="FileNotFoundException">Se lanza cuando el archivo no existe en disco.</exception>
    /// <exception cref="JsonException">Se lanza cuando el contenido JSON no puede deserializarse.</exception>
    public static T? ReadJson<T>(string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            throw new ArgumentException("The JSON path cannot be null or whitespace.", nameof(jsonPath));
        }

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("The JSON file could not be located.", jsonPath);
        }

        using StreamReader reader = new(jsonPath);
        var json = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Intenta leer un archivo JSON desde disco y deserializarlo al tipo solicitado.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapeará el archivo JSON.</typeparam>
    /// <param name="jsonPath">Ruta absoluta o relativa del archivo JSON.</param>
    /// <param name="result">Resultado deserializado cuando la operación tiene éxito.</param>
    /// <returns><c>true</c> si el archivo se procesa correctamente; en caso contrario, <c>false</c>.</returns>
    public static bool TryReadJson<T>(string jsonPath, [MaybeNullWhen(false)] out T result)
    {
        try
        {
            var value = ReadJson<T>(jsonPath);
            result = value!;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            result = default;
            return false;
        }
    }
}