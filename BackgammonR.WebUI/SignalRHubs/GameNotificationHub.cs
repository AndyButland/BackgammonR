namespace BackgammonR.WebUI.SignalRHubs
{
    using System;
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

        public void GetGames()
        {
            Clients.Caller.loadGames(Manager.Instance.Games);
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
                    // Create game
                    game = new Game(challengingPlayer, challengedPlayer);
                    Manager.Instance.Games.Add(game);

                    // Add users to group for the game
                    Groups.Add(challengedPlayer.ConnectionId, game.Id.ToString());
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

        public void Move(Guid gameId, int from1, int to1, int from2, int to2)
        {
            var game = Manager.Instance.Games
                .Where(x => x.Id == gameId)
                .SingleOrDefault();
            if (game != null)
            {
                int playerNumber = 0;
                if (game.Black.ConnectionId == Context.ConnectionId)
                { 
                    playerNumber = 1;
                } 
                else
                {
                    if (game.White.ConnectionId == Context.ConnectionId) {
                        playerNumber = 2;
                    }
                }

                if (playerNumber > 0)
                {
                    if (game.Move(playerNumber, from1, to1, from2, to2))
                    {
                        // Notify all clients in group of move
                        Clients.Group(game.Id.ToString()).moved(playerNumber, from1, to1, from2, to2);
                    }
                    else
                    {
                        Clients.Caller.displayError("Invalid move.");
                    }
                }
                else
                {
                    Clients.Caller.displayError("Player not playing game.");
                }
            }
            else
            {
                Clients.Caller.displayError("Game not found.");
            }
        }
    }
}