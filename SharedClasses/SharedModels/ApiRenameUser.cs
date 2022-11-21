
namespace SharedClasses.SharedModels
{
    public class ApiRenameUser
    {
        public ulong userId { get; set; }

        public string Name { get; set; } = String.Empty;

        public string Account { get; set; } = String.Empty;

        public List<ulong> ServerIds { get; set; } = new List<ulong>();
    }
}