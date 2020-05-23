using System.Collections.Generic;
using ScrumPokerTool.Shared;

namespace ScrumPokerTool.Server.Hubs
{
    public class GameTracker
    {
        public List<GameInfo> Games { get; } = new List<GameInfo>();
    }
}