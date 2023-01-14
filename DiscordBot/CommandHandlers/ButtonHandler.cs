using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot.CommandHandlers
{
    public class ButtonHandler
    {
        private readonly HttpService _httpService;
        private readonly HandlerFunctions _handlerFunctions;

        public ButtonHandler(HttpService httpService)
        {
            _httpService = httpService;
            _handlerFunctions = new HandlerFunctions(_httpService);
        }

        public async Task Handler(SocketMessageComponent component)
        {
            string[] ids = component.Data.CustomId.Split('-');
            switch(ids[0])
            {
                case Constants.ComponentIds.SIGN_UP_BUTTON:
                    await SignUpClicked(component, false, SharedConstants.SIGNED_UP);
                    break;
                case Constants.ComponentIds.MAYBE_BUTTON:
                    await SignUpClicked(component, false, SharedConstants.MAYBE);
                    break;
                case Constants.ComponentIds.BACKUP_BUTTON:
                    await SignUpClicked(component, true, SharedConstants.BACKUP);
                    break;
                case Constants.ComponentIds.FLEX_BUTTON:
                    RaidMessage.ButtonParameters flexParameters = RaidMessage.ParseButtonId(component.Data.CustomId);
                    if(!string.IsNullOrEmpty(await _httpService.IsUserSignedUp(flexParameters.RaidId, component.User.Id)))
                    {
                        await _handlerFunctions.SelectRole(component, flexParameters.RaidId, flexParameters.ButtonType, true, component.User.Id, 0);
                    }
                    else
                    {
                        await component.RespondAsync("Flex sign up is only allowed if you are already signed up.", ephemeral: true);
                    }
                    break;
                case Constants.ComponentIds.SIGN_OFF_BUTTON:
                    RaidMessage.ButtonParameters signOffParameters = RaidMessage.ParseButtonId(component.Data.CustomId);
                    ApiSignUp signOff = new ApiSignUp()
                    {
                        raidId = signOffParameters.RaidId,
                        userId = component.User.Id
                    };
                    await _httpService.SignOff(signOff);
                    await component.RespondAsync("Signed Off", ephemeral: true);
                break;
                case Constants.ComponentIds.OPT_OUT_BUTTON:
                    if(await _httpService.ReminderOptOut(component.User.Id))
                    {
                        await component.RespondAsync("You opted out of the raid reminders.");
                    }
                    else
                    {
                        await component.RespondAsync("Opting out failed, please try again later or change the setting on the website.");
                    }
                    break;
                case Constants.ComponentIds.POLL_ANSWER_BUTTON:
                    PollMessage.Parameters pollAnswerParameters = PollMessage.ParseId(component.Data.CustomId);
                    ApiPollAnswer answer = new ApiPollAnswer()
                    {
                        Answer = string.Empty,
                        OptionId = pollAnswerParameters.OptionId,
                        PollId = pollAnswerParameters.PollId,
                        UserId = component.User.Id
                    };
                    await _httpService.AnswerPoll(answer);
                    await component.RespondAsync("Answer sent.", ephemeral: true);
                    break;
                case Constants.ComponentIds.POLL_CUSTOM_ANSWER_BUTTON:
                    PollMessage.Parameters pollCustomParameters = PollMessage.ParseId(component.Data.CustomId);
                    await component.RespondWithModalAsync(PollCustomModal.buildMessage(pollCustomParameters.PollId, component.Message.Content));
                    break;
            }
        }

        public async Task SignUpClicked(SocketMessageComponent component, bool allRoles, string signUpType)
        {
            RaidMessage.ButtonParameters parameters = RaidMessage.ParseButtonId(component.Data.CustomId);
            string currentSignUp = await _httpService.IsUserSignedUp(parameters.RaidId, component.User.Id);
            if(string.IsNullOrEmpty(currentSignUp) || currentSignUp == signUpType)
            {
                await _handlerFunctions.SelectRole(component, parameters.RaidId, parameters.ButtonType, allRoles, component.User.Id, 0);
            }
            else
            {
                ApiSignUp signUp = new ApiSignUp()
                {
                    raidId = parameters.RaidId,
                    userId = component.User.Id
                };
                switch(signUpType)
                {
                    case SharedConstants.SIGNED_UP:
                        await _httpService.ChangeToSignUp(signUp);
                        break;
                    case SharedConstants.MAYBE:
                        await _httpService.ChangeToMaybe(signUp);
                        break;
                    case SharedConstants.BACKUP:
                        await _httpService.ChangeToBackup(signUp);
                        break;
                }
                await component.RespondAsync("Sign up type changed.", ephemeral: true);
            }
        }
    }
}