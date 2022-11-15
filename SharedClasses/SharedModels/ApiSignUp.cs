
namespace SharedClasses.SharedModels
{
    public class ApiSignUp
    {
        public int raidId {get; set;}
        public ulong userId {get; set;}
        public int gw2AccountId {get; set;}
        public int roleId {get; set;}
        public ulong signedUpByUserId {get; set;}
        public string userName {get; set;} = string.Empty;
    }
}