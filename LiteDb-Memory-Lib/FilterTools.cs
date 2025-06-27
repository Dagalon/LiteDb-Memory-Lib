using System.Linq.Expressions;
using LiteDB;

namespace LiteDb_Memory_Lib;

public static class FilterTools
{ 
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
         var collection = manager.GetCollection<T>(alias,collectionName);
         return collection is not null ? collection.FindOne(predicate) : default;
    }
    
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }
    
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Query qry)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }
    
    
    public static List<T>? Find<T>(ConnectionManager manager, string alias, string collectionName, Query qry)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Find(qry).ToList();
    }
    
    public static List<T>? Find<T>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Find(qry).ToList();
    }
    
    public static T? FindById<T>(ConnectionManager manager, string alias, string collectionName, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is null ? default : collection.FindById(id);
    }

}