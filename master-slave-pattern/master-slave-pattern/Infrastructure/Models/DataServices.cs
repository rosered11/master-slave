using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace master_slave_pattern.Infrastructure.Models
{
    public class ProductServices
    {
        private readonly IMongoCollection<Product> _products;

        public ProductServices()
        {
            var client = new MongoClient("mongodb://root:root@localhost:30500");
            var database = client.GetDatabase("master-slave");
            _products = database.GetCollection<Product>("Products");
        }

        public List<Product> Get() =>
           _products.Find(book => true).ToList();

        public Product Get(string id) =>
            _products.Find<Product>(book => book.Id == id).FirstOrDefault();

        public Product Create(Product book)
        {
            _products.InsertOne(book);
            return book;
        }

        public void Update(string id, Product bookIn) =>
            _products.ReplaceOne(book => book.Id == id, bookIn);

        public void Remove(Product bookIn) =>
            _products.DeleteOne(book => book.Id == bookIn.Id);

        public void Remove(string id) =>
            _products.DeleteOne(book => book.Id == id);
    }
}
