using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using HtmlAgilityPack;

class Program
{
    static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();

    private DiscordSocketClient client;
    private CommandService commands;
    private IServiceProvider services;

    public async Task RunBot()
    {
        client = new DiscordSocketClient();
        commands = new CommandService();
        services = new ServiceCollection()
            .AddSingleton(client)
            .AddSingleton(commands)
            .BuildServiceProvider();

        string token = "MTAxMTY3NDYwMzMxNzM3OTE3OA.GVOFok.ALhJ-OXlRGLbcrPxoaOWLBgaLEmK0v84X-6YOk";

        client.Log += Client_Log;

        await RegisterCommandsAsync();

        await client.LoginAsync(TokenType.Bot, token);

        await client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Client_Log(LogMessage arg)
    {
        Console.WriteLine(arg);
        return Task.CompletedTask;
    }

    public async Task RegisterCommandsAsync()
    {
        client.MessageReceived += HandleCommandAsync;
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        SocketUserMessage message = arg as SocketUserMessage;
        SocketCommandContext context = new SocketCommandContext(client, message);
        if (message.Author.IsBot) return;

        int argPos = 0;
        if(message.HasStringPrefix("/",ref argPos))
        {
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
        }
    }
}

public class Commands : ModuleBase<SocketCommandContext>
{
    [Command("Ping")]
    public async Task Ping()
    {
        await ReplyAsync("Pong");
    }

    [Command("Image")]
    public async Task Image(string query)
    {
        string url = $"https://www.google.com/search?$false&source=lnms&tbm=isch&sa=X&tbs=$&q={query}";
        HtmlWeb web = new HtmlWeb();
        web.UserAgent = "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
        HtmlDocument doc = web.Load(url);
        HtmlNodeCollection imageContainter = doc.DocumentNode.SelectNodes("//div[@class = 'bRMDJf islir']");
        string imgsrc = "Not Found";
        foreach(HtmlNode div in imageContainter)
        {
            if(div != null)
            {
                HtmlNode img = div.FirstChild;
                if(img != null)
                {
                    if (img.Attributes["src"]?.Value.ToString()[0] != 'd' || img.Attributes["data-src"]?.Value.ToString()[0] != 'd')
                    {
                        imgsrc = img.Attributes["src"]?.Value;
                        imgsrc = img.Attributes["data-src"]?.Value;
                    }
                }
            }
        }
        await ReplyAsync(imgsrc);
    }

}
