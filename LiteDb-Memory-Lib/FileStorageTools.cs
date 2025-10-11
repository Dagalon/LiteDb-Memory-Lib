using LiteDB;

namespace LiteDb_Memory_Lib;

public static class FileStorageTools
{
    /// <summary>
    /// Uploads a file from disk into the LiteDB file storage for the provided alias.
    /// </summary>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="id">Identifier of the storage container.</param>
    /// <param name="fileName">Name that the file will have in storage.</param>
    /// <param name="pathFile">Path to the file on disk.</param>
    /// <returns>A value that reports whether the upload succeeded.</returns>
    public static EnumsLiteDbMemory.Output Upload(ConnectionManager manager, string alias, string id, string fileName, string pathFile)
    {
        if (!File.Exists(pathFile))
        {
            return EnumsLiteDbMemory.Output.PATH_NOT_FOUND;
        }

        var db = manager.GetDatabase(alias, createIfMissing: false);
        if (db is null)
        {
            return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
        }

        var fs = db.GetStorage<string>(id, GetAliasFiles(id));

        using var stream = new FileStream(pathFile, FileMode.Open, FileAccess.Read);
        fs.Upload(fileName, pathFile, stream);

        db.Checkpoint();

        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    /// <summary>
    /// Uploads a file represented by a memory stream into the LiteDB storage.
    /// </summary>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="id">Identifier of the storage container.</param>
    /// <param name="fileName">Name that the file will have in storage.</param>
    /// <param name="stream">Optional stream with the file contents.</param>
    /// <returns>A value that reports whether the upload succeeded.</returns>
    public static EnumsLiteDbMemory.Output Upload(ConnectionManager manager, string alias, string id, string fileName, MemoryStream? stream)
    {
        var db = manager.GetDatabase(alias, createIfMissing: false);
        if (db is null)
        {
            return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
        }

        var fs = db.GetStorage<string>(fileName, id);

        if (stream is null)
        {
            if (!File.Exists(fileName))
            {
                return EnumsLiteDbMemory.Output.PATH_NOT_FOUND;
            }

            using var memoryStreamFile = new MemoryStream(File.ReadAllBytes(fileName));
            fs.Upload(id, fileName, memoryStreamFile);
        }
        else
        {
            fs.Upload(id, fileName, stream);
        }

        db.Checkpoint();

        return EnumsLiteDbMemory.Output.SUCCESS;

    }

    /// <summary>
    /// Retrieves the reference of a stored file by using its identifier.
    /// </summary>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="id">Identifier of the storage container.</param>
    /// <param name="fileName">Identifier of the stored file.</param>
    /// <returns>The file information or <c>null</c> when it does not exist.</returns>
    /// <exception cref="Exception">Thrown when the database alias is not registered.</exception>
    public static LiteFileInfo<string>? Find(ConnectionManager manager, string alias, string id, string fileName)
    {
        var db = manager.GetDatabase(alias, createIfMissing: false);

        if (db is null)
        {
            throw new Exception($"The database {alias} is not registered in the connection");
        }

        var fs = db.GetStorage<string>(id, GetAliasFiles(id));
        LiteFileInfo<string>? output = fs.FindById(fileName);

        return output;

    }

    /// <summary>
    /// Builds the internal alias used to store the file chunks.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <returns>The generated alias for the chunks.</returns>
    private static string GetAliasFiles(string alias)
    {
        return $"{alias}chunk";
    }

}
