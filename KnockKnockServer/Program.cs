using System.Net;
using System.Net.Sockets;
using System.Text;
using KnockKnockServer.Domain;
using KnockKnockServer.Selection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using KnockKnockServer.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KnockKnockServer
{
    public class Program
    {
        private static int currentClients = 0;

        static void Main(string[] args)
        {
            var hostBuilder = Host.CreateApplicationBuilder(args);

            IConfiguration configuration = hostBuilder.Configuration;

            hostBuilder.Services.Configure<ServerOptions>(configuration.GetSection("Server"));
            hostBuilder.Services.AddOptions<ServerOptions>().ValidateDataAnnotations();

            hostBuilder.Services.Configure<JokeSelectionOptions>(configuration.GetSection("JokeSelection"));
            hostBuilder.Services.AddOptions<JokeSelectionOptions>().ValidateDataAnnotations();

            hostBuilder.Services.Configure<JokesOptions>(configuration.GetSection("Jokes"));
            hostBuilder.Services.AddOptions<JokesOptions>().ValidateDataAnnotations();

            var host = hostBuilder.Build();


            ServerOptions serverOptions;
            try
            {
                ServerOptions serverOptions = host.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
                JokeSelectionOptions jokeSelectionOptions = host.Services.GetRequiredService<IOptions<JokeSelectionOptions>>().Value;
                JokesOptions jokesOptions = host.Services.GetRequiredService<IOptions<JokesOptions>>().Value;
            }
            catch (OptionsValidationException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            finally
            {

            }

            
            var jokes = jokesOptions.Items;
            var jokeSelectorType = jokeSelectionOptions.Strategy;
            var maxClients = serverOptions.MaxClients;

            IJokeSelector jokeSelector;


            if (jokeSelectorType == "Sequential")
            {
                jokeSelector = new SequentialJokeSelector(jokes);
            }
            else if (jokeSelectorType == "Random")
            {
                jokeSelector = new RandomJokeSelector(jokes);
            }
            else
            {
                throw new InvalidOperationException("Invalid joke selection strategy specified in configuration.");
            }


            int port = configuration.GetValue<int>("Server:Port");

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine("Server started. Waiting for client...");
            
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                if (Volatile.Read(ref currentClients) >= maxClients)
                {
                    Console.WriteLine("Maximum number of clients reached. Waiting for a client to disconnect...");
                    client.Close();
                    continue;
                }
                KnockKnockJoke noClue = jokeSelector.SelectJoke();
                Thread handleClientThread = new Thread(() => HandleClient(client, noClue));
                Interlocked.Increment(ref currentClients);
                handleClientThread.Start();
            }

            listener.Stop();
        }
        public static void HandleClient(TcpClient client, KnockKnockJoke joke)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                

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

                    string clientResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    string serverResponse = conversation.ProcessResponse(clientResponse);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(serverResponse);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Client Disconected {ex.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref currentClients);
                client.Close();
            }

        }
    }
}