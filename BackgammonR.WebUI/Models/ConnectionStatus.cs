namespace BackgammonR.WebUI.Models
{
    using System.ComponentModel;

    public enum ConnectionStatus
    {
        [Description("Ready to play")]
        ReadyToPlay = 0,
        [Description("Challenging")]
        Challenging = 1,
        [Description("Challenged")]
        Challenged = 2,
        [Description("Playing")]
        Playing = 3
    }
}
