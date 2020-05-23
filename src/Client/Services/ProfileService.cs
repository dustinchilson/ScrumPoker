using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace ScrumPokerTool.Client.Services
{
    public class ProfileService
    {
        private readonly ISyncLocalStorageService _localStorage;
        private UserProfile _profile;

        public ProfileService(ISyncLocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public void Load()
        {
            var store = _localStorage.GetItem<UserProfile>("UserProfile");

            if (store != null)
                _profile = store;
            else
                _profile = new UserProfile() { UserId = Guid.NewGuid().ToString() };
        }

        public void Save()
        {
            _localStorage.SetItem("UserProfile", _profile);

            _profile.IsDirty = false;
        }

        private UserProfile Profile
        {
            get
            {
                if (_profile != null)
                    return _profile;

                Load();
                return _profile;
            }
        }

        public string UserId
        {
            get => Profile.UserId;
            set => Profile.UserId = value;
        }

        public string UserName
        {
            get => Profile.UserName;
            set => Profile.UserName = value;
        }

        public bool IsDirty => Profile.IsDirty;
    }
}