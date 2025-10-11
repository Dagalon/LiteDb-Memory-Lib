using LiteDB;

namespace LiteDb_Memory_Lib;

public static class FileStorageTools
{
    /// <summary>
    /// Carga un archivo desde disco al almacenamiento de ficheros de LiteDB para el alias indicado.
    /// </summary>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="id">Identificador del contenedor de almacenamiento.</param>
    /// <param name="fileName">Nombre que tendrá el archivo en el almacenamiento.</param>
    /// <param name="pathFile">Ruta del archivo en disco.</param>
    /// <returns>Un valor que indica si la carga se realizó correctamente.</returns>
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
    /// Carga un archivo representado por un flujo de memoria en el almacenamiento de LiteDB.
    /// </summary>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="id">Identificador del contenedor de almacenamiento.</param>
    /// <param name="fileName">Nombre que tendrá el archivo en el almacenamiento.</param>
    /// <param name="stream">Flujo opcional con el contenido del archivo.</param>
    /// <returns>Un valor que indica si la carga se realizó correctamente.</returns>
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
    /// Obtiene la referencia de un archivo almacenado mediante su identificador.
    /// </summary>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="id">Identificador del contenedor de almacenamiento.</param>
    /// <param name="fileName">Identificador del archivo almacenado.</param>
    /// <returns>La información del archivo o <c>null</c> si no existe.</returns>
    /// <exception cref="Exception">Se lanza cuando el alias de base de datos no está registrado.</exception>
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
    /// Construye el alias interno utilizado para almacenar los fragmentos del archivo.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <returns>El alias generado para los fragmentos.</returns>
    private static string GetAliasFiles(string alias)
    {
        return $"{alias}chunk";
    }

}