namespace IEEE.DTO.CommitteeDto
{
    public class CommitteeUpdateDto
    {
        public string Name { get; set; }
        public int? HeadId { get; set; }
        public List<int>? VicesId { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
