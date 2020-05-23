using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ScrumPokerTool.Shared;

namespace ScrumPokerTool.Client.Services
{
    public class StoryPokerHubClient
    {
        private readonly ILogger<StoryPokerHubClient> _logger;
        private readonly ProfileService _profileService;
        private HubConnection _hubConnection;
        private string _gameId;

        public StoryPokerHubClient(ILogger<StoryPokerHubClient> logger, IJSRuntime jsRuntime, NavigationManager navigationManager, ProfileService profileService)
        {
            _logger = logger;
            _profileService = profileService;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri($"/{SignalRConstants.StoryPokerHub}"))
                .WithAutomaticReconnect()
                .ConfigureLogging(builder =>
                {
                    Assembly assembly = Assembly.LoadFrom("Blazor.Extensions.Logging.dll");
                    Type logProvider = assembly.GetType("Blazor.Extensions.Logging.BrowserConsoleLoggerProvider", true, true);
                    ILoggerProvider loggingProvider = (ILoggerProvider)Activator.CreateInstance(logProvider, BindingFlags.CreateInstance, null, new[] { jsRuntime }, null);
                    builder.AddProvider(loggingProvider);
                })
                .Build();

            _hubConnection.Closed += ex =>
            {
                if (ex == null)
                    _logger.LogWarning("SignalR connection closed without error");
                else
                    _logger.LogError(ex, "SignalR connection closed with error");

                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += s =>
            {
                _logger.LogDebug($"Reconnected.");
                OnReconnected?.Invoke(this, s);
                return Task.CompletedTask;
            };

            _hubConnection.On<PlayerVoted>(SignalRConstants.ReceiveVote, (evnt) =>
            {
                var encodedMsg = $"{evnt.UserId} {evnt.Vote}";
                _logger.LogDebug($"Vote Cast: {encodedMsg}");

                OnReceiveVote?.Invoke(this, evnt);
            });

            _hubConnection.On<PlayerJoined>(SignalRConstants.AddPlayer, (evnt) =>
            {
                _logger.LogDebug($"Player Joined: {evnt.UserName}");
                OnAddPlayer?.Invoke(this, evnt);
            });

            _hubConnection.On<PlayerEvent>(SignalRConstants.RemovePlayer, (evnt) =>
            {
                _logger.LogDebug($"Player Left: {evnt.UserId}");
                OnRemovePlayer?.Invoke(this, evnt);
            });

            _hubConnection.On<GameInfo>(SignalRConstants.ReceiveInitialGameState, (evnt) =>
            {
                _logger.LogDebug("Received GameInfo!");
                OnReceiveInitialGameState?.Invoke(this, evnt);
            });

            _hubConnection.On<PlayerEvent>(SignalRConstants.GameEnded, (evnt) =>
            {
                _logger.LogDebug("Game Deleted!");
                OnGameEnded?.Invoke(this, evnt);
            });

            _hubConnection.On<PlayerEvent>(SignalRConstants.ResetGame, (evnt) =>
            {
                _logger.LogDebug("Game Reset!");
                OnGameReset?.Invoke(this, evnt);
            });
        }

        public void Init(string gameId)
        {
            _gameId = gameId;
        }

        public async Task StartAsync()
        {
            await _hubConnection.StartAsync();
        }

        public async Task StopAsync()
        {
            await _hubConnection.StopAsync();
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public event EventHandler<string> OnReconnected;
        public event EventHandler<PlayerVoted> OnReceiveVote;
        public event EventHandler<PlayerJoined> OnAddPlayer;
        public event EventHandler<PlayerEvent> OnRemovePlayer;
        public event EventHandler<GameInfo> OnReceiveInitialGameState;
        public event EventHandler<PlayerEvent> OnGameEnded;
        public event EventHandler<PlayerEvent> OnGameReset;

        public async Task JoinGameAsync()
        {
            await _hubConnection.SendAsync(SignalRConstants.JoinGame, new PlayerJoined { GameId = _gameId, UserId = _profileService.UserId, UserName = _profileService.UserName });
        }

        public async Task LeaveGameAsync()
        {
            await _hubConnection.SendAsync(SignalRConstants.LeaveGame, new PlayerEvent { GameId = _gameId, UserId = _profileService.UserId });
        }

        public async Task VoteAsync(string vote)
        {
            await _hubConnection.SendAsync(SignalRConstants.Vote, new PlayerVoted { GameId = _gameId, UserId = _profileService.UserId, Vote = vote });
        }

        public async Task ResetGameAsync()
        {
            await _hubConnection.SendAsync(SignalRConstants.ResetGame, new PlayerVoted { GameId = _gameId, UserId = _profileService.UserId });
        }
    }
}
