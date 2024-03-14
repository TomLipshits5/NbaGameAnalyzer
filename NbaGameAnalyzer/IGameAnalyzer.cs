namespace NbaGameLibrary
{   
    public interface IGameAnalyzer
    {
        public Dictionary<TeamDomesticity,  List<string>> GetAllPlayersNames();
        public List<GameAction>? GetAllPlayerActionsByName(string playersName); 
    }
}


