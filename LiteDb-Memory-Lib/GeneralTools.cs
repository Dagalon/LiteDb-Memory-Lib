using System.Linq.Expressions;
using LiteDB;

namespace LiteDb_Memory_Lib;

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
}