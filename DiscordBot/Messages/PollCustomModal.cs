using Discord;
using Discord.WebSocket;
using System;
using System.ComponentModel.DataAnnotations;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class PollCustomModal
    {
        public static Modal buildMessage(int pollId, string question)
        {
            var mb = new ModalBuilder()
                .WithTitle("Answer")
                .WithCustomId($"{Constants.ComponentIds.POLL_CUSTOM_ANSWER_MODAL}-{pollId}")
                .AddTextInput("Answer", Constants.ComponentIds.POLL_CUSTOM_ANSWER_TEXT_BOX, placeholder: "Yes", required: true);

            return mb.Build();
        }

        public static Parameters ParseId(string customId)
        {
            Parameters parameters = new Parameters();

            string[] ids = customId.Split('-');
            if(ids.Length > 1)
            {
                int.TryParse(ids[1],out parameters.PollId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int PollId;
        }
    }
}