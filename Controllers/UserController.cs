using IEEE.Data;
using IEEE.DTO.UserDTO;
using IEEE.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IEEE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;


        public UsersController(Microsoft.AspNetCore.Identity.UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

        }

        // GET: api/Users/GetAllUsers
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.Include(u => u.Committees).ToListAsync();

            var userdto =  new List <GetUsersDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                var dto = new GetUsersDto
                {
                    UserName = user.UserName,
                    Eamil = user.Email,
                    IsActive = user.IsActive,
                    CommitteeNames = user.Committees.Select(c => c.Name).ToList(),
                    Role = role
                };

                userdto.Add(dto);
            }

            return Ok(userdto);
        }


        // POST: api/Users/CreateUser
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(createuserdto dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                FName = dto.FName,
                MName = dto.FName,
                LName = dto.FName,
                Year = dto.Year,
                Sex = dto.Sex,
                Goverment = dto.Goverment,
                Phone = dto.Phone,
                Password = dto.Password,
                Email = dto.Email,
                Faculty = dto.Faculty,
                City    =dto.City,
                IsActive = false ,
                CommitteeId= dto.CommitteeId,
                

            };

            var result = await _userManager.CreateAsync(user, dto.Password);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!string.IsNullOrEmpty(dto.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }
            }

            return Ok(new { message = "User created successfully", userId = user.Id });
        }

        // PUT: api/Users/EditUser/{id}
        [HttpPut("EditUser/{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] EditUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.FName = dto.FName;
            user.MName = dto.MName;
            user.LName = dto.LName;
            user.Sex  = dto.Sex;
            user.Phone = dto.Phone;
            user.Goverment = dto.Goverment;
            user.Year = dto.Year; 
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.Faculty = dto.Faculty;
            user.Password = dto.Password;
            user.City = dto.City;

            // 1. Load the selected committees from DB
            var selectedCommittees = await _context.Committees
                .Where(c => dto.CommitteeIds.Contains(c.Id))
                .ToListAsync();

            // 2. Clear current user committees
            user.Committees.Clear();

            // 3. Add the new selected committees
            foreach (var committee in selectedCommittees)
            {
                user.Committees.Add(committee);
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles); // نحذف الرول القديمة
            await _userManager.AddToRoleAsync(user, dto.RoleName); // نضيف الرول الجديدة
            


            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok("User updated successfully");

            return BadRequest(result.Errors);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }

        // DELETE: api/Users/DeleteUser/{id}
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok("User deleted successfully");

            return BadRequest(result.Errors);
        }

    }
}

