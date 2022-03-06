using System;
namespace battleships
{
    public class board
    {
        char [,] mBoard { get; set; }
        public bool mReady { get; set; }
        Random rnd = new Random();

        public board(int rows, int cols, bool autoPlace = false)
        {
            mBoard = new char[rows,cols];
            fillBoard ();
            if (autoPlace)
                autoFill ();
        }
        /**
         * w = wasser
         * s = schiff
         * o = leer getroffen
         * x = schiff getroffen
        **/
        void autoFill()
        {
            int[] schiffe;
            schiffe = new int[]{4, 3, 3, 2, 2, 2, 1, 1, 1, 1};
            foreach(int i in schiffe)
            {
                bool suc = false;
                while(!suc)
                {
                    int randRow = rnd.Next (0, 10);
                    int randCol = rnd.Next (0, 10);
                    bool randDir = rnd.Next(0, 2) == 0 ? false : true;
                    Console.WriteLine("Row: " + randRow + ", Col: " + randCol + ", Length: " + i + ", RandDir: " + randDir);
                    suc = tryPlace(randRow, randCol, i, randDir);
                    Console.WriteLine("Successfull? " + suc);
                }
            }
        }

        void fillBoard()
        {
            for(int i = 0; i < mBoard.GetLength(0); i++)
            {
                for (int j = 0; j < mBoard.GetLength(1); j++)
                {
                    mBoard[i,j] = 'w';
                }
            }
        }

        public void printBoard()
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
        public char shoot(int theRow, int theCol)
        {
            if (mBoard[theRow, theCol] == 's')
            {
                mBoard[theRow, theCol] = 'x';
                if (checkIfDestroyed(theRow, theCol))
                    return 'd';
                return 'x';
            }
            else
            {
                mBoard[theCol, theCol] = 'o';
                return 'o';
            }
        }

        public bool tryPlace (int theRow, int theCol, int theLength, bool isVertical)
        {
            if (checkPosition(theRow, theCol, theLength, isVertical))
            {
                if (isVertical) 
                    for(int i = theRow; i < theRow + theLength; i++) mBoard[i, theCol] = 's';
                else 
                    for (int i = theCol; i < theCol + theLength; i++) mBoard[theRow, i] = 's';
                
                return true;
            }
            return false;
        }

        public bool checkPosition(int theRow, int theCol, int theLength, bool isVertical)
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
                        Console.WriteLine("Checking Row: " + i + " Col: " + j);
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

        bool checkIfDestroyed(int theRow, int theCol)
        {
            return checkPosition(theRow, theCol, 1, false);

        /*    for (int i = theRow - 1 < 0 ? 0 : theRow - 1; i <= (theRow + 1 > mBoard.GetLength(1) ? mBoard.GetLength(1) : theRow + 1); i++)
            {
                for (int j = theCol - 1 < 0 ? 0 : theCol - 1; j <= (theCol + 1 > mBoard.GetLength(0) ? mBoard.GetLength(0) : theCol + 1); j++)
                {
                    if (mBoard[i, j] == 's')
                        return false;
                }
            }
            return true; */
        }
    }
}

