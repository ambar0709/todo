namespace Todo.Models
{
    public class GravatarProfile
    {
        public GravatarProfile()
        {
            
        }
        public GravatarProfile(string displayName, string avatarUrl, string emailAddres)
        {
            DisplayName = displayName;
            AvatarUrl = avatarUrl;
            EmailAddress = emailAddres;
        }
        public string DisplayName { get; set; }

        public string AvatarUrl { get; set; }

        public string EmailAddress { get; set; }
    }
}
