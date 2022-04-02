using System.Net.Sockets;
using System.Net;
using System.Text;

namespace battleships {
    public class Service {
        private const string MISS = "miss!";
        private const string HIT = "hit!";
        private const string DESTROY = "destroyed!";
        private const string GAME = "Game over, you win!";
        private const string RETRY = "Bad input, shoot again";
        private const string FIRST = "first!";
        private const string SECOND = "second!";
        private board myBoard;
        private board enemyBoard;
        private board pcBoard;
        private TcpClient client;
        private string mData = "";
        private bool myTurn = false; //TODO Need some logic at the start of the game to decide who goes first
        private int[] lastShot = new int[2]; //{theRow, theCol}
       

    public Service (board myBoard, board enemyBoard, TcpClient client) {
        this.myBoard = myBoard;
        this.enemyBoard = enemyBoard;
        this.client = client;
        this.myTurn = false;
        Console.WriteLine("Starting client");
        StartClient(client);
    }

    public Service (board myBoard, board pcBoard){
        this.myBoard = myBoard;
        this.enemyBoard = new board (10, 10, false);
        this.pcBoard = pcBoard;
        Console.WriteLine("You go first.");
    }

    void StartClient(TcpClient client) {
        //"127.0.0.1"
        IPAddress ip = IPAddress.Parse("10.31.250.209");
        int port = 5000;
        string keepTrying = "y";
        while (keepTrying == "y") {
            try {client.Connect(ip, port); keepTrying = "n";}
            catch(SocketException e) {
                Console.WriteLine("Connection refused. Want to try again? y/n");
                string readLine = Console.ReadLine();
                keepTrying = String.IsNullOrEmpty(readLine) ? "y" : readLine;
            }
        }
        // if (!keepTrying) exit;
        
        Console.WriteLine("Application connected to server!");
        Thread threadReceiveData = new Thread(o => ReceiveData((TcpClient)o));
        Thread threadMyTurn = new Thread(Shoot);
        threadReceiveData.Start(client);
        threadMyTurn.Start();
    }
    void ReceiveData(TcpClient client) {
        NetworkStream ns = client.GetStream();
        byte[] receivedBytes = new byte[1024];
        int byte_count;
        while (true) {

        
        while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
        {
            mData = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
            mData = mData.Substring(0, mData.Length-1);
            this.myTurn = false;
            if (mData.Length == 3) {
                String response = checkEnemyShot(mData);
                SendData(client, response);
                this.myTurn = (response != HIT && response != DESTROY);
                if (this.myTurn) refreshConsole(mData);
                else refreshConsole("Your opponent hit ("+mData+") and is taking another turn");
                break;
            }
            String[] myTurnArray = {FIRST, HIT, DESTROY, RETRY};
            this.myTurn = myTurnArray.Contains(mData);
            String[] shotResponseArray = {MISS, HIT, DESTROY};
            if (shotResponseArray.Contains(mData))
                this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');

            refreshConsole(mData);

        }
        }
        }
    }

    void Shoot() { //TODO Need a thread to be constantly checking if its your turn(?)
        while (true) {
            //Console.WriteLine("Checking myTurn...");
            if (this.myTurn) {
                //Take the shot

                Console.WriteLine("Your turn!");

                string readLine = Console.ReadLine();
                if (readLine == "" || readLine == null) return;
                SendData(this.client, readLine);
                this.lastShot[0] = coordinateToRowCol(readLine.Substring(0, 1));
                this.lastShot[1] = coordinateToRowCol(readLine.Substring(1, 2));
                this.myTurn = false;
            }
        }
    }
    void SendData(TcpClient client, String theMessage) {
        NetworkStream ns = client.GetStream ();
        byte[] buffer = Encoding.ASCII.GetBytes(theMessage);
        ns.Write(buffer, 0, buffer.Length);
    }

    string checkEnemyShot(string shot) {
        try {
            int theRow = coordinateToRowCol(mData.Substring(0, 1)); //First character in the string
            int theCol = coordinateToRowCol(mData.Substring(1, 2)); //Second and Thid characters in the string
            switch(this.myBoard.Shoot(theRow, theCol)) {
            case('o'): return MISS;
            case('x'): return HIT;
            case('d'): return DESTROY;
            case('g'): return GAME;
            default: throw new Exception("Something went wrong");
        }
        } catch (Exception e) {
            Console.WriteLine("Your opponent entered bad coordinates, they are trying again");
            return RETRY;
        }
    }

    void refreshConsole(String message = "") {
        Console.Clear();
        Console.WriteLine("Your Board");
        this.myBoard.PrintBoard();
        Console.WriteLine("Enemy Board");
        this.enemyBoard.PrintBoard();
        Console.WriteLine(message);
    }
    //Helper method to conver battleshipe coordinates (A10) to our integers
    int coordinateToRowCol(string co) {
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