using System.Net.Sockets;

namespace battleships
{
    public class board
    {
        char [,] mBoard { get; set; }
        public bool mReady { get; set; }
        Random rnd = new Random();
        String mEnemyMove = "Empty";
        private int[] schiffe = {4, 3, 3, 2, 2, 2, 1, 1, 1, 1};
        private int shipCount;

        public board(int rows, int cols, bool autoPlace = false)
        {
            this.shipCount = schiffe.Length;
            mBoard = new char[rows,cols];
            FillBoard ();
            if (autoPlace)
                AutoFill ();
        }

        /**
* w = wasser
* s = schiff
* o = leer getroffen
* x = schiff getroffen
**/

        

        void StopClient(TcpClient client)
        {
            client.Client.Shutdown(SocketShutdown.Send);
            //ns.Close();
            client.Close();
            Console.WriteLine("Disconnect from server!");
            Console.ReadKey();
        }
        void AutoFill()
        {
            foreach(int i in this.schiffe)
            {
                bool suc = false;
                while(!suc)
                {
                    int randRow = rnd.Next (0, 10);
                    int randCol = rnd.Next (0, 10);
                    bool randDir = rnd.Next(0, 2) == 0 ? false : true;
                    suc = TryPlace(randRow, randCol, i, randDir);
                }
            }
        }

        void FillBoard()
        {
            for(int i = 0; i < mBoard.GetLength(0); i++)
            {
                for (int j = 0; j < mBoard.GetLength(1); j++)
                {
                    mBoard[i,j] = 'w';
                }
            }
        }

        public void PrintBoard()
        {
            for (int i = 0; i < mBoard.GetLength(0); i++)
            {
                for (int j = 0; j < mBoard.GetLength(1); j++)
                {
                    Console.Write(mBoard[i,j] + " ");
                }
                Console.WriteLine("");
            }
        }
        public char Shoot(int theRow, int theCol)
        {
            if (mBoard[theRow, theCol] == 's')
            {
                mBoard[theRow, theCol] = 'x';
                if (CheckIfDestoyed(theRow, theCol)) {
                    this.shipCount -= 1;
                    return this.shipCount == 0 ? 'g' : 'd';
                }
                return 'x';
            }
            else
            {
                mBoard[theCol, theCol] = 'o';
                return 'o';
            }
        }

        //This method is used on enemyBoard to assign hits and misses
        public void AssignChar(int theRow, int theCol, char theChar) {
            mBoard[theRow, theCol] = theChar;
        }

        private bool TryPlace (int theRow, int theCol, int theLength, bool isVertical)
        {
            if (CheckPosition(theRow, theCol, theLength, isVertical))
            {
                if (isVertical) 
                    for(int i = theRow; i < theRow + theLength; i++) mBoard[i, theCol] = 's';
                else 
                    for (int i = theCol; i < theCol + theLength; i++) mBoard[theRow, i] = 's';
                
                return true;
            }
            return false;
        }

        private bool CheckPosition(int theRow, int theCol, int theLength, bool isVertical)
        {
            int startRow =  theRow - 1 < 0 ? 0 : theRow - 1;
            int startCol =  theCol - 1 < 0 ? 0 : theCol - 1;
            int lengthRow = theRow + (isVertical ? theLength : 1);
            int lengthCol = theCol + (isVertical ? 1 : theLength); 
            int endRow = lengthRow < mBoard.GetLength(0) - 1 ? lengthRow : mBoard.GetLength(0) - 1; //Min(mBoard.GetLength, lengthRow)
            int endCol = lengthCol < mBoard.GetLength(1) - 1 ? lengthCol : mBoard.GetLength(1) - 1;

            if ((isVertical ? theRow : theCol) + theLength <= mBoard.GetLength(isVertical ? 0 : 1)) //is in Board
            {
                for (int i = startRow; i <= endRow; i++)
                {
                    for (int j = startCol; j <= endCol; j++)
                    {
                        if (mBoard[i, j] == 's')
                        {
                            return false;
                        }
                    }
                }
            }
            else return false;
            return true;
        }

        bool CheckIfDestoyed(int theRow, int theCol) {
            return CheckPosition(theRow, theCol, 1, false);
        }
    }
}

