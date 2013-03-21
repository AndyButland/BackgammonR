namespace BackgammonR.WebUI.Tests
{
    using System;
    using BackgammonR.WebUI.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GameTests
    {
        [TestMethod]
        public void Game_NewGame_IsInitalizedWithPlayers()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };

            // Act
            var game = new Game(player1, player2);

            // Assert
            Assert.AreEqual("Player 1", game.Black.Name);
            Assert.AreEqual("Player 2", game.White.Name);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_NewGame_IsInitalizedWithCorrectBoardLayout()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };

            // Act
            var game = new Game(player1, player2);

            // Assert
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(0, game.Board[i, 0]);
                Assert.AreEqual(2, game.Board[i, 1]);
                Assert.AreEqual(0, game.Board[i, 2]);
                Assert.AreEqual(0, game.Board[i, 3]);
                Assert.AreEqual(0, game.Board[i, 4]);
                Assert.AreEqual(0, game.Board[i, 5]);
                Assert.AreEqual(0, game.Board[i, 6]);
                Assert.AreEqual(0, game.Board[i, 7]);
                Assert.AreEqual(0, game.Board[i, 8]);
                Assert.AreEqual(0, game.Board[i, 9]);
                Assert.AreEqual(0, game.Board[i, 10]);
                Assert.AreEqual(0, game.Board[i, 11]);
                Assert.AreEqual(5, game.Board[i, 12]);
                Assert.AreEqual(0, game.Board[i, 13]);
                Assert.AreEqual(0, game.Board[i, 14]);
                Assert.AreEqual(0, game.Board[i, 15]);
                Assert.AreEqual(0, game.Board[i, 16]);
                Assert.AreEqual(3, game.Board[i, 17]);
                Assert.AreEqual(0, game.Board[i, 18]);
                Assert.AreEqual(5, game.Board[i, 19]);
                Assert.AreEqual(0, game.Board[i, 20]);
                Assert.AreEqual(0, game.Board[i, 21]);
                Assert.AreEqual(0, game.Board[i, 22]);
                Assert.AreEqual(0, game.Board[i, 23]);
                Assert.AreEqual(0, game.Board[i, 24]);
                Assert.AreEqual(0, game.Board[i, 25]);
            }            
        }

        [TestMethod]
        public void Game_MoveDiffersFromDice_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 2;
            game.Dice[1] = 4;

            // Act  
            var result = game.Move(1, 2, 1, 3);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveMatchesDiceButFromUnoccupiedPoint_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 2;
            game.Dice[1] = 5;

            // Act  
            var result = game.Move(2, 4, 2, 7);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveMatchesDiceButToPointFullyOccupiedByOwnColour_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 5;
            game.Dice[1] = 6;

            // Act  
            var result = game.Move(1, 7, 7, 12);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }


        [TestMethod]
        public void Game_MoveMatchesDiceButToOccupiedPoint_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 2;
            game.Dice[1] = 5;

            // Act  
            var result = game.Move(1, 3, 1, 6);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveTwoCounters_MatchesDiceAndToEmptyPoints_IsValid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 2;
            game.Dice[1] = 4;

            // Act  
            var result = game.Move(1, 3, 1, 5);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveOneCounter_MatchesDiceAndToEmptyPoints_IsValid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 1;
            game.Dice[1] = 2;

            // Act  
            var result = game.Move(1, 2, 2, 4);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveOntoSingleOpponentPiece_SendsToBar()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 1;
            game.Dice[1] = 2;
            game.Move(1, 2, 1, 3);
            game.Dice[0] = 1;
            game.Dice[1] = 4;

            // Act  
            game.Move(19, 20, 19, 23);

            // Assert
            Assert.AreEqual(1, game.Board[0,0]);
        }
    }
}

