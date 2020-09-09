using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace master_slave_pattern.Infrastructure.Models
{
    public class Product : BaseModel
    {
        public string Name { get; set; }
    }
    public class Customer : BaseModel
    {
        public string Name { get; set; }
    }

    public class Relation : BaseModel
    {
        public string ProductId { get; set; }
        public string CustomerId { get; set; }
    }

    public class Logs : BaseModel
    {
        public string Message { get; set; }
        public string DateTime { get; set; }
        public Logs(string message)
        {
            Message = message;
            DateTime = System.DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd HH:mm:ss");
        }
    }

    public class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}
