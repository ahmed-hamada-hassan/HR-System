using IEEE.Data;
using IEEE.DTO.UserDTO;
using IEEE.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IEEE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;

        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(AppDbContext appDbContext,UserManager<User> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = appDbContext;
        }


      // [Authorize(Roles = "High Board,Head,Vice,Hr")]

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Include(u => u.Committees)
                .ToListAsync();

            var userdto = new List<GetUsersDto>();

            foreach (var user in users)
            {
                var roleName = await _context.Roles
                    .Where(r => r.Id == user.RoleId)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();

                var dto = new GetUsersDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.Email,
                    FName = user.FName,
                    MName = user.MName,
                    LName = user.LName,
                    Sex = user.Sex,
                    PhoneNumber = user.PhoneNumber,
                    Goverment = user.Goverment,
                    Faculty = user.Faculty,
                    Year = user.Year,
                    IsActive = user.IsActive,
                    RoleId = user.RoleId,
                    CommitteesId = user.Committees.Select(c => c.Id).ToList(),
                };

                userdto.Add(dto);
            }

            return Ok(userdto);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userManager.Users
                .Include(u => u.Committees)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) 
            { 
                return NotFound(new { message = "User not found" });
            }

            var dto = new GetUsersDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.Email,
                FName = user.FName,
                MName = user.MName,
                LName = user.LName,
                Faculty = user.Faculty,
                Year = user.Year,
                Sex = user.Sex,
                PhoneNumber = user.PhoneNumber,
                Goverment = user.Goverment,
                IsActive = user.IsActive,
                RoleId = user.RoleId,
                CommitteesId = user.Committees.Select(c => c.Id).ToList() ,
           
            }; 
            
            return Ok(dto); 
        
        }

        [Authorize(Roles = "High Board,Head,Vice,Hr")]

        // POST: api/Users/CreateUser
        [HttpPost]
        public async Task<IActionResult> CreateUser(createuserdto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                FName = dto.FirstName,
                MName = dto.MiddleName,
                LName = dto.LastName,
                Year = dto.Year,
                Sex = dto.Sex,
                Goverment = dto.Goverment,
                PhoneNumber = dto.Phone,
                Email = dto.Email,
                Faculty = dto.Faculty,
                IsActive = dto.IsActive,
                RoleId = dto.RoleId
            };

            // تحقق من الـ Role
            var roleExists = await _context.Roles
                .AnyAsync(r => r.Id == dto.RoleId);

            if (!roleExists)
            {
                return BadRequest($"Role with ID {dto.RoleId} does not exist in AspNetRoles.");
            }

            // تحقق من البريد
            var userExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (userExists)
            {
                return BadRequest("Email already exists.");
            }

            // إنشاء المستخدم بكلمة السر
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (dto.CommitteeIds != null && dto.CommitteeIds.Any())
            {
                var committees = await _context.Committees
                    .Where(c => dto.CommitteeIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var committee in committees)
                {
                    user.Committees.Add(committee);
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "User created successfully", userId = user.Id });
        }



        // PUT: api/Users/EditUser/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] EditUserDto dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);


            if (user == null) return NotFound("User not found");


            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.FName = dto.FName;
            user.MName = dto.MName;
            user.LName = dto.LName;
            user.Sex = dto.Sex;
            user.PhoneNumber = dto.PhoneNumber;
            user.Goverment = dto.Goverment;
            user.Year = dto.Year;
            user.Faculty = dto.Faculty;


            
            
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) {

                var updatedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

                var response = new {
                    updatedUser.Id,
                    updatedUser.Email,
                    updatedUser.UserName,
                    updatedUser.FName,
                    updatedUser.MName,
                    updatedUser.LName,
                    updatedUser.Sex,
                    updatedUser.PhoneNumber,
                    updatedUser.Goverment,
                    updatedUser.Faculty,
                    updatedUser.Year,
                    updatedUser.IsActive,
                };
            
               
                return Ok(response); 
            
            } 
            
            return BadRequest(result.Errors);
        
        
        }




        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles =await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }



        [Authorize(Roles = "High Board,Head,Vice,HR")]
        // DELETE: api/Users/User/{id}
        [HttpDelete("{id}")]
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

