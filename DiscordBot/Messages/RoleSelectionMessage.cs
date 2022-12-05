using Discord;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class RoleSelectionMessage
    {
        public static MessageComponent buildMessage(List<ApiRole> roles, int raidId, string buttonType, bool allRoles, ulong userIdToSignUp, ulong signedUpByUserId)
        {
            var signUpSelect = new SelectMenuBuilder()
                .WithPlaceholder($"Select a role - {GetButtonName(buttonType)}")
                .WithCustomId($"{Constants.ComponentIds.ROLE_SELECT_DROP_DOWN}-{raidId}-{buttonType}-{userIdToSignUp}-{signedUpByUserId}")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach(ApiRole role in roles)
            {
                if(allRoles || role.IsSignUpAllowed)
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

        private static string GetButtonName(string buttonType)
        {
            switch(buttonType)
            {
                case Constants.ComponentIds.SIGN_UP_BUTTON:
                    return "Sign Up";
                    break;
                case Constants.ComponentIds.MAYBE_BUTTON:
                    return "Maybe";
                    break;
                case Constants.ComponentIds.BACKUP_BUTTON:
                    return "Backup";
                    break;
                case Constants.ComponentIds.FLEX_BUTTON:
                    return "Flex";
                    break;
                default:
                    return string.Empty;
                    break;
            }
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
                ulong.TryParse(ids[3],out parameters.UserIdToSignUp);
            }
            if(ids.Length > 4)
            {
                ulong.TryParse(ids[4],out parameters.SignedUpByUserId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int RaidId;
            public string ButtonType = string.Empty;
            public  ulong UserIdToSignUp;
            public  ulong SignedUpByUserId;
        }
    }
}