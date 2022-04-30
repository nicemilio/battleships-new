using System.Text;
using System.Net;
using System.Net.Sockets;

namespace battleships
{
    internal class Server
    {
        static readonly object _lock = new object();
        static readonly List<TcpClient[]> lobby_list = new List<TcpClient[]>();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        private bool consoleWrite;
        private Random rnd = new Random();

        public Server(bool consoleWrite = true)
        {
            int count = 0;
            this.consoleWrite = consoleWrite;
            lobby_list.Add(new TcpClient[2]);
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();
            if (this.consoleWrite) Console.WriteLine("Server started!");
            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);
                //Add the client to a new lobby
                lock (_lock) 
                {
                   TcpClient[] lobby = lobby_list.Last();
                   if (lobby.ElementAt(1) != null) {
                       //If the last lobby is full, make a new one
                       lobby = new TcpClient[2];
                       lobby_list.Add(lobby);
                   }
                   if (lobby[0] == null) lobby[0] = client;
                   else {
                       lobby[1] = client;
                       startgame(lobby);
                   }
                   if (this.consoleWrite) Console.WriteLine("Someone connected!!");
                }

                Thread t = new Thread((o) => handle_clients(o));
                t.Start(count);
                count++;
            }
        }

        public void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = list_clients[id];
            TcpClient[] lobby = lobby_list.Find(lobby => lobby.Contains(client));
            Console.WriteLine(lobby == null);
            
            
            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                if (byte_count == 0) break;
                
                //Find partner
                TcpClient partner = lobby[0] == client ? lobby[1] : lobby[0];

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data, partner);
                if (this.consoleWrite) Console.WriteLine(data);
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
            if (lobby != null) {
                if (lobby[0] != null) 
                    if (lobby[0].Connected) {
                        broadcast("disconnect!", lobby[0]);
                        lobby[0].Client.Shutdown(SocketShutdown.Both);
                        lobby[0].Close();
                    }
                if (lobby[1] != null) 
                    if (lobby[1].Connected) {
                        broadcast("disconnect!", lobby[1]);
                        lobby[1].Client.Shutdown(SocketShutdown.Both);
                        lobby[1].Close();
                    }
                lobby_list.Remove(lobby);
            }
        }

        private void startgame(TcpClient[] lobby) {
            if (this.consoleWrite) Console.WriteLine("starting the game");
            bool randChoice = rnd.Next(0, 2) == 0;
            broadcast("first!", lobby[randChoice ? 0 : 1]);
            broadcast("second!", lobby[randChoice ? 1 : 0]);
        }

        public void broadcast(string data, TcpClient client)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
        }
    }
}