using Discord;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class AccountSelectionMessage
    {
        public static MessageComponent buildMessage(List<ApiGuildWars2Account> accounts, int raidId, string buttonType, int roleId, ulong userIdToSignUp, ulong signedUpByUserId)
        {
            var signUpSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select an account")
                .WithCustomId($"{Constants.ComponentIds.ACCOUNT_SELECT_DROP_DOWN}-{raidId}-{buttonType}-{roleId}-{userIdToSignUp}-{signedUpByUserId}")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach(ApiGuildWars2Account account in accounts)
            {
                signUpSelect.AddOption(account.AccountName, account.GuildWars2AccountId.ToString());
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(signUpSelect, 0);

            return builder.Build();
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
                parameters.ButtonType = ids[2];
            }
            if(ids.Length > 3)
            {
                int.TryParse(ids[3],out parameters.RoleId);
            }
            if(ids.Length > 4)
            {
                ulong.TryParse(ids[4],out parameters.UserIdToSignUp);
            }
            if(ids.Length > 5)
            {
                ulong.TryParse(ids[5],out parameters.SignedUpByUserId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int RaidId;
            public string ButtonType = string.Empty;
            public  int RoleId;
            public  ulong UserIdToSignUp;
            public  ulong SignedUpByUserId;
        }
    }
}