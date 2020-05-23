namespace ScrumPokerTool.Shared
{
    public static class SignalRConstants
    {
        public static string StoryPokerHub = "hubs/storypoker";

        public static string JoinGame = "JoinGame";
        public static string LeaveGame = "LeaveGame";
        public static string Vote = "Vote";
        public static string ResetGame = "ResetGame";

        public static string ReceiveVote = "ReceiveVote";
        public static string AddPlayer = "AddPlayer";
        public static string RemovePlayer = "RemovePlayer";
        
        public static string ReceiveInitialGameState = "ReceiveInitialGameState";
        public static string GameEnded = "GameEnded";
    }
}
