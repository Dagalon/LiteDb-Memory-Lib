namespace LiteDb_Memory_Lib;

/// <summary>
/// Shared enumerations that describe LiteDB in-memory operation outcomes.
/// </summary>
public static class EnumsLiteDbMemory
{
    /// <summary>
    /// Result codes returned by LiteDB helper operations.
    /// </summary>
    public enum Output
    {
        SUCCESS = 1,
        DB_NOT_FOUND = 2,
        PATH_NOT_FOUND = 3,
        THERE_EXISTS_DATABASE = 4,
        PATH_IS_NULL_OR_EMPTY = 5,
        COLLECTION_NOT_FOUND = 6
    }
}
