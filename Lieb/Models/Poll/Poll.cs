namespace Lieb.Models.Poll
{
    public class Poll
    {
        public int PollId { get; set; }
        public string Question { get; set; }
        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
        public ICollection<PollAnswer> Answers { get; set; } = new List<PollAnswer>();
        public int? RaidId { get; set; }

    }
}
