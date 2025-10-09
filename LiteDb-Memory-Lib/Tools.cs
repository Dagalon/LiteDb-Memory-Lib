using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LiteDb_Memory_Lib;

/// <summary>
/// Helper methods shared across the library.
/// </summary>
public static class Tools
{
    /// <summary>
    /// Reads a JSON file from disk and deserialises it into the requested type.
    /// </summary>
    /// <typeparam name="T">Type that the JSON file will be mapped to.</typeparam>
    /// <param name="jsonPath">Absolute or relative path to the JSON file.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="jsonPath"/> is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file cannot be found on disk.</exception>
    /// <exception cref="JsonException">Thrown when the JSON content cannot be deserialised.</exception>
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
    /// Attempts to read a JSON file from disk and deserialise it into the requested type.
    /// </summary>
    /// <typeparam name="T">Type that the JSON file will be mapped to.</typeparam>
    /// <param name="jsonPath">Absolute or relative path to the JSON file.</param>
    /// <param name="result">The deserialised result when the operation succeeds.</param>
    /// <returns><c>true</c> when the JSON file is successfully parsed; otherwise, <c>false</c>.</returns>
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