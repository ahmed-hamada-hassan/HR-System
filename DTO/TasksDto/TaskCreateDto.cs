namespace IEEE.DTO.TasksDto
{
    public class TaskCreateDto
    {
        public string Description { get; set; }
        public DateOnly Month { get; set; }
        public int HeadId { get; set; }
        public int CommitteeId { get; set; }
    }
}
