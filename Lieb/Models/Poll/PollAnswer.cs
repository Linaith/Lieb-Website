namespace Lieb.Models.Poll
{
    public class PollAnswer
    {
        public int PollAnswerId { get; set; }

        public int? PollOptionId { get; set; }

        public string Answer { get; set; } = string.Empty;

        public ulong UserId { get; set; }
    }
}
