using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eve_Skynet.Bot.Models
{
    public class WelcomeMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual ulong GuildId { get; set; }
        public virtual string Message { get; set; }
    }
}
