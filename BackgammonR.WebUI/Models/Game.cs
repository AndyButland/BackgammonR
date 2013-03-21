namespace BackgammonR.WebUI.Models
{
    using System;

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

        public bool Move(int from1, int to1, int from2, int to2)
        {
            if (IsMoveValid(from1, to1, from2, to2))
            {
                UpdateBoard(Board, from1, to1);
                UpdateBoard(Board, from2, to2);
                if (CurrentPlayer == 1)
                {
                    CurrentPlayer = 2;
                }
                else
                {
                    CurrentPlayer = 1;
                }

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

        private bool IsMoveValid(int from1, int to1, int from2, int to2)
        {
            // Validate matches to roll
            if ((to1 - from1 == Dice[0] && to2 - from2 == Dice[1]) || 
                (to1 - from1 == Dice[1] && to2 - from2 == Dice[0]) ||
                ((to1 - from1) + (to2 - from2)) == Dice[0] + Dice[1])
            {
                // Validate first from point occupied by pieces of right colour
                if (Board[CurrentPlayer - 1, from1] > 0)
                {
                    // Validate position of counters after move
                    var testBoard = (int[,])Board.Clone();
                    UpdateBoard(testBoard, from1, to1);
                    UpdateBoard(testBoard, from2, to2);
                    return IsBoardValidAfterMove(testBoard);
                }
            }

            return false;
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
    }
}