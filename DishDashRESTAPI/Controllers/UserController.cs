using DishDashRESTAPIDatabase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DishDashRESTAPIDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly DishDashDb _db;

        public UserController(DishDashDb db)
        {
            _db = db;
        }

        // GET: api/user/pendingAdmins
        // Returns a list of admin users that are not verified yet.
        [HttpGet("pendingAdmins")]
        public async Task<IActionResult> GetPendingAdmins()
        {
            var pendingAdmins = await _db.Users
                .Where(u => u.Role == "Admin" && u.IsVerified == false)
                .Select(u => new { u.Id, u.Username, u.Email })
                .ToListAsync();
            return Ok(pendingAdmins);
        }

        // PUT: api/user/approveAdmin/{id}
        // Approves a pending admin by setting IsVerified to true.
        [HttpPut("approveAdmin/{id}")]
        public async Task<IActionResult> ApproveAdmin(int id)
        {
            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Admin");
            if (admin == null)
            {
                return NotFound("Admin user not found.");
            }

            admin.IsVerified = true;
            await _db.SaveChangesAsync();
            return Ok("Admin approved successfully.");
        }

        // DELETE: api/user/rejectAdmin/{id}
        // Rejects (deletes) a pending admin registration.
        [HttpDelete("rejectAdmin/{id}")]
        public async Task<IActionResult> RejectAdmin(int id)
        {
            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Admin");
            if (admin == null)
            {
                return NotFound("Admin user not found.");
            }

            _db.Users.Remove(admin);
            await _db.SaveChangesAsync();
            return Ok("Admin registration rejected.");
        }
    }
}
