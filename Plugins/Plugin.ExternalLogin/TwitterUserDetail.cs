namespace Plugin.ExternalLogin
{
    public partial class TwitterUserDetail
    {
        private string _profileImageUrl;
        public string Name { get; set; }

        public string FirstName
        {
            get
            {
                var fName = Name;
                if (!string.IsNullOrWhiteSpace(Name) && Name.Split(' ').Length > 0)
                {
                    fName = Name.Split(' ')[0];
                }
                return fName;
            }
        }

        public string LastName
        {
            get
            {
                var lName = "";
                if (!string.IsNullOrWhiteSpace(Name) && Name.Split(' ').Length > 1)
                {
                    lName = Name.Split(' ')[1];
                }
                return lName;
            }
        }

        public string Email { get; set; }

        public string ProfileImageUrl
        {
            get => _profileImageUrl.Replace("_normal.", "_bigger.");
            set => _profileImageUrl = value;
        }
    }
}