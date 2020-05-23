using System;
using System.Collections.Generic;

namespace ScrumPokerTool.Shared
{
    public class GameInfo
    {
        public GameInfo()
        {
            Players = new List<Player>();    
        }

        public string Id { get; set; }

        public string OwnerId { get; set; }

        public List<Player> Players { get; set; }
    }
}