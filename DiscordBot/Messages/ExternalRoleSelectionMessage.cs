using Discord;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class ExternalRoleSelectionMessage
    {
        public static MessageComponent buildMessage(List<ApiRole> roles, int raidId)
        {
            var signUpSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId($"{Constants.ComponentIds.ROLE_SELECT_EXTERNAL_DROP_DOWN}-{raidId}")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach(ApiRole role in roles)
            {
                if(role.IsSignUpAllowed)
                {
                    if(!string.IsNullOrEmpty(role.Description))
                    {
                        string description = role.Description.Length <= 100 ? role.Description : role.Description.Substring(0, 100);
                        signUpSelect.AddOption(role.Name, role.roleId.ToString(), description);
                    }
                    else
                    {
                        signUpSelect.AddOption(role.Name, role.roleId.ToString(), role.Name);
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
            if(ids.Length > 1)
            {
                int.TryParse(ids[1],out parameters.RaidId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int RaidId;
        }
    }
}