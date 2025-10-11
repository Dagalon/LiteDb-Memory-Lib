using System.Linq.Expressions;
using LiteDB;

namespace LiteDb_Memory_Lib;

public static class GeneralTools
{
    /// <summary>
    /// Creates an index on the specified collection by using a <see cref="BsonExpression"/>.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="expression">Expression that defines the index.</param>
    /// <param name="unique">Indicates whether the index must be unique.</param>
    /// <returns>A value that reports whether the index creation was successful.</returns>
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

    /// <summary>
    /// Deletes a document from a collection by using its identifier.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="idDocument">Identifier of the document to delete.</param>
    /// <returns>A value that reports whether the deletion succeeded.</returns>
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

    /// <summary>
    /// Updates a document in the specified collection.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="document">Document that will replace the existing record.</param>
    /// <returns>A value that reports whether the update succeeded.</returns>
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

    /// <summary>
    /// Updates multiple documents in the specified collection.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="documents">Documents that will replace the existing records.</param>
    /// <returns>A value that reports whether the update succeeded.</returns>
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

    /// <summary>
    /// Deletes the documents that match the provided LiteDB expression.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="qry">LiteDB expression that defines the documents to remove.</param>
    /// <returns>A value that reports whether the deletion succeeded.</returns>
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

    /// <summary>
    /// Deletes the documents that satisfy the supplied predicate.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="predicate">Predicate used to select documents for deletion.</param>
    /// <returns>A value that reports whether the deletion succeeded.</returns>
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

    /// <summary>
    /// Creates an index on the specified collection by using an expression tree.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <typeparam name="TOutput">Type produced by the indexed expression.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collectionName">Name of the target collection.</param>
    /// <param name="expression">Expression that defines the index.</param>
    /// <param name="unique">Indicates whether the index must be unique.</param>
    /// <returns>A value that reports whether the index creation was successful.</returns>
    public static EnumsLiteDbMemory.Output CreateIndex<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, TOutput>> expression, bool unique = false)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);

        if (collection == null)
        {
            return EnumsLiteDbMemory.Output.COLLECTION_NOT_FOUND;
        }

        collection.EnsureIndex(expression, unique);
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    /// <summary>
    /// Executes a LiteDB query and maps the results to a list of objects.
    /// </summary>
    /// <typeparam name="T">Type of the resulting objects.</typeparam>
    /// <param name="manager">Instance of <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="qry">Query string to execute.</param>
    /// <returns>A list with the results or <c>null</c> if execution fails.</returns>
    public static List<T>? Execute<T>(ConnectionManager manager, string alias, string qry)
    {
        var results = manager.GetDatabase(alias, createIfMissing: false)?.Execute(qry);
        return results != null ? BsonDataReaderToObject<T>(results) : null;
    }

    /// <summary>
    /// Converts a LiteDB data reader into a strongly typed list.
    /// </summary>
    /// <typeparam name="T">Type of the resulting objects.</typeparam>
    /// <param name="reader">Reader that contains the query results.</param>
    /// <returns>A list with the generated objects.</returns>
    public static List<T>? BsonDataReaderToObject<T>(IBsonDataReader reader)
    {
        var output = new List<T>();
        while (reader.Read())
        {
            var doc = (BsonDocument)reader.Current;
            output.Add(BsonMapper.Global.ToObject<T>(doc));
        }
        return output;
    }


}
