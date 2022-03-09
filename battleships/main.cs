using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using battleships;

string choice = "x";
const string UNDERLINE = "\x1B[4m";
const string RESET = "\x1B[0m";

string mData = "";

while (choice != "s" && choice != "c") {
    Console.WriteLine("Should this be a " + UNDERLINE + "s" + RESET + "erver or a " + UNDERLINE + "c" + RESET + "lient application?: ");
    choice = Console.ReadLine ();
}
Console.WriteLine("");
if (choice.Contains ("s"))
{
    Server myServer = new Server();
}
else
{
    TcpClient client = new TcpClient();
    StartClient (client);
    SendData(client, "Lol test 123");
    //board myBoard = new board(10, 10, true);
}

void StartClient(TcpClient client)
{
    IPAddress ip = IPAddress.Parse("192.168.43.84");
    int port = 5000;
    client.Connect(ip, port);
    Console.WriteLine("Application connected to server!");
    Thread thread = new Thread(o => ReceiveData((TcpClient)o));
    thread.Start(client);      
}
void ReceiveData(TcpClient client)
{
    NetworkStream ns = client.GetStream();
    byte[] receivedBytes = new byte[1024];
    int byte_count;

    while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
    {
        mData += (Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
    }
}

void SendData (TcpClient client, String theMessage)
{
    NetworkStream ns = client.GetStream ();
    byte[] buffer = Encoding.ASCII.GetBytes(theMessage);
    ns.Write(buffer, 0, buffer.Length);
}