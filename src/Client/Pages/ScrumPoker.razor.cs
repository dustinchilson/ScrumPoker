using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ScrumPokerTool.Client.Services;
using ScrumPokerTool.Client.Shared;
using ScrumPokerTool.Client.ViewModels;
using ScrumPokerTool.Shared;

namespace ScrumPokerTool.Client.Pages
{
    public partial class ScrumPoker : IAsyncDisposable
    {
        [Inject] 
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected EditProfileModalService EditProfileModal { get; set; }

        [Inject]
        protected ProfileService ProfileSvc { get; set; }

        [Inject]
        protected ScrumPokerHubClient ScrumPokerHub { get; set; }

        [Inject]
        protected ILogger<ScrumPoker> Logger { get; set; }

        [Inject]
        protected IModalService Modal { get; set; }
        
        [Parameter]
        public string GameId { get; set; }

        protected string userName;
        protected string exitGameTxt = "Leave";
        protected string userId;
        protected string gameOwner;
        protected bool voteComplete = false;

        protected string gameModeTxt = "Become Observer";
        protected bool observing = false;

        protected readonly List<Player> players = new List<Player>();

        protected Task ExitGame()
        {
            if (string.IsNullOrWhiteSpace(GameId))
                return Task.CompletedTask;

            Logger.LogDebug("Game Ended!");

            ScrumPokerHub.LeaveGameAsync()
                .ContinueWith(t => ScrumPokerHub.StopAsync());

            NavigationManager.NavigateTo($"/");

            return Task.CompletedTask;
        }

        protected Task ChangeGameMode()
        {
            if (string.IsNullOrWhiteSpace(GameId))
                return Task.CompletedTask;

            Logger.LogDebug("Switched My Game mode!");

            observing = !observing;

            if (observing)
                gameModeTxt = "Become Player";
            else
                gameModeTxt = "Become Observer";

            return Task.CompletedTask;
        }

        protected async Task ResetGame()
        {
            if (string.IsNullOrWhiteSpace(GameId))
                return;

            Logger.LogDebug("Game Reset!");

            await ScrumPokerHub.ResetGameAsync();
        }
        
        protected override async Task OnInitializedAsync()
        {
            userName = ProfileSvc.UserName;
            userId = ProfileSvc.UserId;

            if (string.IsNullOrWhiteSpace(userName))
                userName = await EditProfileModal.ShowAsync();

            if (GameId == null)
                NavigationManager.NavigateTo($"story-poker/{GameHelper.GenerateGameId()}", true);

            if (!string.IsNullOrWhiteSpace(GameId))
            {
                ScrumPokerHub.Init(GameId);

                ScrumPokerHub.OnReconnected += Reconnected;
                ScrumPokerHub.OnReceiveVote += ReceivedVote;
                ScrumPokerHub.OnAddPlayer += NewPlayerAdded;
                ScrumPokerHub.OnRemovePlayer += RemovePlayer;
                ScrumPokerHub.OnReceiveInitialGameState += ReceivedInitialGameState;
                ScrumPokerHub.OnGameEnded += GameEnded;
                ScrumPokerHub.OnGameReset += GameReset;

                await ScrumPokerHub.StartAsync();

                await ScrumPokerHub.JoinGameAsync();
            }
        }

        protected async void Reconnected(object sender, string connectionId)
        {
            await ScrumPokerHub.JoinGameAsync();
        }

        protected async void GameReset(object sender, PlayerEvent e)
        {
            players.ForEach(p => p.Vote = null);
            voteComplete = false;
            StateHasChanged();

            if (!observing)
            {
                var options = new ModalOptions()
                {
                    HideCloseButton = true,
                    DisableBackgroundCancel = true
                };

                var voteModal = Modal.Show<VotingModal>("Cast your vote!", options);
                var modalResult = await voteModal.Result;

                await ScrumPokerHub.VoteAsync((string) modalResult.Data);
            }
            else
            {
                await ScrumPokerHub.VoteAsync("🕵");
            }

            StateHasChanged();
        }

        protected void ReceivedInitialGameState(object sender, GameInfo e)
        {
            if (!players.Any())
                players.AddRange(e.Players);

            gameOwner = e.OwnerId;

            if (gameOwner == userId)
                exitGameTxt = "End";
            else
                exitGameTxt = "Leave";

            StateHasChanged();
        }

        protected async void GameEnded(object sender, PlayerEvent e)
        {
            var options = new ModalOptions()
            {
                HideCloseButton = true,
                DisableBackgroundCancel = true
            };

            var gameEndedModal = Modal.Show<GameEndedModal>("Game Complete", options);
            await gameEndedModal.Result;
            await ScrumPokerHub.StopAsync();

            NavigationManager.NavigateTo($"/");
        }

        protected void RemovePlayer(object sender, PlayerEvent e)
        {
            players.Remove(players.FirstOrDefault(p => p.Id == e.UserId));
            StateHasChanged();
        }

        protected void NewPlayerAdded(object sender, PlayerJoined e)
        {
            players.Add(new Player()
            {
                Id = e.UserId,
                UserName = e.UserName,
            });

            StateHasChanged();
        }

        protected void ReceivedVote(object sender, PlayerVoted e)
        {
            var player = players.FirstOrDefault(p => p.Id == e.UserId);

            if (player != null)
                player.Vote = e.Vote;

            if (players.All(p => p.Vote != null))
                voteComplete = true;

            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            await ScrumPokerHub.LeaveGameAsync()
                .ContinueWith(t => ScrumPokerHub.StopAsync());
        }
    }
}
