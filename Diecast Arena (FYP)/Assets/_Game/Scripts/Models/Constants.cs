using System.Collections.Generic;

public class Constants {
    public const string JoinKey = "j";
    public const string DifficultyKey = "d";
    public const string GameTypeKey = "t";
    public const string HostKey = "h"; // #%

    public static readonly List<string> GameTypes = new() { "Default", "Mode 1", "Mode 2" };
    public static readonly List<string> Difficulties = new() { "Easy", "Medium", "Hard" };

    // #% My Variables
    public static readonly int MaxPlayers = 6;
}