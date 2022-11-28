using Discord;

namespace DiscordBot.Messages
{
    public class CreateAccountModal
    {
        public static Modal buildMessage(int raidId, string pressedButtonId, string defaultUserName)
        {
            var mb = new ModalBuilder()
                    .WithTitle("Create Account")
                    .WithCustomId($"{Constants.ComponentIds.CREATE_ACCOUNT_MODAL}-{raidId}-{pressedButtonId}")
                    .AddTextInput("Name", Constants.ComponentIds.NAME_TEXT_BOX, placeholder: defaultUserName, required: true, value: defaultUserName)
                    .AddTextInput("Guild Wars 2 Account", Constants.ComponentIds.ACCOUNT_TEXT_BOX, placeholder: "Account.1234", required: true);

            return mb.Build();
        }

        public static Parameters ParseId(string customId)
        {
            Parameters parameters = new Parameters();

            string[] ids = customId.Split('-');
            if(ids.Length > 1)
            {
                int.TryParse(ids[1],out parameters.RaidId);
            }
            if(ids.Length > 2)
            {
                parameters.ButtonId = ids[2];
            }
            return parameters;
        }

        public class Parameters
        {
            public int RaidId;
            public string ButtonId = string.Empty;
        }
    }
}