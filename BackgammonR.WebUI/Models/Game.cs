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
            Black = player1;
            White = player2;
            StartedOn = DateTime.Now;
            InitializeBoard();
            Dice = new int[2];
	    }

        public Player Black { get; set; }

        public Player White { get; set; }

        public DateTime StartedOn { get; set; }

        public int[,] Board { get; set; }

        public int[] Dice { get; set; }

        private void InitializeBoard() 
        {
            Board = new int[NumberOfPlayers, NumberOfPoints];  // 1st dimension = player, 2nd dimension = points 
            for (int i = 1; i <= NumberOfPlayers; i++)
            {
                Board[i - 1, 1] = 2;
                Board[i - 1, 12] = 5;
                Board[i - 1, 17] = 3;
                Board[i - 1, 19] = 5;                
            }
        }

        public void RollDice()
        {
            var rnd = new Random();
            Dice[0] = rnd.Next(1, 6);
            Dice[1] = rnd.Next(1, 6);
        }

        public bool Move(int playerNumber, int from1, int to1, int from2, int to2)
        {
            if (IsMoveValid(playerNumber, from1, to1, from2, from2))
            {
                UpdateBoard(Board, playerNumber, from1, to1);
                UpdateBoard(Board, playerNumber, from2, to2);

                return true;
            }

            return false;
        }

        private void UpdateBoard(int[,] board, int playerNumber, int from, int to)
        {
            // Decrement old point            
            board[playerNumber - 1, from]--;

            // Increment new point
            board[playerNumber - 1, to]++;

            // TODO: If new point contains single opposition counter, move it back to start            
        }

        private bool IsMoveValid(int playerNumber, int from1, int to1, int from2, int to2)
        {
            var testBoard = Board;
            UpdateBoard(testBoard, playerNumber, from1, to1);
            UpdateBoard(testBoard, playerNumber, from2, to2);
            return IsBoardValidAfterMove(testBoard);
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
                    if ((board[0, j] > 0 && board[1, NumberOfPoints - j] > 0) ||
                        (board[1, j] > 0 && board[0, NumberOfPoints - j] > 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }        
    }
}