
using System.Net;
using System.Net.Sockets;
using System.Text;

public class group_client{
    public static void Main(string[] args){

        TcpClient client= new TcpClient();

        String server_ip="100.100.100.10";
        int port=5000;

        client.Connect(server_ip, port);
        Console.WriteLine($"Connected to server....via:{server_ip}:{port}");
        
        NetworkStream stream =client.GetStream();

        Task.Run(() => ReceiveMessages(stream));
        Console.Write("Enter message: ");
        // String msg="Hello server";
        while(true){
            // Console.Write("Enter message: ");
            Console.WriteLine();
            string msg=Console.ReadLine();
            if(msg==""){break;}
            byte[] data=Encoding.UTF8.GetBytes(msg); //converting to bytes so that TCP can tranfer
            stream.Write(data,0,data.Length);
        }
    }
    public static void ReceiveMessages(NetworkStream stream)
    {
        while(true){
            byte[] buffer=new byte[1024]; //temp storage area for 1024 bytes
            int bytesRead = stream.Read(buffer,0,buffer.Length);
            string recv_msg= Encoding.UTF8.GetString(buffer,0,bytesRead); //convert bytes to string
            if(bytesRead == 0){
                Console.WriteLine($"Chat disconnected");
                System.Environment.Exit(0);
            }
            Console.WriteLine();
            Console.WriteLine(recv_msg);
        }
    }
}