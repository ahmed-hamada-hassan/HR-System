namespace IEEE.DTO.UserDTO
{
    public class GetUsersDto
    {
        public string UserName { get; set; }

        public string Eamil { get; set; }

        public bool IsActive { get; set; }
        public List<string> CommitteeNames { get; set; }

        public string ? Role { get; set; }



    }
}
