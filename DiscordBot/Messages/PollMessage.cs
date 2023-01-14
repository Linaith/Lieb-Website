using Discord;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class PollMessage
    {
        public static MessageComponent buildMessage(ApiPoll poll, bool isDropdown)
        {
            var builder = new ComponentBuilder();
            if(isDropdown)
            {
                var signUpSelect = new SelectMenuBuilder()
                    .WithPlaceholder(poll.Question)
                    .WithCustomId($"{Constants.ComponentIds.POLL_DROP_DOWN}-{poll.PollId}")
                    .WithMinValues(1)
                    .WithMaxValues(1);
                
                foreach(KeyValuePair<int, string> option in poll.Options)
                {
                    signUpSelect.AddOption(option.Value, option.Key.ToString());
                }

                builder.WithSelectMenu(signUpSelect, 0);
            }
            else
            {
                foreach(KeyValuePair<int, string> option in poll.Options)
                {
                    builder.WithButton(option.Value, $"{Constants.ComponentIds.POLL_ANSWER_BUTTON}-{poll.PollId}-{option.Key}", ButtonStyle.Secondary);
                }
            }

            if(poll.AllowCustomAnswer)
            {
                builder.WithButton("Custom", $"{Constants.ComponentIds.POLL_CUSTOM_ANSWER_BUTTON}-{poll.PollId}", ButtonStyle.Secondary);
            }
            return builder.Build();
        }

        public static Parameters ParseId(string customId)
        {
            Parameters parameters = new Parameters();

            string[] ids = customId.Split('-');
            if(ids.Length > 1)
            {
                int.TryParse(ids[1], out parameters.PollId);
            }
            if(ids.Length > 2)
            {
                int.TryParse(ids[2], out parameters.OptionId);
            }
            return parameters;
        }

        public class Parameters
        {
            public int PollId;
            public int OptionId;
        }
    }
}