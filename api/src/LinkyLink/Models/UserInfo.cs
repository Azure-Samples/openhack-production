namespace LinkyLink.Models
{
    /// <summary>
    /// Data model class to represent user's identity provider and email.
    /// </summary>
    public struct UserInfo
    {
        public string IDProvider { get; }
        public string Email { get; }
        public UserInfo(string provider, string email)
        {
            IDProvider = provider;
            Email = email;
        }

        public static UserInfo Empty = new UserInfo("", "");
    }
}
