using System.Net.Sockets;
using System.Net;
using System.Text;

namespace battleships {
    public class PlayerCLient {
        protected const string MISS = "miss!";
        protected const string HIT = "hit!";
        protected const string DESTROY = "destroyed!";
        protected const string GAME = "Game over, you win!";
        protected const string RETRY = "Bad input, shoot again";
        protected const string FIRST = "first!";
        protected const string SECOND = "second!";
        protected const string DISCONNECT = "disconnect!";
        protected board myBoard;
        protected board enemyBoard;
        protected TcpClient client;
        protected string mData = "";
        protected string message = "";
        protected int[] lastShot = new int[2]; //{theRow, theCol}

    public PlayerCLient (string ipString = "127.0.0.1") {
        this.client = new TcpClient();
        this.myBoard = new board(10, 10, true);
        this.enemyBoard = new board(10, 10, false);
        this.myBoard.PrintBoard();
        Console.WriteLine("Starting client");
        StartClient(ipString);
    }


    protected virtual void StartClient(string ipString) {
        //"127.0.0.1"
        IPAddress ip = IPAddress.Parse(ipString);
        int port = 5000;
        string keepTrying = "y";
        while (keepTrying == "y") {
            try {this.client.Connect(ip, port); keepTrying = "n";}
            catch(SocketException) {
                Console.WriteLine("Connection refused. Want to try again? y/n");
                string? readLine = Console.ReadLine();
                keepTrying = String.IsNullOrEmpty(readLine) ? "y" : readLine;
            }
        }
        // if (!keepTrying) exit;
        
        Console.WriteLine("Application connected to server!");
        IPEndPoint endpoint = this.client.Client.RemoteEndPoint as IPEndPoint;
        Console.WriteLine(endpoint.Address);
        Thread threadReceiveData = new Thread(ReceiveData);
       // Thread threadMyTurn = new Thread(Shoot);
        threadReceiveData.Start();
       // threadMyTurn.Start();
    }

    protected void StopClient()
        {
            this.client.Client.Shutdown(SocketShutdown.Send);
            this.client.Close();
            Console.ReadKey();
        }
    protected virtual void ReceiveData() {
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
            if (mData.Length == 3) {
                Console.WriteLine("Checking enemy shot");
                String response = checkEnemyShot(mData);
                Console.WriteLine("Response is: " + response);
                SendData(response);
                if (response == GAME) {
                    this.message += ("Your opponent hit ("+mData+") and won the game!\n");
                    RefreshConsole();
                    StopClient();
                    return;
                } else if (myTurnArray.Contains(response)) {
                    this.message += ("Your opponent hit ("+mData+") and is taking another turn\n");
                    RefreshConsole();
                }
                else {
                    this.message += ("Your opponent shot (" + mData + ") and missed!\n");
                    RefreshConsole();
                    Shoot();
                }
                break;
            }
            
            
            if (shotResponseArray.Contains(mData)) 
                this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');
            if (mData == DESTROY) this.enemyBoard.fillMisses(this.lastShot[0], this.lastShot[1]);
            this.message = mData+'\n';
            RefreshConsole();
            if (mData == GAME || mData == DISCONNECT) {
                StopClient();
                return;
            }
            if (myTurnArray.Contains(mData)) Shoot();
        }
        }
    }
    

    protected virtual void Shoot() { //TODO Need a thread to be constantly checking if its your turn(?)
        bool success = false;
        while (!success) {
            this.message = "\n";
            string? readLine = Console.ReadLine();
            if (readLine == "" || readLine == null) continue;
            try {
                this.lastShot[0] = coordinateToRowCol(readLine.Substring(0, 1));
                this.lastShot[1] = coordinateToRowCol(readLine.Substring(1, 2));
            } catch (Exception) {
            //    Console.WriteLine(e.StackTrace);
                Console.WriteLine(RETRY);
                continue;
            }
            SendData(readLine);
            success = true;
        }
    }
    protected void SendData(String theMessage) {
        NetworkStream ns = this.client.GetStream ();
        byte[] buffer = Encoding.ASCII.GetBytes(theMessage);
        ns.Write(buffer, 0, buffer.Length);
    }

    protected string checkEnemyShot(string shot) {
        try {
            int theRow = coordinateToRowCol(shot.Substring(0, 1)); //First character in the string
            int theCol = coordinateToRowCol(shot.Substring(1, 2)); //Second and Third characters in the string
            Console.WriteLine("Checking shot ["+theRow+","+theCol+"]");
            switch(this.myBoard.Shoot(theRow, theCol)) {
            case('o'): return MISS;
            case('x'): return HIT;
            case('d'): return DESTROY;
            case('g'): return GAME;
            default: throw new Exception("Something went wrong");
        }
        } catch (Exception e) {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("Your opponent entered bad coordinates, they are trying again");
            return RETRY;
        }
    }

    private void RefreshConsole() {
        Console.Clear();
        //Console.WriteLine();
        Console.WriteLine("Your Board");
        this.myBoard.PrintBoard();
        Console.WriteLine("Enemy Board");
        this.enemyBoard.PrintBoard();
        Console.Write(this.message);
    }

    //Helper method to conver battleshipe coordinates (A10) to our integers
    protected int coordinateToRowCol(string co) {
        switch(co) {
            case("01"):
            case("A"):
            case("a"): return 0;
            case("02"):
            case("B"):
            case("b"): return 1;
            case("03"):
            case("C"):
            case("c"): return 2;
            case("04"):
            case("D"):
            case("d"): return 3;
            case("05"):
            case("E"):
            case("e"): return 4;
            case("06"):
            case("F"):
            case("f"): return 5;
            case("07"):
            case("G"):
            case("g"): return 6;
            case("08"):
            case("H"):
            case("h"): return 7;
            case("09"):
            case("I"):
            case("i"): return 8;
            case("10"):
            case("J"):
            case("j"): return 9;
            default: throw new Exception("Not a valid coordinate");
        }  
    }
    }
}