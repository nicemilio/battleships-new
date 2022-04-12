using System.Net.Sockets;
using System.Net;
using System.Text;

namespace battleships {
    public class BotCLient : PlayerCLient {
    private bool hunting = false;
    protected int[] lastHit = new int[2];
    private Random rnd = new Random();
   
    protected override void StartClient(string ipString = "127.0.0.1") {
        //"127.0.0.1"
        IPAddress ip = IPAddress.Parse(ipString);
        Console.WriteLine ("BotClient started");
        int port = 5000;
        for (int i = 0; i < 10; i++) {
            try {this.client.Connect(ip, port); break;}
            catch(SocketException) {
                Console.WriteLine("Bot failed to start, trying " + (10-i) + " more times.");
            }
        }
        this.lastShot[0] = rnd.Next(0, 10);
        this.lastShot[1] = rnd.Next(0, 10);
       // if (i > 9) exit; //TODO program exits when the connection doesnt work
        
        Thread threadReceiveData = new Thread(ReceiveData);
        threadReceiveData.Start();
    }
    protected override void ReceiveData() {
        NetworkStream ns = this.client.GetStream();
        byte[] receivedBytes = new byte[1024];
        int byte_count;
        String[] myTurnArray = {FIRST, HIT, DESTROY, RETRY};
        String[] shotResponseArray = {MISS, HIT, DESTROY};
        while (true) {

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                mData = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                mData = mData.Substring(0, mData.Length-1);
            //    Console.WriteLine ("Bot receives: " + mData);
                if (mData.Length == 3) {
                    String response = checkEnemyShot(mData);
                    SendData(response);
                    Thread.Sleep(500);
                    if (!myTurnArray.Contains(response)) Shoot();
                    break;
                }
                if (shotResponseArray.Contains(mData)) 
                    this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');
                //Assign hunting
                if (mData == HIT) {
                    this.hunting = true;
                    this.lastHit = this.lastShot;
                } else if (mData == DESTROY) this.hunting = false;
                if (myTurnArray.Contains(mData)) Shoot();
            }
        }
    }
    

    protected override void Shoot() {
        Thread.Sleep(1000);
        int nextRow, nextCol;
        Console.WriteLine("Bot is hunting: "+ this.hunting);

        if (this.hunting) {
            nextRow = this.lastHit[0];
            nextCol = this.lastHit[1];
            if (! this.enemyBoard.CheckPosition(this.lastShot[0], this.lastShot[1], 1, false, 'x', true)) {
                Console.WriteLine("Bot is on a streak and is killing your ship");
                int dir = 1;
                
                bool isVertical = this.enemyBoard.GetCoord(nextRow, nextCol - 1) == 'x' || 
                                this.enemyBoard.GetCoord(nextRow, nextCol + 1) == 'x';
                try {
                    while (this.enemyBoard.GetCoord(nextRow, nextCol) != 'w') {
                        //TODO add check for loop to these if statements
                        if (this.enemyBoard.GetCoord(nextRow, nextCol) == 'o') dir *= -1; //Check if the next coordinate was already tried
                        if (isVertical ? this.enemyBoard.GetCoord(nextRow, nextCol + (2*dir)) == 'x' :
                                            this.enemyBoard.GetCoord(nextRow + (2*dir), nextCol) == 'x') dir *= -1; //Check if a ship is 2 spots away
                        if (this.enemyBoard.GetCoord(nextRow, nextCol) == 's') break;
                        //Move along the ship until you hit water
                        if (isVertical) nextCol += dir;
                        else nextRow += dir;
                    }
                } catch (IndexOutOfRangeException) {
                    dir *= -1;
                    if (dir == 1) throw new Exception("Help, I'm stuck in a loop. Row|Col|isVert: " + nextRow + "|" + nextCol + "|" + isVertical); //TODO more information here
                }
            } else {
                Console.WriteLine("Bot got his first hit and is choosing a random direction");
                int choice = chooseDirection(nextRow, nextCol);
                Console.WriteLine("The choice is: " + choice);
                switch (chooseDirection(nextRow, nextCol)) {
                    case 0: {
                        nextCol -= 1;
                        break;
                    }
                    case 1: {
                        nextCol += 1;
                        break;
                    }
                    case 2: {
                        nextRow -= 1;
                        break;
                    }
                    case 3: {
                        nextRow += 1;
                        break;
                    }
                    default: throw new Exception("Something went wrong");
                }
                Console.WriteLine("Hunting bot will shoot: [" + nextRow + "," + nextCol + "]");
                
            }
            
        } else {
            {
                nextRow = rnd.Next(0, 10); //TODO change from 10 to board length
                nextCol = rnd.Next(0, 10);
            } while ( !this.enemyBoard.CheckPosition(nextRow, nextCol, 1, false, 'x') ||
                       this.enemyBoard.GetCoord(nextRow, nextCol) == 'o');
        }
        this.lastShot[0] = nextRow;
        this.lastShot[1] = nextCol;
        SendData(rowColToCoordinate(nextRow, nextCol));
    }

    private int chooseDirection(int theRow, int theCol) {
        //TODO refactor for DRY and less exception throwing

        //Up
        try {
            if (this.enemyBoard.GetCoord(theRow, theCol - 1) == 'w') try {
                if (this.enemyBoard.GetCoord(theRow, theCol - 2) != 'x') return 0;
            } catch (IndexOutOfRangeException) {return 0;}
        } catch (IndexOutOfRangeException) {}

        //Down
        try {
            if (this.enemyBoard.GetCoord(theRow, theCol + 1) == 'w') try {
                if (this.enemyBoard.GetCoord(theRow, theCol + 2) != 'x') return 1;
            } catch (IndexOutOfRangeException) {return 1;}
        } catch (IndexOutOfRangeException) {}

        //Left
        try {
            if (this.enemyBoard.GetCoord(theRow - 1, theCol) == 'w') try {
                if (this.enemyBoard.GetCoord(theRow - 2, theCol) != 'x') return 2;
            } catch (IndexOutOfRangeException) {return 2;}
        } catch (IndexOutOfRangeException) {}

        //Right
        try {
            if (this.enemyBoard.GetCoord(theRow + 1, theCol) == 'w') try {
                if (this.enemyBoard.GetCoord(theRow + 2, theCol) != 'x') return 3;
            } catch (IndexOutOfRangeException) {return 3;}
        } catch (IndexOutOfRangeException) {}
        return -1; //If we get here, something went wrong
    }

    private string rowColToCoordinate(int theRow, int theCol) {
        string word = "";
        word += (char)(theRow + 65);
        word += (theCol+1).ToString("00");
        return word;
    }
    }
}