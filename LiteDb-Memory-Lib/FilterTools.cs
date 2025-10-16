using System.Linq.Expressions;
using LiteDB;

namespace LiteDb_Memory_Lib;

/// <summary>
/// Query helpers that simplify retrieving documents from LiteDB collections.
/// </summary>
public static class FilterTools
{
    /// <summary>
    /// Retrieves the first document that satisfies the provided predicate.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="predicate">Predicate used to filter documents.</param>
    /// <returns>The first matching document or <c>null</c> when none exists.</returns>
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.FindOne(predicate) : default;
    }

    /// <summary>
    /// Retrieves the first document that satisfies the predicate including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="predicate">Predicate used to filter documents.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>The first matching document or <c>null</c> when none exists.</returns>
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(predicate) : default;
    }

    /// <summary>
    /// Retrieves the first document that satisfies the supplied LiteDB expression.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB expression that defines the filter.</param>
    /// <returns>The first matching document or <c>null</c> when none exists.</returns>
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }

    /// <summary>
    /// Retrieves the first document that satisfies the LiteDB expression including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB expression that defines the filter.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>The first matching document or <c>null</c> when none exists.</returns>
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(qry) : default;
    }

    /// <summary>
    /// Retrieves the first document that satisfies the provided query.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB query used to filter documents.</param>
    /// <returns>The first matching document or <c>null</c> when none exists.</returns>
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Query qry)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }

    /// <summary>
    /// Retrieves the first document that satisfies the query including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB query used to filter documents.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>The first matching document or <c>null</c> when none exists.</returns>
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Query qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(qry) : default;
    }

    /// <summary>
    /// Retrieves all documents that satisfy the predicate including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="predicate">Predicate used to filter documents.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>A list with the matching documents or <c>null</c> when the collection does not exist.</returns>
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, bool>> predicate, Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).Find(predicate).ToList();
    }

    /// <summary>
    /// Retrieves all documents that satisfy the provided predicate.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="predicate">Predicate used to filter documents.</param>
    /// <returns>A list with the matching documents or <c>null</c> when the collection does not exist.</returns>
    public static List<T>? Find<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Find(predicate).ToList();
    }

    /// <summary>
    /// Retrieves all documents that satisfy the provided query including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB query used to filter documents.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>A list with the matching documents or <c>null</c> when the collection does not exist.</returns>
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Query qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).Find(qry).ToList();
    }

    /// <summary>
    /// Retrieves all documents that satisfy the provided expression including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB expression used to filter documents.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>A list with the matching documents or <c>null</c> when the collection does not exist.</returns>
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).Find(qry).ToList();
    }

    /// <summary>
    /// Retrieves a document by using its identifier.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="id">Identifier of the document.</param>
    /// <returns>The found document or <c>null</c> when it does not exist.</returns>
    public static T? FindById<T>(ConnectionManager manager, string alias, string collectionName, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is null ? default : collection.FindById(id);
    }

    /// <summary>
    /// Retrieves a document by its identifier including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <param name="id">Identifier of the document.</param>
    /// <returns>The found document or <c>null</c> when it does not exist.</returns>
    public static T? FindById<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, TOutput>> refFunctional, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is null ? default : collection.Include(refFunctional).FindById(id);
    }

    /// <summary>
    /// Retrieves all documents in the collection including referenced data.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the referenced expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="refFunctional">Expression that describes the referenced relationship to include.</param>
    /// <returns>A list with all documents or <c>null</c> when the collection does not exist.</returns>
    public static List<T>? FindAll<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).FindAll().ToList();
    }

    /// <summary>
    /// Retrieves all documents in the collection.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <returns>A list with all documents or <c>null</c> when the collection does not exist.</returns>
    public static List<T>? FindAll<T>(ConnectionManager manager, string alias, string collectionName)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.FindAll().ToList();
    }
}
