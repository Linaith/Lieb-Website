namespace SharedClasses.SharedModels
{
    public class ApiPoll
    {
        public int PollId { get; set; }
        public string Question { get; set; } = string.Empty;
        public Dictionary<int, string> Options { get; set; } = new Dictionary<int, string>();
        public bool AllowCustomAnswer {get; set;} = false;
        public List<ulong> UserIds {get; set;} = new List<ulong>();
    }

    public class ApiPollAnswer
    {
        public int PollId { get; set; }
        public int OptionId {get; set;}
        public string Answer {get; set;} = string.Empty;
        public ulong UserId {get; set;}
    }
}