
namespace SharedClasses.SharedModels
{
    public class ApiRole
    {
        public string Name { get; set; } = String.Empty;

        public string Description { get; set; } = String.Empty;

        public bool IsSignUpAllowed { get; set; } = false;

        public int roleId {get; set;}
    }
}