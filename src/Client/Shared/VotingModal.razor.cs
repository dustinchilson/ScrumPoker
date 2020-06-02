using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ScrumPokerTool.Client.Services;
using ScrumPokerTool.Client.ViewModels;

namespace ScrumPokerTool.Client.Shared
{
    public partial class VotingModal
    {
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }

        [Inject]
        public ProfileService ProfileSvc { get; set; }

        [CascadingParameter] 
        public BlazoredModalInstance BlazoredModal { get; set; }

        [Parameter]
        public List<VoteOption> StoryPointOptions { get; set; }

        protected readonly List<string> storyPointOptions = new List<string>()
        {
            "½",
            "1",
            "2",
            "3",
            "5",
            "8",
            "13",
            "21",
            "🤷"
        };

        private Task SubmitVoteAsync(string vote)
        {
            if (string.IsNullOrWhiteSpace(vote))
                return Task.CompletedTask;

            BlazoredModal.Close(ModalResult.Ok(vote, typeof(string)));

            return Task.CompletedTask;
        }
    }
}
