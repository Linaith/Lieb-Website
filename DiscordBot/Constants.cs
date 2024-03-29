﻿namespace DiscordBot
{
    public class Constants
    {
        public const string HTTP_CLIENT_NAME = "LiebWebsite";
        public class ComponentIds
        {            
            public const string SIGN_UP_BUTTON = "signUpButton";
            public const string MAYBE_BUTTON = "maybeButton";
            public const string BACKUP_BUTTON = "backupButton";
            public const string FLEX_BUTTON = "flexButton";
            public const string SIGN_OFF_BUTTON = "signOffButton";
            public const string OPT_OUT_BUTTON = "optOutButton";
            
            public const string ROLE_SELECT_DROP_DOWN = "roleSelectDropDown";
            public const string ROLE_SELECT_EXTERNAL_DROP_DOWN = "roleSelectExternalDropDown";
            public const string ACCOUNT_SELECT_DROP_DOWN = "accountSelectDropDown";
            public const string REMOVE_USER_DROP_DOWN = "removeUserDropDown";

            public const string NAME_TEXT_BOX = "nameTextbox";
            public const string ACCOUNT_TEXT_BOX = "accountTextBox";
            public const string CREATE_ACCOUNT_MODAL = "createAccountModal";

            public const string SIGN_UP_EXTERNAL_MODAL = "signUpExternalModal";


            public const string POLL_DROP_DOWN = "pollDropDown";
            public const string POLL_ANSWER_BUTTON = "pollAnswerButton";
            public const string POLL_CUSTOM_ANSWER_BUTTON = "pollCustomAnswerButton";
            public const string POLL_CUSTOM_ANSWER_MODAL = "pollCustomAnswerModal";
            public const string POLL_CUSTOM_ANSWER_TEXT_BOX = "pollCustomAnswerTextBox";
        }

        public class SlashCommands
        {
            public const string RAID = "raid";
            public const string SEND_MESSAGE_COMMAND = "send-message";
            public const string USER = "user";

            public const string ADD_USER_COMMAND = "add";
            public const string REMOVE_USER_COMMAND = "remove";
            public const string ADD_EXTERNAL_USER_COMMAND = "add-external";
            public const string REMOVE_EXTERNAL_USER_COMMAND = "remove-external";
            public const string REMOVE_USER_DROPDOWN_COMMAND = "remove-dropdown";

            public class OptionNames
            {
                public const string USER = "user";
                public const string USER_NAME = "user-name";
                public const string MESSAGE = "message";
                public const string RAID_ID = "raid-id";
            }
        }
    }
}
