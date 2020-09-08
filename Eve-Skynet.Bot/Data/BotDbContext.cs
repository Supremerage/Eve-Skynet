using Eve_Skynet.Bot.Models;
using Eve_Skynet.Bot.Models.Roles;
using Microsoft.EntityFrameworkCore;

namespace Eve_Skynet.Bot.Data
{
    public class BotDbContext : DbContext
    {
        public BotDbContext()
        {
        }

        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AdminRole>().HasKey(x => new {x.GuildId, x.RoleId});
            modelBuilder.Entity<BlacklistedChannel>().HasKey(x => new {x.GuildId, x.ChannelId});
            modelBuilder.Entity<ManagedRole>().HasKey(x => new {x.GuildId, x.RoleId});
            modelBuilder.Entity<CustomCommand>().HasKey(x => new {x.GuildId, x.Name});
            modelBuilder.Entity<WelcomeMessage>().HasKey(x => x.GuildId);
        }

        public virtual DbSet<WelcomeMessage> WelcomeMessages { get; set; }
        public virtual DbSet<BlacklistedChannel> BlacklistedChannels { get; set; }
        public virtual DbSet<AdminRole> AdminRoles { get; set; }
        public virtual DbSet<ManagedRole> ManagedRoles { get; set; }
        public virtual DbSet<ManagedUserRole> ManagedUserRoles { get; set; }
        public virtual DbSet<CustomCommand> CustomCommands { get; set; }
    }
}
