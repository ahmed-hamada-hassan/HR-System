using IEEE.Entities;

namespace IEEE.DTO.TasksDto
{
    public class TaskReadDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateOnly Month { get; set; }
        public string HeadName { get; set; }
        public string CommiteeName { get; set; }


    }
}
