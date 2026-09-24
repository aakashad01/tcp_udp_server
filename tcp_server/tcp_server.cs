using System.Net;
using System.Net.Sockets;
using System.Text;

public class tcp_server
{
    public static void Main(string[] args)
    {
        int port = 5000;
        TcpListener server = new TcpListener(IPAddress.Any, port); //Declared TCP listner (server)
        server.Start();
        Console.WriteLine($"Server Started... TCP Listner is open at {IPAddress.Any}:{port}");
        Console.WriteLine("Waiting for client.........");

        //Accept the connection from CLient
        TcpClient client = server.AcceptTcpClient();
        IPEndPoint clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
        Console.WriteLine($"Client connectred from {clientEndpoint.Address}:{clientEndpoint.Port}");

        NetworkStream stream = client.GetStream();
        Console.WriteLine("TCP connection is active.");
        
        while(true){
            byte[] buffer=new byte[1024]; //temp storage area for 1024 bytes
            int bytesRead = stream.Read(buffer,0,buffer.Length); //Read bytes form the TCP and put them into buffer
            //buffer - where to put
            //0 - start at position 0
            //buffer.length - maximum amount to read
            string recv_msg= Encoding.UTF8.GetString(buffer,0,bytesRead); //convert bytes to string
            Console.WriteLine($"From Client: {recv_msg}");

            Console.Write("Enter message: ");
            string reply=Console.ReadLine();
            if (reply==""){break;}
            byte[] reply_data = Encoding.UTF8.GetBytes(reply); // converting to bytes to send it through TCP
            stream.Write(reply_data, 0,reply_data.Length);
        }
     }
}