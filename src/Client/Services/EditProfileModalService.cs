using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ScrumPokerTool.Client.Shared;

namespace ScrumPokerTool.Client.Services
{
    public class EditProfileModalService
    {
        private readonly IModalService _modal;

        public EditProfileModalService(IModalService modal)
        {
            _modal = modal;
        }

        public async Task<string> ShowAsync()
        {
            var options = new ModalOptions()
            {
                DisableBackgroundCancel = true,
                HideCloseButton = true
            };

            var userNamePrompt = _modal.Show<UserNamePrompt>("Edit Profile", options);
            var result = await userNamePrompt.Result;

            return (string) result.Data;
        }
    }
}
