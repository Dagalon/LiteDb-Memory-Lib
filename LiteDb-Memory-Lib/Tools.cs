using Newtonsoft.Json;

namespace LiteDb_Memory_Lib;

public static class Tools
{
    public static T ReadJson<T>(string jsonPath)
    {
        using StreamReader reader = new(jsonPath);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json);
    }

}