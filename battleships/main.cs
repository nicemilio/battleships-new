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
    TcpClient client = new TcpClient();
    board myBoard = new board(10, 10, true);
    board enemyBoard = new board(10, 10, false);
    Service service = new Service(myBoard, enemyBoard, client);
}
