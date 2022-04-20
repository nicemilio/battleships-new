using System.Net.Sockets;
using System.Net;
using System.Text;

namespace battleships {
    public class BotCLient : PlayerCLient {
    private bool hunting = false;
    protected int[] lastHit = {-1, -1};
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
                    if (response == GAME) break; //End the game here
                    if (!myTurnArray.Contains(response)) Shoot();
                    break;
                }
                if (shotResponseArray.Contains(mData)) 
                    this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');
                if (mData == DESTROY) this.enemyBoard.fillMisses(this.lastShot[0], this.lastShot[1]);
                //Assign hunting
                if (mData == HIT) {
                    this.hunting = true;
                    this.lastHit[0] = this.lastShot[0];
                    this.lastHit[1] = this.lastShot[1];
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
            Console.WriteLine("Bot's last hit was: [" + nextRow + "," + nextCol + "]");
            Console.WriteLine("Bot's last shot was: [" + lastShot[0] + "," + lastShot[1] + "]");
            if (! this.enemyBoard.CheckPosition(nextRow, nextCol, 1, false, 'x', true)) {
                Console.WriteLine("Bot is on a streak and is killing your ship");
                int dir = 1;
                bool isHorizontal = this.enemyBoard.checkHorizontal(nextRow, nextCol);

                while (this.enemyBoard.GetCoord(nextRow, nextCol) != 'w') {
                    if (isHorizontal) {
                        if (nextCol == this.enemyBoard.GetLength(1) - 1 ||
                            nextCol == 0 ||
                            enemyBoard.GetCoord(nextRow, Math.Min(nextCol, this.enemyBoard.GetLength(1)-1)) == 'o') { //Avoid IndexOutOfBoundsError
                                if (dir == -1) throw new Exception("Help, I'm stuck in a loop. Row|Col|isHorizontal: " + nextRow + "|" + nextCol + "|" + isHorizontal);
                                dir = -1;
                            }
                        nextCol += dir;
                    } else {
                        if (nextRow == this.enemyBoard.GetLength(0) - 1 ||
                            nextRow == 0 ||
                            enemyBoard.GetCoord(Math.Min(nextRow, this.enemyBoard.GetLength(0)-1), nextCol) == 'o') {
                                if (dir == -1) throw new Exception("Help, I'm stuck in a loop. Row|Col|isHorizontal: " + nextRow + "|" + nextCol + "|" + isHorizontal);
                                dir = -1;
                            }
                        nextRow += dir;
                    }
                }
                Console.WriteLine("While clause exited...");
            } else {
                Console.WriteLine("Bot got his first hit and is choosing a random direction");
                int choice = chooseDirection(nextRow, nextCol);
                Console.WriteLine("The choice is: " + choice);
                switch (choice) {
                    case 0: {
                        nextRow -= 1;
                        break;
                    }
                    case 1: {
                        nextRow += 1;
                        break;
                    }
                    case 2: {
                        nextCol -= 1;
                        break;
                    }
                    case 3: {
                        nextCol += 1;
                        break;
                    }
                    default: throw new Exception("Something went wrong");
                }
            }
            Console.WriteLine("Hunting bot will shoot: [" + nextRow + "," + nextCol + "]");
        } else {
            do {
                nextRow = rnd.Next(0, 10); //TODO change from 10 to board length
                nextCol = rnd.Next(0, 10);
                Console.WriteLine("Bot is trying [" + nextRow + "," + nextCol + "]");
            } while (this.enemyBoard.GetCoord(nextRow, nextCol) != 'w');
            Console.WriteLine("While clause exited...");
        }
        this.lastShot[0] = nextRow;
        this.lastShot[1] = nextCol;
        Console.WriteLine("Bot is shooting: [" + lastShot[0] + "," + lastShot[1] + "]");
        Console.WriteLine("Bot's latest hit was: [" + lastHit[0] + "," + lastHit[1] + "]");
        SendData(rowColToCoordinate(nextRow, nextCol));
    }

    private int chooseDirection(int theRow, int theCol) {
        //(0, up) (1, down) (2, left) (3. right)
        int[] options = {0, 1, 2, 3};


        if (theRow == 0) options = options.Where(val => val != 0).ToArray();
        else if (this.enemyBoard.GetCoord(theRow - 1, theCol) == 'o') options = options.Where(val => val != 0).ToArray();

        if (theRow >= this.enemyBoard.GetLength(0)-1) options = options.Where(val => val != 1).ToArray();
        else if (this.enemyBoard.GetCoord(theRow + 1, theCol) == 'o') options = options.Where(val => val != 1).ToArray();

        if (theCol == 0) options = options.Where(val => val != 2).ToArray();
        else if (this.enemyBoard.GetCoord(theRow, theCol - 1) == 'o') options = options.Where(val => val != 2).ToArray();

        if (theCol >= this.enemyBoard.GetLength(1)-1) options = options.Where(val => val != 3).ToArray();
        else if (this.enemyBoard.GetCoord(theRow, theCol + 1) == 'o') options = options.Where(val => val != 3).ToArray();
        
        Console.WriteLine("The options left are: " + string.Join(", ", options));
        if (options.Length == 0) return -1; //Something went wrong
        return options[rnd.Next(options.Length)];
    }

    private string rowColToCoordinate(int theRow, int theCol) {
        string word = "";
        word += (char)(theRow + 65);
        word += (theCol+1).ToString("00");
        return word;
    }
    }
}