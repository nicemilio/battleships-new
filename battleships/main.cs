using System.Net.Sockets;
using battleships;

string choice = "x";
const string UNDERLINE = "\x1B[4m";
const string RESET = "\x1B[0m";


while (choice != "s" && choice != "c") {
    Console.WriteLine("Should this be a " + UNDERLINE + "s" + RESET + "erver or a " + UNDERLINE + "c" + RESET + "lient application?: ");
    choice = Console.ReadLine();
}
Console.WriteLine("");
if (choice.Contains("s"))
{
    Server myServer = new Server();
}
else
{
    while (choice != "g" && choice != "p") {
        Console.WriteLine("Willst du gegen einen Gegner oder gegen den PC spielen?");
        choice = Console.ReadLine();
    }
    if (choice.Contains ("g")){
        TcpClient client = new TcpClient();
        board myBoard = new board(10, 10, true);
        board enemyBoard = new board(10, 10, false);
        Service service = new Service(myBoard, enemyBoard, client);
    }
    else {
        board myBoard = new board (10, 10, true);
        board pcBoard = new board (10, 10, true);
        Service service = new Service (myBoard, pcBoard);
    }
}
