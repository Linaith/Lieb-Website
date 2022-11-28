using Discord;
using Discord.WebSocket;
using System;
using System.ComponentModel.DataAnnotations;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class ExternalUserNameModal
    {
        public static Modal buildMessage(int raidId, int roleId)
        {
            var mb = new ModalBuilder()
                .WithTitle("Create Account")
                .WithCustomId($"{Constants.ComponentIds.SIGN_UP_EXTERNAL_MODAL}-{raidId}-{roleId}")
                .AddTextInput("Name", Constants.ComponentIds.NAME_TEXT_BOX, placeholder: "Name", required: true);

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
                int.TryParse(ids[2],out parameters.RoleId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int RaidId;
            public int RoleId;
        }
    }
}