using System.Linq.Expressions;
using LiteDB;

namespace LiteDb_Memory_Lib;

public static class FilterTools
{
    /// <summary>
    /// Recupera el primer documento que cumple con el predicado indicado.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="predicate">Predicado utilizado para filtrar los documentos.</param>
    /// <returns>El primer documento encontrado o <c>null</c> si no existe coincidencia.</returns>
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
         var collection = manager.GetCollection<T>(alias, collectionName);
         return collection is not null ? collection.FindOne(predicate) : default;
    }

    /// <summary>
    /// Recupera el primer documento que cumple con el predicado incluyendo los datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="predicate">Predicado utilizado para filtrar los documentos.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>El primer documento encontrado o <c>null</c> si no existe coincidencia.</returns>
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(predicate) : default;
    }

    /// <summary>
    /// Recupera el primer documento que cumple con la expresión de LiteDB indicada.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Expresión de LiteDB que define el filtro.</param>
    /// <returns>El primer documento encontrado o <c>null</c> si no existe coincidencia.</returns>
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }

    /// <summary>
    /// Recupera el primer documento que cumple con la expresión de LiteDB incluyendo datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Expresión de LiteDB que define el filtro.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>El primer documento encontrado o <c>null</c> si no existe coincidencia.</returns>
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(qry) : default;
    }

    /// <summary>
    /// Recupera el primer documento que cumple con la consulta indicada.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Consulta de LiteDB utilizada para filtrar los documentos.</param>
    /// <returns>El primer documento encontrado o <c>null</c> si no existe coincidencia.</returns>
    public static T? FindOne<T>(ConnectionManager manager, string alias, string collectionName, Query qry)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.FindOne(qry) : default;
    }

    /// <summary>
    /// Recupera el primer documento que cumple con la consulta incluyendo datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Consulta de LiteDB utilizada para filtrar los documentos.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>El primer documento encontrado o <c>null</c> si no existe coincidencia.</returns>
    public static T? FindOne<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Query qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is not null ? collection.Include(refFunctional).FindOne(qry) : default;
    }

    /// <summary>
    /// Recupera todos los documentos que cumplen con el predicado incluyendo datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="predicate">Predicado utilizado para filtrar los documentos.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>Una lista con los documentos coincidentes o <c>null</c> si no existe la colección.</returns>
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, bool>> predicate, Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).Find(predicate).ToList();
    }

    /// <summary>
    /// Recupera todos los documentos que cumplen con el predicado indicado.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="predicate">Predicado utilizado para filtrar los documentos.</param>
    /// <returns>Una lista con los documentos coincidentes o <c>null</c> si no existe la colección.</returns>
    public static List<T>? Find<T>(ConnectionManager manager, string alias, string collectionName, Expression<Func<T, bool>> predicate)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Find(predicate).ToList();
    }

    /// <summary>
    /// Recupera todos los documentos que cumplen con la consulta indicada incluyendo datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Consulta de LiteDB utilizada para filtrar los documentos.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>Una lista con los documentos coincidentes o <c>null</c> si no existe la colección.</returns>
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName, Query qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).Find(qry).ToList();
    }

    /// <summary>
    /// Recupera todos los documentos que cumplen con la expresión indicada incluyendo datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="qry">Expresión de LiteDB utilizada para filtrar los documentos.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>Una lista con los documentos coincidentes o <c>null</c> si no existe la colección.</returns>
    public static List<T>? Find<T, TOutput>(ConnectionManager manager, string alias, string collectionName, BsonExpression qry,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).Find(qry).ToList();
    }

    /// <summary>
    /// Recupera un documento utilizando su identificador.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="id">Identificador del documento.</param>
    /// <returns>El documento encontrado o <c>null</c> si no existe.</returns>
    public static T? FindById<T>(ConnectionManager manager, string alias, string collectionName, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is null ? default : collection.FindById(id);
    }

    /// <summary>
    /// Recupera un documento mediante su identificador incluyendo los datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <param name="id">Identificador del documento.</param>
    /// <returns>El documento encontrado o <c>null</c> si no existe.</returns>
    public static T? FindById<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, TOutput>> refFunctional, BsonValue id)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection is null ? default : collection.Include(refFunctional).FindById(id);
    }

    /// <summary>
    /// Recupera todos los documentos de la colección incluyendo los datos referenciados.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <typeparam name="TOutput">Tipo producido por la expresión de referencia.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <param name="refFunctional">Expresión que describe la relación referenciada a incluir.</param>
    /// <returns>Una lista con todos los documentos o <c>null</c> si no existe la colección.</returns>
    public static List<T>? FindAll<T, TOutput>(ConnectionManager manager, string alias, string collectionName,
        Expression<Func<T, TOutput>> refFunctional)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.Include(refFunctional).FindAll().ToList();
    }

    /// <summary>
    /// Recupera todos los documentos de la colección.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="manager">Instancia de <see cref="ConnectionManager"/>.</param>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collectionName">Nombre de la colección objetivo.</param>
    /// <returns>Una lista con todos los documentos o <c>null</c> si no existe la colección.</returns>
    public static List<T>? FindAll<T>(ConnectionManager manager, string alias, string collectionName)
    {
        var collection = manager.GetCollection<T>(alias, collectionName);
        return collection?.FindAll().ToList();
    }
}