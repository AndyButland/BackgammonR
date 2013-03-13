namespace BackgammonR.WebUI.Models
{
    using BackgammonR.WebUI.Models.Helpers;

    public class Player
    {
        public string ConnectionId { get; set; }

        public string Name { get; set; }

        public ConnectionStatus Status { get; set; }

        public string StatusLabel 
        {
            get
            {
                return Status.GetDescription();
            }
        }
    }
}