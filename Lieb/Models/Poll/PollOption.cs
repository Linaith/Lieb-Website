using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.Poll
{
    public class PollOption
    {
        public int PollOptionId { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "Option too long (100 character limit).")]
        public string Name { get; set;} = string.Empty;
    }
}
