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
            var result = game.Move(new int[] { 1, 1}, new int[] { 2, 3 });

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
            var result = game.Move(new int[] { 2, 2 }, new int[] { 4, 7 });

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
            var result = game.Move(new int[] { 1, 7 }, new int[] { 7, 12 });

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
            var result = game.Move(new int[] { 1, 1 }, new int[] { 3, 6 });

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveOtherCountersWhenCounterOnBar_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Board[0, 0]++;
            game.Dice[0] = 2;
            game.Dice[1] = 4;

            // Act  
            var result = game.Move(new int[] { 1, 1 }, new int[] { 3, 5 });

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_FourMovesWithoutDoubleThrown_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 2;
            game.Dice[1] = 4;

            // Act  
            var result = game.Move(new int[] { 1, 1, 12, 12 }, new int[] { 3, 5, 14, 16 });

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
            var result = game.Move(new int[] { 1, 1 }, new int[] { 3, 5 });

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
            var result = game.Move(new int[] { 1, 2 }, new int[] { 2, 4 });

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_MoveBringsOnCounterFromBar_IsValid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Board[0, 0]++;
            game.Dice[0] = 2;
            game.Dice[1] = 4;

            // Act  
            var result = game.Move(new int[] { 0, 1 }, new int[] { 2, 5 });

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_FourMovesWithDoubleThrown_IsValid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            game.Dice[0] = 2;
            game.Dice[1] = 2;

            // Act  
            var result = game.Move(new int[] { 1, 1, 12, 12 }, new int[] { 3, 3, 14, 14 });

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
            game.Move(new int[] { 1, 1 }, new int[] { 2, 3 });
            game.Dice[0] = 1;
            game.Dice[1] = 4;

            // Act  
            game.Move(new int[] { 19, 19 }, new int[] { 20, 23 });

            // Assert
            Assert.AreEqual(1, game.Board[0,0]);
        }

        [TestMethod]
        public void Game_Pass_PassesTurnToOtherPlayer()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);

            // Act  
            game.Pass();

            // Assert
            Assert.AreEqual(2, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_BearOffCountersWhenOneOrMoreNotInHomeBoard_IsInvalid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            ZeroBoard(game.Board);
            game.Board[0, 24] = 5;
            game.Board[0, 23] = 5;
            game.Board[0, 22] = 4;
            game.Board[0, 15] = 1;
            game.Dice[0] = 1;
            game.Dice[1] = 2;

            // Act  
            var result = game.Move(new int[] { 23, 24 }, new int[] { 25, 25 });

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_BearOffCountersWhenAllCountersInHomeBoardWithExactRoll_IsValid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            ZeroBoard(game.Board);
            game.Board[0, 24] = 5;
            game.Board[0, 23] = 5;
            game.Board[0, 22] = 5;
            game.Dice[0] = 1;
            game.Dice[1] = 2;

            // Act  
            var result = game.Move(new int[] { 23, 24 }, new int[] { 25, 25 });

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, game.CurrentPlayer);
        }

        [TestMethod]
        public void Game_BearOffCountersWhenAllCountersInHomeBoardWithInexactRoll_IsValid()
        {
            // Arrange
            var player1 = new Player { Name = "Player 1", };
            var player2 = new Player { Name = "Player 2", };
            var game = new Game(player1, player2);
            ZeroBoard(game.Board);
            game.Board[0, 24] = 5;
            game.Board[0, 23] = 5;
            game.Board[0, 22] = 5;
            game.Dice[0] = 1;
            game.Dice[1] = 3;

            // Act  
            var result = game.Move(new int[] { 23, 24 }, new int[] { 25, 25 });

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, game.CurrentPlayer);
        }
        
        #region Test helpers

        private void ZeroBoard(int[,] board)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    board[i, j] = 0;
                }
            }
        }

        #endregion
    }
}

