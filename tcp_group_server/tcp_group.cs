using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Collections;

namespace tcp_group_server;
public class tcp_group
{
    private const int port = 5000;
    private static int client_id=0;
    private static Dictionary<int, TcpClient> clients = new Dictionary<int, TcpClient>();
    private static readonly object clientsLock = new object(); //wrapping the access to clients list, so that only one thread can access it at a time
    private const int MaxClients = 4;

    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine($"Group chat server started on port:{port}");
        Console.WriteLine($"Maximum clients connected:{MaxClients}");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            lock(clientsLock){
                clients.Add(++client_id,client);
            }
            Task.Run(() => HandleClient(client,client_id));
        }
    }
    public static void HandleClient(TcpClient client,int client_id)
    {
        IPEndPoint clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
        Console.WriteLine($"Client connected from {clientEndpoint.Address}:{clientEndpoint.Port}");

        NetworkStream stream = client.GetStream();
        Console.WriteLine("TCP connection is active.");
        while(true){
            byte[] buffer=new byte[1024]; //temp storage area for 1024 bytes
            int bytesRead = stream.Read(buffer,0,buffer.Length); //Read bytes form the TCP and put them into buffer
            //buffer - where to put
            //0 - start at position 0
            //buffer.length - maximum amount to read
            string recv_msg= Encoding.UTF8.GetString(buffer,0,bytesRead); //convert bytes to string
            if(bytesRead == 0){
                Console.WriteLine($"{clientEndpoint} left the chat");
                lock(clientsLock){
                    clients.Remove(client_id);
                }
                client.Close();
                break;
            }
            Console.WriteLine($"Client {client_id}: {recv_msg}");

            string tagged_msg =  $"Client {client_id}: {recv_msg}";
            Broadcast(tagged_msg,client);
        }
    }
    public static void Broadcast(string message, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        lock(clientsLock){
            foreach(TcpClient c in clients.Values)
            {
                if (c != sender)
                {
                    NetworkStream s = c.GetStream();
                    s.Write(data,0,data.Length);
                }
            }
        }
    }
}