using IdentityDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    public class LocationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Shop_Location([FromBody] LocationModel location)
        {
            if (location != null)
            {
                // Save location data to the database or process it as needed
                // Example: _context.Locations.Add(location);
                // _context.SaveChanges();

                return Ok();
            }

            return BadRequest();
        }
        public IActionResult DeviceLocation() => View();

        
    }
}
