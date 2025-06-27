using System.Linq.Expressions;
using LiteDb_Memory_Lib;
using LiteDB;

namespace LiteDb_Memory_Tests;

public struct Phone
{
    public int Prefix { get; set; }
    public long Number { get; set; } 
    
    public Phone(int p, int n)
    {
        Prefix = p;
        Number = n;
    }
    
    public long GetPhoneNumber()
    {
        return long.Parse($"00{Prefix}{Number}");
    }
}



public struct Customer(ObjectId customerId, string name, DateTime createdDate, List<Phone> phones, bool isActive)
{
    public ObjectId CustomerId { get; set; } = customerId;
    public string Name { get; set; } = name;
    public DateTime CreateDate { get; set; } = createdDate;
    public List<Phone> Phones { get; set; } = phones;
    public bool IsActive { get; set; } = isActive;
}

public class FindDocuments
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void T_Find_One()
    {
        const string idDataBase = "TEST_DB";
        const string idCollection = "CUSTOMERS";
        var manager = ConnectionManager.Instance();
        
        // Create database
        manager.CreateDatabase(idDataBase);
        
        // Create list customers 
        var customers = new List<Customer>()
        {
            new Customer()
            {
                CustomerId = ObjectId.NewObjectId(), CreateDate = new DateTime(2000, 1, 1),
                IsActive = true, Name = "David", Phones = [new Phone(34, 658566844)]
            },

            new Customer()
            {
                CustomerId = ObjectId.NewObjectId(), CreateDate = new DateTime(2004, 3, 27),
                IsActive = true, Name = "Martin", Phones = [new Phone(34, 658576842)]
            },

            new Customer()
            {
                CustomerId = ObjectId.NewObjectId(), CreateDate = new DateTime(2004, 3, 27),
                IsActive = true, Name = "Nerea", Phones = [new Phone(34, 645576842)]
            },
        };
        
        // Create collection
        manager.CreateCollection<Customer>(idDataBase, idCollection, customers);
        
        // Filter with predicate
        Expression<Func<Customer, bool>> filter = x => x.Name == "David";
        var resultWithPredicate = FilterTools.FindOne<Customer>(manager, idDataBase, idCollection, filter);
        
        // Filter with query
        var query = Query.EQ("name", "David");
        var resultWithQry = FilterTools.FindOne<Customer>(manager, idDataBase, idCollection, query);
        
        // Filter with query
        var bsonExpression = BsonExpression.Create("name='david'");
        var resultWithBsonDocument = FilterTools.FindOne<Customer>(manager, idDataBase, idCollection, bsonExpression);
        
        Assert.Multiple(() =>
        {
            Assert.That(resultWithPredicate.Name, Is.EqualTo("David"));
            Assert.That(resultWithQry.Name, Is.EqualTo("David"));
            Assert.That(resultWithBsonDocument.Name, Is.EqualTo("David"));
        });

    }
}