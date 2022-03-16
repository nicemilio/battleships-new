using System.Net.Sockets;
using System.Net;
using System.Text;

namespace battleships {
    public class Service {
        private const string MISS = "miss!";
        private const string HIT = "hit!";
        private const string DESTROY = "destroyed!";
        private const string GAME = "game!";
        private const string RETRY = "retry!";
        private const string FIRST = "first!";
        private const string SECOND = "second!";
        private board myBoard;
        private board enemyBoard;
        private TcpClient client;
        private string mData = "";
        private bool myTurn; //TODO Need some logic at the start of the game to decide who goes first
        private int[] lastShot = new int[2]; //{theRow, theCol}
       

    public Service(board myBoard, board enemyBoard, TcpClient client) {
        this.myBoard = myBoard;
        this.enemyBoard = enemyBoard;
        this.client = client;
        this.myTurn = true;
        Console.WriteLine("Starting client");
        StartClient(client);
        SendData(client, "Connect");
    }

    void StartClient(TcpClient client) {
        //"127.0.0.1"
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        int port = 5000;
        string keepTrying = "y";
        while (keepTrying == "y") {
            try {client.Connect(ip, port); keepTrying = "n";}
            catch(SocketException e) {
                //Console.WriteLine("Connection refused. Want to try again? y/n");
                //string readLine = Console.ReadLine();
                //keepTrying = String.IsNullOrEmpty(readLine) ? "y" : readLine;
            }
        }
        // if (!keepTrying) exit;
        
        Console.WriteLine("Application connected to server!");
        Thread threadReceiveData = new Thread(o => ReceiveData((TcpClient)o));
        Thread threadMyTurn = new Thread(Shoot);
        threadReceiveData.Start(client);      
    }
    void ReceiveData(TcpClient client) {
        NetworkStream ns = client.GetStream();
        byte[] receivedBytes = new byte[1024];
        int byte_count;

        while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
        {
            if (Encoding.ASCII.GetString(receivedBytes, 0, byte_count) == "?") mData = "";
            else mData += (Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            
            switch(mData) { //TODO finsh different scenerios
                case(FIRST): {
                    Console.WriteLine("You go first!");
                    this.myTurn = true;
                    return;
                }
                case(SECOND): {
                    Console.WriteLine("You go second!");
                    this.myTurn = false;
                    return;
                }
                case(MISS): {
                    Console.WriteLine(mData);
                    this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], 'o');
                    this.myTurn = true;
                    return;
                }
                case(HIT): 
                case(DESTROY): {
                    Console.WriteLine(mData);
                    this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], 'x');
                    this.myTurn = true;
                    return;
                }
                case(RETRY): {
                    Console.WriteLine("Bad input, shoot again");
                    this.myTurn = true;
                    return;
                }
                case(GAME): {
                    Console.WriteLine("Game over, you win!");
                    return;
                }
            }
            if (mData.Length == 3) SendData(client, checkEnemyShot(mData));
            else {
                SendData(client, RETRY);
            }
            

        }
    }

    void Shoot() { //TODO Need a thread to be constantly checking if its your turn(?)
        if (this.myTurn) {
            //Take the shot
            Console.WriteLine("Your turn!");
            string readLine = Console.ReadLine();
            if (readLine == "" || readLine == null) return;
            SendData(this.client, readLine);
            this.myTurn = false;
        }
    }
    void SendData(TcpClient client, String theMessage) {
        NetworkStream ns = client.GetStream ();
        byte[] buffer = Encoding.ASCII.GetBytes("?"+theMessage);
        ns.Write(buffer, 0, buffer.Length);
    }

    string checkEnemyShot(string shot) {
        int theRow = coordinateToRowCol(mData.Substring(0, 1)); //First character in the string
        int theCol = coordinateToRowCol(mData.Substring(1, 2)); //Second and Thid characters in the string
        switch(this.myBoard.Shoot(theRow, theCol)) {
            case('o'): return MISS;
            case('x'): return HIT;
            case('d'): return DESTROY;
            case('g'): return GAME;
            default: throw new Exception("Something went wrong");
        }

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