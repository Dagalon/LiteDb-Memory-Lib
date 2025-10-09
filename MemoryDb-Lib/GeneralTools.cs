using System.Linq.Expressions;
using LiteDB;

namespace MemoryDb_Lib;

public static class GeneralTools
{
    public static EnumsLiteDbMemory.Output CreateIndex<T>(ConnectionManager manager, string alias, string collectionName, 
        BsonExpression expression, bool unique = false)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);

        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }
        
        collection.EnsureIndex(expression, unique);
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public static EnumsLiteDbMemory.Output Delete<T>(ConnectionManager manager, string alias, string collectionName, string idDocument)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }

        collection.Delete(idDocument);
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public static EnumsLiteDbMemory.Output Update<T>(ConnectionManager manager, string alias, string collectionName, T document)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }

        collection.Update(document);
        return EnumsLiteDbMemory.Output.SUCCESS;

    }

    public static EnumsLiteDbMemory.Output UpdateMany<T>(ConnectionManager manager, string alias, string collectionName, List<T> documents)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }

        collection.Update(documents);
        return EnumsLiteDbMemory.Output.SUCCESS;

    }

    public static EnumsLiteDbMemory.Output DeleteMany<T>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);

        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }

        collection.DeleteMany(qry);
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public static EnumsLiteDbMemory.Output DeleteMany<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);

        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }

        collection.DeleteMany(predicate);
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public static EnumsLiteDbMemory.Output CreateIndex<T, TOutput>(ConnectionManager manager, string alias, string collectionName, 
        Expression<Func<T,TOutput>> expression, bool unique = false)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);

        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }
        
        collection.EnsureIndex(expression, unique);
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public static List<T>? Execute<T>(ConnectionManager manager, string alias,  string qry)
    {
        var results = manager.GetDatabase(alias, createIfMissing: false)?.Execute(qry);
        return results != null ? BsonDataReaderToObject<T>(results) : null;
    }
    
    public static List<T>? BsonDataReaderToObject<T>(IBsonDataReader reader) 
    {
        var output  = new List<T>();
        while (reader.Read())
        {
            var doc = (BsonDocument)reader.Current;
            output.Add(BsonMapper.Global.ToObject<T>(doc));
        }
        return output;
    }

    
}