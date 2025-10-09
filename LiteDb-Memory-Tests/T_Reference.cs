using System.Linq.Expressions;
using LiteDb_Memory_Lib;
using LiteDB;

namespace LiteDb_Memory_Tests;

public struct Order
{
    public ObjectId OrderId { get; set; }
    
    [BsonRef("Customers")]
    public Customer Customer { get; set; }
}

public class CrossReference
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void T_CrossQuery()
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
        
        // Create customer collection
        manager.CreateCollection<Customer>(idDataBase, idCollection, customers);
        
        // Create order
        var order1 = new Order(){OrderId = ObjectId.NewObjectId(), Customer = customers[0]};
        var order2 = new Order(){OrderId = ObjectId.NewObjectId(), Customer = customers[1]};
        var orders = new List<Order>(){order1, order2};
        
        // Insert order with associated customer is not in CUSTOMER collection.
        var ghostCustomer = new Customer()
        {
            CustomerId = ObjectId.NewObjectId(), CreateDate = new DateTime(1980, 3, 15),
            IsActive = true, Name = "Ghost", Phones = [new Phone(34, 666666666)]
        };
        
        orders.Add(new Order(){OrderId = ObjectId.NewObjectId(), Customer = ghostCustomer});
        
        // Create order collection
        manager.CreateCollection<Order>(idDataBase, "ORDERS", orders);
        
        // Find element
        var collection = manager.GetCollection<Order>(idDataBase, "ORDERS");
        
        if (collection != null)
        {
            Expression<Func<Order, Customer>> refFunctional = x => x.Customer;
            
            var filteredOrder = FilterTools.FindById(manager, idDataBase, "ORDERS", 
                refFunctional, order1.OrderId);
            
            Assert.That(order1.Customer.CustomerId, Is.EqualTo(filteredOrder.Customer.CustomerId));
            
            var filteredGhostOrder =  FilterTools.FindById(manager, idDataBase, "ORDERS", 
                refFunctional, orders.Last().OrderId);
            
            Assert.That(filteredGhostOrder.Customer.CustomerId is null);
        }
    }

}