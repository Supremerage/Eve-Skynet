using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eve_Skynet.Bot.Models
{
    public class ManagedUserRole
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Key { get; set; }
        public virtual ManagedRole ManagedRole { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual ulong UserId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual ulong GuildId { get; set; }
    }
}
