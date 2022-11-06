using Discord;
using Discord.WebSocket;
using System;
using System.ComponentModel.DataAnnotations;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class SignUpMessage
    {
        public static MessageComponent buildMessage(List<ApiRole> roles, int raidId, string buttonType, bool allRoles)
        {
            var signUpSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId($"{Constants.ComponentIds.SIGN_UP_DROP_DOWN}-{raidId}-{buttonType}")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach(ApiRole role in roles)
            {
                if(allRoles || role.IsSignUpAllowed)
                signUpSelect.AddOption(role.Name, role.roleId.ToString(), role.Description);
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(signUpSelect, 0);

            return builder.Build();
        }
        /*
        public static MessageComponent buildMessage(List<ApiRole> roles, int raidId)
        {
            var signUpSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId(Constants.ComponentIds.SIGN_UP_DROP_DOWN)
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach(ApiRole role in roles)
            {
                if(role.IsSignUpAllowed)
                signUpSelect.AddOption(role.Name, role.roleId.ToString(), role.Description);
            }

            var flexSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId(Constants.ComponentIds.FLEX_DROP_DOWN)
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach(ApiRole role in roles)
            {
                flexSelect.AddOption(role.Name, role.roleId.ToString(), role.Description);
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(signUpSelect, 0)
                .WithButton("SignUp", $"{Constants.ComponentIds.SIGN_UP_BUTTON}-{raidId.ToString()}", ButtonStyle.Success, row: 1)
                .WithSelectMenu(flexSelect, 2)
                .WithButton("Maybe", $"{Constants.ComponentIds.MAYBE_BUTTON}-{raidId.ToString()}", ButtonStyle.Success, row: 3)
                .WithButton("Backup", $"{Constants.ComponentIds.BACKUP_BUTTON}-{raidId.ToString()}", ButtonStyle.Success, row: 3)
                .WithButton("Flex", $"{Constants.ComponentIds.FLEX_BUTTON}-{raidId.ToString()}", ButtonStyle.Success, row: 3);

            return builder.Build();
        }*/

    }
}