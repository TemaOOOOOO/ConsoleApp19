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
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var messageText = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            Console.WriteLine($"Received a text message in chat {chatId}: {messageText}");

            if (messageText.StartsWith("/search"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2)
                {
                    var singer = parts[1];
                    var response = await GetAlbumsOfSinger(singer);

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
           

            if (messageText.StartsWith("/albums"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2)
                {
                    var albums = parts[1];
                    var response = await GetAlbum(albums);

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
                        text: "Please provide an album name. Usage: /search [album]",
                        cancellationToken: cancellationToken
                    );
                }
            }
           

            if (messageText.StartsWith("/add"))
            {
                var parts = messageText.Split(' ', 3);

                if (parts.Length == 3 && int.TryParse(parts[1], out int id))
                {
                    var request = parts[2];
                    var response = await AddHistoryToDatabase(id, request);

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
            

            if (messageText.StartsWith("/delete"))
            {
                var parts = messageText.Split(' ', 2);

                if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                {
                    var response = await DeleteHistoryFromDatabase(id);

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

    private static async Task<string> GetAlbumsOfSinger(string singer)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7284/Spoti/AlbumsOfSinger?singer={singer}"),
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

    private static async Task<string> GetAlbum(string album)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost:7284/Spoti/AlbumByName?album={album}"),
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                Result result = JsonConvert.DeserializeObject<Result>(body);

                if (result == null || result.totalCount == 0)
                {
                    return $"No albums found {album}.";
                }

                return $"Albums of {album}:\n- " + string.Join("\n- ", result.ToString());
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving albums: {ex.Message}";
        }
    }

    private static async Task<string> AddHistoryToDatabase(int id, string request)
    {
        try
        {
            var client = new HttpClient();
            var content = new StringContent(
                JsonConvert.SerializeObject(new { Id = id, Request = request }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync($"https://localhost:7284/databasePut?id={id}&request={request}", null);
            response.EnsureSuccessStatusCode();

            return "History added successfully.";
        }
        catch (Exception ex)
        {
            return $"Error adding history: {ex.Message}";
        }
    }

    private static async Task<string> DeleteHistoryFromDatabase(int id)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.DeleteAsync($"https://localhost:7284/databaseDel?id={id}");
            response.EnsureSuccessStatusCode();

            return "History deleted successfully.";
        }
        catch (Exception ex)
        {
            return $"Error deleting history: {ex.Message}";
        }
    }


    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"An error occurred: {exception.Message}");
        await Task.CompletedTask;

    }
}
