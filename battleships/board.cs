using System;
namespace battleships
{
    public class board
    {
        char [,] mBoard { get; set; }
        public bool mReady { get; set; }
        Random rnd = new Random();

        public board(int x, int y, bool autoPlace = false)
        {
            mBoard = new char[x,y];
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
                    int randX = rnd.Next (0, 10);
                    int randY = rnd.Next (0, 10);
                    bool randDir = rnd.Next(0, 2) == 0 ? false : true;
                    suc = tryPlace(randX, randY, i, randDir);
                    Console.WriteLine("X: " + randX + " Y: " + randY + " Length: " + i + " RandDir: " + randDir + " Successfull ?" + suc);
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
                    Console.Write(mBoard[i,j]);
                }
                Console.WriteLine("");
            }
        }
        public char shoot(int theX, int theY)
        {
            if (mBoard[theX, theY] == 's')
            {
                mBoard[theX, theY] = 'x';
                if (checkIfDestroyed(theX, theY))
                    return 'd';
                return 'x';
            }
            else
            {
                mBoard[theX, theY] = 'o';
                return 'o';
            }
        }

        public bool tryPlace (int theX, int theY, int theLength, bool isVertical)
        {
            if (checkPosition(theX, theY, theLength, isVertical))
            {
                for(int i = (isVertical ? theY : theX); i <= (isVertical ? theX + theLength : 1); i++)
                {
                    for(int j = (isVertical ? theY : theX); j <= (isVertical ? 1 : theY + theLength); j++)
                    {
                        mBoard[i, j] = 's';
                    }
                    return true;
                }
            }
            return false;
        }

        bool checkPosition(int theX, int theY, int theLength, bool isVertical)
        {
            bool isClear = true;
            if ((isVertical ? theY : theX) + theLength < mBoard.GetLength(isVertical ? 1 : 0)) //is in Board
            {
                for (int i = theX - 1 < 0 ? 0 : theX - 1; i < (theX + 1 > mBoard.GetLength(1) ? theX + (isVertical ? theLength : 1) : mBoard.GetLength(1)) ; i++)
                {
                    for (int j = theY - 1 < 0 ? 0 : theY - 1; j < (theY + 1 > mBoard.GetLength(0) ? theY + (isVertical ? 1 : theLength) : mBoard.GetLength(0)) ; j++)
                    {
                        if (mBoard[i, j] == 's')
                        {
                            isClear = false;
                            break;
                        }
                    }
                }
            }
            else
                isClear = false;
            return isClear;
        }

        bool checkIfDestroyed(int theX, int theY)
        {
            for (int i = theX - 1 < 0 ? 0 : theX - 1; i <= (theX + 1 > mBoard.GetLength(1) ? mBoard.GetLength(1) : theX + 1); i++)
            {
                for (int j = theY - 1 < 0 ? 0 : theY - 1; j <= (theY + 1 > mBoard.GetLength(0) ? mBoard.GetLength(0) : theY + 1); j++)
                {
                    if (mBoard[i, j] == 's')
                        return false;
                }
            }
            return true;
        }
    }
}

