using Discord;
using Discord.WebSocket;

namespace DiscordBot.Messages
{
    public class ReminderSubscriptionMessage
    {
        public static async Task sendMessage(DiscordSocketClient client, ulong userId)
        {
            var builder = new ComponentBuilder()
                .WithButton("Opt Out", $"{Constants.ComponentIds.OPT_OUT_BUTTON}", ButtonStyle.Danger);
            
            string message = "Hi, I'm Raid-o-Tron. \n"
                            + "I will send you reminders for raids you have signed up for.\n"
                            + "The reminders will look like\n"
                            + "> Testraid: The raid starts in 30 minutes. \n"
                            + "You can opt out of the reminders here or change it any time at https://lieb.games \n"
                            + " ------------------------------------------- \n"
                            + "Hi, ich bin Raid-o-Tron. \n"
                            + "Ich werde dir Erinnerungen für Raid an denen du dich angemeldet hast schicken.\n"
                            + "Die Erinnerungen werden so aussehen:\n"
                            + "> Testraid: The raid starts in 30 minutes. \n"
                            + "Du kannst dich von den Erinnerungen hier abmelden oder deine Einstellungen jederzeit auf https://lieb.games ändern.";

            var user = await client.GetUserAsync(userId);
            if(user != null)
            {
                await user.SendMessageAsync(message, components: builder.Build());
            }
        }
    }
}