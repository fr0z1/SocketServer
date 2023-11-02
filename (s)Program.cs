using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServer
{
    class Program
    {
        private static List<Client> clients = new List<Client>();
        private static TcpListener tcpListener = new TcpListener(
        IPAddress.Parse("127.0.0.1"),8080);

        static void Main(string[] args)
        {
            tcpListener.Start();
            while (true)
            {
                Socket clientSocket = tcpListener.AcceptSocket();
                Client client = new Client(clientSocket);
                if (client.socket.Connected)
                {
                    clients.Add(client);
                    AddClient(client);

                    Thread ServerConsoleTask = new Thread(() => ServerConsole());
                    ServerConsoleTask.Start();
                }
            }
        }

        private static void ServerConsole()
        {
            while (true)
            {
                if (Console.ReadLine() == "/exit")
                {
                    System.Environment.Exit(1);
                }
            }
        }

        static async Task AddClient(Client client) => await Listeners(client);

        static async Task Listeners(Client client)
        {
            await Task.Run(() =>
            {
                Socket socket = client.socket;
                try
                {
                    string name = ReceiveString(client);
                    client.name = name;

                    Console.WriteLine("\n" + "Client:" + client.endPoint + " connected.");
                    Console.WriteLine("Client name: " + name + "\n" + "Time: " + DateTime.Now + "\n");

                    SendString(socket, "Подключено");

                    while (true)
                    {
                        SendAll(socket, ReceiveString(client), name);
                    }
                }
                catch
                {
                    Console.WriteLine("\n" + "Client:" + client.endPoint + " disconnected.");
                    Console.WriteLine("Client name: " + client.name + "\n" + "Time: " + DateTime.Now + "\n");
                    clients.Remove(client);
                    client.Dispose();
                }
            });
        }

        static void SendAll(Socket socket, string massenge, string name)
        {
            if (massenge == null) return;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].socket != socket)
                {
                    string reply = name + ": " + massenge;
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    clients[i].socket.Send(msg);
                    Console.WriteLine(reply);
                }
            }
        }

        static public void SendString(Socket socket, string massenge)
        {
            byte[] msg = Encoding.UTF8.GetBytes(massenge);
            socket.Send(msg);
        }

        static public string ReceiveString(Client client)
        {
            string data = null;
            byte[] bytes = new byte[1024];
            int bytesRec = client.socket.Receive(bytes);
            data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
            if(data == "/exit")
            {
                Console.WriteLine("Client:" + client.endPoint + " disconnected.");
                clients.Remove(client);
                client.Dispose();
            }
            return data;
        }
    }
}
