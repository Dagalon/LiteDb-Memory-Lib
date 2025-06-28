﻿using LiteDb_Memory_Lib;
using LiteDB;

namespace LiteDb_Memory_Tests;


public class InsertDocument
{
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void T_Insert_One_BsonDocument()
    {
        var customer = new BsonDocument
        {
            ["_id"] = ObjectId.NewObjectId(),
            ["Name"] = "David",
            ["CreateDate"] = DateTime.Now,
            ["Phones"] = new BsonArray { "657488951" },
            ["IsActive"] = true,
            ["IsAdmin"] = new BsonValue(true),
            ["Address"] = new BsonDocument
            {
                ["Street"] = "Calle Lucifer",
                ["City"] = "Comunity",
                ["State"] = "Land",
                ["ZipCode"] = "258525"
            }
        };

        // Instance manager connections
        const string aliasDb = "Address";
        const string collection = "Customers";
        
        var manager = ConnectionManager.Instance();
        
        // Create shared database f
        manager.CreateDatabase(aliasDb);
        
        // Create collection
        manager.CreateCollection<BsonDocument>(aliasDb,collection, [customer]);
        
        // Get collection names
        var collectionNames = manager.GetCollectionNames(aliasDb);
        Assert.That(collectionNames[0], Is.EqualTo(collection));
    }

}