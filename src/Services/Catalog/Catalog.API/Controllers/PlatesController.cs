using Catalog.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PlatesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int pageNumber, int pageSize = 20)
        {
            var plates = await _db.Plates.ToPagedListAsync(pageNumber, pageSize);
            return Ok(plates);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Plate plate)
        {
            //In reality, we should not use the plate model here.
            //I have used it here for speed and convienience.
            //Validation?
            _db.Plates.Add(plate);
            await _db.SaveChangesAsync();

            return Created($"/api/plates", plate);
        }
    }
}
