using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace master_slave_pattern.Infrastructure.Models
{
    public class ProductServices : GenericServce<Product>
    {   
        public ProductServices() : base ("Products")
        {
        }

        public override Product Get(string name)
        {
            return _context.Find(item => item.Name == name).FirstOrDefault();
        }
    }

    public class CustomerServices : GenericServce<Customer>
    {
        public CustomerServices() : base("Customers")
        {
        }
        public override Customer Get(string name)
        {
            return _context.Find(item => item.Name == name).FirstOrDefault();
        }
    }

    public class RelationServices : GenericServce<Relation>
    {
        public RelationServices() : base("Relations")
        {
        }

        public IEnumerable<Relation> GetByProductId(string productId)
        {
            return _context.Find(item => item.ProductId == productId).ToList();
        }
    }

    public class LogServices : GenericServce<Logs>
    {
        public LogServices() : base("Logs")
        {
        }
    }

    public class GenericServce<T> where T : BaseModel
    {
        protected readonly IMongoCollection<T> _context;
        protected readonly MongoClient _client;
        protected readonly IMongoDatabase _database;

        public GenericServce(string collectionName)
        {
            _client = new MongoClient("mongodb://root:root@localhost:30500");
            _database = _client.GetDatabase("master-slave");
            _context = _database.GetCollection<T>(collectionName);
        }

        public virtual List<T> Get() =>
           _context.Find(item => true).ToList();

        public virtual T Get(string id)
        {
            var filterId = Builders<T>.Filter.Eq("_id", id);
            return _context.Find(filterId).FirstOrDefault();
        }

        public T Create(T item)
        {
            _context.InsertOne(item);
            return item;
        }

        public void Update(string id, T item)
        {
            var filterId = Builders<T>.Filter.Eq("_id", id);
            _context.ReplaceOne(filterId, item);
        }

        public void Remove(T item)
        {
            var filterId = Builders<T>.Filter.Eq("_id", item.Id);
            _context.DeleteOne(filterId);
        }

        public void Remove(string id) =>
            _context.DeleteOne(item => item.Id == id);
    }
}
