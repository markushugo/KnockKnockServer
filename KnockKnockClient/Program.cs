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

            UTF8Encoding encoder = new UTF8Encoding(false);
            StreamWriter writer = new StreamWriter(stream, encoder) { AutoFlush = true };
            StreamReader reader = new StreamReader(stream, encoder);


            byte[] buffer = new byte[1024];
            try
            {

                while (true)
                {
                    string? response = reader.ReadLine();

                    if (response == null)
                    {
                        Console.WriteLine("Connection to the server was disconnected..");
                        break;
                    }

                    string serverMessage = response;

                    if (response == "THEMEGASECRETKEYTOCLOSE")
                    {
                        string? punchLine = reader.ReadLine();
                        Console.WriteLine(punchLine);
                        Console.WriteLine("Conversation finished.");
                        break;
                    }

                    Console.WriteLine("Server: " + serverMessage);


                    Console.Write("Your response: ");
                    response = Console.ReadLine();

                    if (response == null)
                    {
                        break;
                    }

                    writer.WriteLine(response);
                }
            } 
            catch (IOException)
            {
                Console.WriteLine("Server closed the connection.");
            }
        }
    }
}