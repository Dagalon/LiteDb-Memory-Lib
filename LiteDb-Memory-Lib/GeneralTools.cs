using System.Linq.Expressions;
using LiteDB;

namespace LiteDb_Memory_Lib;

public static class GeneralTools
{
    /// <summary>
    /// Crea un índice en la colección indicada utilizando una <see cref="BsonExpression"/>.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="expression">Expresión que define el índice.</param>
    /// <param name="unique">Indica si el índice debe ser único.</param>
    /// <returns>Un valor que indica si la creación del índice fue exitosa.</returns>
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
    /// Elimina un documento de una colección utilizando su identificador.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="idDocument">Identificador del documento a eliminar.</param>
    /// <returns>Un valor que indica si la eliminación fue exitosa.</returns>
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
    /// Actualiza un documento en la colección especificada.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="document">Documento que reemplazará al existente.</param>
    /// <returns>Un valor que indica si la actualización fue exitosa.</returns>
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
    /// Actualiza múltiples documentos en la colección especificada.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="documents">Documentos que reemplazarán a los existentes.</param>
    /// <returns>Un valor que indica si la actualización fue exitosa.</returns>
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
    /// Elimina los documentos que cumplan con la expresión de LiteDB indicada.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Expresión de LiteDB que determina los documentos a eliminar.</param>
    /// <returns>Un valor que indica si la eliminación fue exitosa.</returns>
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
    /// Elimina los documentos que cumplen con el predicado indicado.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="predicate">Predicado utilizado para seleccionar los documentos a eliminar.</param>
    /// <returns>Un valor que indica si la eliminación fue exitosa.</returns>
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
    /// Crea un índice sobre la colección indicada utilizando un árbol de expresiones.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión indexada.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="expression">Expresión que define el índice.</param>
    /// <param name="unique">Indica si el índice debe ser único.</param>
    /// <returns>Un valor que indica si la creación del índice fue exitosa.</returns>
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
    /// Ejecuta una consulta de LiteDB y mapea los resultados a una lista de objetos.
    /// </summary>
    /// <typeparam name="T">Tipo de los objetos resultantes.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="qry">Cadena de consulta a ejecutar.</param>
    /// <returns>Una lista con los resultados o <c>null</c> si la ejecución falla.</returns>
    public static List<T>? Execute<T>(ConnectionManager manager, string alias, string qry)
    {
        var results = manager.GetDatabase(alias, createIfMissing: false)?.Execute(qry);
        return results != null ? BsonDataReaderToObject<T>(results) : null;
    }

    /// <summary>
    /// Convierte un lector de datos de LiteDB en una lista fuertemente tipada.
    /// </summary>
    /// <typeparam name="T">Tipo de los objetos resultantes.</typeparam>
    /// <param name="reader">Lector que contiene los resultados de la consulta.</param>
    /// <returns>Una lista con los objetos generados.</returns>
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