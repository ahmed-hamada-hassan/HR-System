using Microsoft.AspNetCore.Identity;

namespace IEEE.Entities
{
    public class ApplicationRole : IdentityRole<int>
    {

        public virtual ICollection<User> Users { get; set; }
    }
}
