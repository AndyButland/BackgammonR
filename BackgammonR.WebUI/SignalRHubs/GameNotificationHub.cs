namespace BackgammonR.WebUI.SignalRHubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Security;
    using BackgammonR.WebUI.Models;
    using Microsoft.AspNet.SignalR;

    public class GameNotificationHub : Hub
    {
        #region Connection management

        public override Task OnConnected()
        {
            // Reconnect player if already logged on
            if (!string.IsNullOrEmpty(Context.User.Identity.Name))
            {
                var player = Manager.Instance.Players
                    .Where(x => x.Name == Context.User.Identity.Name)
                    .SingleOrDefault();

                if (player != null)
                {
                    player.ConnectionId = Context.ConnectionId;
                    Clients.Caller.updateSelf(player.Name);
                }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        #endregion

        #region Hub methods

        public void GetPlayers()
        {
            Clients.Caller.loadPlayers(Manager.Instance.Players);
        }

        public void GetGames()
        {
            Clients.Caller.loadGames(Manager.Instance.Games);
        }
        
        public void Join(string name)
        {
            if (Membership.ValidateUser(name, string.Empty))
            {
                var user = new Player 
                { 
                    Name = name, 
                    ConnectionId = Context.ConnectionId,
                    Status = ConnectionStatus.ReadyToPlay 
                };
                Manager.Instance.Players.Add(user);

                FormsAuthentication.SetAuthCookie(name, false);

                Clients.All.joined(user, Context.ConnectionId);
                Clients.Caller.callerJoined(name);                
            }
            else
            {
                Clients.Caller.displayError("We already have someone of that name playing.  Please choose another.");
            }
        }

        public void Leave(string name)
        {
            Manager.Instance.Players.RemoveAll(x => x.Name == name);
            FormsAuthentication.SignOut();

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
                    // Create game
                    game = new Game(challengingPlayer, challengedPlayer);
                    Manager.Instance.Games.Add(game);

                    // Add users to group for the game
                    Groups.Add(challengingPlayer.ConnectionId, game.Id.ToString());
                    Groups.Add(challengedPlayer.ConnectionId, game.Id.ToString());

                    // Update status of players
                    challengingPlayer.Status = challengedPlayer.Status = ConnectionStatus.Playing;
                }
                else
                {
                    // Update status of players
                    challengingPlayer.Status = challengedPlayer.Status = ConnectionStatus.ReadyToPlay;
                }

                // Notify all clients of result of challenge
                Clients.All.challengeRespondedTo(challengingPlayer.Name, challengedPlayer.Name, accept, game);
            }
            else
            {
                Clients.Caller.displayError(name + " not available to be receive your challenge response.");
            }  
        }

        public void RollDice(Guid gameId)
        {
            var game = GetGame(gameId);
            if (game != null)
            {
                game.RollDice();

                // Notify all clients in group of dice roll
                Clients.Group(game.Id.ToString()).diceRolled(game.Dice, Context.ConnectionId);
            }
        }

        public void Move(Guid gameId, int[] from, int[] to)
        {
            var game = GetGame(gameId);
            if (game != null)
            {
                if ((game.CurrentPlayer == 1 && game.Black.ConnectionId == Context.ConnectionId) ||
                    (game.CurrentPlayer == 2 && game.White.ConnectionId == Context.ConnectionId))
                {
                    if (game.Move(from, to))
                    {
                        // Notify all clients in group of move
                        Clients.Group(game.Id.ToString()).moved(game);
                    }
                    else
                    {
                        Clients.Caller.displayError("Invalid move.");
                    }
                }
                else
                {
                    Clients.Caller.displayError("Not your turn.");
                }
            }
            else
            {
                Clients.Caller.displayError("Game not found.");
            }
        }

        public void Pass(Guid gameId)
        {
            var game = GetGame(gameId);
            if (game != null)
            {
                if ((game.CurrentPlayer == 1 && game.Black.ConnectionId == Context.ConnectionId) ||
                    (game.CurrentPlayer == 2 && game.White.ConnectionId == Context.ConnectionId))
                {
                    game.Pass();

                    // Notify all clients in group of pass
                    Clients.Group(game.Id.ToString()).passed(game);
                }
                else
                {
                    Clients.Caller.displayError("Not your turn.");
                }
            }
            else
            {
                Clients.Caller.displayError("Game not found.");
            }
        }

        #endregion

        #region Helpers

        private Game GetGame(Guid gameId)
        {
            return Manager.Instance.Games
                .Where(x => x.Id == gameId)
                .SingleOrDefault();
        }

        #endregion
    }
}