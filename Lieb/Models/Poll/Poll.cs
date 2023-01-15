using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.Poll
{
    public enum AnswerType
    {
        Buttons = 1,
        Dropdown = 2
    }
    public class Poll
    {
        public int PollId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Question too long (200 character limit).")]
        public string Question { get; set; }
        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
        public ICollection<PollAnswer> Answers { get; set; } = new List<PollAnswer>();
        public AnswerType AnswerType {get; set;}
        public bool AllowCustomAnswer {get; set;} = false;
        public int? RaidId { get; set; }

    }
}
