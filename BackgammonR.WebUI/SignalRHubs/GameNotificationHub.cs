namespace BackgammonR.WebUI.SignalRHubs
{
    using System.Collections.Generic;
    using System.Linq;
    using BackgammonR.WebUI.Models;
    using Microsoft.AspNet.SignalR;

    public class GameNotificationHub : Hub
    {
        public void GetPlayers()
        {
            Clients.Caller.loadPlayers(Manager.Instance.Players);
        }

        public void Join(string name)
        {
            if (Manager.Instance.Players.Any(x => x.Name == name))
            {
                Clients.Caller.displayError("We already have someone of that name playing.  Please choose another.");
            }
            else
            {
                var user = new Player 
                { 
                    Name = name, 
                    ConnectionId = Context.ConnectionId,
                    Status = ConnectionStatus.ReadyToPlay 
                };
                Manager.Instance.Players.Add(user);
                Clients.All.joined(user, Context.ConnectionId);
                Clients.Caller.callerJoined(name);
            }
        }

        public void Leave(string name)
        {
            Manager.Instance.Players.RemoveAll(x => x.Name == name);
            Clients.All.left(name, Context.ConnectionId);
            Clients.Caller.callerLeft();
        }

        public void Challenge(string name)
        {
            var challengingPlayer = Manager.Instance.Players
                .Where(x => x.ConnectionId == Context.ConnectionId)
                .SingleOrDefault();
            var challengedPlayer = Manager.Instance.Players
                .Where(x => x.Name == name)
                .SingleOrDefault();

            if (challengedPlayer != null)
            {
                challengingPlayer.Status = ConnectionStatus.Challenging;
                challengingPlayer.Status = ConnectionStatus.Challenged;
                Clients.All.challengeMade(challengingPlayer.Name, challengedPlayer.Name);    
            }
            else
            {
                Clients.Caller.displayError(name + " not available to be challenged.");    
            }            
        }

        public void RespondToChallenge(string name, bool accept)
        {
            var challengingPlayer = Manager.Instance.Players
                .Where(x => x.Name == name)
                .SingleOrDefault();
            var challengedPlayer = Manager.Instance.Players
                .Where(x => x.ConnectionId == Context.ConnectionId)
                .SingleOrDefault();

            if (challengingPlayer != null)
            {
                Game game = null;
                if (accept)
                {
                    game = new Game(challengingPlayer, challengedPlayer);
                    Manager.Instance.Games.Add(game);
                    challengingPlayer.Status = challengedPlayer.Status = ConnectionStatus.Playing;
                }
                else
                {
                    challengingPlayer.Status = challengedPlayer.Status = ConnectionStatus.ReadyToPlay;
                }
                Clients.All.challengeRespondedTo(challengingPlayer.Name, challengedPlayer.Name, accept, game);
            }
            else
            {
                Clients.Caller.displayError(name + " not available to be receive your challenge response.");
            }  

        }
    }
}