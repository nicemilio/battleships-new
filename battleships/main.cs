using System.Net.Sockets;
using battleships;

string? choice = "x";
const string UNDERLINE = "\x1B[4m";
const string RESET = "\x1B[0m";

while (choice != "a" && choice != "m") {
    Console.WriteLine("Do you want to play " + UNDERLINE + "a" + RESET + "lone or " + UNDERLINE + "m" + RESET + "ultiplayer?: ");
    choice = Console.ReadLine();
}
Console.WriteLine("");
if (choice.Contains("m")) {
    while (choice != "s" && choice != "c") {
        Console.WriteLine("Should this be a " + UNDERLINE + "s" + RESET + "erver or a " + UNDERLINE + "c" + RESET + "lient application?: ");
        choice = Console.ReadLine();
    }
    
    if (choice.Contains("s")) {
        Server myServer = new Server();
    } else {
        Console.WriteLine("Please enter the server's ip address: ");
        string? readLine = Console.ReadLine();
        string ip = String.IsNullOrEmpty(readLine) ? "127.0.0.1" : readLine; //TODO if IPAddress.parse can't handle the 'enter' code, remove it from readline
        PlayerCLient pc = new PlayerCLient(ip);
    }


} else {
    //TODO add threads
    Thread serverThread = new Thread(startServer);
    Thread playerThread = new Thread(startPlayerClient);
    Thread botThread = new Thread(startBotClient);
    serverThread.Start();
    botThread.Start();
    playerThread.Start();

}

void startServer() {
    Server server = new Server();
}
void startPlayerClient() {
    PlayerCLient playerCLient = new PlayerCLient();
}

void startBotClient() {
    BotCLient botCLient = new BotCLient();
}

/*
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
        PlayerCLient service = new PlayerCLient(myBoard, enemyBoard, client);
    }
    else {
        board myBoard = new board (10, 10, true);
        board pcBoard = new board (10, 10, true);
       // PlayerCLient service = new PlayerCLient (myBoard, pcBoard);
    }
}
*/