using LiteDB;

namespace LiteDb_Memory_Lib;

public static class FileStorageTools
{
    public static EnumsLiteDbMemory.Output Upload(ConnectionManager manager, string alias, string id, string fileName, string pathFile)
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
        
        var fs = db.GetStorage<string>(id, GetAliasFiles(id));

        using var stream = new FileStream(pathFile, FileMode.Open, FileAccess.Read);
        fs.Upload(fileName, pathFile, stream);

        db.Checkpoint();

        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public static EnumsLiteDbMemory.Output Upload(ConnectionManager manager, string alias, string id, string fileName, MemoryStream? stream)
    {
        var db = manager.GetDatabase(alias);
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

    public static LiteFileInfo<string>? Find(ConnectionManager manager, string alias, string id, string fileName)
    {
        var db = manager.GetDatabase(alias);

        if (db is null)
        {
            throw new Exception($"The database {alias} is not registered in the connection");
        }

        var fs = db.GetStorage<string>(id, GetAliasFiles(id));
        LiteFileInfo<string>? output = fs.FindById(fileName);

        return output;

    }

    private static string GetAliasFiles(string alias)
    {
        return $"{alias}chunk";
    }

}