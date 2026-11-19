using System.Net;
using System.Net.Sockets;
using System.Text;
using KnockKnockServer.Domain;

namespace KnockKnockServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();

            Console.WriteLine("Server started. Waiting for client...");

            TcpClient client = listener.AcceptTcpClient();
            HandleClient(client);

            listener.Stop();
        }
        public static void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                KnockKnockJoke joke = new KnockKnockJoke
                {
                    Who = "Lettuce",
                    Punchline = "Lettuce in, it's cold out here!"
                };

                KnockKnockConversation conversation = new KnockKnockConversation(joke);

                string message = "Knock Knock!";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(messageBytes, 0, messageBytes.Length);


                while (conversation.State != ConversationState.Finished)
                {
                    byte[] buffer = new byte[1024];

                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client closed the connection.");
                        break;
                    }

                    string clientResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string serverResponse = conversation.ProcessResponse(clientResponse);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(serverResponse);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client Disconected");
            }
            finally
            {
                client.Close();
            }

        }
    }
}