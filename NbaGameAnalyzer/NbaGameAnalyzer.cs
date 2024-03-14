using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Text.Json;

namespace NbaGameLibrary
{
    //Usefull Enums
    public enum Quarter
    {
        first = 1,
        second = 2,
        third = 3,
        fourth = 4,
        overTime = 5
    }

    public enum TeamDomesticity
    {
        home = 0,
        away = 1
    }


    /// <summary>
    /// Class that analyzes NBA games and provides data about players and their actions.
    /// </summary>
    public class NbaGameAnalyzer : IGameAnalyzer
    {
        //Class Fields
        private string gameId;
        private Dictionary<TeamDomesticity, List<Player>> DomesticityToPlayersMap = [];
        public const string BaseNbaUrlFormat = "https://cdn.nba.com/static/json/liveData/playbyplay/playbyplay_{0}.json";

        //Public Methods

        /// <summary>
        /// Creates a new instance of the NbaGameAnalyzer class and fetches game data asynchronously.
        /// </summary>
        /// <param name="gameId">The ID of the NBA game to analyze.</param>
        /// <returns>A new instance of the NbaGameAnalyzer class.</returns>        
        public async Task<NbaGameAnalyzer> CreateAsync(string gameId = "0022000180")
        {
            this.gameId = gameId;
            string rawData = await fetchGameData(this.gameId);
            parseRawData(rawData);
            return this;
        }

        /// <summary>
        /// Returns a list of all game actions for a given player.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <returns>A list of game actions for the player.</returns>
        public List<GameAction> GetAllPlayerActionsByName(string playersName)
        {   
            List<GameAction>? output = [];
            List<GameAction>? playersActions = null;
            foreach(Player player in this.DomesticityToPlayersMap[TeamDomesticity.home]){
                if(playersName == player.GetName()) playersActions = player.GetActions();
                
            }
            foreach(Player player in this.DomesticityToPlayersMap[TeamDomesticity.away]){
                if(playersName == player.GetName()) playersActions = player.GetActions();
            }
            if(playersActions != null)
            {
                foreach(GameAction action in playersActions)
                {
                    output.Add(new GameAction(action));
                }
            }
            return output;
        }

        /// <summary>
        /// Returns a dictionary of all players' names on both teams.
        /// </summary>
        /// <returns>A dictionary with player names as keys and their team as values.</returns>
        public Dictionary<TeamDomesticity, List<string>> GetAllPlayersNames()
        {
            Dictionary<TeamDomesticity, List<string>> output = [];
            List<string> home = [];
            List<string> away = [];
            foreach(Player player in this.DomesticityToPlayersMap[TeamDomesticity.home]) home.Add(player.GetName());
            foreach(Player player in this.DomesticityToPlayersMap[TeamDomesticity.away]) away.Add(player.GetName());
            output[TeamDomesticity.home] = home;
            output[TeamDomesticity.away] = away;
            return output;
        }

        //Private Methods:

        /// <summary>
        /// Fatches json play by play data of the game asynchronously.
        /// </summary>
        /// <returns>A json string of the game data.</returns>
        private static async Task<string> fetchGameData(string GameId)
        {
            HttpClient client = new HttpClient(); 
            string url = string.Format(BaseNbaUrlFormat, GameId);

            for(int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch(HttpRequestException e)
                {
                    if (attempt == 2)
                    {
                        throw new Exception(string.Format("Failed fatching game data for game id: {0}", GameId));
                    }
                    string errorMsg = string.Format("Failed getting game info from server, attempt number: {0}, error: {1}", attempt + 1, e.Message);
                    Console.WriteLine(errorMsg);
                }
            }
            return "";
        }

        /// <summary>
        /// Serielize the data and parse into Player and GameAction classes, and fill assign them to the DomesticityToPlayersMap.
        /// </summary>
        private void parseRawData(string gameInfo)
        {
            this.DomesticityToPlayersMap[TeamDomesticity.home] = [];
            this.DomesticityToPlayersMap[TeamDomesticity.away] = [];
            string homeTeam;
            JsonData? gameData = JsonSerializer.Deserialize<JsonData>(gameInfo);
            List<RawGameAction>? actions = gameData?.game?.actions;
            if(actions != null){
                homeTeam = getHomeTeamName(actions);
                foreach(RawGameAction rawAction in actions)
                {
                    if(rawAction.personId != 0)
                    {
                        GameAction gameAction = new(rawAction.actionNumber, convertClockToTimeSpan(rawAction.clock), (Quarter)rawAction.period, rawAction.actionType, rawAction.subType);
                        this.addActionToPlayer(rawAction.personId, rawAction.playerNameI, rawAction.teamTricode, homeTeam, gameAction);
                        
                    }
                }
            }else{
                Console.WriteLine("Failed to deserialize Json data");
            }

        }

        /// <summary>
        /// Finds the player that match the personId in DomesticityToPlayersMap and assign the action to him, If no player is found one will be created.
        /// </summary>
        private void addActionToPlayer(int personId, string playerNameI, string teamTricode, string homeTeam, GameAction action)
        {
            bool home = teamTricode == homeTeam;
            Player? player = null;
            List<Player> team = home ? this.DomesticityToPlayersMap[TeamDomesticity.home] : this.DomesticityToPlayersMap[TeamDomesticity.away];
            foreach(Player p in team){
                if(p.GetPlayerId() == personId){
                    player = p;
                }
            }

            if(player == null)
            {
                player = new(personId, playerNameI, teamTricode, home);
                team.Add(player);
            }
            player.AddPlayerAction(action);
        }

        /// <summary>
        /// Convert durationString to a TimeSpan object (String format: P00M00S).
        /// </summary>
        /// <returns>A json string of the game data.</returns>
        private TimeSpan convertClockToTimeSpan(string durationString)
        {
            if (!durationString.StartsWith("PT"))
            {
                throw new ArgumentException("Invalid duration string format");
            }
            
            durationString = durationString.Substring(2);
            TimeSpan duration = TimeSpan.Zero;
            
            try
            {
                int minutesIndex = durationString.IndexOf('M');
                if (minutesIndex != -1)
                {
                    string minutesString = durationString.Substring(0, minutesIndex);
                    int minutes = int.Parse(minutesString);
                    duration += TimeSpan.FromMinutes(minutes);
                    durationString = durationString.Substring(minutesIndex + 1);
                }
                
                int secondsIndex = durationString.IndexOf('S');
                if (secondsIndex != -1)
                {
                    string secondsString = durationString.Substring(0, secondsIndex);
                    double seconds = double.Parse(secondsString);
                    duration += TimeSpan.FromSeconds(seconds);
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid duration string format", ex);
            }
            
            return duration;
        }


        /// <summary>
        /// Detect which team is home team based on first home score change in action.
        /// </summary>
        /// <returns>A three letter representation of the home team name.</returns>
        private string getHomeTeamName(List<RawGameAction> actions)
        {
            return actions.Where(action => action.scoreHome != null && action.scoreHome != "0").First<RawGameAction>().teamTricode;
        }
    }




    //Helper Classes

    /// <summary>
    /// Class representing a player in an NBA game.
    /// </summary>   
    public class Player(int playerId, string name, string teamName, bool home)
    {
        private readonly int playerId = playerId;
        private readonly string name = name;
        private readonly string teamName = teamName;
        private readonly bool home = home;
        private readonly List<GameAction> playerActions = [];

        public int GetPlayerId()
        {
            return this.playerId;
        }

        public string GetName()
        {
            return this.name;
        }

        public string GetTeamName()
        {
            return this.teamName;
        }

        public List<GameAction> GetActions()
        {
            return this.playerActions;
        }

        public void AddPlayerAction(GameAction action)
        {
            this.playerActions.Add(action);
        }
    }


    /// <summary>
    /// Class representing an action taken by a player in an NBA game.
    /// </summary>
    public class GameAction
    {
        private readonly int actionNumber;
        private readonly TimeSpan clock;
        private readonly Quarter quarter;
        private readonly string? actionType;
        private readonly string? subActionType;

        public GameAction(int actionNumber, TimeSpan clock, Quarter quarter, string actionType, string subActionType)
        {
            this.actionNumber = actionNumber;
            this.clock = clock;
            this.quarter = quarter;
            this.actionType = actionType;
            this.subActionType = subActionType;
        }
        
        
        public GameAction(GameAction action)
        {
            this.actionNumber = action.actionNumber;
            this.quarter = action.quarter;
            this.clock = action.clock;
            this.actionType = action.actionType;
            this.subActionType = action.subActionType;
        }

        public int GetActionNumber()
        {
            return this.actionNumber;
        }

        public TimeSpan GetClock()
        {
            return this.clock;
        }

        public Quarter GetQuarter()
        {
            return this.quarter;
        }

        public string? GetActionType()
        {
            return this.actionType;
        }

        public string? GetSubActionType()
        {
            return this.subActionType;
        }

    }
}