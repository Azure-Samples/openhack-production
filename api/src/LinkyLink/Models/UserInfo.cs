namespace LinkyLink.Models
{
    /// <summary>
    /// Data model class to represent User Info.
    /// </summary>
    public struct UserInfo
    {
        public string IDProvider { get; }
        public string HashedID { get; }
        public UserInfo(string provider, string hashedID)
        {
            IDProvider = provider;
            HashedID = hashedID;
        }

        public static UserInfo Empty = new UserInfo("", "");
    }
}
