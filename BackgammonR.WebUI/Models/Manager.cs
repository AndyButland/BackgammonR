namespace BackgammonR.WebUI.Models
{
    using System.Collections.Generic;

    public class Manager
    {
        private static readonly Manager _instance = new Manager();

        private Manager()
        {
            Players = new List<Player>();
            Games = new List<Game>();
        }

        public static Manager Instance
        {
            get
            {
                return _instance;
            }
        }
        
        public List<Player> Players { get; set; }

        public List<Game> Games { get; set; }
    }
}