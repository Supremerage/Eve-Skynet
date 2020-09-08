using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eve_Skynet.Bot.Models
{
    public class BlacklistedChannel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual ulong GuildId { get; set; }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual ulong ChannelId { get; set; }
    }
}
