namespace LiteDb_Memory_Lib;

public static class FileStorageTools
{
    public static EnumsLiteDbMemory.Output Upload(ConnectionManager manager, string alias, string idFile,
        string pathFile)
    {
        if (!File.Exists(pathFile))
        {
            return EnumsLiteDbMemory.Output.PATH_NOT_FOUND;
        }
        
        var db = manager.GetDatabase(alias);

        if (db is null)
        {
            return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
        }
        
        var fs = db.FileStorage;
        fs.Upload(idFile, pathFile);

        return EnumsLiteDbMemory.Output.SUCCESS;
    }

}