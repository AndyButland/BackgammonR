namespace BackgammonR.WebUI.Models
{
    using System;
    using System.Linq;

    public class Game
    {
        private const int NumberOfPlayers = 2;
        private const int NumberOfPoints = 26;      // (24 + 1 for the bar + 1 for off the board)
        private const int MaxNumberOfCountersPerPoint = 5;

        public Game (Player player1, Player player2)
	    {
            Id = Guid.NewGuid();
            Black = player1;
            White = player2;
            CurrentPlayer = 1;
            StartedOn = DateTime.Now;
            InitializeBoard();
            Dice = new int[2];
	    }

        public Guid Id { get; set; }

        public Player Black { get; set; }

        public Player White { get; set; }

        public int CurrentPlayer { get; set; }

        public DateTime StartedOn { get; set; }

        public int[,] Board { get; set; }

        public int[] Dice { get; set; }

        private void InitializeBoard() 
        {
            Board = GetNewBoard();
            for (int i = 1; i <= NumberOfPlayers; i++)
            {
                Board[i - 1, 1] = 2;
                Board[i - 1, 12] = 5;
                Board[i - 1, 17] = 3;
                Board[i - 1, 19] = 5;
            }
        }

        private int[,] GetNewBoard() 
        {
            return new int[NumberOfPlayers, NumberOfPoints];
        }

        public void RollDice()
        {
            var rnd = new Random();
            Dice[0] = rnd.Next(1, 6);
            Dice[1] = rnd.Next(1, 6);
        }

        public bool Move(int[] from, int[] to)
        {
            if (IsMoveValid(from, to))
            {
                for (int i = 0; i < from.Length; i++)
                {
                    UpdateBoard(Board, from[i], to[i]);
                }

                UpdateCurrentPlayer();

                return true;
            }

            return false;
        }

        private void UpdateBoard(int[,] board, int from, int to)
        {
            // Decrement old point            
            board[CurrentPlayer - 1, from]--;

            // Increment new point
            board[CurrentPlayer - 1, to]++;

            // If new point contains single opposition counter, move it back to start            
            var otherPlayer = CurrentPlayer == 1 ? 2 : 1;
            if (board[otherPlayer - 1, NumberOfPoints - to - 1] == 1)
            {
                board[otherPlayer - 1, NumberOfPoints - to - 1]--;
                board[otherPlayer - 1, 0]++;
            }
        }

        private bool IsMoveValid(int[] from, int[] to)
        {
            // Valid numbers
            if (DoesMoveContainValidNumbers(from, to))
            {
                // Validate number of moves with dice roll
                if (DoesNumberOfMovesMatchDiceDoubleStatus(from))
                {
                    // Validate matches to roll
                    if (DoesMoveMatchRoll(from, to))
                    {
                        // Validate first from point occupied by pieces of right colour
                        if (DoesMoveStartFromPointWithCountersOfCorrectColor(from[0]))
                        {
                            // Validate moves any counter that's on the bar
                            if (DoesMoveBringOnCounterFromBar(from))
                            {
                                // Validate position of counters after move
                                var testBoard = (int[,])Board.Clone();
                                for (int i = 0; i < from.Length; i++)
                                {
                                    UpdateBoard(testBoard, from[i], to[i]);
                                }

                                return IsBoardValidAfterMove(testBoard);
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool DoesMoveContainValidNumbers(int[] from, int[] to)
        {
            for (int i = 0; i < from.Length; i++)
            {
                if (from[i] < 0 || from[i] > NumberOfPoints || to[i] < 1 || to[i] > NumberOfPoints)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DoesNumberOfMovesMatchDiceDoubleStatus(int[] from)
        {
            return from.Length == 2 && Dice[0] != Dice[1] || from.Length == 4 && Dice[0] == Dice[1];
        }

        private bool DoesMoveMatchRoll(int[] from, int[] to)
        {
            // Check single move
            var result = (to[0] - from[0] == Dice[0] && to[1] - from[1] == Dice[1]) ||
                (to[0] - from[0] == Dice[1] && to[1] - from[1] == Dice[0]) ||
                ((to[0] - from[0]) + (to[1] - from[1])) == Dice[0] + Dice[1];

            // Check double move if present
            if (from.Length > 2)
            {
                result = result && to[2] - from[2] == Dice[0] && to[3] - from[3] == Dice[0];
            }

            return result;
        }

        private bool DoesMoveStartFromPointWithCountersOfCorrectColor(int from1)
        {
            return Board[CurrentPlayer - 1, from1] > 0;
        }

        private bool DoesMoveBringOnCounterFromBar(int[] from)
        {
            return Board[CurrentPlayer - 1, 0] == 0 || from.Contains(0);
        }

        private bool IsBoardValidAfterMove(int[,] board)
        {
            return DoAllPointsHaveAllowedNumberOfCounters(board) && 
                AreAllPointsOccupiedBySingleColour(board);
        }

        private bool DoAllPointsHaveAllowedNumberOfCounters(int[,] board)
        {
            for (int i = 1; i <= NumberOfPlayers; i++)
            {
                for (int j = 0; j < NumberOfPoints; j++)
                {
                    if (j > 0 && j < NumberOfPoints - 1)    // skip the bar and off the board
                    {
                        if (board[i - 1, j] > MaxNumberOfCountersPerPoint)
                        {
                            return false;
                        }
                    }
                }                
            }

            return true;
        }

        private bool AreAllPointsOccupiedBySingleColour(int[,] board)
        {
            for (int j = 0; j < NumberOfPoints; j++)
            {
                if (j > 0 && j < NumberOfPoints - 1)    // skip the bar and off the board
                {
                    if ((board[0, j] > 0 && board[1, NumberOfPoints - j - 1] > 0) ||
                        (board[1, j] > 0 && board[0, NumberOfPoints - j - 1] > 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Pass()
        {
            UpdateCurrentPlayer();
        }

        private void UpdateCurrentPlayer()
        {
            if (CurrentPlayer == 1)
            {
                CurrentPlayer = 2;
            }
            else
            {
                CurrentPlayer = 1;
            }
        }
    }
}