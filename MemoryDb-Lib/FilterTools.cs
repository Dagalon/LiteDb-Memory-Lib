using System.Linq.Expressions;
using LiteDB;

namespace MemoryDb_Lib;

public static class FilterTools
{ 
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
         var collection = manager.GetCollection<T>(alias,collectionName);
         return collection is not null ? collection.FindOne(predicate) : default;
    }
    
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(predicate) : default;
    }
    
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }
    
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry, 
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(qry) : default;
    }
    
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Query qry)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }
    
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Query qry, 
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(qry) : default;
    }
    
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName, 
        Expression<Func<T, bool>> predicate, Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Include(refFunctional).Find(predicate).ToList();
    }
    
    public static List<T>? Find<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Find(predicate).ToList();
    }
    
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Query qry, 
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Include(refFunctional).Find(qry).ToList();
    }
    
    public static List<T>? Find<T,TOutput>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Include(refFunctional).Find(qry).ToList();
    }
    
    public static T? FindById<T>(ConnectionManager manager, string alias, string collectionName, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is null ? default : collection.FindById(id);
    }
    
    public static T? FindById<T,TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T,TOutput>> refFunctional, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection is null ? default : collection.Include(refFunctional).FindById(id);
    }
    
    public static List<T>? FindAll<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T,TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.Include(refFunctional).FindAll().ToList();
    }
    
    public static List<T>? FindAll<T>(ConnectionManager manager, string alias, string collectionName)
    {
        var collection = manager.GetCollection<T>(alias,collectionName);
        return collection?.FindAll().ToList();
    }
}