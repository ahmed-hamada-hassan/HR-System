using IEEE.Configurations;
using IEEE.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IEEE.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new TaskConfigurations());
            modelBuilder.ApplyConfiguration(new Users_TasksConfigurations());
            modelBuilder.ApplyConfiguration(new MeetingConfig());
            modelBuilder.ApplyConfiguration(new CommitteeConfig());



        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Users_Tasks> Users_Tasks { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Committee> Committees { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
    }
}
