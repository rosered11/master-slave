using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace master_slave_pattern.Serializer
{
    #region Request
    public class Request
    {
        public ReqProduct ReqProduct { get; set; }
        public ReqCustomer ReqCustomer { get; set; }
        public IEnumerable<Relation> Relations { get; set; }
    }

    public class ReqProduct
    {
        public int Timeout { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
    public class Product
    {
        public string Name { get; set; }
    }

    public class ReqCustomer
    {
        public int Timeout { get; set; }
        public IEnumerable<Customer> Customers { get; set; }
    }
    public class Customer
    {
        public string Name { get; set; }
    }

    public class Relation
    {
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
    }

    #endregion
    public class Response
    {
        public string Message { get; set; }
        public IEnumerable<string> IsFailed { get; set; }
    }
}
