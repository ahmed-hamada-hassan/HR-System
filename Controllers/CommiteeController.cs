using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IEEE.Data;
using IEEE.Entities;
using IEEE.DTO.CommitteeDto;
using IEEE.DTO.MeetingsDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;

namespace IEEE.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class CommitteesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;



        public CommitteesController(AppDbContext context, IWebHostEnvironment env , Microsoft.AspNetCore.Identity.UserManager<User> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommitteeGetDto>>> GetCommittees()
        {
            var committees = await _context.Committees
                .Include(c => c.Users)
                .Include(c => c.Vices)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}"; 

            var committeesDto = committees.Select(c => new CommitteeGetDto
            {
                Id = c.Id,
                Name = c.Name,
                HeadId = c.HeadId ?? 0,
                MemberCount = _context.Users.Count(u => u.CommitteeId == c.Id),
                VicesId = c.Vices.Select(v => v.Id).ToList(),
                ImageUrl = string.IsNullOrEmpty(c.ImageUrl) ? null : $"{baseUrl}{c.ImageUrl}", // 👈 رجع اللينك كامل
                Description = c.Description
            });

            return Ok(committeesDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommitteeGetDto>> GetCommittee(int id)
        {
            var committee = await _context.Committees
                .Include(c => c.Users)
                .Include(c => c.Vices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (committee == null)
                return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var committeeDto = new CommitteeGetDto
            {
                Id = committee.Id,
                Name = committee.Name,
                HeadId = committee.HeadId ?? 0,
                MemberCount = _context.Users.Count(u => u.CommitteeId == committee.Id),
                VicesId = committee.Vices.Select(v => v.Id).ToList(),
                ImageUrl = string.IsNullOrEmpty(committee.ImageUrl) ? null : $"{baseUrl}{committee.ImageUrl}", // 👈 هنا كمان
                Description = committee.Description
            };

            return Ok(committeeDto);
        }

        [Authorize(Roles = "High Board,Head,Vice,HR")]

        [HttpPost]
        public async Task<ActionResult> CreateCommittee([FromForm] CommitteeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ⚙️ تطبيع القيم: اعتبر 0 كأنه null
            int? headId = (dto.HeadId.HasValue && dto.HeadId.Value > 0) ? dto.HeadId : null;
            var viceIds = (dto.VicesId ?? new List<int>())
                          .Where(id => id > 0)      // تجاهل الأصفار والقيم السالبة
                          .Distinct()
                          .ToList();

            // ✅ تحقق الهيد فقط لو فيه قيمة بعد التطبيع
            User? head = null;
            if (headId.HasValue)
            {
                head = await _context.Users.FindAsync(headId.Value);
                if (head == null)
                    return BadRequest("Head user not found.");

                if (head.RoleId != 2)
                    return BadRequest($"User {head.UserName} does not have the required role to be a head.");

                bool headExistsInAnotherCommittee = await _context.Committees
                    .AnyAsync(c => c.HeadId == headId.Value);

                if (headExistsInAnotherCommittee)
                    return BadRequest($"User {head.UserName} is already assigned as a head in another committee.");
            }

            // ✅ تحقق الفايسز لو فيه IDs صالحه
            var vices = new List<User>();
            if (viceIds.Count > 0)
            {
                vices = await _context.Users
                    .Where(u => viceIds.Contains(u.Id))
                    .ToListAsync();

                // لو فيه IDs مش موجودة
                var notFound = viceIds.Except(vices.Select(v => v.Id)).ToList();
                if (notFound.Count > 0)
                    return BadRequest($"Some vice user IDs not found: {string.Join(", ", notFound)}");

                foreach (var vice in vices)
                {
                    if (vice.RoleId != 5)
                        return BadRequest($"User {vice.UserName} does not have the required role to be a vice.");
                }
            }

            // 📤 رفع الصورة (اختياري)
            string? imageUrl = null;
            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "committees");
                Directory.CreateDirectory(uploadsFolder);

                var safeFileName = Path.GetFileName(dto.ImageUrl.FileName);
                var fileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = System.IO.File.Create(filePath))
                    await dto.ImageUrl.CopyToAsync(stream);

                imageUrl = $"/images/committees/{fileName}";
            }

            var committee = new Committee
            {
                Name = dto.Name,
                HeadId = headId,        
                Vices = vices,
                ImageUrl = imageUrl,
                Description = dto.Description
            };

            _context.Committees.Add(committee);
            await _context.SaveChangesAsync();

            return Created("", new { message = "Committee created successfully", committee.Id, committee.HeadId });
        }

        [Authorize(Roles = "High Board,Head,Vice,HR")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommittee(int id, [FromForm] CommitteeUpdateDto committeeUpdateDto) // 👈 FromForm
        {
            if (string.IsNullOrWhiteSpace(committeeUpdateDto.Name))
                return BadRequest("Committee name is required.");

            var committee = await _context.Committees
                .Include(c => c.Vices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (committee == null)
                return NotFound();

            if (committeeUpdateDto.HeadId.HasValue)
            {
                var head = await _context.Users.FindAsync(committeeUpdateDto.HeadId.Value);
                if (head == null)
                    return BadRequest("Head user not found.");

                if (head.RoleId != 2)
                    return BadRequest($"User {head.UserName} does not have the required role to be a head.");

                bool headExistsInAnotherCommittee = await _context.Committees
                    .AnyAsync(c => c.HeadId == committeeUpdateDto.HeadId.Value && c.Id != id);

                if (headExistsInAnotherCommittee)
                    return BadRequest($"User {head.UserName} is already assigned as a head in another committee.");

                committee.HeadId = committeeUpdateDto.HeadId.Value;
            }

            if (committeeUpdateDto.VicesId != null)
            {
                committee.Vices.Clear();

                if (committeeUpdateDto.VicesId.Any())
                {
                    var vices = await _context.Users
                        .Where(u => committeeUpdateDto.VicesId.Contains(u.Id))
                        .ToListAsync();

                    if (vices.Count != committeeUpdateDto.VicesId.Count)
                        return BadRequest("Some vice users were not found.");

                    foreach (var vice in vices)
                    {
                        if (vice.RoleId != 5)
                            return BadRequest($"User {vice.UserName} does not have the required role to be a vice.");

                        bool isViceInAnotherCommittee = await _context.Committees
                            .AnyAsync(c => c.Id != id && c.Vices.Any(v => v.Id == vice.Id));

                        if (isViceInAnotherCommittee)
                            return BadRequest($"User {vice.UserName} is already assigned as a vice in another committee.");
                    }

                    committee.Vices = vices;
                }
            }

            // ✅ تحديث الصورة
            if (committeeUpdateDto.ImageUrl != null)
            {
                var fileName = $"{Guid.NewGuid()}_{committeeUpdateDto.ImageUrl.FileName}";
                var filePath = Path.Combine("wwwroot/images/committees", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await committeeUpdateDto.ImageUrl.CopyToAsync(stream);
                }

                committee.ImageUrl = $"/images/committees/{fileName}";
            }

            committee.Name = committeeUpdateDto.Name;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the committee.");
            }
        }


        [Authorize(Roles = "High Board,Head,Vice,HR")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommittee(int id)
        {
            var committee = await _context.Committees
                .Include(c => c.Users)
                .Include(c => c.Meetings)
                    .ThenInclude(m => m.Users_Meetings)
                .Include(c => c.Vices) // 👈 لو عندك علاقة Vices لازم تفكها كمان
                .FirstOrDefaultAsync(c => c.Id == id);

            if (committee == null)
                return NotFound();

            // فك ارتباط الـ Users
            foreach (var user in committee.Users.ToList())
            {
                user.CommitteeId = null;
            }

            // فك ارتباط الـ Vices (لو Many-to-Many)
            committee.Vices.Clear();

            // امسح UsersMeetings المرتبطة
            foreach (var meeting in committee.Meetings)
            {
                _context.Users_Meetings.RemoveRange(meeting.Users_Meetings);
            }

            // امسح الـ Meetings
            _context.Meetings.RemoveRange(committee.Meetings);

            // امسح الـ Committee
            _context.Committees.Remove(committee);

            // ✅ احفظ التغييرات
            await _context.SaveChangesAsync();

            return NoContent();
        }



        //[HttpPost("image")]
        //public async Task<IActionResult> UploadCommitteeImage(int committeeId, IFormFile file)
        //{
        //    var committee = await _context.Committees.FindAsync(committeeId);
        //    if (committee == null)
        //        return NotFound("Committee not found");

        //    if (file == null || file.Length == 0)
        //        return BadRequest("No file uploaded");

        //    var fileName = Path.GetFileName(file.FileName); // اسم الملف الأصلي
        //    var path = Path.Combine(_env.WebRootPath, "images", fileName); // مكان التخزين على السيرفر

        //    // إنشاء مجلد الصور لو مش موجود
        //    Directory.CreateDirectory(Path.Combine(_env.WebRootPath, "images"));

        //    // حفظ الملف فعلياً في المجلد
        //    using (var stream = new FileStream(path, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    // حفظ المسار في قاعدة البيانات
        //    committee.ImageUrl = "/images/" + fileName;
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Image uploaded successfully", imageUrl = committee.ImageUrl });
        //}


    }
}
