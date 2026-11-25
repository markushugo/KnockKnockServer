using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace KnockKnockClient
{
    internal class Program
    {

        static void Main(string[] args)
        {

            int port = 5000;
            using TcpClient client = new TcpClient();
            client.Connect("localhost", port);

            Console.WriteLine("Connected to server.");

            using NetworkStream stream = client.GetStream();


            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("Server closed the connection.");
                    break;
                }

                string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Server: " + serverMessage);

                Console.Write("Your response: ");
                string? response = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(response))
                {
                    continue;
                }

                byte[] data = Encoding.UTF8.GetBytes(response);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}