using ConsoleApp19;
using Newtonsoft.Json;
using System;
using RestSharp;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using System.Reflection;

class Program
{
    private static TelegramBotClient botClient;
    private static HttpClient httpClient;

    static async Task Main(string[] args)
    {
        botClient = new TelegramBotClient("7027020902:AAEDxzmcc4r0jBMtRnHF21p32LDTacmU8sY");
        httpClient = new HttpClient();

        try
        {
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting bot information: {ex.Message}");
            return;
        }

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message } 
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        
        cts.Cancel();
        cts.Dispose();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var messageText = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            Console.WriteLine($"Received a text message in chat {chatId}: {messageText}");

            if (messageText.StartsWith("/album"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2)
                {
                    var singer = parts[1];
                    var response = await GetAlbums(singer);

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Please provide a singer name. Usage: /search [singer]",
                        cancellationToken: cancellationToken
                    );
                }
            }
           

            if (messageText.StartsWith("/artist"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2)
                {
                    var artist = parts[1];
                    var response = await GetArtist(artist);

                    string str = string.Empty;
                    try
                    {
                        str += response.visuals.avatarImage.sources[0].url + "\n";
                    }
                    catch (Exception ex)
                    {
                        str += "Image failed to load";
                    }
                    str += "Name: " + response.profile.name + "\n";
                    str += response.uri;
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: str,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Please provide an album name. Usage: /search [album]",
                        cancellationToken: cancellationToken
                    );
                }
            }
           

            if (messageText.StartsWith("/add"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2)
                {
                    string request = parts[1];
                    string response = await AddHistoryToDatabase(request);

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Please provide an id and a request. Usage: /add [id] [request]",
                        cancellationToken: cancellationToken
                    );
                }
            }
            if (messageText.StartsWith("/favorites"))
            {
                var artists = GetHistory().Result;
                foreach (Response artist in artists)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: artist.ToString(),
                        cancellationToken: cancellationToken
                    );
                }
            }


            if (messageText.StartsWith("/delete"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2)
                {
                    var response = await DeleteHistoryFromDatabase(parts[1]);

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Please provide an id. Usage: /delete [id]",
                        cancellationToken: cancellationToken
                    );
                }
            }
            

        }
    }

    private static async Task<string> GetAlbums(string singer)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7284/Spoti/SearchAlbums?singer={singer}"),
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                Result result = JsonConvert.DeserializeObject<Result>(body);

                if (result == null || result.totalCount == 0)
                {
                    return $"No albums found for singer {singer}.";
                }

                return $"Albums of {singer}:\n- " + string.Join("\n- ", result.ToString());
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving albums: {ex.Message}";
        }
    }

    private static async Task<Data> GetArtist(string album)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://localhost:7284/Spoti/SearchArtists?artist={album}"),
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            Result result = JsonConvert.DeserializeObject<Result>(body);

            return result.items[0].data;
        }
    }

    private static async Task<string> AddHistoryToDatabase(string name)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"https://localhost:7284/databasePut?name={name}"),
            };
            client.SendAsync(request);

            return "History added successfully.";
        }
        catch (Exception ex)
        {
            return $"Error adding history: {ex.Message}";
        }
    }

    private static async Task<string> DeleteHistoryFromDatabase(string name)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.DeleteAsync($"https://localhost:7284/databaseDel?name={name}");
            response.EnsureSuccessStatusCode();

            return "History deleted successfully.";
        }
        catch (Exception ex)
        {
            return $"Error deleting history: {ex.Message}";
        }
    }

    private static async Task<List<Response>> GetHistory()
    {   
        var client = new HttpClient();
        var response = await client.GetAsync($"https://localhost:7284/Spoti/database");
        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<List<Response>>(response.Content.ReadAsStringAsync().Result);
    }


    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"An error occurred: {exception.Message}");
        await Task.CompletedTask;

    }
}
