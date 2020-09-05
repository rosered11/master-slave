using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public MasterController(Master master)
        {
            _master = master;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_master.Mannage());
        }
    }
}
