using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyMulticasting
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hi guys, this is an example about IP Multicasting.");
            Console.WriteLine("Type 'send' to send a message to the group");
            Console.WriteLine("Type 'receive' to receive a message from the group");
            Console.Write("So, what is your command? (send/receive): ");

            var command = Console.ReadLine();
            if (command == "send")
                StartSender();
            else if (command == "receive")
                StartReceiver();
        }

        private static void StartSender()
        {
            // We first create a socket as if we were creating a normal unicast UDP socket.
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // We now need to join a multicast group. Multicast IP addresses are within the Class D range of 224.0.0.0-239.255.255.255
            // We can join any of these addresses but most we will use 224.5.6.7 for example purposes.
            var ip = IPAddress.Parse("224.5.6.7");

            // We now issue the join command, the socket will be a member of the multicast group 224.5.6.7 once we have joined it.
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
            // This sets the time to live for the socket, Setting a value of 1 will mean the multicast data will not leave the local network,
            // setting it to anything above this will allow the multicast data to pass through several routers, with each router decrementing the TTL by 1
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

            // This creates the end point that allows us to send multicast data, we connect the socket to this end point
            var ipep = new IPEndPoint(ip, 4567);
            socket.Connect(ipep);

            // We now send a text to the group.
            string message;
            do
            {
                Console.WriteLine("Type a message to send or type 'exit' to exit");
                message = Console.ReadLine();
                var buffer = Encoding.UTF8.GetBytes(message);
                socket.Send(buffer, buffer.Length, SocketFlags.None);
            } while (string.Compare(message, "exit", StringComparison.OrdinalIgnoreCase) != 0);

            // Release all resources
            socket.Close();
        }

        private static void StartReceiver()
        {
            // We setup the socket in the same manner as we would for a unicast UDP socket.
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // We create an IP endpoint for the incoming data to any IP address on port 4567 and bind that to the socket.
            var ipep = new IPEndPoint(IPAddress.Any, 4567);
            socket.Bind(ipep);

            // The socket is added to the multicast group 224.5.6.7.
            var ip = IPAddress.Parse("224.5.6.7");
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

            // Waiting for the data from others in the group 224.5.6.7
            Console.WriteLine("Wating for incomming messages...");
            while (true)
            {
                var buffer = new byte[1024];
                var length = socket.Receive(buffer);
                var str = Encoding.UTF8.GetString(buffer, 0, length);
                Console.WriteLine(str.Trim());
            }
        }
    }
}