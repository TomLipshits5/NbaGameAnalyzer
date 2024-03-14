namespace NbaGameLibrary;


public class JsonData
{
    public Game? game { get; set; }
}

public class Game
{
    public string? gameId { get; set; }
    public List<RawGameAction>? actions { get; set; }
}

public class RawGameAction
{
    public int actionNumber { get; set; }
    public string? clock { get; set; }
    public int period { get; set; }
    public string? actionType { get; set; }
    public string? subType { get; set; }
    public int personId { get; set; }
    public string? playerNameI { get; set; }
    public string? teamTricode { get; set; }
    public string? scoreHome { get; set; }
    public string? scoreAway { get; set; }
}