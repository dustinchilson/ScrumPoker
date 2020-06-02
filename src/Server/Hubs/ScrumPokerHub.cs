using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ScrumPokerTool.Shared;

namespace ScrumPokerTool.Server.Hubs
{
    public class ScrumPokerHub : Hub
    {
        private static readonly Dictionary<string, List<string>> ConnectionGameTracker = new Dictionary<string, List<string>>();
        
        private readonly GameTracker _gameTracker;

        public ScrumPokerHub(GameTracker gameTracker)
        {
            _gameTracker = gameTracker;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Vote(PlayerVoted playerVoted)
        {
            //var currentGame = GetCurrentGame(playerVoted.GameId);
            await Clients.Group(playerVoted.GameId).SendAsync(SignalRConstants.ReceiveVote, playerVoted);
        }

        public async Task ResetGame(PlayerEvent gameReset)
        {
            var currentGame = _gameTracker.Games.FirstOrDefault(g => g.Id == gameReset.GameId);
            if (currentGame != null && currentGame.OwnerId == gameReset.UserId)
                await Clients.Group(gameReset.GameId).SendAsync(SignalRConstants.ResetGame, gameReset);
        }

        public async Task JoinGame(PlayerJoined playerJoined)
        {
            bool newGame = false;
            var currentGame = _gameTracker.Games.FirstOrDefault(g => g.Id == playerJoined.GameId);
            
            if (currentGame == null)
            {
                newGame = true;
                currentGame = new GameInfo()
                {
                    Id = playerJoined.GameId, 
                    OwnerId = playerJoined.UserId
                };

                currentGame.Players.Add(new Player() { Id = playerJoined.UserId, UserName = playerJoined.UserName });

                _gameTracker.Games.Add(currentGame);
            }

            await Clients.Caller.SendAsync(SignalRConstants.ReceiveInitialGameState, currentGame);

            await Groups.AddToGroupAsync(Context.ConnectionId, playerJoined.GameId);

            if (currentGame.Players.All(p => p.Id != playerJoined.UserId) && !newGame)
            {
                currentGame.Players.Add(new Player(){ Id = playerJoined.UserId, UserName = playerJoined.UserName } );
                await Clients.Group(playerJoined.GameId).SendAsync(SignalRConstants.AddPlayer, playerJoined);
            }
        }

        public async Task LeaveGame(PlayerEvent playerLeft)
        {
            var currentGame = _gameTracker.Games.FirstOrDefault(g => g.Id == playerLeft.GameId);

            if (currentGame != null)
            {
                var targetPlayer = currentGame.Players.FirstOrDefault(p => p.Id == playerLeft.UserId);
                if (targetPlayer == null)
                    return;

                currentGame.Players.Remove(targetPlayer);

                if (currentGame.OwnerId == playerLeft.UserId)
                {
                    await Clients.Group(playerLeft.GameId).SendAsync(SignalRConstants.GameEnded, playerLeft);
                    _gameTracker.Games.Remove(currentGame);
                }
                else
                {
                    await Clients.Group(playerLeft.GameId).SendAsync(SignalRConstants.RemovePlayer, playerLeft);
                }
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, playerLeft.GameId);
        }

        private GameInfo GetCurrentGame(string gameId)
        {
            return _gameTracker.Games.FirstOrDefault(g => g.Id == gameId);
        }
    }
}
