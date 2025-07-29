using IEEE.Data;
using IEEE.DTO.UserDTO;
using IEEE.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IEEE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly AppDbContext _context;
        private readonly UserManager<User> userManager;
        private readonly IConfiguration config;

        public AccountController(UserManager<User> UserManager , IConfiguration config)
        {
            userManager = UserManager;
            this.config = config;
        }



        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto UserFromRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);



            User user = new User
            {
                UserName = UserFromRequest.UserName,
                FName = UserFromRequest.FName,
                MName = UserFromRequest.MName,
                LName = UserFromRequest.LName,
                Faculty = UserFromRequest.Faculty,
                Email = UserFromRequest.Email,
                City = UserFromRequest.City,
                Phone = UserFromRequest.Phone,
                Sex = UserFromRequest.Sex,
                Goverment = UserFromRequest.Goverment,
                Year = UserFromRequest.Year,
                IsActive = false
            };

            // حفظ المستخدم بالباسورد
            IdentityResult result = await userManager.CreateAsync(user, UserFromRequest.Password);

            if (result.Succeeded)
            {
                //  إضافة الرول
                if (!string.IsNullOrEmpty(UserFromRequest.RoleName))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(UserFromRequest.RoleName);
                    if (!roleExists)
                        return BadRequest("Invalid role");

                    await userManager.AddToRoleAsync(user, UserFromRequest.RoleName);
                }

                // إضافة الكوميتيز
                if (UserFromRequest.CommitteeIds != null && UserFromRequest.CommitteeIds.Any())
                {
                    var committees = await _context.Committees
                        .Where(c => UserFromRequest.CommitteeIds.Contains(c.Id))
                        .ToListAsync();

                    user.Committees = committees;

                    _context.Users.Update(user); 
                    await _context.SaveChangesAsync();
                }

                return Ok("User created successfully");
            }

            // في حالة وجود أخطاء
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("Password", item.Description);
            }

            return BadRequest(ModelState);
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            if (ModelState.IsValid)
            {
                User userfromdb = await userManager.FindByNameAsync(userFromRequest.UserName);

                if (userfromdb != null)
                {
                    bool found = await userManager.CheckPasswordAsync(userfromdb, userFromRequest.Password);
                    if (found)
                    {
                        //  Check if user is active
                        if (!userfromdb.IsActive)
                        {
                            return Unauthorized(new { message = "Your account is not activated yet." });
                        }

                        List<Claim> UserClaim = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userfromdb.Id.ToString()),
                    new Claim(ClaimTypes.Name, userfromdb.UserName)
                };

                        var UserRoles = await userManager.GetRolesAsync(userfromdb);
                        foreach (var roleName in UserRoles)
                        {
                            UserClaim.Add(new Claim(ClaimTypes.Role, roleName));
                        }

                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: config["Jwt:IssuerIP"] , 
                            audience: config["Jwt:AudienceIP"],
                            expires: DateTime.Now.AddHours(1),
                            claims: UserClaim,
                            signingCredentials: new SigningCredentials(
                                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("KiraSuperUltraMegaSecretKey!1234567890")),
                                SecurityAlgorithms.HmacSha256
                            )
                        );

                        var tokenString = new JwtSecurityTokenHandler().WriteToken(mytoken);

                        return Ok(new
                        {
                            token = tokenString
                        });
                    }
                }

                ModelState.AddModelError("Username", "Username OR Password Invalid");
                return Unauthorized(ModelState);
            }

            return BadRequest(ModelState);
        }


    }
}
