using System;
using System.Collections.Generic;
using System.Text;

namespace ScrumPokerTool.Shared
{
    public static class GameHelper
    {
        public static string GenerateGameId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8).ToLower();
        }
    }
}
