using System.ComponentModel.DataAnnotations.Schema;

namespace Eve_Skynet.Bot.Models
{
    public class CustomCommand
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; set; }
        public string Response { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }
    }
}
