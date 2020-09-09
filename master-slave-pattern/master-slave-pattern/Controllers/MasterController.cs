using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using master_slave_pattern.Infrastructure.Models;
using master_slave_pattern.Serializer;
using master_slave_pattern.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace master_slave_pattern.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private Master _master;
        private readonly ProductServices _productService;
        public MasterController(Master master, ProductServices productService)
        {
            _master = master;
            _productService = productService;
        }
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Get()
        {
            //Product product = new Product { Name = "tes2t" };
            //_productService.Create(product);
            //return Ok();

            var request = new Request
            {
                ReqCustomer = new ReqCustomer
                {
                    Timeout = 3000,
                    Customers = new List<Serializer.Customer> { new Serializer.Customer { Name = "Nunui" } }
                },
                ReqProduct = new ReqProduct
                {
                    Timeout = 2000,
                    Products = new List<Serializer.Product> { new Serializer.Product { Name = "Tea" } }
                },
                Relations = new List<Serializer.Relation> { new Serializer.Relation { CustomerName = "Nunui", ProductName = "Tea" } }
            };

            return Ok(_master.Mannage(request));
        }

        public IActionResult Post()
        {
            return Ok();
        }
    }
}
