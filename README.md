# NBA Game Analyzer

This is a C# class called **NbaGameAnalyzer** that implements the **IGameAnalyzer** interface:
```c#
namespace NbaGameLibrary
{   
    public interface IGameAnalyzer
    {
        public Dictionary<TeamDomesticity,  List<string>> GetAllPlayersNames();
        public List<GameAction>? GetAllPlayerActionsByName(string playersName); 
    }
}
``` 
The class analyzes NBA games by fetching and parsing play-by-play data from the NBA server and organizing the data into players and their actions.

## Useage Example
```c#
using NbaGameLibrary;
class Program
{
    static void Main(string[] args)
    {
        NbaGameAnalyzer game =  new NbaGameAnalyzer().CreateAsync().GetAwaiter().GetResult();
        Dictionary<TeamDomesticity, List<string>> allPlayers = game.GetAllPlayersNames();
        List<string> homeTeam = allPlayers[TeamDomesticity.home];
        string playerName = homeTeam[0];
        List<GameAction> playerActions = game.GetAllPlayerActionsByName(playerName);
        Console.WriteLine(string.Format("This are the actions that {0}, did during the game:", playerName));
        foreach(GameAction action in playerActions)
        {
            Console.WriteLine(action.GetActionType());
        }
    }
}
```
### Output
```
This are the actions that G. Williams, did during the game:
jumpball
2pt
substitution
substitution
substitution
substitution
2pt
rebound
foul
3pt
rebound
substitution
```

## Enums
### Quarter
enum that represents the four quarters of a regular game and the overtime in case the game is tied.

### TeamDomesticity
enum that represents whether a team is playing at home or away.

## Fields
```gameId (type string)```: The ID of the game to analyze.
```DomesticityToPlayersMap (type Dictionary<TeamDomesticity, List<Player>>)```: A dictionary that maps the domesticity of a team to a list of players representing that team.

## Public Methods
```GetAllPlayerActionsByName(string playersName)```: Returns a list of all actions made by a player with a specific name. This method receives a stringparameter representing the name of the player to analyze and returns a list of GameAction
objects representing all the actions made by that player.

```GetAllPlayersNames()```: Returns a dictionary of all players' names. This method returns a dictionary that maps the TeamDomesticity enum to a list of strings representing the names of all the players in that team.

## Private Methods
```fetchGameData(string GameId)```: Fetches the game data from the NBA server. This method is a private async method that receives a GameId parameter representing the game ID to fetch the data for and returns a string representing the raw data fetched from the server.

```parseRawData(string gameInfo)```: Parses the raw game data and organizes the data into players and their actions. This method receives a string parameter representing the raw data from the server.

```addActionToPlayer(int personId, string playerNameI, string teamTricode, string homeTeam, GameAction action)```: Adds an action to the player's actions list. This method receives parameters representing the player ID, player name, team tricode, current home team status, and GameAction object to add to the player's actions list.

```convertClockToTimeSpan(string durationString)```: Converts the game time clock format in the JSON data to a time span. This method receives a string parameter representing the game time clock format in the JSON data and returns a TimeSpan
representing the game time.

```getHomeTeamName(List<RawGameAction> actions)```: Gets the name of the home team. This method receives a list of RawGameAction objects and returns a string representing the name of the home team.

## Helper Classes
### Player:
A class that represents a player in the game. The class has private readonly fields to hold the player ID, name, team name, home-team status, and a list of GameAction objects representing the actions made by the player. The class has the public methods:

```c#
GetPlayerId()
GetName()
GetTeamName()
GetActions()
AddPlayerAction(GameAction action)
```

### GameAction:
A class that represents an action made by a player. The class has private readonly fields to hold the action number, game time, quarter,action type, and sub-action type. The class has the public methods:
```c#
GetActionNumber()
GetClock()
GetQuarter()
GetActionType()
GetSubActionType()
```
