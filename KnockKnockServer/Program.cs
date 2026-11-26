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
        private static List<TcpClient> connectedClients = new List<TcpClient>();
        private static readonly object clientLock = new object();
        private static bool isShuttingDown = false;


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

            hostBuilder.Services.AddSingleton<IJokeSelector>(services => 
            {
                var selectionOptions = services.GetRequiredService<IOptions<JokeSelectionOptions>>().Value;
                var jokeOptions = services.GetRequiredService<IOptions<JokesOptions>>().Value;


            });
            

            var host = hostBuilder.Build();


            ServerOptions serverOptions;
            JokeSelectionOptions jokeSelectionOptions;
            JokesOptions jokesOptions;
            try
            {
                serverOptions = host.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
                jokeSelectionOptions = host.Services.GetRequiredService<IOptions<JokeSelectionOptions>>().Value;
                jokesOptions = host.Services.GetRequiredService<IOptions<JokesOptions>>().Value;
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
                Console.WriteLine("Invalid joke selection strategy specified in configuration.");
                return;
            }



            TcpListener listener = new TcpListener(IPAddress.Any, serverOptions.Port);

            listener.Start();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating immediately
                Console.WriteLine("Shutting down server...");
                lock (clientLock)
                {
                    isShuttingDown = true;
                    foreach (var client in connectedClients)
                    {
                        client.Close();
                    }
                    connectedClients.Clear();
                }
                listener.Stop();
            };
            


            Console.WriteLine("Server started. Waiting for client...");
            
            while (true)
            {
                TcpClient client;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (SocketException)
                {
                    lock (clientLock)
                    {
                        if (isShuttingDown == true)
                        {
                            break;
                        }
                    }
                    throw;
                }
                catch (InvalidOperationException)
                {
                    lock (clientLock)
                    {
                        if (isShuttingDown == true)
                        {
                            break;
                        }
                    }
                    throw;
                }

                if (Volatile.Read(ref currentClients) >= maxClients)
                {
                    Console.WriteLine("Maximum number of clients reached. Waiting for a client to disconnect...");
                    client.Close();
                    continue;
                }

                KnockKnockJoke noClue = jokeSelector.SelectJoke();
                Thread handleClientThread = new Thread(() => HandleClient(client, noClue));
                Interlocked.Increment(ref currentClients);

                lock (clientLock)
                {
                    if (isShuttingDown)
                    {
                        Console.WriteLine("Server is shutting down. Rejecting new client connection.");
                        client.Close();
                        Interlocked.Decrement(ref currentClients);
                        break;
                    }

                    connectedClients.Add(client);
                }
                handleClientThread.Start();
                Console.WriteLine("Client connected.");
            }

            listener.Stop();
        }
        public static void HandleClient(TcpClient client, KnockKnockJoke joke)
        {
            try
            {
                UTF8Encoding encoder = new UTF8Encoding(false);
                NetworkStream stream = client.GetStream();

                StreamWriter writer = new StreamWriter(stream, encoder) { AutoFlush = true };
                StreamReader reader = new StreamReader(stream, encoder);


                KnockKnockConversation conversation = new KnockKnockConversation(joke);

                string message = "Knock Knock!";
                writer.WriteLine(message);



                while (conversation.State != ConversationState.Finished)
                {
                    var result = reader.ReadLine();

                    // Check if the client has closed the connection
                    if (result == null)
                    {
                        Console.WriteLine($"Client {currentClients} closed the connection.");
                        break;
                    }

                    string clientResponse = result.Trim();
                    string serverResponse = conversation.ProcessResponse(clientResponse);


                    if (ConversationState.Finished == conversation.State)
                    {
                        Console.WriteLine("Client finished the conversation.");
                        writer.WriteLine("THEMEGASECRETKEYTOCLOSE");
                    }

                    writer.WriteLine(serverResponse);

                }
            }

            catch (IOException ex)
            {
                Console.WriteLine("Connection to client closed : " + ex.Message);
            }
            catch (ObjectDisposedException)
            {
                lock  (clientLock)
                {
                    if (isShuttingDown == false)
                    {
                        throw;
                    }
                }
            }
            finally
            {
                Console.WriteLine("Connection to client closed.");

                Interlocked.Decrement(ref currentClients);
                client.Close();
                lock (clientLock)
                {
                    connectedClients.Remove(client);
                }
            }

        }
    }
}