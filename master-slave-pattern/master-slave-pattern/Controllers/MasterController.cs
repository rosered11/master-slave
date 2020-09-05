using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using master_slave_pattern.Infrastructure.Models;
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
            Product product = new Product { Name = "tes2t" };
            _productService.Create(product);
            return Ok();
            //return Ok(_master.Mannage());
        }

        public IActionResult Post()
        {
            Product product = new Product { Name = "test" };
            _productService.Create(product);

            //return CreatedAtRoute("GetBook", new { id = book.Id.ToString() }, book);
            return Ok();
        }
    }
}
