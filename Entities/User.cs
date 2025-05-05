using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IEEE.Entities
{
    public class User : IdentityUser<int>
    {

        public string FName { get; set; }
        public string LName { get; set; }
        public string FullName => FName  + LName;

        // Users commitee
        public int CommitteeId { get; set; }
        public Committee? Committee { get; set; }

        // Head Committees
        public ICollection<Committee> HeadCommittees { get; set; } = new List<Committee>();

        public ICollection<Tasks>? HeadTasks { get; set; } = new List<Tasks>();
        public ICollection<Users_Tasks>? Users_Tasks { get; set; } = new List<Users_Tasks>();


        public ICollection<Meeting>? HeadMeetings { get; set; } = new List<Meeting>();
        public ICollection<Meeting>? Meetings { get; set; } = new List<Meeting>();



    }
}
