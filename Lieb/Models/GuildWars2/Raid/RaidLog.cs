
using System.Text.Json;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidLog
    {
        public enum LogType
        {
            Raid = 1,
            RaidTemplate = 2,
            RaidSignUp = 3
        }

        public int RaidLogId { get; set; }

        public LogType Type {get; set;}

        public ulong UserId {get; set;}

        public int? RaidId { get; set; }

        public int? RaidTemplateId { get; set; }

        public string LogEntry {get; set;} = string.Empty;

        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;

        public LiebUser User {get; set;}

        public Raid? Raid { get; set; }

        public RaidTemplate? RaidTemplate { get; set; }

        public static RaidLog CreateRaidLog(ulong userId, Raid raid)
        {
            raid.RaidLogs.Clear();
            return new RaidLog()
            {
                Type = LogType.Raid,
                UserId = userId,
                RaidId = raid.RaidId,
                LogEntry = JsonSerializer.Serialize(raid),
                Time = DateTimeOffset.UtcNow
            };
        }

        public static RaidLog CreateSignUpLog(ulong userId, RaidSignUp signUp, string signedUpUserName)
        {
            string message = $"changed Status of {signedUpUserName} to: {signUp.SignUpType.ToString()}";
            return new RaidLog()
            {
                Type = LogType.RaidSignUp,
                UserId = userId,
                RaidId = signUp.RaidId,
                LogEntry = message,
                Time = DateTimeOffset.UtcNow
            };
        }

        public static RaidLog CreateRaidTemplateLog(ulong userId, RaidTemplate template)
        {
            template.TemplateLogs.Clear();
            return new RaidLog()
            {
                Type = LogType.RaidTemplate,
                UserId = userId,
                RaidTemplateId = template.RaidTemplateId,
                LogEntry = JsonSerializer.Serialize(template),
                Time = DateTimeOffset.UtcNow
            };
        }
    }
}
