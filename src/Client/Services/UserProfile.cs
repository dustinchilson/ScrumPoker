using System;

namespace ScrumPokerTool.Client.Services
{
    public class UserProfile
    {
        private string _userId;
        private string _userName;

        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                IsDirty = true;
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                IsDirty = true;
            }
        }

        public bool IsDirty
        {
            get; 
            internal set;
        }
    }
}