using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

public class udp_server
{
    public static void Main(string[] args)
    {
        int port = 5010;
        UdpClient server = new UdpClient(port); //creates udp socket and binds it to port 5010
        Console.WriteLine($"UDP Video Server Started... Listner is open at {IPAddress.Any}:{port}");

        IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0); //Server waits for client,from any interfaces(IPAddress.Any) on this server 
        byte[] data = server.Receive(ref clientEndpoint); //bytes recieved from clientEndpoint is stored in data, and ref fill clientEndpoint with ip and port
        Console.WriteLine($"Recieved {data.Length} bytes from {clientEndpoint}");

        int sequenceNumber = BitConverter.ToInt32(data, 0);//reads sequenceNumber of stream - 4 bytes(32 bits) of data starting at index(0-3)
        int totalPackets = BitConverter.ToInt32(data, 4);//reads totalPackets from stream - 4 bytes(32 bits) of data at index(4-7)
        Console.WriteLine($"Packet {sequenceNumber + 1} bytes from {totalPackets}");

        byte[] videoChunk = new byte[data.Length - 8];//Extract only video data , video length = total-8(sequenceNumber+totalPackets)
        Array.Copy(
            data,                           //source array
            8,                              //source index
            videoChunk,                     //destination array
            0,                              //destination index
            videoChunk.Length               //number of bytes
        );
        Console.WriteLine($"Video data in this packet: {videoChunk.Length} bytes");

        Dictionary<int, byte[]> packets = new Dictionary<int, byte[]>();// declare dictionary with int(packet sequence number),byte(video data)
        packets[sequenceNumber] = videoChunk;
        Console.WriteLine($"Stored packets:{packets.Count}/{totalPackets}");
        //Recieved only first packet as of now
        
        
        //receive remaning packets
        while (packets.Count < totalPackets)
        {
            byte[] packet = server.Receive(ref clientEndpoint);
            int sequence = BitConverter.ToInt32(packet, 0);
            byte[] chunk = new byte[packet.Length - 8];

            Array.Copy(
                packet,
                8,
                chunk,
                0,
                chunk.Length
            );
            if (!packets.ContainsKey(sequence))
            {
                packets[sequence] = chunk;
            }
            Console.WriteLine($"[{sequence+1}] Received packet {packets.Count}/{totalPackets}");
            if (packets.Count == 450)
            {
                Thread.Sleep(10000);
                //Thread.SpinWait(10000);
                Console.WriteLine("Ulla vantan .....");
                //server.Close();
            }

        }

        string video_output = "received_video.mp4";
        using FileStream output =                   //filestream allows to write bytes to a file
            new FileStream(
                video_output,               //create the file, if already exsits replace it
                FileMode.Create,
                FileAccess.Write
            );

        //write the chunks in order
        for (int i = 0; i < totalPackets; i++)
        {
            output.Write(
                packets[i],             //bytes to write
                0,                      //starting index
                packets[i].Length       //number of byte to write
            );
        }

        Console.WriteLine($"Video Received sucessfully:{video_output}");

        output.Close();
        server.Close();




    }
}