namespace Lieb.Models.Poll
{
    public class PollAnswer
    {
        public int PollAnswerId { get; set; }

        public int? PollOptionId { get; set; }

        public ulong UserId { get; set; }
    }
}
