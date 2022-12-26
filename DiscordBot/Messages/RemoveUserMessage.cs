using Discord;
using SharedClasses.SharedModels;
using System.Security.Principal;

namespace DiscordBot.Messages
{
    public class RemoveUserMessage
    {

        public static MessageComponent buildMessage(ApiRaid raid, ulong signedUpByUserId)
        {
            var signUpSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select an account")
                .WithCustomId($"{Constants.ComponentIds.REMOVE_USER_DROP_DOWN}-{raid.RaidId}-{signedUpByUserId}")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (ApiRaid.Role role in raid.Roles)
            {
                foreach (ApiRaid.Role.User user in role.Users)
                {
                    if(user.UserId > 0)
                    {
                        signUpSelect.AddOption($"({user.UserName} | {user.AccountName}", user.UserId.ToString());
                    }
                    else
                    {
                        signUpSelect.AddOption(user.UserName, user.UserName);
                    }
                }
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(signUpSelect, 0);

            return builder.Build();
        }

        public static Parameters ParseId(string customId)
        {
            Parameters parameters = new Parameters();

            string[] ids = customId.Split('-');
            if (ids.Length > 1)
            {
                int.TryParse(ids[1], out parameters.RaidId);
            }
            if (ids.Length > 2)
            {
                ulong.TryParse(ids[2], out parameters.SignedUpByUserId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int RaidId;
            public ulong SignedUpByUserId;
        }
    }
}
